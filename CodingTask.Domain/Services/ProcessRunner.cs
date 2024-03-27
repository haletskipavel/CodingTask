using CodingTask.Domain.Abstraction;
using CodingTask.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CodingTask.Domain.Services
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly ILogger<ProcessRunner> _logger;

        public ProcessRunner(ILogger<ProcessRunner> logger)
        {
            _logger = logger;
        }

        public async Task<ResultMetric> RunConsoleAsync(
            string consoleFileName,
            string arguments,
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden,
            CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                var memoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", true);
                var resultMetric = new ResultMetric();

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = consoleFileName,
                    Arguments = arguments,
                    WindowStyle = windowStyle,
                    CreateNoWindow = false,
                    UseShellExecute = true
                });

                while (!process.HasExited)
                {
                    resultMetric.CpuLoad = cpuCounter.NextValue();
                    resultMetric.MemoryLoad = memoryCounter.NextValue();
                }

                resultMetric.IsSuccess = process.ExitCode == 0;

                if (!resultMetric.IsSuccess)
                {
                    _logger.LogError("Running console ended with an error");
                }

                return resultMetric;
            }, ct);
        }
    }
}
