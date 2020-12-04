using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Simulator.Services.IoT
{
    public class DTDLCommandHandlerContext
    {
        public string CommandName { get; set;}
        public JToken CommandRequestPayload { get; set; }
        public JToken CommandResponsePayload { get; set; }
    }
}
