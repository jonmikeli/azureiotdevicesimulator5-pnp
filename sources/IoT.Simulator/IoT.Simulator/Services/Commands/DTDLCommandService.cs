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
    public class DTDLCommandService : IDTDLCommandService
    {
        private ILogger _logger;

        public DTDLCommandService(ILoggerFactory loggerFactory)
        {            
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<DTDLCommandService>();
        }

        public async Task<JArray> GetCommandsAsync(string modelId, string modelPath)
        {
            string logPrefix = "DTDLCommandService.GetCommandsAsync".BuildLogPrefix();

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, modelPath);

            if (modelContainer == null)
                throw new Exception($"No model container has been found corresponding to the parameters provided:: modelId: {modelId} - modelPath: {modelPath}");

            var modelContent = modelContainer.SingleOrDefault(i => i.Key == modelId);
            if (modelContent.Equals(default(KeyValuePair<string, DTDLContainer>)))
                throw new Exception($"No model corresponding to the modelId {modelId} has been found.");

            string messageString = string.Empty;
            if (
                modelContent.Value != null
                && modelContent.Value.DTDL != null)
            {
                _logger.LogDebug($"Commands found in the provided model::modelId: {modelId} - modelPath: {modelPath}");
                messageString = JsonConvert.SerializeObject(modelContent.Value.DTDL, Formatting.Indented);
            }
            else
                _logger.LogDebug($"No commands have been found  the provided model::modelId: {modelId} - modelPath: {modelPath}");

            return messageString;
        }
    }
}
