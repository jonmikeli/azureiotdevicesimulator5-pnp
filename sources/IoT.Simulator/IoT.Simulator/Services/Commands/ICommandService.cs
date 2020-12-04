using Newtonsoft.Json.Linq;

using System.Threading.Tasks;

namespace IoT.Simulator.Services
{
    public interface ICommandService
    {
        Task<JArray> GetCommandsAsync(string modelId, string modelPath);
    }
}
