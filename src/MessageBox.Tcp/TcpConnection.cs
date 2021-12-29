using MessageBox.Messages;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace MessageBox
{
    public abstract class TcpConnection
    {
        protected readonly IServiceProvider ServiceProvider;

        public TcpConnection(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async void StartConnectionLoop(
            Socket connectedSocket,
            IMessageSource messageSource,
            IMessageSink messageSink, 
            CancellationToken cancellationToken = default)
            => await RunConnectionLoop(connectedSocket, messageSource, messageSink, cancellationToken);

        protected virtual async Task RunConnectionLoop(
            Socket connectedSocket,
            IMessageSource messageSource,
            IMessageSink messageSink,
            CancellationToken cancellationToken = default)
        {
            var messageFactory = ServiceProvider.GetRequiredService<IMessageFactory>();

            var receiver = new TcpConnectionMessageReceiver(
                messageFactory,
                messageSink, 
                connectedSocket, 
                cancellationToken);

            var sender = new TcpConnectionMessageSender(
                messageSource, 
                connectedSocket, 
                cancellationToken);

            await Task.WhenAll(receiver.RunLoop(), sender.RunLoop());

            OnConnectionLoopEnded();
        }

        protected abstract void OnConnectionLoopEnded();

        private class TcpConnectionMessageReceiver
        {
            private readonly IMessageFactory _messageFactory;
            private readonly IMessageSink _messageSink;
            private readonly Socket _connectedSocket;
            private readonly CancellationToken _cancellationToken;

            public TcpConnectionMessageReceiver(
                IMessageFactory messageFactory,
                IMessageSink messageSink,
                Socket connectedSocket, 
                CancellationToken cancellationToken)
            {
                _messageFactory = messageFactory;
                _messageSink = messageSink;
                _connectedSocket = connectedSocket;
                _cancellationToken = cancellationToken;
            }

            public async Task RunLoop()
            { 
                var pipe = new Pipe();
                var writing = FillPipeAsync(pipe.Writer);
                var reading = ReadPipeAsync(pipe.Reader);

                await Task.WhenAll(reading, writing);
            }

            private async Task FillPipeAsync(PipeWriter writer)
            {
                const int minimumBufferSize = 512;

                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Allocate at least 512 bytes from the PipeWriter.
                        var memory = writer.GetMemory(minimumBufferSize);

                        var bytesRead = await _connectedSocket.ReceiveAsync(memory, SocketFlags.None, _cancellationToken);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        // Tell the PipeWriter how much was read from the Socket.
                        writer.Advance(bytesRead);
                        // Make the data available to the PipeReader.
                        var result = await writer.FlushAsync(_cancellationToken);

                        if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        //LogError(ex);
                        break;
                    }

                }

                // By completing PipeWriter, tell the PipeReader that there's no more data coming.
                await writer.CompleteAsync();
            }

            private async Task ReadPipeAsync(PipeReader reader)
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await reader.ReadAsync(_cancellationToken);

                        if (result.IsCanceled)
                        {
                            break;
                        }

                        var buffer = result.Buffer;

                        while (_messageFactory.TryDeserialize(ref buffer, out var message) && message != null)
                        {
                            await _messageSink.OnReceivedMessage(message, _cancellationToken);
                        }

                        reader.AdvanceTo(buffer.Start, buffer.End);

                        if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                // Mark the PipeReader as complete.
                await reader.CompleteAsync();
            }
        }

        private class TcpConnectionMessageSender
        {
            private readonly IMessageSource _messageSource;
            private readonly Socket _connectedSocket;
            private readonly CancellationToken _cancellationToken;

            public TcpConnectionMessageSender(IMessageSource messageSink, Socket connectedSocket, CancellationToken cancellationToken)
            {
                _messageSource = messageSink;
                _connectedSocket = connectedSocket;
                _cancellationToken = cancellationToken;
            }

            public async Task RunLoop()
            {
                var pipe = new Pipe();
                var writing = FillPipeAsync(pipe.Writer);
                var reading = ReadPipeAsync(pipe.Reader);

                await Task.WhenAll(reading, writing);
            }

            private async Task FillPipeAsync(PipeWriter writer)
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var message = await _messageSource.GetNextMessageToSend(_cancellationToken);

                        ((ISerializableMessage)message).Serialize(writer);

                        (message as IDisposable)?.Dispose();

                        // Make the data available to the PipeReader.
                        var result = await writer.FlushAsync(_cancellationToken);

                        if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                // By completing PipeWriter, tell the PipeReader that there's no more data coming.
                await writer.CompleteAsync();
            }

            private async Task ReadPipeAsync(PipeReader reader)
            {
                var networkStream = new NetworkStream(_connectedSocket, false);

                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await reader.ReadAsync(_cancellationToken);

                        if (result.IsCanceled)
                        {
                            break;
                        }

                        foreach (var segment in result.Buffer)
                        {
                            await networkStream.WriteAsync(segment, _cancellationToken).ConfigureAwait(false);
                        }

                        reader.AdvanceTo(result.Buffer.End);

                        if (result.IsCompleted)
                        {
                            break;
                        }

                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                // Mark the PipeReader as complete.
                await reader.CompleteAsync();
            }

        }
    }
}