
using MessageBox.Tcp;
using System.Net.Sockets;

namespace MessageBox.Server.Implementation
{
    internal class ConnectionFromClient : TcpConnection
    {
        private class ConnectionFromClientForBoxWrapper : IMessageSink
        {
            private readonly IQueue _queue;
            private readonly IMessageSink _originalMessageSink;

            public ConnectionFromClientForBoxWrapper(IQueue queue, IMessageSink originalMessageSink)
            {
                _queue = queue;
                _originalMessageSink = originalMessageSink;
            }

            public Task OnReceivedMessage(Message message, CancellationToken cancellationToken = default) 
                => _originalMessageSink.OnReceivedMessage(message.ReplyToBoxId != null ? message : message with { ReplyToBoxId = _queue.Id }, cancellationToken);
        }

        private readonly IQueue _queue;
        private readonly Action<Guid> _actionWhenSocketConnectionFails;

        public ConnectionFromClient(IQueue queue, Action<Guid> actionWhenSocketConnectionFails)
        {
            _queue = queue;
            _actionWhenSocketConnectionFails = actionWhenSocketConnectionFails;
        }

        protected override Task RunConnectionLoop(Socket connectedSocket, IMessageSink messageSink, IMessageSource messageSource, CancellationToken cancellationToken) 
            => base.RunConnectionLoop(connectedSocket, new ConnectionFromClientForBoxWrapper(_queue, messageSink), messageSource, cancellationToken);

        protected override void OnConnectionLoopEnded() => 
            _actionWhenSocketConnectionFails.Invoke(_queue.Id);
    }
}