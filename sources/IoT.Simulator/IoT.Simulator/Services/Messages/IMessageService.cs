using System.Threading.Tasks;

namespace IoT.Simulator.Services
{
    public interface IMessageService
    {
        Task<string> GetMessageAsync(string modelId, string modelPath);

        Task<string> GetMessageAsync(string deviceId, string moduleId, string modelId, string modelPath);

        Task<string> GetRandomizedMessageAsync(string deviceId, string moduleId, string modelId, string modelPath);
    }
}
