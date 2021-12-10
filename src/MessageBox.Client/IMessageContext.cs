namespace MessageBox.Client
{
    public interface IMessageContext<T>
    { 
        T Model { get; }

        Task Reply<R>(R replyModel, CancellationToken cancellationToken = default);
    }


}
