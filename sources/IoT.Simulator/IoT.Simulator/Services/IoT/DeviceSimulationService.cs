﻿using IoT.DTDL;
using IoT.Simulator.Extensions;
using IoT.Simulator.Models;
using IoT.Simulator.Settings;
using IoT.Simulator.Tools;

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Simulator.Services
{
    public class DeviceSimulationService : ISimulationService
    {
        private const string _TWIN_TAG_SUPPORTED_MODELS_PROPERTY_NAME = "";
        private readonly ILogger<DeviceSimulationService> _logger;

        private DeviceSettings _deviceSettings;
        private SimulationSettingsDevice _simulationSettings;
        private DeviceClient _deviceClient;
        private string _deviceId;
        private string _iotHub;
        private int _telemetryInterval;
        private bool _stopProcessing = false;

        private IDTDLMessageService _dtdlMessageService;
        private IDTDLCommandService _dtdlCommandService;

        private DTDLModelItem _defaultModel;

        public DeviceSimulationService(IOptions<DeviceSettings> deviceSettings, IDTDLMessageService dtdlMessageService, IDTDLCommandService dtdlCommandService, ILoggerFactory loggerFactory)
        {
            if (deviceSettings == null)
                throw new ArgumentNullException(nameof(deviceSettings));

            if (deviceSettings.Value == null)
                throw new ArgumentNullException("deviceSettings.Value", "No device configuration has been loaded.");

            if (deviceSettings.Value.SimulationSettings == null)
                throw new ArgumentNullException("deviceSettings.Value.SimulationSettings");

            if (dtdlMessageService == null)
                throw new ArgumentNullException(nameof(dtdlMessageService));

            if (dtdlCommandService == null)
                throw new ArgumentNullException(nameof(dtdlCommandService));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory), "No logger factory has been provided.");

            _deviceSettings = deviceSettings.Value;
            _simulationSettings = deviceSettings.Value.SimulationSettings;

            _deviceId = _deviceSettings.DeviceId;
            _iotHub = _deviceSettings.HostName;

            _telemetryInterval = _simulationSettings.TelemetryFrecuency;

            _logger = loggerFactory.CreateLogger<DeviceSimulationService>();

            _dtdlMessageService = dtdlMessageService;
            _dtdlCommandService = dtdlCommandService;

            string logPrefix = "system".BuildLogPrefix();
            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Logger created.");
            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Device simulator created.");

            //Default DTDL Model
            _defaultModel = _deviceSettings?.SupportedModels?.SingleOrDefault(i => i.ModelId == _deviceSettings.DefaultModelId);
            if (_defaultModel == null)
                throw new Exception("No supported model corresponds to the default model Id.");
        }

        ~DeviceSimulationService()
        {
            if (_deviceClient != null)
            {
                _deviceClient.CloseAsync();
                _deviceClient.Dispose();

                string logPrefix = "system".BuildLogPrefix();

                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Device simulator disposed.");
            }
        }

        public async Task InitiateSimulationAsync()
        {
            string logPrefix = "system".BuildLogPrefix();

            try
            {
                IoTTools.CheckDeviceConnectionStringData(_deviceSettings.ConnectionString, _logger);
                
                // Connect to the IoT hub using the MQTT protocol
                if (!string.IsNullOrEmpty(_deviceSettings.DefaultModelId))
                {
                    _deviceClient = DeviceClient.CreateFromConnectionString(_deviceSettings.ConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt, new ClientOptions { ModelId = _deviceSettings.DefaultModelId });
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Device client created. ModelId:{_deviceSettings.DefaultModelId}");
                }
                else
                {
                    _deviceClient = DeviceClient.CreateFromConnectionString(_deviceSettings.ConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Device client created.");
                }

                if (_simulationSettings.EnableTwinPropertiesDesiredChangesNotifications)
                {
                    await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChange, null);
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Twin Desired Properties update callback handler registered.");
                }

                //Configuration
                if (_simulationSettings.EnableC2DDirectMethods)
                    //Register C2D Direct methods handlers            
                    await RegisterC2DDirectMethodsHandlersAsync();

                if (_simulationSettings.EnableC2DMessages)
                    //Start receiving C2D messages
                    ReceiveC2DMessagesAsync();

                //Messages
                if (_simulationSettings.EnableLatencyTests)
                    SendDeviceToCloudLatencyTestAsync(_deviceId, _simulationSettings.LatencyTestsFrecuency);

                if (_simulationSettings.EnableTelemetryMessages)
                    SendDeviceToCloudMessagesAsync(_deviceId); //interval is a global variable changed by processes

                if (_simulationSettings.EnableReadingTwinProperties)
                {
                    //Twins
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::INITIALIZATION::Retrieving twin.");
                    Twin twin = await _deviceClient.GetTwinAsync();

                    if (twin != null)
                        _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::INITIALIZATION::Device twin: {JsonConvert.SerializeObject(twin, Formatting.Indented)}.");
                    else
                        _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::INITIALIZATION::No device twin.");
                }

                if (_simulationSettings.EnableFileUpload)
                {
                    throw new NotImplementedException("File upload feature has not been implemented yet.");
                }

                _deviceClient.SetConnectionStatusChangesHandler(new ConnectionStatusChangesHandler(ConnectionStatusChanged));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::ERROR::InitiateSimulationAsync:{ex.Message}.");
            }
        }

        private async Task<CloudDTDLSettings> GetCloudDTDLModelSettingsAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            string logPrefix = "getdtdlmodelsettings".BuildLogPrefix();
            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Getting the DTDL settings of the device.");

            Twin twin = await _deviceClient.GetTwinAsync();
            if (twin == null)
                throw new Exception($"No device twin has been found for the device {deviceId}.");

            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Device twin loaded.");

            CloudDTDLSettings result = new CloudDTDLSettings { DefaultModelId = twin.ModelId };
            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Default DTDL model settings updated.");

            if (twin.Tags != null && twin.Tags.Contains(_TWIN_TAG_SUPPORTED_MODELS_PROPERTY_NAME))
            {
                TwinCollection supportedModels = twin.Tags[_TWIN_TAG_SUPPORTED_MODELS_PROPERTY_NAME];
                if (supportedModels != null)
                {
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Other supported DTDL models loaded.");

                    JArray jSupportedModels = JArray.Parse(supportedModels.ToJson());
                    string itemModelId = string.Empty;
                    DTDLModelType itemModelType;
                    foreach (var jModel in jSupportedModels)
                    {
                        itemModelId = jModel.Value<string>("modelId");
                        itemModelType = jModel.Value<DTDLModelType>("modelType");
                        if (itemModelId == result.DefaultModelId)
                        {
                            if (result.Models.SingleOrDefault(i=>i.ModelId == result.DefaultModelId) != null)
                                result.Models.Single(i => i.ModelId == result.DefaultModelId).ModelType = itemModelType;
                            else
                                result.Models.Add(new DTDLModelItem { ModelId = itemModelId, ModelType = itemModelType });
                        }
                        else
                            result.Models.Add(new DTDLModelItem { ModelId = itemModelId, ModelType = itemModelType });

                        _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Additional supported DTDL model {itemModelId} of type {itemModelType.ToString()} added.");
                    }
                }
            }

            return result;
        }

        private void ConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            string logPrefix = "system".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Connection status changed-New status:{status.ToString()}-Reason:{reason.ToString()}.");
        }

        #region D2C                

        // Async method to send simulated telemetry
        internal async Task SendDeviceToCloudMessagesAsync(string deviceId)
        {
            int counter = 0;
            string logPrefix = "data".BuildLogPrefix();

            string messageString = string.Empty;
            Message message = null;

            using (_logger.BeginScope($"{logPrefix}::{DateTime.Now}::{_deviceSettings.ArtifactId}::MEASURED DATA"))
            {
                while (true)
                {
                    //Randomize data                    
                    messageString = await _dtdlMessageService.GetRandomizedMessageAsync(deviceId, string.Empty, _deviceSettings.DefaultModelId, _defaultModel.ModelPath);

                    message = new Message(Encoding.UTF8.GetBytes(messageString));
                    message.Properties.Add("messageType", "dtdlMessage");

                    // Add a custom application property to the message.
                    // An IoT hub can filter on these properties without access to the message body.
                    //message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
                    message.ContentType = "application/json";
                    message.ContentEncoding = "utf-8";

                    // Send the tlemetry message
                    await _deviceClient.SendEventAsync(message);
                    counter++;

                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Sent message: {messageString}.");
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::COUNTER: {counter}.");

                    if (_stopProcessing)
                    {
                        _logger.LogDebug($"{logPrefix}::STOP PROCESSING.");
                        break;
                    }

                    await Task.Delay(_telemetryInterval * 1000);
                }
            }
        }

        //Latency tests
        internal async Task SendDeviceToCloudLatencyTestAsync(string deviceId, int interval)
        {
            string logPrefix = "latency".BuildLogPrefix();
            string messageString = string.Empty;
            Message message = null;

            while (true)
            {

                var data = new
                {
                    deviceId = deviceId,
                    messageType = "latency",
                    startTimestamp = DateTime.UtcNow.TimeStamp()
                };


                messageString = JsonConvert.SerializeObject(data);
                message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("messagetype", "latency");
                message.ContentType = "application/json";
                message.ContentEncoding = "utf-8";

                // Send the tlemetry message
                await _deviceClient.SendEventAsync(message);
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::LATENCY TEST::message sent:{messageString}.");

                if (interval <= 0)
                    break;

                if (_stopProcessing)
                {
                    _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::LATENCY TEST::STOP PROCESSING.");
                    break;
                }

                await Task.Delay(interval * 1000);
            }

        }

        //https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-file-upload
        private async Task FileUploadAsync<T>(string iotHub, string deviceId, List<T> dataToUpload)
        {
            try
            {
                if (dataToUpload == null)
                    throw new ArgumentNullException("dataToUpload");

                string sasUriEndPoint = $@"https://{iotHub}/devices/{deviceId}/files"; //1
                string endUploadEndpoint = $@"https://{iotHub}/devices/{deviceId}/files/notifications"; //2

                string localFileName = $"{DateTime.UtcNow.TimeStamp()}-{deviceId}.json";


                File.WriteAllText(localFileName, JsonConvert.SerializeObject(dataToUpload));

                using (var sourceData = new FileStream(localFileName, FileMode.Open))
                {
                    await _deviceClient.UploadToBlobAsync(localFileName, sourceData);
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

        }
        #endregion

        #region C2D
        #region Messages
        private async Task ReceiveC2DMessagesAsync()
        {
            string logPrefix = "c2dmessages".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Device listening to cloud to device messages.");

            Message receivedMessage = null;
            while (true)
            {
                receivedMessage = await _deviceClient.ReceiveAsync();

                if (receivedMessage == null) continue;

                await _deviceClient.CompleteAsync(receivedMessage);
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Received message: {Encoding.ASCII.GetString(receivedMessage.GetBytes())}.");

                receivedMessage = null;
            }
        }
        #endregion

        #region Direct Methods
        private async Task RegisterC2DDirectMethodsHandlersAsync()
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            try
            {
                // Create a handler for the direct method call

                await _deviceClient.SetMethodHandlerAsync("SetTelemetryInterval", SetTelemetryInterval, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD SetTelemetryInterval registered.");

                await _deviceClient.SetMethodHandlerAsync("LatencyTestCallback", LatencyTestCallback, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD LatencyTestCallback registered.");

                await _deviceClient.SetMethodHandlerAsync("SendLatencyTest", SendLatencyTest, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD SendLatencyTest registered.");

                await _deviceClient.SetMethodHandlerAsync("Reboot", Reboot, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD Reboot registered.");

                await _deviceClient.SetMethodHandlerAsync("OnOff", StartOrStop, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD OnOff registered.");

                await _deviceClient.SetMethodHandlerAsync("ReadTwins", ReadTwinsAsync, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD ReadTwins registered.");

                await _deviceClient.SetMethodHandlerAsync("GenericJToken", GenericJToken, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD GenericJToken registered.");

                await _deviceClient.SetMethodHandlerAsync("Generic", Generic, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD Generic registered.");

                //Default
                await _deviceClient.SetMethodDefaultHandlerAsync(DefaultC2DMethodHandler, null);
                _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD Default handler registered.");

                //ADD DTDL COMMANDS                
                var modelWithCommands = await _dtdlCommandService.GetCommandsAsync(_deviceSettings.DefaultModelId, _defaultModel.ModelPath);
                if (modelWithCommands != null && modelWithCommands.Any())
                {
                    _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD DTDL command handlers.");
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
                                    await _deviceClient.SetMethodHandlerAsync(
                                    commandName,
                                    DTDLCommandHandler,
                                    new DTDLCommandHandlerContext
                                    {
                                        CommandName = commandName,
                                        CommandRequestPayload = command.Descendants().Where(d=>d is JObject && d["request"] != null).SingleOrDefault(), //TODO: replace this with the actual JSON Schema of the request
                                        CommandResponsePayload = command.Descendants().Where(d => d is JObject && d["response"] != null).SingleOrDefault() //TODO: replace this with the actual JSON Schema of the response
                                    });

                                    _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD DTDL commands handlers registered:: {commandName}");
                                }
                                else
                                    _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD DTDL commands handlers::bad formed command structure.");
                            }                            
                        }
                        else
                            _logger.LogTrace($"{logPrefix}::{_deviceSettings.ArtifactId}::DIRECT METHOD DTDL commands:: no commands have been declared in this model.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::ERROR::RegisterC2DDirectMethodsHandlersAsync:{ex.Message}.");
            }
        }

        // Handle the direct method call
        //https://docs.microsoft.com/en-us/azure/iot-hub/quickstart-control-device-dotnet
        private Task<MethodResponse> SetTelemetryInterval(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // Check the payload is a single integer value
            if (Int32.TryParse(data, out _telemetryInterval))
            {
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Telemetry interval set to {_telemetryInterval} seconds.");

                // Acknowlege the direct method call with a 200 success message
                string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            else
            {
                // Acknowlege the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
        }

        private Task<MethodResponse> GenericJToken(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);
            var content = JToken.FromObject(data);

            if (content != null)
            {
                _logger.LogDebug($"{logPrefix}::Generic call received: {JsonConvert.SerializeObject(content)}.");

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

            _logger.LogDebug($"{logPrefix}::Generic call received: {data}.");

            // Acknowledge the direct method call with a 200 success message
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> LatencyTestCallback(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            var callbackTimestamp = DateTime.UtcNow.TimeStamp();
            var data = Encoding.UTF8.GetString(methodRequest.Data);
            long initialtimestamp = 0;

            // Check the payload is a single integer value
            if (Int64.TryParse(data, out initialtimestamp))
            {
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Received data: {data}.");
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Latency test callback::latency: {callbackTimestamp - initialtimestamp} s.");
            }

            // Acknowledge the direct method call with a 200 success message
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\",\"latency\":" + (callbackTimestamp - initialtimestamp).ToString() + "}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> SendLatencyTest(MethodRequest methodRequest, object userContext)
        {
            SendDeviceToCloudLatencyTestAsync(_deviceId, 0);

            // Acknowledge the direct method call with a 200 success message
            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private Task<MethodResponse> Reboot(MethodRequest methodRequest, object userContext)
        {
            // In a production device, you would trigger a reboot scheduled to start after this method returns.
            // For this sample, we simulate the reboot by writing to the console and updating the reported properties 
            RebootOrchestration();

            string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private async Task RebootOrchestration()
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Reboot order received.");
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Stoping processes...");

                _stopProcessing = true;


                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Processes stopped.");
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Rebooting...");

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

                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

                await Task.Delay(10000);

                // Update device twin with reboot time. 
                reboot["rebootStatus"] = "online";
                reportedProperties["iothubDM"] = reboot;

                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Reboot over and system runing again.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::ERROR::RebootOrchestration:{ex.Message}.");
            }
            finally
            {
                _stopProcessing = false;
            }
        }

        private Task<MethodResponse> StartOrStop(MethodRequest methodRequest, object userContext)
        {
            string logPrefix = "c2ddirectmethods".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::StartOrStop command has been received and planified.");

            if (methodRequest.Data == null)
                throw new ArgumentNullException("methodRequest.Data");

            JObject jData = JsonConvert.DeserializeObject<JObject>(methodRequest.DataAsJson);

            //Send feedback
            var response = new
            {
                result = $"Executed direct method:{methodRequest.Name}.",
                payload = methodRequest.DataAsJson
            };

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response, Formatting.Indented)), 200));
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

            _logger.LogWarning($"{logPrefix}::{_deviceSettings.ArtifactId}::WARNING::{methodRequest.Name} has been called but there is no registered specific method handler.");

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
            string logPrefix = "c2ddirectmethods.dtdlcommand.handler".BuildLogPrefix();

            var data = Encoding.UTF8.GetString(methodRequest.Data);

            _logger.LogDebug($"{logPrefix}::DTDL Command called: {methodRequest.Name} with payload {methodRequest.DataAsJson}.");

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
                _logger.LogDebug($"{logPrefix}::Sending connectivity data as reported property.");

                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity["type"] = "cellular";
                connectivity["signalPower"] = "low";
                reportedProperties["connectivity"] = connectivity;
                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::ERROR::ReportConnectivityAsync:{ex.Message}.");
            }
        }

        internal async Task GenericTwinReportedUpdateAsync(string deviceId, string sensorId, string propertyName, dynamic value)
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Sending generic reported property update:: {propertyName}-{value}.");

                //TODO: send DTDL readable properties               
                //https://docs.microsoft.com/en-us/azure/iot-central/core/concepts-telemetry-properties-commands

                TwinCollection reportedProperties;
                reportedProperties = new TwinCollection();
                reportedProperties[propertyName] = value;

                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::ERROR::GenericTwinReportedUpdateAsync:{ex.Message}.");
            }
        }

        internal async Task ReportFirwmareUpdateAsync(string firmwareVersion, string firmwareUrl)
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            try
            {
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::Sending firmware update notification.");

                TwinCollection reportedProperties;
                reportedProperties = new TwinCollection();

                reportedProperties["newFirmwareVersion"] = firmwareVersion;
                reportedProperties["Ur"] = firmwareUrl;
                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{logPrefix}::{_deviceSettings.ArtifactId}::ERROR::ReportFirwmareUpdateAsync:{ex.Message}.");
            }
        }

        internal async Task ReadTwins()
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            //Twins
            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS::Reading...");

            Twin twin = await _deviceClient.GetTwinAsync();

            if (twin != null)
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS:: {JsonConvert.SerializeObject(twin, Formatting.Indented)}.");
            else
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS:: No twins available.");
        }

        private async Task OnDesiredPropertyChange(TwinCollection desiredproperties, object usercontext)
        {
            string logPrefix = "c2dtwins".BuildLogPrefix();

            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED properties changes request notification.");

            if (desiredproperties != null)
            {                
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED::{JsonConvert.SerializeObject(desiredproperties, Formatting.Indented)}");

                //Check if the properties belong to the WritableProperties
                //https://docs.microsoft.com/en-us/azure/iot-central/core/concepts-telemetry-properties-commands
                
                var dtdlParsedMode = await DTDLHelper.GetAndParseDTDLAsync(_defaultModel.ModelId, _defaultModel.ModelPath);

                if (dtdlParsedMode != null && dtdlParsedMode.Any())
                {
                    //Writable properties
                    var writableProperties = dtdlParsedMode.Where(i => i.Value.EntityKind == DTEntityKind.Property && ((DTPropertyInfo)i.Value).Writable)?.Select(i=>i.Value as DTPropertyInfo).AsQueryable();
                    if (writableProperties != null && writableProperties.Any())
                    {
                        JObject objectDesiredProperties = JObject.Parse(desiredproperties.ToJson(Formatting.Indented));
                        var intersection = writableProperties.Join<DTPropertyInfo, JProperty, string, DTPropertyInfo>(objectDesiredProperties.Properties(), w => w.Name, o => o.Name, (w,o) => w);

                        if (intersection != null && intersection.Any())
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (var item in intersection)
                            {
                                builder.Append($"Desired property:'{item.Name}'-Value:{desiredproperties[item.Name]}.");
                            }

                            _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED INCLUDED IN DTDL-CHANGED: {builder.ToString()}");
                        }
                        else
                            _logger.LogWarning($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED::None of the properties sent by the solution is defined in the DTDL model.");

                    }
                }
            }
            else
                _logger.LogDebug($"{logPrefix}::{_deviceSettings.ArtifactId}::TWINS-PROPERTIES-DESIRED properties change is emtpy.");

        }
        #endregion
    }
}
