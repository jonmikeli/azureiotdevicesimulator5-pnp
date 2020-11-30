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
        public async Task GetModelsAndBuildDynamicContentAsync_Thermostat_OK()
        {
            string dtdlModelPath = @"./Tests/thermostat.json";
            string modelId = "dtmi:com:example:thermostat;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsTrue(modelContainer.Count > 0);

            Assert.IsNotNull(modelContainer[modelId].DTDLGeneratedData);
            Assert.IsNotNull(modelContainer[modelId].DTDLGeneratedData.Telemetries);
        }

        [TestMethod()]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic2_OneTelemetry_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic2.json";
            string modelId = "dtmi:com:jmi:simulator:devicemessages;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsTrue(modelContainer.Count > 0);

            Assert.IsNotNull(modelContainer[modelId].DTDLGeneratedData);
            Assert.IsNotNull(modelContainer[modelId].DTDLGeneratedData.Telemetries);
        }

        [TestMethod()]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_ManyTelemetries_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator:devicemessages;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsTrue(modelContainer.Count > 0);

            Assert.IsNotNull(modelContainer[modelId].DTDLGeneratedData);
            Assert.IsNotNull(modelContainer[modelId].DTDLGeneratedData.Telemetries);
        }
    }
}
