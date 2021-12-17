namespace MessageBox
{
    public interface IHandler
    { }

    public interface IHandler<in T> : IHandler where T : class
    {
        Task Handle(IMessageContext<T> messageContext, CancellationToken cancellationToken = default);
    }

    public interface IHandler<in T, TRType> : IHandler where T : class
    {
        Task<TRType> Handle(IMessageContext<T> messageContext, CancellationToken cancellationToken = default);
    }
}
