using CodingTask.Benchmark;
using CodingTask.Domain.Abstraction;
using CodingTask.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices(x =>
{
    x.AddScoped<IProcessRunner, ProcessRunner>();
    x.AddScoped<IResourceManager, ResourceManager>();
});

var host = builder.Build();

var lifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
var resourceManager = host.Services.GetRequiredService<IResourceManager>();
var configuration = host.Services.GetService<IConfiguration>();

await host.StartAsync();
var benchmarkConfig = configuration!.GetSection("BenchmarkConfig")
    .Get<BenchmarkConfig>();

await resourceManager.RunResourceManagerAsync(
    benchmarkConfig!.ProjectsFileName,
    benchmarkConfig.ConsoleAppFileName,
    benchmarkConfig.GlobalThreadsCount,
    benchmarkConfig.TimeoutMinutes);

await host.WaitForShutdownAsync();

