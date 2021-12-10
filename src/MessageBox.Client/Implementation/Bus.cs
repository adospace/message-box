using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MessageBox.Client.Implementation
{
    internal class Bus : IBus, IMessageSink, IMessageSource, IBusClient
    {
        private readonly ITransport _transport;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IHandler> _allRegisteredHandlers;
        private readonly IMessageSerializerFactory _messageSerializerFactory;

        private readonly ConcurrentDictionary<Guid, ITransport> _transports = new();

        private readonly Channel<Message> _outgoingMessages = Channel.CreateUnbounded<Message>();
        
        private readonly ConcurrentDictionary<Guid, (AsyncAutoResetEvent Event, Message Message)> _waitingEvents = new();

        private readonly ActionBlock<Message> _innomingMessages;

        public Bus(ITransportFactory transportFactory, IMessageSerializerFactory messageSerializerFactory, IServiceProvider serviceProvider, IEnumerable<IHandler> allHandlers)
        {
            _transport = transportFactory.Create(this, this);
            _messageSerializerFactory = messageSerializerFactory;
            _serviceProvider = serviceProvider;
            _allRegisteredHandlers = allHandlers;
            _innomingMessages = new ActionBlock<Message>(ProcessIncomingMessage);
        }

        private async Task ProcessIncomingMessage(Message message)
        {
            if (message.ReplyToId != null)
            {
                if (_waitingEvents.TryGetValue(message.ReplyToId.Value, out var replyHandler))
                {
                    replyHandler.Message = message;
                    replyHandler.Event.Set();
                }
            }
            else
            {
                var serializer = _messageSerializerFactory.CreateMessageSerializer();
                var deserializedModel = serializer.Deserialize(message.Payload ?? throw new InvalidOperationException());
                var modelType = typeof(IHandler<>).MakeGenericType(deserializedModel.GetType());

                var messageContextType = typeof(MessageContext<>).MakeGenericType(deserializedModel.GetType());

                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService(modelType);

                await CallHandlerHandleFunc(handler, Activator.CreateInstance(messageContextType) ?? throw new InvalidOperationException());
            }
        }

        private static async Task CallHandlerHandleFunc(object handler, object messageContext)
        {
            var method = typeof(IHandler<>).GetMethod("Handle") ?? throw new InvalidOperationException();
            var task = (Task)method.Invoke(handler, new[] { messageContext })!;

            await task.ConfigureAwait(false);
        }

        public void Start()
        {
            foreach (var registeredHandler in _allRegisteredHandlers)
            { 

            }

            _transport.Start();
        }

        public void Stop()
        {
            _transport.Stop();
        }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken = default)
        {
            await _innomingMessages.SendAsync(message, cancellationToken);
        }

        internal async Task Post(Message message, CancellationToken cancellationToken = default)
        {
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        internal async Task Reply<R>(Message sourceMessage, R replyModel, CancellationToken cancellationToken)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var replyModelSerialized = serializer.Serialize(replyModel ?? throw new InvalidOperationException());

            await Post(new Message
            {
                Id = Guid.NewGuid(),
                ReplyToId = sourceMessage.Id,
                CorrelationId = sourceMessage.CorrelationId,
                Payload = replyModelSerialized
            }, cancellationToken);
        }

        public async Task<Message> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            return await _outgoingMessages.Reader.ReadAsync(cancellationToken);
        }

        public async Task Publish<T>(T model, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());

            await Post(new Message
            { 
                Id = Guid.NewGuid(),
                BoardKey = model.GetType().Name.ToString(),
                CorrelationId = Guid.NewGuid(),
                Payload = modelSerialized            
            });
        }

        public async Task Send<T>(T model, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());

            var message = new Message
            {
                Id = Guid.NewGuid(),
                BoardKey = model.GetType().Name.ToString(),
                CorrelationId = Guid.NewGuid(),
                Payload = modelSerialized
            };

            var replyEvent = new AsyncAutoResetEvent();

            _waitingEvents[message.Id] = new(replyEvent, message);
            
            await Post(message);

            await replyEvent.WaitAsync(cancellationToken);

            _waitingEvents.TryRemove(message.Id, out var _);
        }

        public async Task<R> SendAndGetReply<T, R>(T model, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            var modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());

            var message = new Message
            {
                Id = Guid.NewGuid(),
                BoardKey = model.GetType().Name.ToString(),
                CorrelationId = Guid.NewGuid(),
                Payload = modelSerialized
            };

            var replyEvent = new AsyncAutoResetEvent();

            _waitingEvents[message.Id] = new(replyEvent, message);

            await Post(message);

            await replyEvent.WaitAsync(cancellationToken);

            var deserializedReplyModel = serializer.Deserialize(_waitingEvents[message.Id].Message.Payload ?? throw new InvalidOperationException());

            _waitingEvents.Remove(message.Id, out var _);

            return (R)deserializedReplyModel;
        }
    }
}
