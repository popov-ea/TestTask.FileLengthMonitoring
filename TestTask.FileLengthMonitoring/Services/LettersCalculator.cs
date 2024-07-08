using System.Collections.Concurrent;
using TestTask.FileLengthMonitoring.Contracts;

namespace TestTask.FileLengthMonitoring.Services;
public class LettersCalculator
{
    private readonly IProducer<string> _filePathProducer;
    private readonly string _outputDir;

    public LettersCalculator(IProducer<string> filePathProducer, string outputDir)
    {
        _filePathProducer = filePathProducer;
        _outputDir = outputDir;
    }

    public Task[] StartCalculatingProcess(short degreeOfParallelism, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_outputDir))
            Directory.CreateDirectory(_outputDir);

        var tasks = new Task[degreeOfParallelism];
        for (int i = 0; i < degreeOfParallelism; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!_filePathProducer.TryDequeueProducedItem(out string fileToBeHandled))
                        continue;

                    try
                    {
                        var outputFilePath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(fileToBeHandled) + ".txt");
                        File.WriteAllText(
                            outputFilePath, 
                            CalculateLetters(fileToBeHandled).ToString());
                        Console.WriteLine($"File processing result saved in output dir: {outputFilePath}");
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"IOException occurred while trying to process file {fileToBeHandled}. Error message: {e.Message}");
                        _filePathProducer.OnItemProcessingFailed(fileToBeHandled);
                    }
                }
                Console.WriteLine("Processing task has been Cancelled");
            });
        }
        return tasks;
    }

    private long CalculateLetters(string filePath)
    {
        if (!File.Exists(filePath)) 
            throw new ApplicationException($"File {filePath} not found");

        var fileName = Path.GetFileName(filePath);
        Console.WriteLine(
            $"Started file processing. File path: {fileName}. " +
            $"Thread: {Thread.CurrentThread.ManagedThreadId}");

        using var readStream = new FileStream(filePath, FileMode.Open);
        using var streamReader = new StreamReader(readStream);
        var buffer = new char[1000];
        long lettersCount = 0;
        int lastReadCharsCount = 0;
        while((lastReadCharsCount = streamReader.Read(buffer, 0, buffer.Length)) > 0)
        {
            lettersCount += buffer.Take(lastReadCharsCount).Count(char.IsLetter);
        }

        Console.WriteLine($"File processing finished. File path: {fileName}");

        return lettersCount;
    }
}
