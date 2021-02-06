using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.IO;
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
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.telemetries.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(!data.Any());
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_ReadableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.readableproperties.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(!data.Any());
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_WritableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.writableproperties.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Commands_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.commands.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(!data.Any());
        }
        #endregion

        #region Combined
        [TestMethod()]
        [TestCategory("Components")]
        [TestProperty("dtdlModelPath", "./Tests/Components/jmi.simulator.pnp.model.telemetries.readableproperties.json")]
        [TestProperty("modelId", "dtmi:com:jmi:simulator5;1")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_ReadableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.telemetries.readableproperties.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(!data.Any());
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_WritableProperties_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.telemetries.writableproperties.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Telemetries_Commands_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.telemetries.commands.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(!data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(data.Any());

            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(!data.Any());
        }
        #endregion

        #region Full
        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Full_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.full.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

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

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Full_WithAdditionalItems_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.full.withAdditionalItems.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            //Readable properties
            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(data.Any());

            //Telemetries
            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            var rawModel = await DTDLHelper.GetDTDLFromModelIdAsync(modelId, dtdlModelPath);
            Assert.IsNotNull(rawModel);

            JArray arrayModel = null;
            if (rawModel is JObject)
            {
                arrayModel = new JArray();
                arrayModel.Add(rawModel);
            }
            else if (rawModel is JArray)
                arrayModel = rawModel as JArray;
            
            //We control the number of models / components with telemetries
            var components = arrayModel.Single(i => i.Value<string>("@id") == modelId)["contents"].Where(i => i.Value<string>("@type").ToLower() == "component");
            Assert.IsNotNull(components);
            Assert.IsTrue(components.Any());

            var modelsWithTelemetries = arrayModel.Where(i => ((JArray)i["contents"]).Count(i=>i.Value<string>("@type").ToLower()=="telemetry") > 0);
            Assert.IsNotNull(modelsWithTelemetries);
            Assert.IsTrue(modelsWithTelemetries.Any());

            var actualModelsWithTelemetries = components.Join(modelsWithTelemetries, c => c.Value<string>("schema"), m => m.Value<string>("@id"), (c, m) => c);
            Assert.IsNotNull(actualModelsWithTelemetries);

            Assert.IsTrue(data.Count() == actualModelsWithTelemetries.Count()); 

            //Commands
            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(data.Any());

            //Writable properties
            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        [TestCategory("Components")]
        public async Task GetModelsAndBuildDynamicContentAsync_Generic3_Components_Full_WithAdditionalItems_MixedTelemetriesInComponents_OK()
        {
            string dtdlModelPath = @"./Tests/Components/jmi.simulator.pnp.model.full.withAdditionalItems.mixedTelemetriesInComponents.json";
            string modelId = "dtmi:com:jmi:simulator5;1";

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            //Readable properties
            var data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(data.Any());

            //Telemetries
            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(data.Any());

            var rawModel = await DTDLHelper.GetDTDLFromModelIdAsync(modelId, dtdlModelPath);
            Assert.IsNotNull(rawModel);

            JArray arrayModel = null;
            if (rawModel is JObject)
            {
                arrayModel = new JArray();
                arrayModel.Add(rawModel);
            }
            else if (rawModel is JArray)
                arrayModel = rawModel as JArray;

            //We control the number of models / components with telemetries
            var components = arrayModel.Single(i => i.Value<string>("@id") == modelId)["contents"].Where(i => i.Value<string>("@type").ToLower() == "component");
            Assert.IsNotNull(components);
            Assert.IsTrue(components.Any());

            var modelsWithTelemetries = arrayModel.Where(i => ((JArray)i["contents"]).Count(i => i.Value<string>("@type").ToLower() == "telemetry") > 0);
            Assert.IsNotNull(modelsWithTelemetries);
            Assert.IsTrue(modelsWithTelemetries.Any());

            var actualModelsWithTelemetries = components.Join(modelsWithTelemetries, c => c.Value<string>("schema"), m => m.Value<string>("@id"), (c, m) => c);
            Assert.IsNotNull(actualModelsWithTelemetries);

            Assert.IsTrue(data.Count() == actualModelsWithTelemetries.Count() + 1);

            //Telemetries at root level
            var telemetriesAtRootLevel = arrayModel.Single(i => i.Value<string>("@id") == modelId)["contents"].Where(i => i.Value<string>("@type").ToLower() == "telemetry");
            Assert.IsNotNull(telemetriesAtRootLevel);
            Assert.IsTrue(telemetriesAtRootLevel.Any());
            Assert.IsNotNull(data.Single(i=>i.Key == modelId).Value.DTDLGeneratedData.Telemetries);
            Assert.IsTrue(data.Single(i => i.Key == modelId).Value.DTDLGeneratedData.Telemetries.Count == telemetriesAtRootLevel.Count());

            //Commands
            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(data.Any());

            //Writable properties
            data = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(data.Any());
        }
        #endregion
        #endregion
    }
}
