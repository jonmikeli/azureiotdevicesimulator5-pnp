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

        public static JObject BuildMessageBodyFromModelId(string modelId, string[] locations)
        {            
            //Get the full DTDL model
            JObject dtdlModel = GetDTDLFromModelId(modelId, locations);            

            if (dtdlModel == null)
                throw new Exception($"No DTDL model with the id {modelId} has been provided at the provided locations.");

            //Build the JSON Message corresponding to the model
            return BuildMessageBodyFromDTDL(dtdlModel);
        }

        public static JObject BuildMessageBodyFromDTDL(JObject dtdl)
        {
            throw new NotImplementedException();
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
            JObject result = JObject.Parse(File.ReadAllText("DTDLTest.json"));

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
