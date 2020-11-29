using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DTDL.Tests
{
    [TestClass()]
    public class DTDLTests
    {
        [TestMethod()]
        public async Task BuildMessageBodyFromDTDLAsync_Thermostat_OK()
        {
            string dtdlModelPath = @"./Tests/thermostat.json";

            JObject dtdlModel = JObject.Parse(File.ReadAllText(dtdlModelPath));

            Assert.IsNotNull(dtdlModel);

            var messageBody = await DTDLHelper.GetModelsAndBuildDynamicContent(dtdlModel);

            Assert.IsNotNull(messageBody);
        }

        [TestMethod()]
        public async Task ParseDTDLAndBuildDynamicContentAsync_Generic2_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic2.json";

            JArray dtdlModels = JArray.Parse(File.ReadAllText(dtdlModelPath));

            Assert.IsNotNull(dtdlModels);
            Assert.IsNotNull(dtdlModels.Count > 0);

            var modelContainer = await DTDLHelper.ParseDTDLAndBuildDynamicContentAsync(dtdlModels);

            Assert.IsNotNull(modelContainer);
            Assert.IsTrue(modelContainer.Count > 0);
        }
    }
}
