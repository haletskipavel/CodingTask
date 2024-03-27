namespace CodingTask.Benchmark
{
    internal class BenchmarkConfig
    {
        public string ProjectsFileName { get; set; } = default!;
        public string ConsoleAppFileName { get; set; } = default!;
        public int GlobalThreadsCount { get; set; }
        public int TimeoutMinutes { get; set; } = 20;
    }
}
