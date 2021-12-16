using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MessageBox.Client.Implementation
{
    internal class Bus : IBus, IMessageSink, IMessageSource, IBusClient
    {
        private class RpcCall
        {
            public RpcCall(Message callMessage)
            {
                CallMessage = callMessage;
                WaitReplyEvent = new AsyncAutoResetEvent();
            }

            public Message CallMessage { get; }
            
            public Guid Id => CallMessage.Id;

            public AsyncAutoResetEvent WaitReplyEvent { get; }

            public Message? ReplyMessage { get; set; }
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IBusClientOptions _options;
        private readonly IMessageSerializerFactory _messageSerializerFactory;
        private readonly ITransport _transport;

        private readonly Channel<Message> _outgoingMessages = Channel.CreateUnbounded<Message>();

        private readonly ConcurrentDictionary<Guid, RpcCall> _waitingCalls = new();
        private readonly ConcurrentDictionary<Type, IMessageReceiverCallback> _receiveActionForMessageType = new();
        

        private readonly ActionBlock<Message> _innomingMessages;

        public Bus(
            IServiceProvider serviceProvider,
            IBusClientOptions options)
        {
            _transport = serviceProvider.GetRequiredService<ITransportFactory>().Create();
            _messageSerializerFactory = serviceProvider.GetRequiredService<IMessageSerializerFactory>();
            _serviceProvider = serviceProvider;
            _options = options;
            _innomingMessages = new ActionBlock<Message>(ProcessIncomingMessage);
        }

        private async Task ProcessIncomingMessage(Message message)
        {
            if (message.ReplyToId != null)
            {
                if (_waitingCalls.TryGetValue(message.ReplyToId.Value, out var replyHandler))
                {
                    replyHandler.ReplyMessage = message;
                    replyHandler.WaitReplyEvent.Set();
                }
            }
            else
            {
                var serializer = _messageSerializerFactory.CreateMessageSerializer();
                var typeOfTheModel = Type.GetType(message.PayloadType ?? throw new InvalidOperationException()) ?? throw new InvalidOperationException($"Unable to find type {message.PayloadType}");

                if (!_receiveActionForMessageType.TryGetValue(typeOfTheModel, out var actionToCallOnReceiver))
                {
                    return;
                }

                var deserializedModel = serializer.Deserialize(message.Payload ?? throw new InvalidOperationException(), typeOfTheModel);

                object? returnValue;

                try
                {
                    returnValue = await actionToCallOnReceiver.Call(message, deserializedModel);
                }
                catch (TargetInvocationException ex)
                {
                    returnValue = new MessageBoxCallException($"Exception raised when calling handler for model type '{typeOfTheModel}:{Environment.NewLine}{ex.InnerException}");
                }                

                if (message.RequireReply)
                { 
                    byte[]? replyPayload = null;
                    if (returnValue != null)
                    {
                        replyPayload = serializer.Serialize(returnValue);
                    }

                    await Post(new Message(
                        Id: Guid.NewGuid(),
                        ReplyToId: message.Id,
                        ReplyToBoxId: message.ReplyToBoxId,
                        CorrelationId: message.CorrelationId,
                        PayloadType: returnValue?.GetType()?.AssemblyQualifiedName,
                        Payload: replyPayload
                    ));
                }
            }

            message.MessageMemoryOwner?.Dispose();
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            foreach (var receiverCallback in _serviceProvider.GetServices<IMessageReceiverCallback>())
            {
                _receiveActionForMessageType[receiverCallback.ModelType] = receiverCallback;

                //subscribe to the board
                await Post(new Message(
                    Id: Guid.NewGuid(),
                    BoardKey: receiverCallback.ModelType.FullName), 
                    cancellationToken);
            }

            await _transport.Run(cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            await _transport.Stop(cancellationToken);
        }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken = default)
        {
            await _innomingMessages.SendAsync(message, cancellationToken);
        }

        internal async Task Post(Message message, CancellationToken cancellationToken = default)
        {
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task<Message> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            return await _outgoingMessages.Reader.ReadAsync(cancellationToken);
        }

        public async Task Publish<T>(T model, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());

            await Post(new Message(
                Id: Guid.NewGuid(),
                BoardKey: model.GetType().FullName,
                CorrelationId: Guid.NewGuid(),
                IsEvent: true,
                Payload: modelSerialized,
                PayloadType: typeof(T).AssemblyQualifiedName
            ));
        }

        public async Task Send<T>(T model, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());

            var message = new Message(
                Id: Guid.NewGuid(),
                BoardKey: model.GetType().FullName,
                CorrelationId: Guid.NewGuid(),
                Payload: modelSerialized,
                PayloadType: typeof(T).AssemblyQualifiedName,
                RequireReply: true
            );

            var call = new RpcCall(message);

            _waitingCalls[message.Id] = call;
            
            await Post(message);

            await call.WaitReplyEvent.WaitAsync(cancellationToken);

            _waitingCalls.TryRemove(message.Id, out var _);

            if (call.ReplyMessage != null && call.ReplyMessage.Payload != null)
            {
                var replyMessage = call.ReplyMessage;
                var replyMessagePayload = replyMessage.Payload ?? throw new InvalidOperationException();
                var replyMessagePayloadType = Type.GetType(replyMessage.PayloadType ?? throw new InvalidOperationException()) ?? throw new InvalidOperationException();

                var deserializedReplyModel = serializer.Deserialize(
                    replyMessagePayload,
                    replyMessagePayloadType);

                if (deserializedReplyModel is MessageBoxCallException exception)
                {
                    throw exception;
                }
            }
        }

        public async Task<R> SendAndGetReply<R>(object model, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());

            var message = new Message(
                Id:Guid.NewGuid(),
                BoardKey: model.GetType().FullName,
                CorrelationId: Guid.NewGuid(),
                Payload: modelSerialized,
                PayloadType: model.GetType().AssemblyQualifiedName,
                RequireReply: true
            );

            var call = new RpcCall(message);

            _waitingCalls[message.Id] = call;

            await Post(message);

            await call.WaitReplyEvent.WaitAsync(cancellationToken);

            var replyMessage = call.ReplyMessage ?? throw new InvalidOperationException();
            var replyMessagePayload = replyMessage.Payload ?? throw new InvalidOperationException();
            var replyMessagePayloadType = Type.GetType(replyMessage.PayloadType ?? throw new InvalidOperationException()) ?? throw new InvalidOperationException();

            var deserializedReplyModel = serializer.Deserialize(
                replyMessagePayload,
                replyMessagePayloadType);

            _waitingCalls.Remove(message.Id, out var _);

            return (R)deserializedReplyModel;
        }
    }
}
