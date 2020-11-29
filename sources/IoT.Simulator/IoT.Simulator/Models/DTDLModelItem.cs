using Newtonsoft.Json;

namespace IoT.Simulator.Models
{
    public class DTDLModelItem
    {
        [JsonProperty("modelId")]
        public string ModelId { get; set; }
        [JsonProperty("modelPath")]
        public string ModelPath { get; set; }
        [JsonProperty("modelType")]
        public DTDLModelType ModelType { get; set; }
    }
}
