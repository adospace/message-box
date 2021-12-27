using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using MessageBox.Messages;
using System.Buffers;

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

            public IReplyMessage? ReplyMessage { get; set; }
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IBusClientOptions _options;
        private readonly IMessageSerializerFactory _messageSerializerFactory;
        private readonly IMessageFactory _messageFactory;
        private readonly ITransport _transport;
        private readonly ILogger<Bus> _logger;

        private readonly Channel<IMessage> _outgoingMessages = Channel.CreateUnbounded<IMessage>();

        private readonly ConcurrentDictionary<Guid, RpcCall> _waitingCalls = new();
        private readonly ConcurrentDictionary<Type, IMessageReceiverCallback> _receiveActionForMessageType = new();

        private readonly ActionBlock<IMessage> _innomingMessages;

        public Bus(
            IServiceProvider serviceProvider,
            IBusClientOptions options)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<Bus>>();
            _transport = serviceProvider.GetRequiredService<ITransportFactory>().Create();
            _messageSerializerFactory = serviceProvider.GetRequiredService<IMessageSerializerFactory>();
            _messageFactory = serviceProvider.GetRequiredService<IMessageFactory>();
            _serviceProvider = serviceProvider;
            _options = options;
            _innomingMessages = new ActionBlock<IMessage>(ProcessIncomingMessage, new ExecutionDataflowBlockOptions
            { 
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            });
        }

        private async Task ProcessIncomingMessage(IMessage message)
        {
            //using var scope = _logger.BeginScope(new Dictionary<string, object?>
            //{
            //    {"MessageId", message.Id},
            //    {"ReplyToId", message.ReplyToId},
            //    {"RequireReply", message.RequireReply},
            //    {"PayloadType", message.PayloadType},
            //    {"PayloadSize", message.Payload?.Length},
            //    {"CorrelationId", message.CorrelationId},
            //});

            switch (message)
            {
                case IReplyMessage replyMessage:
                    ProcessIncomingMessage(replyMessage);
                    break;
                case ICallQueuedMessage callMessage:
                    await ProcessIncomingMessage(callMessage);
                    break;
                case IPublishEventMessage publishEventMessage:
                    await ProcessIncomingMessage(publishEventMessage);
                    break;
                default:
                    throw new NotSupportedException();
            }


            //message.MessageMemoryOwner?.Dispose();
        }

        private void ProcessIncomingMessage(IReplyMessage message)
        { 
            if (_waitingCalls.TryGetValue(message.ReplyToId, out var replyHandler))
            {
                replyHandler.ReplyMessage = message;
                replyHandler.WaitReplyEvent.Set();
            }
        }

        private async Task ProcessIncomingMessage(ICallQueuedMessage message)
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
                deserializedModel = serializer.Deserialize(
                    message.Payload,
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

                _logger.LogTrace("Sending reply with return value: {ReturnValueType} (Size: {ReplyPayloadLength})", returnValue.GetType(), replyPayload.Length);
            }
            else
            {
                _logger.LogTrace("Reply with no return value");
            }

            if (returnValue != null)
            {
                await Post(_messageFactory.CreateReplyWithPayloadMessage(
                    message: message,
                    payloadType: returnValue?.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException(),
                    payload: replyPayload));
            }
            else
            {
                await Post(_messageFactory.CreateReplyMessage(
                    message: message));
            }
        }

        private async Task ProcessIncomingMessage(IPublishEventMessage message)
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
                deserializedModel = serializer.Deserialize(
                    message.Payload,
                    typeOfTheModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to deserialize model of type {ModelType}", typeOfTheModel);
                return;
            }

            try
            {
                await actionToCallOnReceiver.Call(message, deserializedModel);
                _logger.LogTrace("Called handler method {ActionToCallOnReceiver}", actionToCallOnReceiver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Called handler method {ActionToCallOnReceiver} raised exception", actionToCallOnReceiver);
            }
        }


        public async Task Run(CancellationToken cancellationToken)
        {
            foreach (var receiverCallback in _serviceProvider.GetServices<IMessageReceiverCallback>())
            {
                _receiveActionForMessageType[receiverCallback.ModelType] = receiverCallback;

                //subscribe to the exchange
                await Post(_messageFactory.CreateSubsribeMessage(
                    receiverCallback.ModelType.FullName ?? throw new InvalidOperationException()), cancellationToken);
            }

            await _transport.Run(cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            await _transport.Stop(cancellationToken);
        }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken = default)
        {
            await _innomingMessages.SendAsync(message, cancellationToken);
        }

        private async Task Post(IMessage message, CancellationToken cancellationToken = default)
        {
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task<IMessage> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            return await _outgoingMessages.Reader.ReadAsync(cancellationToken);
        }

        public async Task Publish<T>(T model, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
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

            await Post(_messageFactory.CreatePublishEventMessage(
                exchangeName: model.GetType().FullName ?? throw new InvalidOperationException(),
                timeToLiveSeconds: (int)(timeout ?? _options.DefaultCallTimeout).TotalSeconds,
                payloadType: typeof(T).AssemblyQualifiedName ?? throw new InvalidOperationException(),
                payload: modelSerialized), cancellationToken);
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
            
            var message = _messageFactory.CreateCallMessage(
                exchangeName: model.GetType().FullName ?? throw new InvalidOperationException(),
                timeToLiveSeconds: (int)(timeout ?? _options.DefaultCallTimeout).TotalSeconds,
                payloadType: typeof(T).AssemblyQualifiedName ?? throw new InvalidOperationException(),
                payload: modelSerialized
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

            if (call.ReplyMessage is IReplyWithPayloadMessage replyWithPayloadMessage)
            {
                var replyMessagePayload = replyWithPayloadMessage.Payload;
                var replyMessagePayloadType = Type.GetType(replyWithPayloadMessage.PayloadType) ?? throw new InvalidOperationException();
                
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

            var message = _messageFactory.CreateCallMessage(
                exchangeName: model.GetType().FullName ?? throw new InvalidOperationException(),
                timeToLiveSeconds: (int)(timeout ?? _options.DefaultCallTimeout).TotalSeconds,
                payloadType: model.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException(),
                payload: modelSerialized
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

            var replyMessage = (call.ReplyMessage as IReplyWithPayloadMessage) ?? throw new InvalidOperationException();
            var replyMessagePayload = replyMessage.Payload;
            var replyMessagePayloadType = Type.GetType(replyMessage.PayloadType) ?? throw new InvalidOperationException($"Unable to load reply message paylod type '{replyMessage.PayloadType}'");

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
