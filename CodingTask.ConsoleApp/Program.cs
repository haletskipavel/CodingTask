try
{
    var timeout = args[0];
    ArgumentNullException.ThrowIfNull(timeout, nameof(timeout));

    var memoryCount = args[1];
    ArgumentNullException.ThrowIfNull(memoryCount, nameof(memoryCount));

    Console.WriteLine("Runing console app with {0} timeout and {1} memoryCount",
        timeout,
        memoryCount);

    Run(double.Parse(timeout), int.Parse(memoryCount));

    Console.WriteLine("Exit from console app...");

    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine("Exit from console app with error state...");
    Environment.Exit(-1);
}

static byte[] Run(double timeout, int memoryCount)
{
    using var ct = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout));
    var memory = new byte[memoryCount * 1024 * 1024];
    var rnd = new Random();
    
    while (!ct.IsCancellationRequested)
    {
        memory[rnd.Next(0, memory.Length - 1)]++;
    }

    return memory;
}