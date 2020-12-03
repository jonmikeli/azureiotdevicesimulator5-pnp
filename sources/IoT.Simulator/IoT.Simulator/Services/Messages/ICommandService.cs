using System.Threading.Tasks;

namespace IoT.Simulator.Services
{
    public interface ICommandService
    {
        Task<string> GetCommandsAsync(string modelId, string modelPath);

        Task<string> GetCommandsAsync(string deviceId, string moduleId, string modelId, string modelPath);

        Task<string> GetRandomizedCommandPayloadsAsync(string deviceId, string moduleId, string modelId, string modelPath);
    }
}
