using System.Collections.Concurrent;
using TestTask.FileLengthMonitoring.Contracts;

namespace TestTask.FileLengthMonitoring.Services;
public class FileLettersCalculator
{
    private readonly IProducer<string> _filePathProducer;
    private readonly string _outputDirPath;

    public FileLettersCalculator(IProducer<string> filePathProducer, string outputDir)
    {
        _filePathProducer = filePathProducer;
        _outputDirPath = outputDir;
    }

    public Task[] StartCalculating(short degreeOfParallelism, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_outputDirPath))
            Directory.CreateDirectory(_outputDirPath);

        var calculationTasks = new Task[degreeOfParallelism];
        for (int i = 0; i < degreeOfParallelism; i++)
        {
            calculationTasks[i] = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!_filePathProducer.TryDequeueProducedItem(out string filePathToBeHandled))
                        continue;
                    SaveResults(
                        Path.GetFileNameWithoutExtension(filePathToBeHandled) + ".txt",
                        CalculateLetters(filePathToBeHandled));
                }
                Console.WriteLine("Processing task has been Cancelled");
            });
        }
        return calculationTasks;
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

    private void SaveResults(string processedFileName, long lettersCount)
    {
        try
        {
            var outputFilePath = Path.Combine(_outputDirPath, processedFileName);
            File.WriteAllText(outputFilePath, lettersCount.ToString());
            Console.WriteLine($"File processing result saved in output dir: {outputFilePath}");
        }
        catch (IOException e)
        {
            Console.WriteLine($"IOException occurred while trying to process file {processedFileName}. Error message: {e.Message}");
            _filePathProducer.OnItemProcessingFailed(processedFileName);
        }
    }
}
