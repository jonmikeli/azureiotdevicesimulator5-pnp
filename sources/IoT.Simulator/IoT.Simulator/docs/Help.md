# Azure IoT Device Simulator - Help

This section describes the different artifacts of the solution and how they work.

## Application
The application consist of:
 - an application console (.NET 5)
 - configuration files:
   - [appsettings.json](####appsettings.json) (described below)
   - [devicesettings.json](####devicesettings.json) (described below). It includes the references to the used DTDL models.
   - [modulessettings.json](####modulessettings.json) (described below). It includes the references to the used DTDL models. (IoT PnP at module level has not been implemented yet in the simulator).


<br/>

*Global device model architecture*


![Global device model architecture](images/GlobalDiagrams.png)

<br/>

## Features
### Device
#### D2C
##### Messages
The regular version of the simulator proposes a fully and open customizable process to create messages. The examples provided included messages such as:
1. `Commissioning` messages
2. `Measured` data messages (aka telemetry)
3. `Error` messages (functional errors sent by devices)

This version of the simulator has another approach: the messages should be build according to the definition included in the referenced model.


##### Twins
The device sends updated Reported Properties (Twins) after many operations/commands.

> [!NOTE]
> 
> Example: after an OnOff Direct Method request, the device sends its status to the cloud solution (Microsoft Azure IoT Hub) using the Twin Reported Properties.

#### C2D
##### Direct Methods

The simulator includes a default set of commands that can be used as Direct Methods.

|Method name|Description|Request|Response|Comments|
|:-|:-|:-|:-|:-|
| SendLatencyTest | Allows to start a [latency](LatencyTests.md) test between a given device and the Microsoft Azure IoT Hub where the device is registered. | ```{ "deviceId":"", "messageType":"latency", "startTimestamp":12345} ```| NA |The request contains the initial  timpestamp, which is sent back to the device after all the process in order to allow him to measure latency. <br>***NOTE: this feature requires an [Azure Function](https://github.com/jonmikeli/azureiotdevicesimulator/tree/master/sources/IoT.Simulator/IoT.Simulator.AF) responding to the latency test requests and calling back the C2D LatencyTestCallBack Direct Method.***|
|LatencyTestCallBack|Allows to end a [latency](LatencyTests.md) test between a given device and the Microsoft Azure IoT Hub where the device is registered. |```startTimestamp``` value, allowing to math the [latency](LatencyTests.md) (string)|<ul><li>message notifying that the LatencyTestCallBack Direct Method has been called (string).</li><li> result code, 200</li></ul>|NA|
| Reboot | Simulates a device reboot operation. | NA | <ul><li>message notifiying that the Reboot Direct Method has been called (string).</li><li> result code, 200</li></ul>|Sends Twins (Reported properties) notifying the reboot.|
| OnOff | Turns a given device on/off. | JSON Object | <ul><li>message notifying that the OnOff Direct Method has been called (string). The message contains request's payload.</li><li> result code, 200</li></ul>|
| ReadTwins | Orders a given device to read its Twin data. | NA | <ul><li>message notifying that the ReadTwins Direct Method has been called (string).</li><li>result code, 200</li></ul>|
| GenericJToken | Generic method | JSON Token | <ul><li>message notifying that the GenericJToken Direct Method has been called (string).</li><li> result code, 200</li></ul>|
| Generic | Generic method | string | <ul><li>message notifying that the Generic Direct Method has been called (string).</li><li> result code, 200</li></ul>|
| SetTelemetryInterval | Updates the time rate used to send telemetry data. | seconds (int) | <ul><li>message notifying that the SetTelemetryInterval Direct Method has been called (string).</li><li> result code, 200</li></ul>|

Similarly to the messages, this IoT Plug and Play simulator adds commands that whould be defined in the DTDL referenced model.

> NOTE
>
> The simulator should should only use the commands defined in the DTDL model. This said, it can be interesting to keep the other set of commands for test purposes.
> Also, this illustrates that the DTDL may be combined with elements not defined in the models. However, be aware that all the elements not declared in the models will be unknown to the IoT Solutions that use the models to integrate your devices.

##### Messages
The device can be configured to receive generic **messages** coming from the cloud (Microsoft Azure IoT Hub C2D Messages).

##### Twins
###### Desired
Any change in a **Desired Property** (device level) is notified to the device and it can be handled.


> ![NOTE]
> 
> ###### Tags and Microsoft IoT Hub Jobs
>
> The simulator can benefit from the use of **Microsoft IoT Hub Jobs** for operations based in property queries.
> A typical example of this would be a **FOTA** (Firmware Over The Air) update according to criteria based in **Twin.Tag properties* (ex: firmwareVersion, location, environment, etc).


### Modules

#### M2C
##### Messages
The approach at modules level is exactly the same that the approach at the device level.

##### Twins
Modules send updated **Reported Properties (Twins)** after many operations/commands.

> [!NOTE]
> 
> Example: after a OnOff Direct Method request, a givne module sends its status to the cloud solution (Microsoft Azure IoT Hub) using the Twin Reported Properties.

#### C2M
##### Direct Methods

The simulator includes a default set of commands that can be used as Direct Methods.

|Method name |Description|Request|Response|Comments|
|:-|:-|:-|:-|:-|
| Reboot | Simulates a device reboot operation. | NA | <ul><li>message notifying that the Reboot Direct Method has been called (string).</li><li> result code, 200</li></ul>|Sends Twins (Reported properties) notifying the reboot.|
| OnOff | Turns a given device on/off. | JSON Object | <ul><li>message notifying that the OnOff Direct Method has been called (string). The message contains request's payload.</li><li> result code, 200</li></ul>|
| ReadTwins | Orders a given device to read its Twin data. | NA | <ul><li>message notifying that the ReadTwins Direct Method has been called (string).</li><li>result code, 200.</li></ul>|
| GenericJToken | Generic method | JSON Token | <ul><li>message notifying that the GenericJToken Direct Method has been called (string).</li><li> result code, 200</li></ul>|
| Generic | Generic method | string | <ul><li>message notifying that the Generic Direct Method has been called (string).</li><li> result code, 200</li></ul>|
| SetTelemetryInterval | Updates the time rate used to send telemetry data. | seconds (int) | <ul><li>message notifying that the SetTelemetryInterval Direct Method has been called (string).</li><li> result code, 200</li></ul>|

The approach at modules level is exactly the same that the approach at the device level.

##### Messages
Each module can be configured to receive generic **messages** coming from the cloud (Microsoft Azure IoT Hub).

##### Twins
###### Desired
Any change in a **Desired property** (***module level***) is notified to the module and it can be handled.



## How does the simulator work?
### Description
The application is configurable by an ***appsettings.json*** file.

The features of the application rely on two main components:
 - device (**one single device per application**)
 - modules (**none, one or many modules per device**)
 
 The device component is configured by a ***devicesettings.json*** file while the modules are configured by a ***modulessettings.json*** file.

<br/>

 *Device model architecture*
 ![Device model architecture](images/GlobalDiagrams.png)

<br/>

### Runing the simulator
 The simulator is a .NET Core application.
 
 To run the simulator, there are two alternatives:
  1. running the simulator as a **.NET Core application** (Console Application)
  1. running the *Docker container* (which contains in turn the .NET Core binaries, packages and other required prerequisites)
  
 > ![NOTE]
 > 
 > See [Docker](https://www.docker.com) and container oriented development for those who are not familiar with.
 
 Whatever the alternative will be, check that the **3 configuration** files are set properly.

 > ![IMPORTANT]
 > 
 > The 3 configurations files have be present and contain the proper Microsoft Azure IoT Hub connection strings, IDs or keys.

 
 #### Runing .NET Core application
 Run the command below:
 ```dotnet
 dotnet IoT.Simulator.dll
 ```

 #### Runing Docker container
 ```cmd
 docker run -ti --name [containername] [imagename]
 ```

 You can get ready to use Docker images of the Azure IoT Device Simulator [here](https://hub.docker.com/r/jonmikeli/azureiotdevicesimulator).


 ### Configurations
 #### Application
 Technical settings of the application can be configured at *appsettings.json*.

 > Example (Production environment):
 ```json
 {
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
 ```


 > [!NOTE]
 >
 > The solution contains different settings depending on the environment (similar to transformation files).


 > Example (Development environment) - *appsettings.Development.json*:
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


#### Device
IoT Simulator is linked to **one and only one** device.
The device behavior is configured by the *devicessettings.json* configuration file.

> Example:

```json
{
  "connectionString": "HostName=[IOTHUB NAME].azure-devices.net;DeviceId=[DEVICE ID];SharedAccessKey=[KEY]",
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
    "latencyTestsFrecuency": 10,
    "enableDevice": true,
    "enableModules": true,
    "enableTelemetryMessages": false,
    "telemetryFrecuency": 60,
    "enableErrorMessages": false,
    "errorFrecuency": 60,
    "enableCommissioningMessages": false,
    "commissioningFrecuency": 60,
    "enableTwinReportedMessages": false,
    "twinReportedMessagesFrecuency": 60,
    "enableReadingTwinProperties": false,
    "enableC2DDirectMethods": true,
    "enableC2DMessages": true,
    "enableTwinPropertiesDesiredChangesNotifications": true
  }
}
```
Properties are quite self-explanatory.

> [!NOTE]
> 
> Emission intervals are set in seconds.


##### IoT Plug and Play related settings

A few settings are related to IoT Pnp:

```json
...
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
  ]
...
```
These properties ARE NOT PART of IoT PnP or DTDL. They are custom properties created for the simulator.

Descriptions:
- defaultModelId: it contains the default DTDL model Id. It is mandatory in this version of the simulator.
- supportedModels: a collection of the supported models by the simulator. This notion of "collection" does not exist in DTDL v2. However, many real life projects may need this. In order to make the simulator (and your IoT device) compatible with a IoT PnP approach, a collection of "usable" models by the device has been created.
- modelId: DTDL model Id (in DTDL expected format). Refer to the documentation [here](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md) for details about the format.
- modelPath: path of the JSON containing the DTDL model.
- modelType: describes the model type. This notion does not exist either in DTDL. Having decide to support many models in the simulator, this property allows to type each of them (ex: Telemetry, Error, Warning).

> NOTE
>
> At least one supported model is required.
> The default model id has to be one of the values included in the supported models.


#### Modules
IoT Simulator's device can contain **zero, one or more modules but no module is mandatory**.
Behaviors of modules are configured by the *modulessettings.json* configuration file.


> Example of a configuration file of two modules:
```json
{
 "modules":[
    {
      "connectionString": "HostName=[IOTHUB NAME].azure-devices.net;DeviceId=[DEVICE ID];ModuleId=[MODULE ID];SharedAccessKey=[KEY]",
      "simulationSettings": {
        "enableLatencyTests": false,
        "latencyTestsFrecuency": 10,
        "enableTelemetryMessages": false,
        "telemetryFrecuency": 60,
        "enableErrorMessages": false,
        "errorFrecuency": 60,
        "enableCommissioningMessages": false,
        "commissioningFrecuency": 60,
        "enableTwinReportedMessages": false,
        "twinReportedMessagesFrecuency": 60,
        "enableReadingTwinProperties": false,
        "enableC2DDirectMethods": true,
        "enableC2DMessages": true,
        "enableTwinPropertiesDesiredChangesNotifications": true
      }
    },
    {
      "connectionString": "HostName=[IOTHUB NAME].azure-devices.net;DeviceId=[DEVICE ID];ModuleId=[MODULE ID];SharedAccessKey=[KEY]",
      "simulationSettings": {
        "enableLatencyTests": false,
        "latencyTestsFrecuency": 10,
        "enableTelemetryMessages": false,
        "telemetryFrecuency": 60,
        "enableErrorMessages": false,
        "errorFrecuency": 60,
        "enableCommissioningMessages": false,
        "commissioningFrecuency": 60,
        "enableTwinReportedMessages": false,
        "twinReportedMessagesFrecuency": 60,
        "enableReadingTwinProperties": false,
        "enableC2DDirectMethods": true,
        "enableC2DMessages": true,
        "enableTwinPropertiesDesiredChangesNotifications": true
      }
    }
  ]
}
```

> [!NOTE]
> 
> Emission intervals are set in seconds.

The IoT PnP integration with modules has not been implemented yet. However, the approach will be exactly the same than with devices.

### Configuration files reminder
#### appsettings.json
 ```json
 {
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
 ```

#### devicesettings.json

```json
{
  "connectionString": "HostName=[IOTHUB NAME].azure-devices.net;DeviceId=[DEVICE ID];SharedAccessKey=[KEY]",
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
    "latencyTestsFrecuency": 10,
    "enableDevice": true,
    "enableModules": true,
    "enableTelemetryMessages": false,
    "telemetryFrecuency": 60,
    "enableErrorMessages": false,
    "errorFrecuency": 60,
    "enableCommissioningMessages": false,
    "commissioningFrecuency": 60,
    "enableTwinReportedMessages": false,
    "twinReportedMessagesFrecuency": 60,
    "enableReadingTwinProperties": false,
    "enableC2DDirectMethods": true,
    "enableC2DMessages": true,
    "enableTwinPropertiesDesiredChangesNotifications": true
  }
}
```
Properties are quite self-explanatory.

> [!NOTE]
> 
> Emission intervals are set in seconds.

#### modulessettings.json
```json
{
 "modules":[
    {
      "connectionString": "HostName=[IOTHUB NAME].azure-devices.net;DeviceId=[DEVICE ID];ModuleId=[MODULE ID];SharedAccessKey=[KEY]",
      "simulationSettings": {
        "enableLatencyTests": false,
        "latencyTestsFrecuency": 10,
        "enableTelemetryMessages": false,
        "telemetryFrecuency": 60,
        "enableErrorMessages": false,
        "errorFrecuency": 60,
        "enableCommissioningMessages": false,
        "commissioningFrecuency": 60,
        "enableTwinReportedMessages": false,
        "twinReportedMessagesFrecuency": 60,
        "enableReadingTwinProperties": false,
        "enableC2DDirectMethods": true,
        "enableC2DMessages": true,
        "enableTwinPropertiesDesiredChangesNotifications": true
      }
    },
    {
      "connectionString": "HostName=[IOTHUB NAME].azure-devices.net;DeviceId=[DEVICE ID];ModuleId=[MODULE ID];SharedAccessKey=[KEY]",
      "simulationSettings": {
        "enableLatencyTests": false,
        "latencyTestsFrecuency": 10,
        "enableTelemetryMessages": false,
        "telemetryFrecuency": 60,
        "enableErrorMessages": false,
        "errorFrecuency": 60,
        "enableCommissioningMessages": false,
        "commissioningFrecuency": 60,
        "enableTwinReportedMessages": false,
        "twinReportedMessagesFrecuency": 60,
        "enableReadingTwinProperties": false,
        "enableC2DDirectMethods": true,
        "enableC2DMessages": true,
        "enableTwinPropertiesDesiredChangesNotifications": true
      }
    }
  ]
}
```

> [!NOTE]
> 
> Emission intervals are set in seconds.

## Evolutivity

This version of the IoT PnP simulator does not process propertly the `component` concept of DTDL. This would be a good point to work on and totally cover DTDL features.

Additionnally, I guess parts of the code may be reviewed or improved.

A `DTDLHelper` has been created to factorize and ease working with DTDL models. It will probably be worth to make it stronger and publish it independently.


# Glossary
A few word explanations to be sure they are understood in the context of this document.

## Commissioning

Commissioning represents the act of linking a provisioned device and a user (or user related information).

## Provisioning

Provisioning represents the action of creating an identity for the device in the Microsoft Azure IoT Hub.

## Azure IoT related vocabulary

## Twin
[Device twins](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-device-twins) store device state information including metadata, configurations, and conditions. Microsoft Azure IoT Hub maintains a device twin for each device that you connect to IoT Hub.

Similarly, [module twins](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-module-twins) play the same role thant device twins but at module level.

Twins contain 3 main sections:
 - Tags
 - Properties (Desired)
 - Properties (Reported)

For more details, follow the links provided.

## Tags
A section of the JSON document that the solution back end can read from and write to. Tags are not visible to device apps.

## Twin Desired properties
Used along with reported properties to synchronize device configuration or conditions. The solution back end can set desired properties, and the device app can read them. The device app can also receive notifications of changes in the desired properties.

## Twin Reported properties
Used along with desired properties to synchronize device configuration or conditions. The device app can set reported properties, and the solution back end can read and query them.

