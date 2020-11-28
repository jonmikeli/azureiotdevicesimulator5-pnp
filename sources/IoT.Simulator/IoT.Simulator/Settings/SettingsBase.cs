using IoT.Simulator.Extensions;
using IoT.Simulator.Models;

using Newtonsoft.Json;

using System;

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

        [JsonProperty("modelId")]
        public string DefaultModelId
        {
            get
            {
                return DTDLSettings?.DefaultModelId;
            }
            set
            {
                if (DTDLSettings == null)
                    DTDLSettings = new DTDLSettings(value, DTDLModelType.Telemetry);
                else
                    DTDLSettings.DefaultModelId = value;                
            }
        }

        public DTDLSettings DTDLSettings { get; set; }
    }
}
