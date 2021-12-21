﻿
using MessageBox.Messages;
using MessageBox.Tcp;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace MessageBox.Server.Implementation
{
    internal class ConnectionFromClient : TcpConnection
    {
        private class ConnectionFromClientForBoxWrapper : IMessageSink
        {
            private readonly IQueue _queue;
            private readonly IMessageSink _originalMessageSink;
            private readonly IMessageFactory _messageFactory;

            public ConnectionFromClientForBoxWrapper(IQueue queue, IMessageSink originalMessageSink, IMessageFactory messageFactory)
            {
                _queue = queue;
                _originalMessageSink = originalMessageSink;
                _messageFactory = messageFactory;
            }

            public Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken = default)
            {
                if (message is ICallMessage callMessage)
                {
                    return _originalMessageSink.OnReceivedMessage(_messageFactory.CreateCallQueuedMessage(callMessage, _queue.Id), cancellationToken);
                }
                else if (message is ISubscribeMessage subscribeToExchangeMessage)
                {
                    return _originalMessageSink.OnReceivedMessage(_messageFactory.CreateSubsribeQueuedMessage(subscribeToExchangeMessage, _queue.Id), cancellationToken);
                }

                return _originalMessageSink.OnReceivedMessage(message, cancellationToken);
            }
        }

        private readonly IQueue _queue;
        private readonly Action<Guid> _actionWhenSocketConnectionFails;

        public ConnectionFromClient(
            IServiceProvider serviceProvider,
            IQueue queue, 
            Action<Guid> actionWhenSocketConnectionFails)
            :base(serviceProvider)
        {
            _queue = queue;
            _actionWhenSocketConnectionFails = actionWhenSocketConnectionFails;
        }

        protected override Task RunConnectionLoop(
            Socket connectedSocket,
            IMessageSource messageSource,
            IMessageSink messageSink,
            CancellationToken cancellationToken = default) 
            => base.RunConnectionLoop(
                connectedSocket: connectedSocket, 
                messageSource: messageSource,
                messageSink: new ConnectionFromClientForBoxWrapper(_queue, messageSink, _serviceProvider.GetRequiredService<IMessageFactory>()), 
                cancellationToken);

        protected override void OnConnectionLoopEnded() => 
            _actionWhenSocketConnectionFails.Invoke(_queue.Id);
    }
}