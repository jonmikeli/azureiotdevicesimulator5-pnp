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

        [TestMethod()]
        public async Task GetTelemetries_Generic2_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic2.json";

            JArray dtdlModels = JArray.Parse(File.ReadAllText(dtdlModelPath));

            Assert.IsNotNull(dtdlModels);
            Assert.IsNotNull(dtdlModels.Count > 0);

            var messageBody = await DTDLHelper.BuildMessageBodyFromDTDLAsync(dtdlModels);

            Assert.IsNotNull(messageBody);
            Assert.IsTrue(messageBody.Count > 0);

            foreach (var item in messageBody)
            {
                Assert.IsNotNull(item.Value);
            }
        }
    }
}
