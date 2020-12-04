using IoT.DTDL;

using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoT.Simulator.Services
{
    public interface ICommandService
    {
        Task<Dictionary<string, DTDLCommandContainer>> GetCommandsAsync(string modelId, string modelPath);
    }
}
