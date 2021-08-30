using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using System.Linq;
using System.Threading.Tasks;

namespace IoT.DTDL.Tests
{
    [TestClass()]
    public class DTDLAvnetTests
    {       
        #region Avnet

        [TestMethod()]
        [TestCategory("Interface")]
        [TestCategory("mt3620starterkit")]
        public async Task MT3620StarterKit_Telemetries_With_Units_OK()
        {
            string dtdlModelPath = @"./Tests/Avnet/mt3620starterkit-1.json";
            string modelId = "dtmi:avnet:mt3620Starterkit;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(data.Any());
        }
        #endregion
    }
}
