﻿using IoT.DTDL;
using IoT.Simulator.Extensions;
using IoT.Simulator.Models;
using IoT.Simulator.Settings;
using IoT.Simulator.Tools;

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Simulator.Services
{
    public class ModuleSimulationService : IModuleSimulationService
    {
        public string ServiceId { get { return ModuleSettings?.ArtifactId; } }
        public ModuleSettings ModuleSettings { get; private set; }
        public SimulationSettingsModule SimulationSettings { get; private set; }
        private ILogger _logger;
        private int _telemetryInterval;
        private bool _stopProcessing;
        private ModuleClient _moduleClient;

        private IDTDLMessageService _dtdlMessagingService;
        private IDTDLCommandService _dtdlCommandService;

        private DTDLModelItem _defaultModel;

        public ModuleSimulationService(ModuleSettings settings, SimulationSettingsModule simulationSettings, IDTDLMessageService dtdlMessagingService, IDTDLCommandService dtdlCommandService, ILoggerFactory loggerFactory)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (simulationSettings == null)
                throw new ArgumentNullException(nameof(simulationSettings));

            if (dtdlMessagingService == null)
                throw new ArgumentNullException(nameof(dtdlMessagingService));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            string logPrefix = "system".BuildLogPrefix();

            ModuleSettings = settings;
            SimulationSettings = simulationSettings;
            _logger = loggerFactory.CreateLogger<ModuleSimulationService>();

            _dtdlMessagingService = dtdlMessagingService;
            _dtdlCommandService = dtdlCommandService;

            _telemetryInterval = 10;
            _stopProcessing = false;

            _moduleClient = ModuleClient.CreateFromConnectionString(ModuleSettings.ConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Logger created.");
            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Module simulator created.");

            //Default DTDL Model
            _defaultModel = ModuleSettings?.SupportedModels?.SingleOrDefault(i => i.ModelId == ModuleSettings.DefaultModelId);
            if (_defaultModel == null)
                throw new Exception("No supported model corresponds to the default model Id.");
        }

        ~ModuleSimulationService()
        {
            if (_moduleClient != null)
            {
                _moduleClient.CloseAsync();
                _moduleClient.Dispose();

                string logPrefix = "system".BuildLogPrefix();

                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Module simulator disposed.");
            }
        }

        public async Task InitiateSimulationAsync()
        {
            string logPrefix = "system".BuildLogPrefix();

            IoTTools.CheckModuleConnectionStringData(ModuleSettings.ConnectionString, _logger);

            // Connect to the IoT hub using the MQTT protocol

            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Module client created.");

            if (SimulationSettings.EnableTwinPropertiesDesiredChangesNotifications)
            {
                await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChange, null);
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Twin Desired Properties update callback handler registered.");
            }

            //Configuration
            if (SimulationSettings.EnableC2DDirectMethods)
                //Register C2D Direct methods handlers            
                await RegisterC2DDirectMethodsHandlersAsync(_moduleClient, ModuleSettings, _logger);

            if (SimulationSettings.EnableC2DMessages)
                //Start receiving C2D messages

                ReceiveC2DMessagesAsync(_moduleClient, ModuleSettings, _logger);

            //Messages
            if (SimulationSettings.EnableTelemetryMessages)
                SendDeviceToCloudMessagesAsync(_moduleClient, ModuleSettings.DeviceId, ModuleSettings.ModuleId, _logger); //interval is a global variable changed by processes

            if (SimulationSettings.EnableReadingTwinProperties)
            {
                //Twins
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::INITIALIZATION::Retrieving twin.");
                Twin twin = await _moduleClient.GetTwinAsync();

                if (twin != null)
                    _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::INITIALIZATION::Device twin: {JsonConvert.SerializeObject(twin, Formatting.Indented)}.");
                else
                    _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::INITIALIZATION::No device twin.");
            }

            _moduleClient.SetConnectionStatusChangesHandler(new ConnectionStatusChangesHandler(ConnectionStatusChanged));
        }

        private void ConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            string logPrefix = "c2dmessages".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Connection status changed-New status:{status.ToString()}-Reason:{reason.ToString()}.");
        }


        #region D2C                

        // Async method to send simulated telemetry
        internal async Task SendDeviceToCloudMessagesAsync(ModuleClient moduleClient, string deviceId, string moduleId, ILogger logger)
        {
            // Initial telemetry values
            int counter = 1;
            string logPrefix = "data".BuildLogPrefix();

            string messageString = string.Empty;
            Message message = null;

            using (logger.BeginScope($"{logPrefix}::{ModuleSettings.ArtifactId}::MEASURED DATA"))
            {
                var defaultModel = ModuleSettings.SupportedModels.SingleOrDefault(i => i.ModelId == ModuleSettings.DefaultModelId);
                if (defaultModel == null)
                    throw new Exception("No supported model corresponds to the default model Id.");

                while (true)
                {
                    //Randomize data                    
                    messageString = await _dtdlMessagingService.GetRandomizedMessageAsync(deviceId, moduleId, ModuleSettings.DefaultModelId, defaultModel.ModelPath);

                    message = new Message(Encoding.UTF8.GetBytes(messageString));
                    message.Properties.Add("messageType", "dtdlMessage");

                    // Add a custom application property to the message.
                    // An IoT hub can filter on these properties without access to the message body.
                    //message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
                    message.ContentType = "application/json";
                    message.ContentEncoding = "utf-8";

                    // Send the tlemetry message
                    await moduleClient.SendEventAsync(message);
                    counter++;

                    logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Sent message: {messageString}.");
                    logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::COUNTER: {counter}.");

                    if (_stopProcessing)
                    {
                        logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::STOP PROCESSING.");
                        break;
                    }

                    await Task.Delay(_telemetryInterval * 1000);
                }
            }
        }

        #endregion

        #region C2D
        #region Messages
        private async Task ReceiveC2DMessagesAsync(ModuleClient moduleClient, ModuleSettings settings, ILogger logger)
        {
            string logPrefix = "c2dmessages".BuildLogPrefix();

            await moduleClient.SetMessageHandlerAsync(C2DMessageHandler, null);
            logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::C2D MESSAGE reception handler registered.");
        }

        private async Task<MessageResponse> C2DMessageHandler(Message message, object context)
        {
            string logPrefix = "c2dmessages".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Receiving cloud to device messages from service.");

            if (message != null)
            {
                _logger.LogInformation($"{logPrefix}::C2D MESSAGE RECEIVED:{JsonConvert.SerializeObject(message, Formatting.Indented)}.");
                return MessageResponse.Completed;
            }
            else
                return MessageResponse.None;
        }

        private async Task RegisterC2DDirectMethodsHandlersAsync(ModuleClient moduleClient, ModuleSettings settings, ILogger logger)
        {
            string logPrefix = "modules.c2ddirectmethods".BuildLogPrefix();

            try
            {
                // Create a handler for the direct method call

                await moduleClient.SetMethodHandlerAsync("SetTelemetryInterval", SetTelemetryInterval, null);
                logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD SetTelemetryInterval registered.");

                await moduleClient.SetMethodHandlerAsync("Reboot", Reboot, null);
                logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD Reboot registered.");

                await moduleClient.SetMethodHandlerAsync("OnOff", StartOrStop, null);
                logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD OnOff registered.");

                await moduleClient.SetMethodHandlerAsync("ReadTwins", ReadTwinsAsync, null);
                logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD ReadTwins registered.");

                await moduleClient.SetMethodHandlerAsync("GenericJToken", GenericJToken, null);
                logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD GenericJToken registered.");

                await moduleClient.SetMethodHandlerAsync("Generic", Generic, null);
                logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD Generic registered.");

                //Default
                await moduleClient.SetMethodDefaultHandlerAsync(DefaultC2DMethodHandler, null);
                _logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD Default handler registered.");

                //DTDL Commands                
                var modelWithCommands = await _dtdlCommandService.GetCommandsAsync(ModuleSettings.DefaultModelId, _defaultModel.ModelPath);
                if (modelWithCommands != null && modelWithCommands.Any())
                {
                    _logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD DTDL command handlers.");
                    foreach (var model in modelWithCommands)
                    {
                        if (model.Value != null && model.Value.Commands != null && model.Value.Commands.Count > 0)
                        {
                            string commandName = string.Empty;
                            foreach (JObject command in model.Value.Commands)
                            {
                                if (command.Properties().Any())
                                {
                                    commandName = command.Properties().First().Name;
                                    await moduleClient.SetMethodHandlerAsync(
                                    commandName,
                                    DTDLCommandHandler,
                                    new DTDLCommandHandlerContext
                                    {
                                        CommandName = commandName,
                                        CommandRequestPayload = command.Descendants().Where(d => d is JObject && d["request"] != null).SingleOrDefault(), //TODO: replace this with the actual JSON Schema of the request
                                        CommandResponsePayload = command.Descendants().Where(d => d is JObject && d["response"] != null).SingleOrDefault() //TODO: replace this with the actual JSON Schema of the response
                                    });

                                    _logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD DTDL commands handlers registered:: {commandName}");
                                }
                                else
                                    _logger.LogError($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD DTDL commands handlers::bad formed command structure.");
                            }
                        }
                        else
                            _logger.LogTrace($"{logPrefix}::{ModuleSettings.ArtifactId}::DIRECT METHOD DTDL commands:: no commands have been declared in this model.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{ModuleSettings.ArtifactId}::ERROR:RegisterC2DDirectMethodsHandlersAsync:{ex.Message}.");
            }
        }
        #endregion

        #region Direct Methods
        // Handle the direct method call
        //https://docs.microsoft.com/en-us/azure/iot-hub/quickstart-control-device-dotnet
        private Task<MethodResponse> SetTelemetryInterval(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // Check the payload is a single integer value
            if (Int32.TryParse(data, out _telemetryInterval))
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Telemetry interval set to {_telemetryInterval} seconds.");

                // Acknowledge the direct method call with a 200 success message
                string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            else
            {
                // Acknowledge the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }

        private Task<MethodResponse> GenericJToken(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);
            var content = JToken.FromObject(data);

            if (content != null)
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Generic call received: {JsonConvert.SerializeObject(content)}.");

                // Acknowledge the direct method call with a 200 success message
                string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            else
            {
                // Acknowledge the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }

        private Task<MethodResponse> Generic(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);

            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Generic call received: {data}");

            // Acknowledge the direct method call with a 200 success message
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> Reboot(MethodRequest methodRequest, object userContext)
        {
            // In a production device, you would trigger a reboot scheduled to start after this method returns
            // For this sample, we simulate the reboot by writing to the console and updating the reported properties 
            RebootOrchestration();

            string result = "'Reboot command has been received and planified.'";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private async Task RebootOrchestration()
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Reboot order received.");
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Stoping processes...");

                _stopProcessing = true;


                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Processes stopped.");
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Rebooting...");

                // Update device twin with reboot time. 
                TwinCollection reportedProperties, reboot, lastReboot, rebootStatus;
                lastReboot = new TwinCollection();
                reboot = new TwinCollection();
                reportedProperties = new TwinCollection();
                rebootStatus = new TwinCollection();

                lastReboot["lastReboot"] = DateTime.Now;
                reboot["reboot"] = lastReboot;
                reboot["rebootStatus"] = "rebooting";
                reportedProperties["iothubDM"] = reboot;

                await _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);

                await Task.Delay(10000);

                // Update device twin with reboot time. 
                reboot["rebootStatus"] = "online";
                reportedProperties["iothubDM"] = reboot;

                await _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Reboot over and system runing again.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{ModuleSettings.ArtifactId}::ERROR::RebootOrchestration:{ex.Message}.");
            }
            finally
            {
                _stopProcessing = false;
            }
        }

        private Task<MethodResponse> StartOrStop(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::StartOrStop command has been received and planified.");

            if (methodRequest.Data == null)
                throw new ArgumentNullException("methodRequest.Data");

            JObject jData = JsonConvert.DeserializeObject<JObject>(methodRequest.DataAsJson);

            JArray settings = (JArray)jData["data"];

            //Send feedback
            string result = "'StartOrStop command has been received.'";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> ReadTwinsAsync(MethodRequest methodRequest, object userContext)
        {
            ReadTwins();

            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> DefaultC2DMethodHandler(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            _logger.LogWarning($"{logPrefix}::{ModuleSettings.ArtifactId}::WARNING::{methodRequest.Name} has been called but there is no registered specific method handler.");

            string message = $"Request direct method: {methodRequest.Name} but no specifif direct method handler.";

            //Send feedback
            var response = new
            {
                result = message,
                payload = methodRequest.Data != null ? methodRequest.DataAsJson : string.Empty
            };

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response, Formatting.Indented)), 200));

        }

        private Task<MethodResponse> DTDLCommandHandler(MethodRequest methodRequest, object commandContext)
        {
            string logPrefix = "modules.c2ddirectmethods.dtdlcommand.handler".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);

            _logger.LogDebug($"{logPrefix}::DTDL Command called: {data}.");

            if (commandContext != null && commandContext is DTDLCommandHandlerContext)
            {
                var context = commandContext as DTDLCommandHandlerContext;
                _logger.LogDebug($"{logPrefix}::DTDL Command response: {context.CommandResponsePayload} to request {data}.");
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(context.CommandResponsePayload, Formatting.Indented)), 200));
            }
            else
            {
                _logger.LogDebug($"{logPrefix}::No DTDL command context has been found for the command.");
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { error = "No DTDL command context has been found." })), 500));
            }
        }
        #endregion
        #endregion

        #region Twins
        //https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-twin-getstarted
        internal async Task ReportConnectivityAsync()
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Sending connectivity data as reported property.");

                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity["type"] = "cellular";
                connectivity["signalPower"] = "low";
                reportedProperties["connectivity"] = connectivity;
                await _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{ModuleSettings.ArtifactId}::ERROR::ReportConnectivityAsync:{ex.Message}.");
            }
        }

        internal async Task GenericTwinReportedUpdateAsync(string deviceId, string sensorId, string propertyName, dynamic value)
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Sending generic reported property update:: {propertyName}-{value}.");

                TwinCollection reportedProperties, configuration;
                reportedProperties = new TwinCollection();
                reportedProperties[propertyName] = value;

                await _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{ModuleSettings.ArtifactId}::ERROR::GenericTwinReportedUpdateAsync:{ex.Message}.");
            }
        }

        internal async Task ReportFirwmareUpdateAsync(string firmwareVersion, string firmwareUrl)
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::Sending firmware update notification.");

                TwinCollection reportedProperties;
                reportedProperties = new TwinCollection();

                reportedProperties["newFirmwareVersion"] = firmwareVersion;
                reportedProperties["Ur"] = firmwareUrl;
                await _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{ModuleSettings.ArtifactId}::ERROR::ReportFirwmareUpdateAsync:{ex.Message}.");
            }
        }

        internal async Task ReadTwins()
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            //Twins
            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS::Reading...");

            Twin twin = await _moduleClient.GetTwinAsync();

            if (twin != null)
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS:: {JsonConvert.SerializeObject(twin, Formatting.Indented)}.");
            else
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS:: No twins available.");
        }

        private async Task OnDesiredPropertyChange(TwinCollection desiredproperties, object usercontext)
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED properties changes request notification.");

            if (desiredproperties != null)
            {
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED::{JsonConvert.SerializeObject(desiredproperties, Formatting.Indented)}");

                //Check if the properties belong to the WritableProperties
                //https://docs.microsoft.com/en-us/azure/iot-central/core/concepts-telemetry-properties-commands

                var dtdlParsedMode = await DTDLHelper.GetAndParseDTDLAsync(_defaultModel.ModelId, _defaultModel.ModelPath);

                if (dtdlParsedMode != null && dtdlParsedMode.Any())
                {
                    //Writable properties
                    var writableProperties = dtdlParsedMode.Where(i => i.Value.EntityKind == DTEntityKind.Property && ((DTPropertyInfo)i.Value).Writable)?.Select(i => i.Value as DTPropertyInfo).AsQueryable();
                    if (writableProperties != null && writableProperties.Any())
                    {
                        JObject objectDesiredProperties = JObject.Parse(desiredproperties.ToJson(Formatting.Indented));
                        var intersection = writableProperties.Join<DTPropertyInfo, JProperty, string, DTPropertyInfo>(objectDesiredProperties.Properties(), w => w.Name, o => o.Name, (w, o) => w);

                        if (intersection != null && intersection.Any())
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (var item in intersection)
                            {
                                builder.Append($"Desired property:'{item.Name}'-Value:{desiredproperties[item.Name]}.");
                            }

                            _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED INCLUDED IN DTDL-CHANGED: {builder.ToString()}");
                        }
                        else
                            _logger.LogWarning($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED::None of the properties sent by the solution is defined in the DTDL model.");

                    }
                }
            }
            else
                _logger.LogDebug($"{logPrefix}::{ModuleSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED properties change is emtpy.");

        }
        #endregion
    }
}
