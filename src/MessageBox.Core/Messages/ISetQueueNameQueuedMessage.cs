namespace MessageBox.Messages;

public interface ISetQueueNameQueuedMessage : ISetQueueNameMessage
{
    Guid QueueId { get; }
}