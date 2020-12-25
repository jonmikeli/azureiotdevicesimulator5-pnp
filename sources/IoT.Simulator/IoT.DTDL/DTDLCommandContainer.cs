using Newtonsoft.Json.Linq;

using System.Collections.Generic;

namespace IoT.DTDL
{
    public class DTDLCommandContainer
    {
        //Model
        public string ModelId { get; set; }
        public JToken DTDL { get; set; }
        public IEnumerable<string> ParsingErrors { get; set; }

        public JArray Commands { get; set; }
    }
}
