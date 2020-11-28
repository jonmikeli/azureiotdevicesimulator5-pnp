using Microsoft.Azure.DigitalTwins.Parser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Simulator.Tools
{
    //https://docs.microsoft.com/en-us/azure/iot-pnp/concepts-model-parser
    public class DTDLHelper
    {

        public static async Task<JObject> BuildMessageBodyFromModelId(string modelId, string[] locations)
        {
            //Get the full DTDL model
            JObject dtdlModel = GetDTDLFromModelId(modelId, locations);

            if (dtdlModel == null)
                throw new Exception($"No DTDL model with the id {modelId} has been provided at the provided locations.");

            //Build the JSON Message corresponding to the model
            return await BuildMessageBodyFromDTDLAsync(dtdlModel);
        }

        //https://docs.microsoft.com/en-us/azure/iot-pnp/concepts-model-parser
        public static async Task<JObject> BuildMessageBodyFromDTDLAsync(JObject dtdl)
        {
            if (dtdl == null)
                throw new ArgumentNullException(nameof(dtdl));

            JObject result = null;

            ModelParser parser = new ModelParser();
            try
            {
                IReadOnlyDictionary<Dtmi, DTEntityInfo> parseResult = await parser.ParseAsync(new string[] { JsonConvert.SerializeObject(dtdl) });

                //CONTENT
                if (!dtdl.ContainsKey("contents"))
                    throw new Exception("");

                JArray contents = (JArray)dtdl["contents"];
                //Look for telemetries (JSON)
                var telemetries = contents.Select(i => i["@type"].Value<string>().ToLower() == "telemetry");
                if (telemetries != null && telemetries.Any())
                {
                    result = new JObject();
                    JObject tmp = null;
                    foreach (var item in telemetries)
                    {
                        tmp = new JObject();
                        tmp.Add(item["name"], "");

                        switch (item["schema"].ToLower())
                        {
                            case "double":
                                tmp.Add("", "");
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
