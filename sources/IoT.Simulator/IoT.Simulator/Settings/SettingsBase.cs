using IoT.Simulator.Extensions;
using IoT.Simulator.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace IoT.Simulator.Settings
{
    public class SettingsBase
    {

        [JsonProperty("connectionString")]
        public string ConnectionString
        { get; set; }

        public string DeviceId
        {
            get
            {
                if (!string.IsNullOrEmpty(ConnectionString))
                    return ConnectionString.ExtractValue("DeviceId");
                else
                    return string.Empty;
            }
        }

        public string HostName
        {
            get
            {
                if (!string.IsNullOrEmpty(ConnectionString))
                    return ConnectionString.ExtractValue("HostName");
                else
                    return string.Empty;
            }
        }

        [JsonProperty("defaultModelId")]
        public string DefaultModelId
        {
            get; set;
        }

        [JsonProperty("supportedModels")]
        public IList<DTDLModelItem> SupportedModels { get; set; }
    }
}
