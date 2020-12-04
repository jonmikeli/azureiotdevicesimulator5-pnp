using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
