using System.Collections.Concurrent;

namespace TestTask.FileLengthMonitoring.Contracts;
public interface IProducer<TProducedItem>
{
    bool TryDequeueProducedItem(out TProducedItem item);
    void OnItemProcessingFailed(TProducedItem producedItem);
}
