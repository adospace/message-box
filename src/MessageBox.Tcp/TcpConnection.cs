using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace MessageBox.Tcp
{
    public abstract class TcpConnection
    {
        public TcpConnection()
        {
        }

        public async void StartConnectionLoop(Socket connectedSocket, IMessageSink messageSink, IMessageSource messageSource, CancellationToken cancellationToken)
            => await RunConnectionLoop(connectedSocket, messageSink, messageSource, cancellationToken);

        protected virtual async Task RunConnectionLoop(Socket connectedSocket, IMessageSink messageSink, IMessageSource messageSource, CancellationToken cancellationToken)
        {
            var receiver = new TcpConnectionMessageReceiver(messageSink, connectedSocket, cancellationToken);
            var sender = new TcpConnectionMessageSender(messageSource, connectedSocket, cancellationToken);

            await Task.WhenAll(receiver.RunLoop(), sender.RunLoop());

            OnConnectionLoopEnded();
        }

        protected abstract void OnConnectionLoopEnded();
    }

    internal class TcpConnectionMessageReceiver
    {
        private readonly IMessageSink _messageSink;
        private readonly Socket _connectedSocket;
        private readonly CancellationToken _cancellationToken;

        public TcpConnectionMessageReceiver(IMessageSink messageSink, Socket connectedSocket, CancellationToken cancellationToken)
        {
            _messageSink = messageSink;
            _connectedSocket = connectedSocket;
            _cancellationToken = cancellationToken;
        }

        public async Task RunLoop()
        { 
            var pipe = new Pipe();
            Task writing = FillPipeAsync(pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

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
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);

                    int bytesRead = await _connectedSocket.ReceiveAsync(memory, SocketFlags.None, _cancellationToken);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // Tell the PipeWriter how much was read from the Socket.
                    writer.Advance(bytesRead);
                    // Make the data available to the PipeReader.
                    FlushResult result = await writer.FlushAsync(_cancellationToken);

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
                    ReadResult result = await reader.ReadAsync(_cancellationToken);

                    if (result.IsCanceled)
                    {
                        break;
                    }

                    ReadOnlySequence<byte> buffer = result.Buffer;

                    while (Message.TryDeserialize(ref buffer, out var message) && message != null)
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

    internal class TcpConnectionMessageSender
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
            Task writing = FillPipeAsync(pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
        }

        private async Task FillPipeAsync(PipeWriter writer)
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _messageSource.GetNextMessageToSend(_cancellationToken);

                    message.Serialize(writer);

                    message.MessageMemoryOwner?.Dispose();
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // Make the data available to the PipeReader.
                FlushResult result = await writer.FlushAsync(_cancellationToken);

                if (result.IsCompleted)
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
                    ReadResult result = await reader.ReadAsync(_cancellationToken);

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