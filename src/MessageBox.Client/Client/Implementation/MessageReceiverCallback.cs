
using MessageBox.Messages;

namespace MessageBox.Client.Implementation
{
    internal interface IMessageReceiverCallback
    {
        Type ModelType { get; }

        bool HasReturnType { get; }

        Task<object?> Call(IMessage message, object model, CancellationToken cancellationToken = default);
    }

    internal class MessageReceiverCallbackWithoutReturnValue : IMessageReceiverCallback
    {
        private readonly Func<IMessage, object, CancellationToken, Task> _callback;

        public MessageReceiverCallbackWithoutReturnValue(Type modelType, Func<IMessage, object, CancellationToken, Task> callback)
        {
            ModelType = modelType;
            _callback = callback;
        }

        public bool HasReturnType => false;

        public Type ModelType { get; }

        public async Task<object?> Call(IMessage message, object model, CancellationToken cancellationToken)
        {
            await _callback(message, model, cancellationToken);

            return null;
        }
    }

    internal class MessageReceiverCallbackWithReturnValue : IMessageReceiverCallback
    {
        private readonly Func<IMessage, object, CancellationToken, Task<object?>> _callback;

        public MessageReceiverCallbackWithReturnValue(Type modelType, Func<IMessage, object, CancellationToken, Task<object?>> callback)
        {
            ModelType = modelType;
            _callback = callback;
        }

        public bool HasReturnType => true;

        public Type ModelType { get; }

        public async Task<object?> Call(IMessage message, object model, CancellationToken cancellationToken)
        {
            return await _callback(message, model, cancellationToken);
        }
    }
}