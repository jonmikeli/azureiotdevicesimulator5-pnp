using Microsoft.Azure.DigitalTwins.Parser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DTDL
{
    //https://docs.microsoft.com/en-us/azure/iot-pnp/concepts-model-parser
    //TYPES: https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md
    public class DTDLHelper
    {

        public static async Task<JArray> BuildMessageBodyFromModelId(string modelId, string[] locations)
        {
            //Get the full DTDL model
            JObject dtdlModel = GetDTDLFromModelId(modelId, locations);

            if (dtdlModel == null)
                throw new Exception($"No DTDL model with the id {modelId} has been provided at the provided locations.");

            //Build the JSON Message corresponding to the model
            return await BuildMessageBodyFromDTDLAsync(dtdlModel);
        }

        //https://docs.microsoft.com/en-us/azure/iot-pnp/concepts-model-parser
        public static async Task<JArray> BuildMessageBodyFromDTDLAsync(JObject dtdl)
        {
            if (dtdl == null)
                throw new ArgumentNullException(nameof(dtdl));

            JArray result = null;

            ModelParser parser = new ModelParser();
            try
            {
                IReadOnlyDictionary<Dtmi, DTEntityInfo> parseResult = await parser.ParseAsync(new string[] { JsonConvert.SerializeObject(dtdl) });

                //CONTENT
                if (!dtdl.ContainsKey("contents"))
                    throw new Exception("");

                JArray contents = (JArray)dtdl["contents"];
                //Look for telemetries (JSON)
                var telemetries = contents.Where(i => i["@type"].Value<string>().ToLower() == "telemetry");
                if (telemetries != null && telemetries.Any())
                {
                    result = new JArray();
                    JObject tmp = null;

                    string tmpPropertyName = string.Empty;
                    Random random = new Random(DateTime.Now.Millisecond);
                    foreach (var item in telemetries)
                    {
                        tmp = new JObject();
                        tmpPropertyName = item["name"].Value<string>();

                        switch (item["schema"].Value<string>().ToLower())
                        {
                            case "double":
                                tmp.Add(tmpPropertyName, random.NextDouble());
                                break;
                            case "datetime":
                                tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)));
                                break;
                            case "string":
                                tmp.Add(tmpPropertyName, "string to be randomized");
                                break;
                            case "integer":
                                tmp.Add(tmpPropertyName, random.Next());
                                break;
                            default:
                                break;
                        }

                        result.Add(tmp);
                    }
                }


                //Look for properties (JSON) - Reported properties
                //var properties = contents.Select(i => i["@type"].Value<string>().ToLower() == "property");


            }
            catch (ParsingException pex)
            {
                //Console.WriteLine(pex.Message);
                //foreach (var err in pex.Errors)
                //{
                //    Console.WriteLine(err.PrimaryID);
                //    Console.WriteLine(err.Message);
                //}
            }

            return result;
        }

        public static async Task<Dictionary<string, DTDLContainer>> ParseDTDLAndBuildDynamicContentAsync(JArray dtdlArray)
        {
            if (dtdlArray == null)
                throw new ArgumentNullException(nameof(dtdlArray));

            Dictionary<string, DTDLContainer> globalResult = null;
            DTDLContainer itemResult = null;

            ModelParser parser = new ModelParser();
            try
            {
                IReadOnlyDictionary<Dtmi, DTEntityInfo> parseResult = await parser.ParseAsync(dtdlArray.Select(i => JsonConvert.SerializeObject(i)));
                globalResult = new Dictionary<string, DTDLContainer>();

                foreach (JObject dtdl in dtdlArray)
                {
                    //CONTENT
                    if (!dtdl.ContainsKey("contents"))
                        throw new Exception("");

                    JArray contents = (JArray)dtdl["contents"];
                    
                    //Look for telemetries (JSON)
                    itemResult = BuildDynamicContent(dtdl);

                    if (itemResult != null)
                        globalResult.Add(dtdl["@id"].Value<string>(), itemResult);

                }
            }
            catch (ParsingException pex)
            {
                //Console.WriteLine(pex.Message);
                //foreach (var err in pex.Errors)
                //{
                //    Console.WriteLine(err.PrimaryID);
                //    Console.WriteLine(err.Message);
                //}
            }


            return globalResult;
        }


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
                    tmp = new JObject();
                    tmpPropertyName = item["name"].Value<string>();

                    switch (item["schema"].Value<string>().ToLower())
                    {
                        case "double":
                            tmp.Add(tmpPropertyName, random.NextDouble());
                            break;
                        case "datetime":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)));
                            break;
                        case "string":
                            tmp.Add(tmpPropertyName, "string to be randomized");
                            break;
                        case "integer":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "boolean":
                            tmp.Add(tmpPropertyName, random.Next(0, 1) == 1 ? true : false);
                            break;
                        case "date":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)).Date);
                            break;
                        case "duration":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "float":
                            tmp.Add(tmpPropertyName, random.NextDouble());
                            break;
                        case "long":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "time":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)).TimeOfDay);
                            break;
                        default:
                            tmp.Add(tmpPropertyName, "Coplex or not identified schema");
                            break;
                    }

                    result.Add(tmp);
                }
            }

            return result;
        }

        private static JArray ExtractWritableProperties(JArray contents)
        {
            JArray result = null;
            var properties = contents.Where(i => i["@type"].Value<string>().ToLower() == "property" && i["writable"].Value<string>().ToLower() == "true");
            if (properties != null && properties.Any())
            {
                result = new JArray();

                JObject tmp = null;
                string tmpPropertyName = string.Empty;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in properties)
                {
                    tmp = new JObject();
                    tmpPropertyName = item["name"].Value<string>();

                    switch (item["schema"].Value<string>().ToLower())
                    {
                        case "double":
                            tmp.Add(tmpPropertyName, random.NextDouble());
                            break;
                        case "datetime":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)));
                            break;
                        case "string":
                            tmp.Add(tmpPropertyName, "string to be randomized");
                            break;
                        case "integer":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "boolean":
                            tmp.Add(tmpPropertyName, random.Next(0, 1) == 1 ? true : false);
                            break;
                        case "date":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)).Date);
                            break;
                        case "duration":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "float":
                            tmp.Add(tmpPropertyName, random.NextDouble());
                            break;
                        case "long":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "time":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)).TimeOfDay);
                            break;
                        default:
                            tmp.Add(tmpPropertyName, "Coplex or not identified schema");
                            break;
                    }

                    result.Add(tmp);
                }
            }

            return result;
        }

        private static JArray ExtractReadableProperties(JArray contents)
        {
            JArray result = null;
            var properties = contents.Where(i => i["@type"].Value<string>().ToLower() == "property" && i["writable"].Value<string>().ToLower() == "false");
            if (properties != null && properties.Any())
            {
                JObject tmp = null;
                string tmpPropertyName = string.Empty;

                Random random = new Random(DateTime.Now.Millisecond);
                foreach (var item in properties)
                {
                    tmp = new JObject();
                    tmpPropertyName = item["name"].Value<string>();

                    switch (item["schema"].Value<string>().ToLower())
                    {
                        case "double":
                            tmp.Add(tmpPropertyName, random.NextDouble());
                            break;
                        case "datetime":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)));
                            break;
                        case "string":
                            tmp.Add(tmpPropertyName, "string to be randomized");
                            break;
                        case "integer":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "boolean":
                            tmp.Add(tmpPropertyName, random.Next(0, 1) == 1 ? true : false);
                            break;
                        case "date":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)).Date);
                            break;
                        case "duration":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "float":
                            tmp.Add(tmpPropertyName, random.NextDouble());
                            break;
                        case "long":
                            tmp.Add(tmpPropertyName, random.Next());
                            break;
                        case "time":
                            tmp.Add(tmpPropertyName, DateTime.Now.AddHours(random.Next(0, 148)).TimeOfDay);
                            break;
                        default:
                            tmp.Add(tmpPropertyName, "Coplex or not identified schema");
                            break;
                    }

                    result.Add(tmp);
                }
            }

            return result;
        }

        public static JObject GetDTDLFromModelId(string modelId, string[] locations)
        {
            if (string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            if (locations == null | !locations.Any())
                throw new ArgumentNullException(nameof(locations));

            //Get the full DTDL model
            JObject dtdlModel = null;
            int i = 0;
            while (dtdlModel == null && i < locations.Length)
            {
                dtdlModel = GetDTDLFromModelId(modelId, locations[i]);
                i++;
            }

            return dtdlModel;
        }

        public static JObject GetDTDLFromModelId(string modelId, string modelRepositoryPath)
        {
            if (string.IsNullOrEmpty(modelId))
                throw new ArgumentNullException(nameof(modelId));

            if (string.IsNullOrEmpty(modelRepositoryPath))
                throw new ArgumentNullException(nameof(modelRepositoryPath));

            //TODO: to be replaced with a generic solution
            JObject result = JObject.Parse(File.ReadAllText("~/Tests/thermostat.json"));

            //TODO add a cache system to optimize the calls
            //if cache contains the model and it's valid, send it
            //if not, request the repository and get the model. Put it in the cache and send the value.

            //Get the model from the given repository


            //Cloud provider

            //Local path?

            return result;
        }
    }
}
