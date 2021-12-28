namespace MessageBox.Messages;

public interface IKeepAliveMessage : IControlMessage
{
    Guid QueueId { get; }
}