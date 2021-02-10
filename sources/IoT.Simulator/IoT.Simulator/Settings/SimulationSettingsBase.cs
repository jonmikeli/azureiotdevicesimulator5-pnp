﻿using Newtonsoft.Json;

namespace IoT.Simulator.Settings
{
    public class SimulationSettingsBase
    {
        [JsonProperty("enableTelemetryMessages")]
        public bool EnableTelemetryMessages { get; set; }
        [JsonProperty("telemetryFrecuency")]
        public int TelemetryFrecuency { get; set; }

        [JsonProperty("enableTwinReportedMessages")]
        public bool EnableTwinReportedMessages { get; set; }

        [JsonProperty("twinReportedMessagesFrecuency")]
        public int TwinReportedMessagesFrecuency { get; set; }

        [JsonProperty("enableReadingTwinProperties")]
        public bool EnableReadingTwinProperties { get; set; }

        [JsonProperty("enableC2DDirectMethods")]
        public bool EnableC2DDirectMethods { get; set; }

        [JsonProperty("enableC2DMessages")]
        public bool EnableC2DMessages { get; set; }

        [JsonProperty("enableTwinPropertiesDesiredChangesNotifications")]
        public bool EnableTwinPropertiesDesiredChangesNotifications { get; set; }
    }
}
