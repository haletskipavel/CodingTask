using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using CodingTask.Domain.Abstraction;
using CodingTask.Domain.Extensions;
using CodingTask.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CodingTask.Domain.Services
{
    public class ResourceManager : IResourceManager
    {
        private readonly IProcessRunner _processRunner;
        private readonly ILogger<ResourceManager> _logger;

        public ResourceManager(IProcessRunner processRunner, ILogger<ResourceManager> logger)
        {
            _processRunner = processRunner;
            _logger = logger;
        }

        public async Task RunResourceManagerAsync(string jsonFileName, string consoleAppFileName, int maxGlobalThreadsCount, int runtimeTimeout)
        {
            ArgumentNullException.ThrowIfNull(jsonFileName, nameof(jsonFileName));
            ArgumentNullException.ThrowIfNull(consoleAppFileName, nameof(consoleAppFileName));

            try
            {
                maxGlobalThreadsCount = Math.Min(maxGlobalThreadsCount, Environment.ProcessorCount);
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(runtimeTimeout));
                var projects = await LoadProjectsFromJsonAsync(jsonFileName, cancellationTokenSource.Token);
                await ProcessProjectsAsync(projects, consoleAppFileName, maxGlobalThreadsCount, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the ResourceManager");
            }
        }

        private async Task<List<ResultMetric>> ProcessProjectsAsync(
            IEnumerable<Project> projects,
            string consoleAppFileName,
            int maxGlobalThreadsCount,
            CancellationToken cancellationToken = default)
        {
            var results = new List<ResultMetric>();
            var totalTimeSpent = 0L;

            foreach (var project in projects)
            {
                _logger.LogInformation("Processing project with MemoryCount: {MemoryCount}, AppTimeout: {AppTimeout}, TryCount: {TryCount}, MaxThreadsCount: {MaxThreads}",
                    project.MemoryCount,
                    project.AppTimeout,
                    project.TryCount,
                    project.MaxThreads);

                var (projectResults, timeSpentMilliseconds) = await ExecuteProjectAsync(project, consoleAppFileName, maxGlobalThreadsCount, cancellationToken);
                results.AddRange(projectResults);
                totalTimeSpent += timeSpentMilliseconds;
            }

            _logger.LogInformation("Results Summary:");
            _logger.LogInformation($"{"Total Time Spent (ms):",-25} {totalTimeSpent:N0}");
            _logger.LogInformation($"{"Average CPU Load (%):",-25} {results.Average(x => x.CpuLoad):F2}");
            _logger.LogInformation($"{"Average Memory Load (%):",-25} {results.Average(x => x.MemoryLoad):F2}");
            return results;
        }

        private async Task<(IEnumerable<ResultMetric> Results, long TimeSpentMilliseconds)> ExecuteProjectAsync(Project project, string processFileName, int maxGlobalThreadsCount, CancellationToken cancellationToken = default)
        {
            var projectStopwatch = Stopwatch.StartNew();

            var projectResults = await SplitProjectByTryCountAsync(project, processFileName, maxGlobalThreadsCount, cancellationToken);
            
            projectStopwatch.Stop();

            return (projectResults, projectStopwatch.ElapsedMilliseconds);
        }

        private async Task<IEnumerable<ResultMetric>> SplitProjectByTryCountAsync(Project project, string processFileName, int maxGlobalThreadsCount, CancellationToken cancellationToken = default)
        {
            var projectsToRun = Enumerable.Repeat(project, project.TryCount);
            var batchSize = Math.Min(project.MaxThreads, maxGlobalThreadsCount);

            var projectMetrics = new List<ResultMetric>();
            
            foreach (var projectBatch in projectsToRun.Batch(batchSize))
            {
                var batchTasks = projectBatch.Select(x => _processRunner.RunConsoleAsync(processFileName, $"{x.AppTimeout} {x.MemoryCount}", ct: cancellationToken));

                projectMetrics.AddRange(await Task.WhenAll(batchTasks));
            }

            return projectMetrics;
        }

        private static async Task<List<Project>> LoadProjectsFromJsonAsync(string jsonFileName, CancellationToken ct = default)
        {
            var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFileName);
            using var stream = File.OpenRead(jsonFilePath);
            var root = await JsonSerializer.DeserializeAsync<Root>(stream, cancellationToken: ct);

            return root?.Projects ?? new List<Project>();
        }
    }
}