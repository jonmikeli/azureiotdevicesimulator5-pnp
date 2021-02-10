using System;

namespace IoT.Simulator.Settings
{

    public class SecuritySettings
    {
        public const string IOTHUB_DEVICE_SECURITY_TYPE_ENV_KEY = "IOTHUB_DEVICE_SECURITY_TYPE";

        //DEVICE
        public const string IOTHUB_CONNECTION_STRING_ENV_KEY = "IOTHUB_CONNECTION_STRING";
        public const string IOTHUB_DEVICE_ID_ENV_KEY = "IOTHUB_DEVICE_ID";

        //MODULE
        public const string IOTHUB_MODULE_CONNECTION_STRING_ENV_KEY = "IOTHUB_MODULE_CONNECTION_STRING";

        //DPS        
        public const string IOTHUB_DEVICE_DPS_ID_SCOPE_ENV_KEY = "IOTHUB_DEVICE_DPS_ID_SCOPE";
        public const string IOTHUB_DEVICE_DPS_DEVICE_ID_ENV_KEY = "IOTHUB_DEVICE_DPS_DEVICE_ID";
        public const string IOTHUB_DEVICE_DPS_DEVICE_KEY_ENV_KEY = "IOTHUB_DEVICE_DPS_DEVICE_KEY";
        public const string IOTHUB_DEVICE_DPS_ENDPOINT_ENV_KEY = "IOTHUB_DEVICE_DPS_ENDPOINT";
        public const string DPS_ENPOINT = "global.azure-devices-provisioning.net";

        //Other
        public bool IsModule { get; set; }        

        public string SecurityType { get; } = Environment.GetEnvironmentVariable(IOTHUB_DEVICE_SECURITY_TYPE_ENV_KEY);
        
        public string IoTHubConnectionString { get; } = Environment.GetEnvironmentVariable(IOTHUB_CONNECTION_STRING_ENV_KEY);
        public string DeviceId { get; } = Environment.GetEnvironmentVariable(IOTHUB_DEVICE_ID_ENV_KEY);
        public string ModuleConnectionString { get; } = Environment.GetEnvironmentVariable(IOTHUB_MODULE_CONNECTION_STRING_ENV_KEY);

        public string DeviceDpsIdScope { get; } = Environment.GetEnvironmentVariable(IOTHUB_DEVICE_DPS_ID_SCOPE_ENV_KEY);
        public string DeviceDpsDeviceId { get; } = Environment.GetEnvironmentVariable(IOTHUB_DEVICE_DPS_DEVICE_ID_ENV_KEY);
        public string DeviceDpsDeviceKey { get; } = Environment.GetEnvironmentVariable(IOTHUB_DEVICE_DPS_DEVICE_KEY_ENV_KEY);
        public string DeviceDpsEndpoint { get; } = DPS_ENPOINT;
    }
}
