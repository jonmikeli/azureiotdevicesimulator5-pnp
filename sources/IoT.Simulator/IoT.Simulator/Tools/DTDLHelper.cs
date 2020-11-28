using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
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
                throw new Exception($"No DTDL model with the id {modelId} has been provided at the provided location {}.");

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
            //TODO add a cache system to optimize the calls
            throw new NotImplementedException();
            //Get the model from the given repository
        }
    }
}
