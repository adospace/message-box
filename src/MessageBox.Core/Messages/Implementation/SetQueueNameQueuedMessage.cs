namespace MessageBox.Messages.Implementation;

internal class SetQueueNameQueuedMessage : SetQueueNameMessage, ISetQueueNameQueuedMessage
{
    public SetQueueNameQueuedMessage(string setQueueName, Guid queueId)
        :base(setQueueName)
    {
        QueueId = queueId;
    }

    public Guid QueueId { get; }
}