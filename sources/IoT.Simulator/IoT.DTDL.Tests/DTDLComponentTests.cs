﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using System.Threading.Tasks;

namespace IoT.DTDL.Tests
{
    [TestClass()]
    public class DTDLComponentTests
    {       
        #region Components       
        #region Single
        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.telemetries.readableproperties.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_ReadableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_WritableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Commands_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }
        #endregion

        #region Combined
        [TestMethod()]
        [TestCategory("Components")]
        [TestProperty("dtdlModelPath", "./Tests/jmi.simulator.pnp.model.generic3.json")]
        [TestProperty("modelId", "dtmi:com:jmi:simulator5;1")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_ReadableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_WritableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.telemetries.writableproperties.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_Commands_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }
        #endregion

        #region Full
        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Full_OK()
        {
            string dtdlModelPath = @"./Tests/jmi.simulator.pnp.model.generic3.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null));
            Assert.IsNotNull(modelContainer.Select(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null));
        }
        #endregion
        #endregion
    }
}