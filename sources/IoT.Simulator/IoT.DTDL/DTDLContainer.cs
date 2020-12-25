using Newtonsoft.Json.Linq;

using System.Collections.Generic;

namespace IoT.DTDL
{
    public class DTDLContainer
    {
        //Model
        public string ModelId { get; set; }
        public JToken DTDL { get; set; }
        public IEnumerable<string> ParsingErrors { get; set; }

        //Sample data
        public DTDLGeneratedData DTDLGeneratedData { get; set; }
    }

    //Sample data
    public class DTDLGeneratedData
    {
        public JArray Telemetries { get; set; }
        public JArray ReadableProperties { get; set; }

        public JArray WritableProperties { get; set; }

        public JArray Commands { get; set; }
    }
}
