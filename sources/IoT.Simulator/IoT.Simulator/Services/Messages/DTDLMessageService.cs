using IoT.Simulator.Extensions;
using IoT.Simulator.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IoT.DTDL;
using System.Linq;
using System.Collections.Generic;

namespace IoT.Simulator.Services
{
    //https://dejanstojanovic.net/aspnet/2018/december/registering-multiple-implementations-of-the-same-interface-in-aspnet-core/
    public class DTDLMessageService : IDTDLMessageService
    {
        private ILogger _logger;

        public DTDLMessageService(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<DTDLMessageService>();
        }

        public async Task<string> GetMessageAsync(string modelId, string modelPath)
        {
            string messageString = string.Empty;

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, modelPath);

            if (modelContainer == null)
                throw new Exception($"No model container has been found corresponding to the parameters provided:: modelId: {modelId} - modelPath: {modelPath}");

            //var modelContent = modelContainer.SingleOrDefault(i => i.Key == modelId);
            var contentWithTelemetries = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);

            if (contentWithTelemetries != null && contentWithTelemetries.Any())
            {
                JArray telemetries = new JArray();
                foreach (var item in contentWithTelemetries)
                {
                    foreach (var telemetry in item.Value.DTDLGeneratedData.Telemetries)
                    {
                        telemetries.Add(telemetry);
                    }
                }

                messageString = JsonConvert.SerializeObject(telemetries, Formatting.Indented);
            }
            else
                throw new ArgumentException($"No telemetry has been built from the provided model::modelId: {modelId} - modelPath: {modelPath}");

            return messageString;
        }

        public async Task<string> GetMessageAsync(string deviceId, string moduleId, string modelId, string modelPath)
        {
            string artifactId = string.IsNullOrEmpty(moduleId) ? deviceId : moduleId;

            string logPrefix = "DTDLTelemetryMessageService".BuildLogPrefix();
            string messageString = await GetMessageAsync(modelId, modelPath);

            if (string.IsNullOrEmpty(messageString))
                throw new ArgumentNullException(nameof(messageString), "DATA: The message to send is empty or not found.");

            _logger.LogTrace($"{logPrefix}::{artifactId}::Message body according to a given model has been loaded.");

            //messageString = IoTTools.UpdateIds(messageString, deviceId, moduleId);
            //_logger.LogTrace($"{logPrefix}::{artifactId}::DeviceId and moduleId updated in the message template.");

            return messageString;
        }

        public async Task<string> GetRandomizedMessageAsync(string deviceId, string moduleId, string modelId, string modelPath)
        {
            string artifactId = string.IsNullOrEmpty(moduleId) ? deviceId : moduleId;

            string messageString = await this.GetMessageAsync(deviceId, moduleId, modelId, modelPath);
            string logPrefix = "DTDLTelemetryMessageService".BuildLogPrefix();

            //Randomize data           
            //messageString = IoTTools.RandomizeData(messageString);
            //_logger.LogTrace($"{logPrefix}::{artifactId}::Randomized data to update template's values before sending the message.");

            return messageString;
        }
    }
}
