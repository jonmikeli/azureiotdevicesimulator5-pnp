using IoT.Simulator.Extensions;
using IoT.Simulator.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoT.Simulator.Services
{
    //https://dejanstojanovic.net/aspnet/2018/december/registering-multiple-implementations-of-the-same-interface-in-aspnet-core/
    public class DTDLTelemetryMessageService : ITelemetryMessageService
    {
        private ILogger _logger;
        private string  _modelId;

        public DTDLTelemetryMessageService(ILoggerFactory loggerFactory, string modelId)
        {            
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            if (string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            _logger = loggerFactory.CreateLogger<DTDLTelemetryMessageService>();
            _modelId = modelId;
        }

        public async Task<string> GetMessageAsync()
        {
            JArray jMessageBody = await DTDLHelper.BuildMessageBodyFromModelId(_modelId, null);

            if (jMessageBody == null)
                throw new Exception("No message body has been build according to the model.");

            string messageString = JsonConvert.SerializeObject(jMessageBody, Formatting.Indented);

            if (string.IsNullOrEmpty(messageString))
                throw new ArgumentNullException(nameof(messageString), "DATA: The message to send is empty or not found.");

            return messageString;
        }

        public async Task<string> GetMessageAsync(string deviceId, string moduleId)
        {
            string artifactId = string.IsNullOrEmpty(moduleId) ? deviceId : moduleId;

            string logPrefix = "DTDLTelemetryMessageService".BuildLogPrefix();
            string messageString = await GetMessageAsync();

            if (string.IsNullOrEmpty(messageString))
                throw new ArgumentNullException(nameof(messageString), "DATA: The message to send is empty or not found.");

            _logger.LogTrace($"{logPrefix}::{artifactId}::Message body according to a given model has been loaded.");

            messageString = IoTTools.UpdateIds(messageString, deviceId, moduleId);
            _logger.LogTrace($"{logPrefix}::{artifactId}::DeviceId and moduleId updated in the message template.");

            return messageString;
        }

        public async Task<string> GetRandomizedMessageAsync(string deviceId, string moduleId)
        {
            string artifactId = string.IsNullOrEmpty(moduleId) ? deviceId : moduleId;

            string messageString = await this.GetMessageAsync(deviceId, moduleId);
            string logPrefix = "DTDLTelemetryMessageService".BuildLogPrefix();

            //Randomize data           
            messageString = IoTTools.RandomizeData(messageString);
            _logger.LogTrace($"{logPrefix}::{artifactId}::Randomized data to update template's values before sending the message.");

            return messageString;
        }
    }
}
