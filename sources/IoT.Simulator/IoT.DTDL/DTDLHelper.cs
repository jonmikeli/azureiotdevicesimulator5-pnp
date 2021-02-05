﻿using Microsoft.Azure.DigitalTwins.Parser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IoT.DTDL
{
    //https://docs.microsoft.com/en-us/azure/iot-pnp/concepts-model-parser
    //TYPES: https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md
    public class DTDLHelper
    {
        #region Public method(s)
        public static async Task<Dictionary<string, DTDLContainer>> GetModelsAndBuildDynamicContentAsync(string modelId, string modelPath)
        {
            //Get the full DTDL model
            JToken dtdlModel = await GetDTDLFromModelIdAsync(modelId, modelPath);

            if (dtdlModel == null)
                throw new Exception($"No DTDL model with the id {modelId} has been provided at the provided locations.");

            JArray jArrayDTDLModel = null;
            if (dtdlModel is JObject)
            {
                jArrayDTDLModel = new JArray();
                jArrayDTDLModel.Add(dtdlModel);
            }
            else if (dtdlModel is JArray)
                jArrayDTDLModel = dtdlModel as JArray;

            //TODO: store in the cache the processed data (all the models are processed in a row in the method)
            Dictionary<string, DTDLContainer> data = await ParseDTDLAndBuildDynamicContentAsync(jArrayDTDLModel);

            if (data != null && data.Any() && data.ContainsKey(modelId))
            {
                //Filter by only the required models (initial model and dependencies)
                var entryModel = data.Single(i => i.Key == modelId);

                var referencedModelsIds = ((JObject)(entryModel.Value.DTDL)).Value<JArray>("contents").Where(i => i.Value<string>("@type").ToLower() == "component").Select(i => i.Value<string>("schema")).ToList(); ;
                referencedModelsIds.Add(modelId);

                var entreModelAndReferences = data.Join(referencedModelsIds, a=>a.Value.ModelId, r=>r, (a, r) => a);

                if (entreModelAndReferences != null && entreModelAndReferences.Any())
                    data = new Dictionary<string, DTDLContainer>(entreModelAndReferences);

                return data;
            }
            else
                return null;
        }

        public static async Task<Dictionary<string, DTDLCommandContainer>> GetModelsAndExtratCommandsAsync(string modelId, string modelPath)
        {
            //Get the full DTDL model
            JToken dtdlModel = await GetDTDLFromModelIdAsync(modelId, modelPath);

            if (dtdlModel == null)
                throw new Exception($"No DTDL model with the id {modelId} has been provided at the provided locations.");

            JArray jArrayDTDLModel = null;
            if (dtdlModel is JObject)
            {
                jArrayDTDLModel = new JArray();
                jArrayDTDLModel.Add(dtdlModel);
            }
            else if (dtdlModel is JArray)
                jArrayDTDLModel = dtdlModel as JArray;

            //Build the JSON Message corresponding to the model
            return await ParseDTDLAndGetCommandsAsync(jArrayDTDLModel);
        }

        public static async Task<JToken> GetDTDLFromModelIdAsync(string modelId, string modelPath)
        {
            if (string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            if (string.IsNullOrEmpty(modelPath))
                throw new ArgumentNullException(nameof(modelPath));

            JToken result = null;

            if (modelPath.StartsWith("http"))
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(modelPath);
                    if (response != null)
                    {
                        response.EnsureSuccessStatusCode();
                        var data = await response.Content.ReadAsStringAsync();

                        if (data != null)
                            result = JToken.Parse(data);
                    }
                }
            }
            else
                result = JToken.Parse(File.ReadAllText(modelPath));

            return result;
        }

        //https://docs.microsoft.com/en-us/azure/iot-pnp/concepts-model-parser
        public static async Task<Dictionary<string, DTDLContainer>> ParseDTDLAndBuildDynamicContentAsync(JArray dtdlArray)
        {
            if (dtdlArray == null)
                throw new ArgumentNullException(nameof(dtdlArray));

            Dictionary<string, DTDLContainer> globalResult = new Dictionary<string, DTDLContainer>();
            DTDLContainer itemResult = null;

            ModelParser parser = new ModelParser();
            
            IReadOnlyDictionary<Dtmi, DTEntityInfo> parseResult = await parser.ParseAsync(dtdlArray.Select(i => JsonConvert.SerializeObject(i)));            

            foreach (JObject dtdl in dtdlArray)
            {
                try
                {
                    //PROCESS COMPONENTS
                    await ProcessComponentsWithDynamicContent(dtdlArray, dtdl, globalResult);

                    //ALL EXCEPT COMPONENTS
                    itemResult = BuildDynamicContent(dtdl);
                }
                catch (ParsingException pex)
                {
                    if (itemResult == null)
                        itemResult = new DTDLContainer();

                    itemResult.ParsingErrors = pex.Errors.Select(i => i.Message);
                }
                catch (Exception ex)
                {
                    itemResult = null;
                }
                finally
                {
                    if (itemResult != null)
                    {
                        if (!globalResult.ContainsKey(dtdl["@id"].Value<string>()))
                            globalResult.Add(dtdl["@id"].Value<string>(), itemResult);
                    }

                    itemResult = null;
                }
            }


            return globalResult;
        }        

        public static async Task<Dictionary<string, DTDLCommandContainer>> ParseDTDLAndGetCommandsAsync(JArray dtdlArray)
        {
            if (dtdlArray == null)
                throw new ArgumentNullException(nameof(dtdlArray));

            Dictionary<string, DTDLCommandContainer> globalResult = new Dictionary<string, DTDLCommandContainer>();
            DTDLCommandContainer itemResult = null;
            
            ModelParser parser = new ModelParser();
            JArray contents = null;

            IReadOnlyDictionary<Dtmi, DTEntityInfo> parseResult = await parser.ParseAsync(dtdlArray.Select(i => JsonConvert.SerializeObject(i)));

            foreach (JObject dtdl in dtdlArray)
            {
                try
                {
                    //PROCESS COMPONENTS
                    await ProcessComponentsWithCommands(dtdlArray, dtdl, globalResult);
                    
                    //CONTENT
                    if (!dtdl.ContainsKey("contents"))
                        throw new Exception("The DTDL model does not contain any 'content' property.");

                    contents = (JArray)dtdl["contents"];

                    itemResult = new DTDLCommandContainer { ModelId = dtdl["@id"].Value<string>(), DTDL = dtdl };                    
                    itemResult.Commands = ExtractCommands(contents);
                }
                catch (ParsingException pex)
                {
                    if (itemResult == null)
                        itemResult = new DTDLCommandContainer();

                    itemResult.ParsingErrors = pex.Errors.Select(i => i.Message);
                }
                catch (Exception ex)
                {
                    itemResult = null;
                }
                finally
                {
                    if (itemResult != null && itemResult.Commands != null)
                    {
                        if (!globalResult.ContainsKey(dtdl["@id"].Value<string>()))
                            globalResult.Add(dtdl["@id"].Value<string>(), itemResult);
                    }

                    itemResult = null;
                }
            }


            return globalResult;
        }
        #endregion

        #region Private method(s)
        private static async Task ProcessComponentsWithDynamicContent(JArray dtdlArray, JObject dtdl, Dictionary<string, DTDLContainer> globalResult)
        {
            //CONTENT (COMPONENTSS AND OTHER)
            if (!dtdl.ContainsKey("contents"))
                throw new Exception("The DTDL model does not contain any 'content' property.");

            JArray componentLevelContents = (JArray)dtdl["contents"];

            if (componentLevelContents != null && componentLevelContents.Any())
            {
                var components = ExtractComponents(componentLevelContents);
                if (components != null && components.Any())
                {
                    JToken dtdlComponentModel = null;
                    JArray jArrayDTDLModel = null;

                    foreach (JObject item in components)
                    {
                        dtdlComponentModel = dtdlArray.Single(i => i["@id"].Value<string>().ToLower() == item.Value<string>("schema"));

                        if (dtdlComponentModel is JObject)
                        {
                            jArrayDTDLModel = new JArray();
                            jArrayDTDLModel.Add(dtdlComponentModel);
                        }
                        else if (dtdlComponentModel is JArray)
                            jArrayDTDLModel = dtdlComponentModel as JArray;

                        var tmpData = await ParseDTDLAndBuildDynamicContentAsync(jArrayDTDLModel);

                        if (tmpData != null && tmpData.Any())
                        {
                            var dataToAdd = tmpData.Except(globalResult);//TODO: define the proper EqualityComparer
                            if (dataToAdd != null && dataToAdd.Any())
                            {
                                foreach (var itemToAdd in dataToAdd)
                                {
                                    globalResult.Add(itemToAdd.Key, itemToAdd.Value);
                                }
                            }
                        }
                    }
                }
                componentLevelContents = null;
            }
        }

        private static async Task ProcessComponentsWithCommands(JArray dtdlArray, JObject dtdl, Dictionary<string, DTDLCommandContainer> globalResult)
        {
            //CONTENT (COMPONENTSS AND OTHER)
            if (!dtdl.ContainsKey("contents"))
                throw new Exception("The DTDL model does not contain any 'content' property.");

            JArray componentLevelContents = (JArray)dtdl["contents"];

            if (componentLevelContents != null && componentLevelContents.Any())
            {
                var components = ExtractComponents(componentLevelContents);
                if (components != null && components.Any())
                {
                    JToken dtdlComponentModel = null;
                    JArray jArrayDTDLModel = null;

                    foreach (JObject item in components)
                    {
                        dtdlComponentModel = dtdlArray.Single(i => i["@id"].Value<string>().ToLower() == item.Value<string>("schema"));

                        if (dtdlComponentModel is JObject)
                        {
                            jArrayDTDLModel = new JArray();
                            jArrayDTDLModel.Add(dtdlComponentModel);
                        }
                        else if (dtdlComponentModel is JArray)
                            jArrayDTDLModel = dtdlComponentModel as JArray;

                        var tmpData = await ParseDTDLAndGetCommandsAsync(jArrayDTDLModel);

                        if (tmpData != null && tmpData.Any())
                        {
                            var dataToAdd = tmpData.Except(globalResult);//TODO: define the proper EqualityComparer
                            if (dataToAdd != null && dataToAdd.Any())
                            {
                                foreach (var itemToAdd in dataToAdd)
                                {
                                    globalResult.Add(itemToAdd.Key, itemToAdd.Value);
                                }
                            }
                        }
                    }
                }
                componentLevelContents = null;
            }
        }
        private static DTDLContainer BuildDynamicContent(JObject dtdl)
        {
            if (dtdl == null)
                throw new ArgumentNullException(nameof(dtdl));

            DTDLContainer result = new DTDLContainer { ModelId = dtdl["@id"].Value<string>(), DTDL = dtdl };            

            //CONTENT
            if (!dtdl.ContainsKey("contents"))
                throw new Exception("The DTDL model does not contain any 'contents' property.");

            JArray contents = (JArray)dtdl["contents"];

            //Look for telemetries (JSON)
            JArray telemetries = ExtractTelemetries(contents);
            if (telemetries != null && telemetries.Any())
            {
                if (result.DTDLGeneratedData == null)
                    result.DTDLGeneratedData = new DTDLGeneratedData();

                result.DTDLGeneratedData.Telemetries = telemetries;
            }

            //Look for properties (JSON)
            JArray readableProperties = ExtractReadableProperties(contents);
            if (readableProperties != null && readableProperties.Any())
            {
                if (result.DTDLGeneratedData == null)
                    result.DTDLGeneratedData = new DTDLGeneratedData();

                result.DTDLGeneratedData.ReadableProperties = readableProperties;
            }

            JArray writableProperties = ExtractWritableProperties(contents);
            if (writableProperties != null && writableProperties.Any())
            {
                if (result.DTDLGeneratedData == null)
                    result.DTDLGeneratedData = new DTDLGeneratedData();

                result.DTDLGeneratedData.WritableProperties = writableProperties;
            }

            //Commands
            JArray commands = ExtractCommands(contents);
            if (commands != null && commands.Any())
            {
                if (result.DTDLGeneratedData == null)
                    result.DTDLGeneratedData = new DTDLGeneratedData();
                result.DTDLGeneratedData.Commands = commands;
            }

            return result;
        }

        private static JArray ExtractTelemetries(JArray contents)
        {
            JArray result = null;
            var telemetries = contents.Where(i => i["@type"].Value<string>().ToLower() == "telemetry");
            if (telemetries != null && telemetries.Any())
            {
                result = new JArray();

                JObject tmp = null;
                string tmpPropertyName = string.Empty;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in telemetries)
                {
                    tmpPropertyName = item["name"].Value<string>();

                    tmp = new JObject();

                    JProperty jProperty = AddCreatedProperties(tmpPropertyName, item["schema"].Value<string>(), random);

                    if (jProperty != null)
                        tmp.Add(jProperty);

                    result.Add(tmp);
                }
            }

            return result;
        }

        private static JArray ExtractWritableProperties(JArray contents)
        {
            JArray result = null;

            var properties = contents.Where(
               i =>
                   (i is JObject)
                   &&
                   i["@type"].Value<string>().ToLower() == "property"
                   &&
                   ((JObject)i).ContainsKey("writable") && i["writable"].Value<bool>()
               );


            if (properties != null && properties.Any())
            {
                result = new JArray();

                JObject tmp = null;
                string tmpPropertyName = string.Empty;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in properties)
                {
                    tmpPropertyName = item["name"].Value<string>();

                    tmp = new JObject();

                    JProperty jProperty = AddCreatedProperties(tmpPropertyName, item["schema"].Value<string>(), random);

                    if (jProperty != null)
                        tmp.Add(jProperty);

                    result.Add(tmp);
                }
            }

            return result;
        }

        private static JArray ExtractReadableProperties(JArray contents)
        {
            JArray result = null;
            var properties = contents.Where(
                i => 
                    (i is JObject)
                    &&
                    i["@type"].Value<string>().ToLower() == "property" 
                    && 
                    (
                        !((JObject)i).ContainsKey("writable") 
                        |
                        (((JObject)i).ContainsKey("writable") && !i["writable"].Value<bool>())
                    )
                );

            if (properties != null && properties.Any())
            {
                result = new JArray();
                JObject tmp = null;
                string tmpPropertyName = string.Empty;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in properties)
                {
                    tmpPropertyName = item["name"].Value<string>();

                    tmp = new JObject();

                    JProperty jProperty = AddCreatedProperties(tmpPropertyName, item["schema"].Value<string>(), random);

                    if (jProperty != null)
                        tmp.Add(jProperty);

                    result.Add(tmp);
                }
            }

            return result;
        }

        private static JArray ExtractCommands(JArray contents)
        {
            JArray result = null;
            var commands = contents.Where(i => i["@type"].Value<string>().ToLower() == "command");
            if (commands != null && commands.Any())
            {
                result = new JArray();

                string tmpCommandName = string.Empty;
                JObject tmpRequest = null;
                JObject tmpResponse = null;
                string tmpRequestName = string.Empty;
                string tmpResponseName = string.Empty;

                JObject tmpCreatedCommand = null;
                JObject tmpCreatedRequestAndResponseContainer = null;
                JObject tmpCreatedRequest = null;
                JObject tmpCreatedResponse = null;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in commands)
                {
                    tmpCreatedCommand = new JObject();
                    tmpCreatedRequestAndResponseContainer = new JObject();
                    //command
                    tmpCommandName = item["name"].Value<string>();

                    //request
                    tmpRequest = (JObject)item["request"];
                    if (tmpRequest != null)
                    {
                        tmpCreatedRequest = new JObject();
                        tmpRequestName = tmpRequest["name"].Value<string>();
                        JProperty jProperty = AddCreatedProperties(tmpRequestName, tmpRequest["schema"].Value<string>(), random);

                        if (jProperty != null)
                            tmpCreatedRequest.Add(jProperty);

                        tmpCreatedRequestAndResponseContainer.Add("request", tmpCreatedRequest);
                    }

                    //response
                    tmpResponse = (JObject)item["response"];
                    if (tmpResponse != null)
                    {
                        tmpCreatedResponse = new JObject();
                        tmpResponseName = tmpResponse["name"].Value<string>();
                        JProperty jProperty = AddCreatedProperties(tmpResponseName, tmpResponse["schema"].Value<string>(), random);

                        if (jProperty != null)
                            tmpCreatedResponse.Add(jProperty);

                        tmpCreatedRequestAndResponseContainer.Add("response", tmpCreatedResponse);
                    }

                    tmpCreatedCommand.Add(tmpCommandName, tmpCreatedRequestAndResponseContainer);

                    result.Add(tmpCreatedCommand);
                }
            }

            return result;
        }

        private static JArray ExtractComponents(JArray contents)
        {
            JArray result = null;
            var properties = contents.Where(i => i["@type"].Value<string>().ToLower() == "component");
            if (properties != null && properties.Any())
                result = JArray.FromObject(properties);

            return result;
        }

        private static JProperty AddCreatedProperties(string propertyName, string schemaName, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            JProperty jProperty = null;

            switch (schemaName.ToLower())
            {
                case "double":
                    jProperty = new JProperty(propertyName, random.NextDouble());
                    break;
                case "datetime":
                    jProperty = new JProperty(propertyName, DateTime.Now.AddHours(random.Next(0, 148)));
                    break;
                case "string":
                    jProperty = new JProperty(propertyName, "string to be randomized");
                    break;
                case "integer":
                    jProperty = new JProperty(propertyName, random.Next());
                    break;
                case "boolean":
                    jProperty = new JProperty(propertyName, random.Next(0, 1) == 1 ? true : false);
                    break;
                case "date":
                    jProperty = new JProperty(propertyName, DateTime.Now.AddHours(random.Next(0, 148)).Date);
                    break;
                case "duration":
                    jProperty = new JProperty(propertyName, random.Next());
                    break;
                case "float":
                    jProperty = new JProperty(propertyName, random.NextDouble());
                    break;
                case "long":
                    jProperty = new JProperty(propertyName, random.Next());
                    break;
                case "time":
                    jProperty = new JProperty(propertyName, DateTime.Now.AddHours(random.Next(0, 148)).TimeOfDay);
                    break;
                default:
                    jProperty = new JProperty(propertyName, "Coplex or not identified schema");
                    break;
            }

            return jProperty;
        }
        #endregion

    }
}
