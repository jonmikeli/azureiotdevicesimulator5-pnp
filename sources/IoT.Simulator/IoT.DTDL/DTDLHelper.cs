using Microsoft.Azure.DigitalTwins.Parser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

            JArray jArrayDTDLModel=null;
            if (dtdlModel is JObject)
            {
                jArrayDTDLModel = new JArray();
                jArrayDTDLModel.Add(dtdlModel);
            }
            else if (dtdlModel is JArray)
                jArrayDTDLModel = dtdlModel as JArray;

            //Build the JSON Message corresponding to the model
            return await ParseDTDLAndBuildDynamicContentAsync(jArrayDTDLModel);
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

            Dictionary<string, DTDLContainer> globalResult = null;
            DTDLContainer itemResult = null;

            ModelParser parser = new ModelParser();
            IReadOnlyDictionary<Dtmi, DTEntityInfo> parseResult = null;
            JArray contents = null;
            foreach (JObject dtdl in dtdlArray)
            {
                try
                {
                    globalResult = new Dictionary<string, DTDLContainer>();
                    parseResult = await parser.ParseAsync(dtdlArray.Select(i => JsonConvert.SerializeObject(i)));
                 
                    //CONTENT
                    if (!dtdl.ContainsKey("contents"))
                        throw new Exception("");

                    contents = (JArray)dtdl["contents"];

                    //Look for telemetries (JSON)
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
                        globalResult.Add(dtdl["@id"].Value<string>(), itemResult);

                    itemResult = null;
                }
            }


            return globalResult;
        }
        #endregion

        #region Private method(s)
        private static DTDLContainer BuildDynamicContent(JObject dtdl)
        {
            if (dtdl == null)
                throw new ArgumentNullException(nameof(dtdl));

            DTDLContainer result = new DTDLContainer { ModelId = dtdl["@id"].Value<string>(), DTDL = dtdl };
            result.DTDLGeneratedData = new DTDLGeneratedData();

            //CONTENT
            if (!dtdl.ContainsKey("contents"))
                throw new Exception("");

            JArray contents = (JArray)dtdl["contents"];

            //Look for telemetries (JSON)
            result.DTDLGeneratedData.Telemetries = ExtractTelemetries(contents);

            //Look for properties (JSON)
            result.DTDLGeneratedData.ReadableProperties = ExtractReadableProperties(contents);
            result.DTDLGeneratedData.WritableProperties = ExtractWritableProperties(contents);
            result.DTDLGeneratedData.Commands = ExtractCommands(contents);

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
            var properties = contents.Where(i => i["@type"].Value<string>().ToLower() == "property" && i.Contains("writable") &&  i["writable"].Value<string>().ToLower() == "true");
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
            var properties = contents.Where(i => i["@type"].Value<string>().ToLower() == "property" && i.Contains("writable") && i["writable"].Value<string>().ToLower() == "false");
            if (properties != null && properties.Any())
            {
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

                JObject tmp = null;
                string tmpCommandName = string.Empty;
                JObject tmpRequest = null;
                JObject tmpResponse = null;
                string tmpRequestName = string.Empty;
                string tmpResponseName = string.Empty;

                JObject tmpCreatedRequest = null;
                JObject tmpCreatedResponse = null;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in commands)
                {
                    tmp = new JObject();
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

                        tmp.Add("request", tmpCreatedRequest);
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

                        tmp.Add("response", tmpCreatedResponse);
                    }                    
                    
                    result.Add(tmp);
                }
            }

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
