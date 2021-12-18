namespace MessageBox.Messages.Implementation
{
    internal class SetQueueNameMessage : ISetQueueNameMessage
    {
        public SetQueueNameMessage(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; }
    }
}