// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using StreamSentinel.Pipeline;
using StreamSentinel.Pipeline.Settings;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("settings.json", true, true)
    .Build();

Console.WriteLine($"Analysis begin...");

var _pipeLineSettings = config.GetSection("Pipeline").Get<PipelineSettings>();
Console.WriteLine($"  Video uri:{_pipeLineSettings.Uri}");

using var pipeline = new AnalysisPipeline(config);

pipeline.Run();
