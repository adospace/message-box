namespace MessageBox.Server.Tcp.Host.Shared
{
    public interface IMessageCountStatistics
    {
        string Key { get; }

        string Label { get; }

        int TotalCount { get; }

        int CurrentCount { get; }
    }

    public record QueueMessageCountStatistic : IMessageCountStatistics
    {
        public QueueMessageCountStatistic(Guid id, string name, DateTime timeStamp, int totalCount, int currentCount)
        {
            Id = id;
            Name = name;
            TimeStamp = timeStamp;
            TotalCount = totalCount;
            CurrentCount = currentCount;
        }

        public Guid Id { get; }
        public string Name { get; }
        public DateTime TimeStamp { get; }
        public string Key => Id.ToString();
        public string Label => Name;
        public int TotalCount { get; }
        public int CurrentCount { get; }

        public void Deconstruct(out Guid id, out string name, out DateTime timeStamp, out int totalCount, out int currentCount)
        {
            id = Id;
            name = Name;
            timeStamp = TimeStamp;
            totalCount = TotalCount;
            currentCount = CurrentCount;
        }
    }

    public record ExchangeMessageCountStatistic : IMessageCountStatistics
    {
        public ExchangeMessageCountStatistic(string key, DateTime timeStamp, int totalCount, int currentCount)
        {
            this.Key = key;
            this.TimeStamp = timeStamp;
            this.TotalCount = totalCount;
            this.CurrentCount = currentCount;
        }

        public string Key { get; }
        public string Label => Key;
        public DateTime TimeStamp { get; }
        public int TotalCount { get; }
        public int CurrentCount { get; }

        public void Deconstruct(out string key, out DateTime timeStamp, out int totalCount, out int currentCount)
        {
            key = Key;
            timeStamp = TimeStamp;
            totalCount = TotalCount;
            currentCount = CurrentCount;
        }
    }

    public record ServerMessageCountStatistic(QueueMessageCountStatistic[] QueueMessageCountStatistics, ExchangeMessageCountStatistic[] ExchangeMessageCountStatistics);
}