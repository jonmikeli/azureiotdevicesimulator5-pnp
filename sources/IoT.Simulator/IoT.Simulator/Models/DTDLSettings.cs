using System.Collections.Generic;

namespace IoT.Simulator.Models
{
    public class DTDLSettings
    {
        public string DefaultModelId { get; set; }

        public IList<DTDLModelItem> Models { get; set; }

        public DTDLSettings(string modelId, DTDLModelType type)
        {
            DefaultModelId = modelId;
            Models = new List<DTDLModelItem> { new DTDLModelItem { ModelId = modelId, ModelType = type } }; 
        }
    }
}
