using System.Collections.Concurrent;
using TestTask.FileLengthMonitoring.Contracts;

namespace TestTask.FileLengthMonitoring.Services;
public class DirectoryInspector : IProducer<string>
{
    private bool _isMonitoringStarted = false;
    private readonly string _monitoredDirPath;
    private readonly HashSet<string> _enqueuedFiles = new HashSet<string>();
    private readonly ConcurrentQueue<string> _filePathQueue = new ConcurrentQueue<string>();
    public DirectoryInspector(string monitoredDirPath)
    {
        _monitoredDirPath = monitoredDirPath ?? throw new ArgumentNullException(nameof(monitoredDirPath));
    }

    public void StartInspecting(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_monitoredDirPath))
            throw new ApplicationException($"Input directory does not exist: {_monitoredDirPath}");
        if (_isMonitoringStarted)
            throw new ApplicationException("File monitoring is already started");

        Console.WriteLine("Starting files monitoring");
        Task.Run(() => 
        {
            Console.WriteLine($"Monitoring thread id: {Thread.CurrentThread.ManagedThreadId}");
            DirectoryInfo dirInfo = new DirectoryInfo(_monitoredDirPath);
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach(var file in dirInfo.GetFiles())
                {
                    if (!_enqueuedFiles.Contains(file.FullName) && IsFileReadable(file.FullName))
                    {
                        Console.WriteLine($"Found new file {file.FullName}");
                        _filePathQueue.Enqueue(file.FullName);
                        _enqueuedFiles.Add(file.FullName);
                    }
                }
            }
            _isMonitoringStarted = false;
            Console.WriteLine("Monitoring task has been cancelled");
        });
        _isMonitoringStarted = true;
        Console.WriteLine("Files monitoring has been started");
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
