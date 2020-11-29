﻿using IoT.Simulator.Extensions;
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
        private string  _modelId;
        private string _modelPath;

        public DTDLMessageService(ILoggerFactory loggerFactory, string modelId, string modelPath)
        {            
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            if (string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            _logger = loggerFactory.CreateLogger<DTDLMessageService>();
            _modelId = modelId;
            _modelPath = modelPath;
        }

        public async Task<string> GetMessageAsync()
        {

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(_modelId, _modelPath);

            if (modelContainer == null)
                throw new Exception($"No model container has been found corresponding to the parameters provided:: modelId: {_modelId} - modelPath: {_modelPath}");

            var modelContent = modelContainer.SingleOrDefault(i => i.Key == _modelId);
            if (modelContent.Equals(default(KeyValuePair<string, DTDLContainer>)))
                throw new Exception($"No model corresponding to the modelId {_modelId} has been found.");

            string messageString = string.Empty;
            if (
                modelContent.Value != null
                && modelContent.Value.DTDLGeneratedData != null
                && modelContent.Value.DTDLGeneratedData.Telemetries != null)
                messageString = JsonConvert.SerializeObject(modelContent.Value.DTDLGeneratedData.Telemetries, Formatting.Indented);
            else
                throw new ArgumentException($"No telemetry has been built from the provided model::modelId: {_modelId} - modelPath: {_modelPath}");

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
