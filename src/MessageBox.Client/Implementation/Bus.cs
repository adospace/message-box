using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace MessageBox.Client.Implementation
{
    internal class Bus : IBus, IMessageSink, IMessageSource, IBusClient
    {
        private class RpcCall
        {
            public RpcCall()
            {
                WaitReplyEvent = new AsyncAutoResetEvent();
            }

            public AsyncAutoResetEvent WaitReplyEvent { get; }

            public Message? ReplyMessage { get; set; }
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IBusClientOptions _options;
        private readonly IMessageSerializerFactory _messageSerializerFactory;
        private readonly ITransport _transport;
        private readonly ILogger<Bus> _logger;

        private readonly Channel<Message> _outgoingMessages = Channel.CreateUnbounded<Message>();

        private readonly ConcurrentDictionary<Guid, RpcCall> _waitingCalls = new();
        private readonly ConcurrentDictionary<Type, IMessageReceiverCallback> _receiveActionForMessageType = new();


        private readonly ActionBlock<Message> _innomingMessages;

        public Bus(
            IServiceProvider serviceProvider,
            IBusClientOptions options)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<Bus>>();
            _transport = serviceProvider.GetRequiredService<ITransportFactory>().Create();
            _messageSerializerFactory = serviceProvider.GetRequiredService<IMessageSerializerFactory>();
            _serviceProvider = serviceProvider;
            _options = options;
            _innomingMessages = new ActionBlock<Message>(ProcessIncomingMessage, new ExecutionDataflowBlockOptions
            { 
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            });
        }

        private async Task ProcessIncomingMessage(Message message)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                {"MessageId", message.Id},
                {"ReplyToId", message.ReplyToId},
                {"RequireReply", message.RequireReply},
                {"PayloadType", message.PayloadType},
                {"PayloadSize", message.Payload?.Length},
                {"CorrelationId", message.CorrelationId},
            });

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
                    _logger.LogWarning("Unable to find an handler for model type {TypeOfTheModel}", typeOfTheModel);
                    return;
                }

                object deserializedModel;
            
                try
                {
                    deserializedModel = serializer.Deserialize(message.Payload ?? throw new InvalidOperationException(),
                        typeOfTheModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to deserialize model of type {ModelType}", typeOfTheModel);
                    return;
                }
                
                object? returnValue;

                try
                {
                    returnValue = await actionToCallOnReceiver.Call(message, deserializedModel);
                    _logger.LogTrace("Called handler method {ActionToCallOnReceiver}", actionToCallOnReceiver);
                }
                catch (Exception ex)
                {
                    returnValue = new MessageBoxCallException($"Exception raised when calling handler for model type '{typeOfTheModel}:{Environment.NewLine}{ex.InnerException}");
                }                

                if (message.RequireReply)
                { 
                    byte[]? replyPayload = null;
                    if (returnValue != null)
                    {
                        try
                        {
                            replyPayload = serializer.Serialize(returnValue);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unable to serialize return value of type {ReturnValueType}", returnValue.GetType());
                            return;
                        }
                        
                        _logger.LogTrace("Sending reply with return value: {ReturnValueType} (Size: {ReplyPayloadLength})",returnValue.GetType(), replyPayload.Length);
                    }
                    else
                    {
                        _logger.LogTrace("Reply with no return value");
                    }

                    await Post(new Message(
                        Id: Guid.NewGuid(),
                        ReplyToId: message.Id,
                        ReplyToBoxId: message.ReplyToBoxId,
                        CorrelationId: message.CorrelationId,
                        PayloadType: returnValue?.GetType().AssemblyQualifiedName,
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

        private async Task Post(Message message, CancellationToken cancellationToken = default)
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
            byte[] modelSerialized;
            
            try
            {
                modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to serialize model value of type {ModelType}", model?.GetType());
                throw;
            }
            
            await Post(new Message(
                Id: Guid.NewGuid(),
                BoardKey: model.GetType().FullName,
                CorrelationId: Guid.NewGuid(),
                IsEvent: true,
                Payload: modelSerialized,
                PayloadType: typeof(T).AssemblyQualifiedName
            ), cancellationToken);
        }

        public async Task Send<T>(T model, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            byte[] modelSerialized;
            
            try
            {
                modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to serialize model value of type {ModelType}", model?.GetType());
                throw;
            }
            
            var message = new Message(
                Id: Guid.NewGuid(),
                BoardKey: model.GetType().FullName,
                CorrelationId: Guid.NewGuid(),
                Payload: modelSerialized,
                PayloadType: typeof(T).AssemblyQualifiedName,
                RequireReply: true
            );

            var call = new RpcCall();

            _waitingCalls[message.Id] = call;
            
            await Post(message, cancellationToken);

            try
            {
                if (!await call.WaitReplyEvent.WaitAsync(cancellationToken).CancelAfter(timeout ?? _options.DefaultCallTimeout, cancellationToken: cancellationToken))
                {
                    throw new TimeoutException();
                }
            }
            finally
            {
                _waitingCalls.TryRemove(message.Id, out var _);
            }

            if (call.ReplyMessage is { Payload: { } })
            {
                var replyMessage = call.ReplyMessage;
                var replyMessagePayload = replyMessage.Payload ?? throw new InvalidOperationException();
                var replyMessagePayloadType = Type.GetType(replyMessage.PayloadType ?? throw new InvalidOperationException()) ?? throw new InvalidOperationException();
                
                object deserializedReplyModel;
                
                try
                {
                    deserializedReplyModel = serializer.Deserialize(
                        replyMessagePayload,
                        replyMessagePayloadType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to deserialize reply model of type {ModelType}", replyMessagePayloadType);
                    throw;
                }                

                if (deserializedReplyModel is MessageBoxCallException exception)
                {
                    throw exception;
                }
            }
        }

        public async Task<TR> SendAndGetReply<TR>(object model, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var serializer = _messageSerializerFactory.CreateMessageSerializer();
            byte[] modelSerialized;
            
            try
            {
                modelSerialized = serializer.Serialize(model ?? throw new InvalidOperationException());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to serialize model value of type {ModelType}", model?.GetType());
                throw;
            }           

            var message = new Message(
                Id:Guid.NewGuid(),
                BoardKey: model.GetType().FullName,
                CorrelationId: Guid.NewGuid(),
                Payload: modelSerialized,
                PayloadType: model.GetType().AssemblyQualifiedName,
                RequireReply: true
            );

            var call = new RpcCall();

            _waitingCalls[message.Id] = call;

            await Post(message, cancellationToken);

            try
            {
                if (!await call.WaitReplyEvent.WaitAsync(cancellationToken).CancelAfter(timeout ?? _options.DefaultCallTimeout, cancellationToken: cancellationToken))
                {
                    throw new TimeoutException($"Unable to get a reply to the message '{model.GetType()}' in {timeout ?? _options.DefaultCallTimeout}");
                }
            }
            finally
            {
                _waitingCalls.TryRemove(message.Id, out var _);
            }

            var replyMessage = call.ReplyMessage ?? throw new InvalidOperationException();
            var replyMessagePayload = replyMessage.Payload ?? throw new InvalidOperationException();
            var replyMessagePayloadType = Type.GetType(replyMessage.PayloadType ?? throw new InvalidOperationException()) ?? throw new InvalidOperationException();

            object deserializedReplyModel;
                
            try
            {
                deserializedReplyModel = serializer.Deserialize(
                    replyMessagePayload,
                    replyMessagePayloadType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to deserialize reply model of type {ModelType}", replyMessagePayloadType);
                throw;
            }

            _waitingCalls.Remove(message.Id, out var _);

            return (TR)deserializedReplyModel;
        }
    }
}
