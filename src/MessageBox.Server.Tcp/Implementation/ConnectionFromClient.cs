
using MessageBox.Tcp;
using System.Net.Sockets;

namespace MessageBox.Server.Tcp.Implementation
{
    internal class ConnectionFromClient : TcpConnection
    {
        private class ConnectionFromClientForBoxWrapper : IMessageSink
        {
            private readonly IBox _box;
            private readonly IMessageSink _originalMessageSink;

            public ConnectionFromClientForBoxWrapper(IBox box, IMessageSink originalMessageSink)
            {
                _box = box;
                _originalMessageSink = originalMessageSink;
            }

            public Task OnReceivedMessage(Message message, CancellationToken cancellationToken = default) 
                => _originalMessageSink.OnReceivedMessage(message.ReplyToBoxId != null ? message : message with { ReplyToBoxId = _box.Id }, cancellationToken);
        }

        private readonly IBox _box;
        private readonly Action<Guid> _actionWhenSocketConnectionFails;

        public ConnectionFromClient(IBox box, Action<Guid> actionWhenSocketConnectionFails)
        {
            _box = box;
            _actionWhenSocketConnectionFails = actionWhenSocketConnectionFails;
        }

        protected override Task RunConnectionLoop(Socket connectedSocket, IMessageSink messageSink, IMessageSource messageSource, CancellationToken cancellationToken) 
            => base.RunConnectionLoop(connectedSocket, new ConnectionFromClientForBoxWrapper(_box, messageSink), messageSource, cancellationToken);

        protected override void OnConnectionLoopEnded() => 
            _actionWhenSocketConnectionFails.Invoke(_box.Id);
    }
}