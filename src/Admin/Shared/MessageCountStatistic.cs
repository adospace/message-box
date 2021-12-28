namespace MessageBox.Server.Tcp.Host.Shared
{
    public record QueueMessageCountStatistic(Guid Id, DateTime TimeStamp, int Value);
    
    public record ExchangeMessageCountStatistic(string Key, DateTime TimeStamp, int Value);
    
    public record ServerMessageCountStatistic(QueueMessageCountStatistic[] QueueMessageCountStatistics, ExchangeMessageCountStatistic[] ExchangeMessageCountStatistics);
}