namespace MessageBox.Server
{
    public interface IBusServer
    {
        IQueue GetOrCreateQueue(Guid id);

        IExchange GetOrCreateExchange(string key);
    }
}
