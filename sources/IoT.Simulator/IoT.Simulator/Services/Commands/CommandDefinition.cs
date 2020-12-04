using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Simulator.Services.Commands
{

    public class CommandDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("request")]
        public string Request { get; set; }
        [JsonProperty("response")]
        public string Response { get; set; }
    }
}
