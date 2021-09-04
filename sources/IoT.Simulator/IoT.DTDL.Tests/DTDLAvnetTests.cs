using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.IO;
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

        [TestMethod()]
        [TestCategory("Interface")]
        [TestCategory("mt3620starterkit")]
        public async Task MT3620StarterKit_Telemetries_With_Semantics_And_Units_OK()
        {
            string dtdlModelPath = @"./Tests/Avnet/mt3620starterkit-1.json";
            string modelId = "dtmi:avnet:mt3620Starterkit;1";

            Assert.IsTrue(File.Exists(dtdlModelPath));

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var readableProperties = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(readableProperties.Any());

            var telemetriesGenerated = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(telemetriesGenerated.Any());

            var commands = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(commands.Any());

            var writableProperties = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(writableProperties.Any());

            //Additional assets
            string stringModel = await File.ReadAllTextAsync(dtdlModelPath);
            Assert.IsTrue(!string.IsNullOrEmpty(stringModel));

            ModelParser modelParser = new ModelParser();            
            IReadOnlyDictionary<Dtmi, DTEntityInfo> dtdl = await modelParser.ParseAsync(new List<string> { stringModel });

            Assert.IsNotNull(dtdl);
            Assert.IsNotNull(dtdl.Values);

            var interfaces = dtdl.Values.Where(i => i.EntityKind == DTEntityKind.Interface);
            Assert.IsNotNull(interfaces);

            //Telemetries
            var telemetriesParsed = dtdl.Values.Where(i => i.EntityKind == DTEntityKind.Telemetry);
            Assert.IsNotNull(telemetriesParsed);

            var single = telemetriesGenerated.SingleOrDefault();
            Assert.IsNotNull(single.Value);
            Assert.IsNotNull(single.Value.DTDLGeneratedData);

            var generatedContent = single.Value.DTDLGeneratedData.Telemetries;
            Assert.IsTrue(generatedContent.Count() == telemetriesParsed.Count());

            //Commands
            var commands2 = dtdl.Values.Where(i => i.EntityKind == DTEntityKind.Command);
            Assert.IsNotNull(commands2);
            Assert.IsTrue(commands.Count() == commands2.Count());

            var properties2 = dtdl.Values.Where(i => i.EntityKind == DTEntityKind.Property);
            Assert.IsNotNull(properties2);
            Assert.IsTrue(writableProperties.Count() + readableProperties.Count() == commands2.Count());
        }
        #endregion
    }
}
