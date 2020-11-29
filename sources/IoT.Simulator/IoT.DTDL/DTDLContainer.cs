﻿using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DTDL
{
    public class DTDLContainer
    {
        //Model
        public string ModelId { get; set; }
        public JToken DTDL { get; set; }

        //Sample data
        public DTDLGeneratedData DTDLGeneratedData { get; set; }
    }

    //Sample data
    public class DTDLGeneratedData
    {
        public JArray Telemetries { get; set; }
        public JArray Properties { get; set; }
    }
}
