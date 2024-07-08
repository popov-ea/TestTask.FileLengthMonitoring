using CommandLine;
namespace TestTask.FileLengthMonitoring.Models;
public class CommandLineOptions
{
    [Option('i', "input", Required = true, HelpText = "Directory of monitored files")]
    public string InputFilesDirPath { get; set; }

    [Option('o', "output", Required = true, HelpText = "Directory for results storing")]
    public string ResultsDirPath { get; set; }
}
