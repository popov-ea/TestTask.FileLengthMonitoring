using System.Collections.Concurrent;
using TestTask.FileLengthMonitoring.Contracts;

namespace TestTask.FileLengthMonitoring.Services;
public class DirectoryInspector : IProducer<string>
{
    private bool _isInspectionStarted = false;
    private readonly string _inspectedDirPath;
    private readonly HashSet<string> _enqueuedFiles = new HashSet<string>();
    private readonly ConcurrentQueue<string> _filePathQueue = new ConcurrentQueue<string>();
    public DirectoryInspector(string inspectedDirPath)
    {
        _inspectedDirPath = inspectedDirPath ?? throw new ArgumentNullException(nameof(inspectedDirPath));
    }

    public void StartInspecting(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_inspectedDirPath))
            throw new ApplicationException($"Input directory does not exist: {_inspectedDirPath}");
        if (_isInspectionStarted)
            throw new ApplicationException("Directory inspection is already started");

        Console.WriteLine($"Starting inspection of directory: {_inspectedDirPath}");
        Task.Run(() => 
        {
            Console.WriteLine($"Directory inspection thread id: {Thread.CurrentThread.ManagedThreadId}");
            DirectoryInfo dirInfo = new DirectoryInfo(_inspectedDirPath);
            while (!cancellationToken.IsCancellationRequested)
            {
                InspectDirectory(dirInfo);
            }
            _isInspectionStarted = false;
            Console.WriteLine("Inspection task has been cancelled");
        });
        _isInspectionStarted = true;
        Console.WriteLine("Directory inspection has been started");
    }

    private void InspectDirectory(DirectoryInfo dirInfo)
    {
        foreach (var file in dirInfo.GetFiles())
        {
            if (!_enqueuedFiles.Contains(file.FullName) && IsFileReadable(file.FullName))
            {
                Console.WriteLine($"Found new file {file.FullName}");
                _filePathQueue.Enqueue(file.FullName);
                _enqueuedFiles.Add(file.FullName);
            }
        }
    }

    public bool TryDequeueProducedItem(out string item)
    {
        return _filePathQueue.TryDequeue(out item);
    }

    public void OnItemProcessingFailed(string producedItem)
    {
        _filePathQueue.Enqueue(producedItem);
    }

    private bool IsFileReadable(string path)
    {
        try
        {
            using (FileStream fs = File.OpenRead(path))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
