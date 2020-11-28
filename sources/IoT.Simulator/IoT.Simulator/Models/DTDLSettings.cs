using System.Collections.Generic;

namespace IoT.Simulator.Models
{
    public class DTDLSettings
    {
        public DTDLModelItem DefaultModel { get; set; }

        public IEnumerable<DTDLModelItem> Models { get; set; }
    }
}
