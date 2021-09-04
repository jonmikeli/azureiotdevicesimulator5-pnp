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
        public async Task MT3620StarterKit_Simple_OK()
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
        public async Task MT3620StarterKit_DTDL_With_Semantics_And_Units_OK()
        {
            string dtdlModelPath = @"./Tests/Avnet/mt3620starterkit-1.json";
            string modelId = "dtmi:avnet:mt3620Starterkit;1";

            Assert.IsTrue(File.Exists(dtdlModelPath));

            var modelContainer = await DTDLHelper.GetModelsAndBuildDynamicContentAsync(modelId, dtdlModelPath);

            Assert.IsNotNull(modelContainer);

            var readablePropertiesGenerated = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.ReadableProperties != null);
            Assert.IsTrue(readablePropertiesGenerated.Any());

            var telemetriesGenerated = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Telemetries != null);
            Assert.IsTrue(telemetriesGenerated.Any());

            var commandsGenerated = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.Commands != null);
            Assert.IsTrue(commandsGenerated.Any());

            var writablePropertiesGenerated = modelContainer.Where(i => i.Value != null && i.Value.DTDLGeneratedData != null && i.Value.DTDLGeneratedData.WritableProperties != null);
            Assert.IsTrue(writablePropertiesGenerated.Any());

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

            var generatedTelemetriesContainer = telemetriesGenerated.SingleOrDefault();
            Assert.IsNotNull(generatedTelemetriesContainer.Value);
            Assert.IsNotNull(generatedTelemetriesContainer.Value.DTDLGeneratedData);

            var generatedTelemetriesContent = generatedTelemetriesContainer.Value.DTDLGeneratedData.Telemetries;
            Assert.IsNotNull(generatedTelemetriesContent);
            Assert.IsTrue(generatedTelemetriesContent.Count() == telemetriesParsed.Count());

            //Commands
            var commandsParsed = dtdl.Values.Where(i => i.EntityKind == DTEntityKind.Command);
            Assert.IsNotNull(commandsParsed);

            var generatedCommandContainer = commandsGenerated.SingleOrDefault();
            Assert.IsNotNull(generatedCommandContainer.Value);
            Assert.IsNotNull(generatedCommandContainer.Value.DTDLGeneratedData);

            var generatedCommandsContent = generatedCommandContainer.Value.DTDLGeneratedData.Commands;
            Assert.IsNotNull(generatedCommandsContent);
            Assert.IsTrue(generatedCommandsContent.Count() == commandsParsed.Count());

            //Properties
            var propertiesParsed = dtdl.Values.Where(i => i.EntityKind == DTEntityKind.Property);
            Assert.IsNotNull(propertiesParsed);

            var generatedWritablePropertiesContainer = writablePropertiesGenerated.SingleOrDefault();
            Assert.IsNotNull(generatedWritablePropertiesContainer.Value);
            Assert.IsNotNull(generatedWritablePropertiesContainer.Value.DTDLGeneratedData);
            var generatedWritableContent = generatedWritablePropertiesContainer.Value.DTDLGeneratedData.WritableProperties;

            int propertiesCounter = generatedWritableContent != null ? generatedWritableContent.Count : 0;

            var generatedReadablePropertiesContainer = readablePropertiesGenerated.SingleOrDefault();
            Assert.IsNotNull(generatedReadablePropertiesContainer.Value);
            Assert.IsNotNull(generatedReadablePropertiesContainer.Value.DTDLGeneratedData);
            var generatedReadableContent = generatedReadablePropertiesContainer.Value.DTDLGeneratedData.ReadableProperties;
            propertiesCounter = propertiesCounter + ((generatedReadableContent != null) ? generatedReadableContent.Count : 0);

            Assert.IsTrue(propertiesCounter == propertiesParsed.Count());
        }
        #endregion
    }
}
