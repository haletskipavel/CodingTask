using CodingTask.Domain.Models;
using System.Diagnostics;

namespace CodingTask.Domain.Abstraction
{
    public interface IProcessRunner
    {
        Task<ResultMetric> RunConsoleAsync(
            string consoleFileName,
            string arguments,
            ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden,
            CancellationToken ct = default);
    }
}
