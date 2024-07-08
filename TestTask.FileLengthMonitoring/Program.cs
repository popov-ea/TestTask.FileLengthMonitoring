using CommandLine;
using TestTask.FileLengthMonitoring.Models;
using TestTask.FileLengthMonitoring.Services;

var parseResult = Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithNotParsed(_ =>
    {
        Environment.Exit(1);
    });

var dirInspector = new DirectoryInspector(parseResult.Value.InputFilesDirPath);
var lettersCalculator = new LettersCalculator(dirInspector, parseResult.Value.ResultsDirPath);
var cancellationTokenSource = new CancellationTokenSource();
dirInspector.StartMonitoring(cancellationTokenSource.Token);
// one thread for monitoring, 3 threads for processing
var processingTasks = lettersCalculator.StartCalculatingProcess(3, cancellationTokenSource.Token);

Console.WriteLine("Press any key to stop processing");
Console.ReadKey();

cancellationTokenSource.Cancel();
Task.WaitAll(processingTasks.ToArray());