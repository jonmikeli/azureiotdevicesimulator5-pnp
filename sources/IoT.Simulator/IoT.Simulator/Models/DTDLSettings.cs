﻿using System.Collections.Generic;

namespace IoT.Simulator.Models
{
    public class CloudDTDLSettings
    {
        public string DefaultModelId { get; set; }

        public IList<DTDLModelItem> Models { get; set; }
    }
}
