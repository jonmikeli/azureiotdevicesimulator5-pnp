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

        public async Task<Dictionary<string, DTDLCommandContainer>> GetCommandsAsync(string modelId, string modelPath)
        {
            string logPrefix = "DTDLCommandService.GetCommandsAsync".BuildLogPrefix();

            return await DTDLHelper.GetModelsAndExtratCommandsAsync(modelId, modelPath);
        }
    }
}
