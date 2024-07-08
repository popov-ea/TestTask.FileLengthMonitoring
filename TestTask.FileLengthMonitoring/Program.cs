﻿using CommandLine;
using TestTask.FileLengthMonitoring.Models;
using TestTask.FileLengthMonitoring.Services;

var parseResult = Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithNotParsed(_ =>
    {
        Environment.Exit(1);
    });

var dirInspector = new DirectoryInspector(parseResult.Value.InputFilesDirPath);
var lettersCalculator = new FileLettersCalculator(dirInspector, parseResult.Value.ResultsDirPath);
var cancellationTokenSource = new CancellationTokenSource();
dirInspector.StartInspecting(cancellationTokenSource.Token);
// one thread for inspecting, 3 threads for processing
var processingTasks = lettersCalculator.StartCalculating(3, cancellationTokenSource.Token);

Console.WriteLine("Press any key to stop processing");
Console.ReadKey();

cancellationTokenSource.Cancel();
Task.WaitAll(processingTasks.ToArray());