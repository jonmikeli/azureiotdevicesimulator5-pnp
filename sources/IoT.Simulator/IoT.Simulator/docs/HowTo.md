# Azure IoT Device Simulator - How To


## How to use the Azure IoT Device Simulator?

The IoT simulator can easily be containerized in order to simplify its delivery and use.
Also, .NET 5 includes interesting capabilities to build lighter and/or selfsufficient applications (ex: selfcontained or trim build options).

If you need detailed documentation about what Azure IoT Device Simulator is, you can find additional information at:
 - [Readme](../../../../Readme.md)
 - [Help](Help.md)


## How to get a Docker image?
### Prerequisites
In order to use a Docker container, you need to check [Docker](https://www.docker.com/) prerequisites.

Do not forget you will need an internet connection with specific open ports:
 - 8883
 - 5671
 - 443
[Ports](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-protocols) required to communicate with Microsoft Azure IoT Hub.

The new .NET 5 docker images are smaller and offer better performances.


### Steps to follow
The Azure IoT Device Simulator needs two basic prerequisites before starting:
 - settings (do not forget to update the connection string)
 - the referenced DTDL models have to be reachable (from the device as well as from the cloud solution)

 Once these items are ready, a single command allows starting the application.

#### Settings
Settings are based on 3 files:
 - [appsettings.json](#appsettings.json)
 - [devicesettings.json](#devicesettings.json)
 - [modulessettings.json](#modulessettings.json)

For details and explanations, see [help](Help.md).

> [!TIP]
> 
> The solution takes into account **settings** depending on the environment.
> It can be set trough the environment variable ENVIRONMENT.
> The solution looks for settings files following the pattern *file.ENVIRONMENT.json* (similar to the former transformation files).
> Default setting files will be loaded first in case no environment file is found.

##### appsettings.json
This file allows configuring system related items (logs, etc).

**Release**

Minimal logs settings.
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

**Development (appsettings.Development.json)**

Detailed logs settings.
```json
{
  "Logging": {
    "Debug": {
      "LogLevel": {
        "Default": "Trace"
      }
    },
    "Console": {
      "IncludeScopes": true,
      "LogLevel": {
        "Default": "Trace"
      }
    },
    "LogLevel": {
      "Default": "Trace",
      "System": "Trace",
      "Microsoft": "Trace"
    }
  }
}
```

##### devicesettings.json
This file allows configuring device simulation settings.

```json
{
  "connectionString": "[IOT HUB NAME].azure-devices.net;DeviceId=[DEVIVE ID];SharedAccessKey=[SHARED KEY]",
  "defaultModelId": "dtmi:com:example:thermostat;1",
  "supportedModels": [
    {
      "modelId": "dtmi:com:example:thermostat;1",
      "modelPath": "[HTTP path or local physical path to the model definition]",
      "modelType": "Telemetry" //Telemetry, Error, Warning
    },
    {
      "modelId": "dtmi:com:jmi:simulator:devicemessages;1",
      "modelPath": "[HTTP path or local physical path to the model definition]",
      "modelType": "Telemetry" //Telemetry, Error, Warning
    }
  ],
  "simulationSettings": {
    "enableLatencyTests": false,
    "latencyTestsFrecuency": 30,
    "enableDevice": true,
    "enableModules": false,
    "enableTelemetryMessages": true,
    "telemetryFrecuency": 10,
    "enableErrorMessages": false,
    "errorFrecuency": 20,
    "enableCommissioningMessages": false,
    "commissioningFrecuency": 30,
    "enableTwinReportedMessages": false,
    "twinReportedMessagesFrecuency": 60,
    "enableReadingTwinProperties": false,
    "enableC2DDirectMethods": true,
    "enableC2DMessages": true,
    "enableTwinPropertiesDesiredChangesNotifications": true
  }
}

```

##### modulessettings.json
This file allows configuring module(s) simulation settings.

```json
{
 "modules":[
    {
      "connectionString": "[IOT HUB NAME].azure-devices.net;DeviceId=[DEVIVE ID];ModuleId=[MODULE ID];SharedAccessKey=[SHARED KEY]",
      "simulationSettings": {
        "enableLatencyTests": false,
        "latencyTestsFrecuency": 10,
        "enableTelemetryMessages": true,
        "telemetryFrecuency": 20,
        "enableErrorMessages": false,
        "errorFrecuency": 30,
        "enableCommissioningMessages": false,
        "commissioningFrecuency": 60,
        "enableTwinReportedMessages": false,
        "twinReportedMessagesFrecuency": 60,
        "enableReadingTwinProperties": true,
        "enableC2DDirectMethods": true,
        "enableC2DMessages": true,
        "enableTwinPropertiesDesiredChangesNotifications": true
      }
    }
  ]
}

```


> [!IMPORTANT]
>
> Do not forget to set your own values for `connectionString`. 


#### Commands
**Regular**
```cmd
dotnet IoT.Simulator.dll
```

**Changing the environment**

Linux
```cmd
export ENVIRONMENT=Development
dotnet IoT.Simulator.dll
```

Windows
```cmd
set ENVIRONMENT=Development
dotnet IoT.Simulator.dll
```
