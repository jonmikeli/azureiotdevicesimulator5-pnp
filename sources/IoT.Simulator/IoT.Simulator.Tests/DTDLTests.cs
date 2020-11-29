using IoT.Simulator.Tools;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Simulator.Tests
{
    [TestClass()]
    public class DTDLTests
    {
        [TestMethod()]
        public async Task GetTelemetries_Thermostat_OK()
        {
            string dtdlModelPath = @"./Tests/thermostat.json";

            JObject dtdlModel = JObject.Parse(File.ReadAllText(dtdlModelPath));

            Assert.IsNotNull(dtdlModel);

            var messageBody = await DTDLHelper.BuildMessageBodyFromDTDLAsync(dtdlModel);

            Assert.IsNotNull(messageBody);
        }
    }
}
