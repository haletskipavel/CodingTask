namespace CodingTask.Domain.Abstraction
{
    public interface IResourceManager
    {
        Task RunResourceManagerAsync(
            string jsonFileName,
            string consoleAppFileName,
            int maxGlobalThreadsCount,
            int runtimeTimeout);
    }
}
