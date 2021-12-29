namespace MessageBox.Server
{
    public interface IBusServer
    {
        IQueue? GetQueue(Guid id);
        
        IQueue CreateQueue(Guid id, string key);

        IExchange GetOrCreateExchange(string key);
    }
}
