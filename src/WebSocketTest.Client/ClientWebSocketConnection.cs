using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketTest.Common;

namespace WebSocketTest.Client
{
    public class ClientWebSocketConnection : IDisposable
    {
        private readonly ClientWebSocket _socket = new ClientWebSocket();
        private readonly Uri _uri;
        private readonly MemoryStream _receiveStream = new MemoryStream();
        private readonly ArraySegment<byte> _receiveBuffer = WebSocket.CreateClientBuffer(4096, 4096);

        public bool IsConnected => _socket.State == WebSocketState.Open;
        //private WebSocketState State => _socket?.State ?? WebSocketState.None;

        private const int CloseSocketTimeout = 20000;

        public ClientWebSocketConnection(string url)
        {
            _uri = new Uri(url);
        }

        public async Task Connect(CancellationToken cancellationToken = default)
        {
            await _socket.ConnectAsync(_uri, cancellationToken);
        }

        public async Task Disconnect()
        {
            //Logger.Info($"Disconnecting from {_uri}");
            if (_socket == null || _socket.State != WebSocketState.Open) 
                return;

            // close the socket first, because ReceiveAsync leaves an invalid socket (state = aborted) when the token is cancelled
            using var timeout = new CancellationTokenSource(CloseSocketTimeout);

            try
            {
                // after this, the socket state which change to CloseSent
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing from client", timeout.Token);
            }
            catch (OperationCanceledException)
            {
                // normal upon task/token cancellation, disregard
            }
        }

        public async Task<Packet> Receive(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //Re-use the memory stream so we don't have to keep re-allocating memory.
                _receiveStream.Seek(0, SeekOrigin.Begin);
                _receiveStream.SetLength(0);

                WebSocketReceiveResult result;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    result = await _socket.ReceiveAsync(_receiveBuffer, cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _socket.CloseOutputAsync(_socket.CloseStatus ?? WebSocketCloseStatus.NormalClosure, _socket.CloseStatusDescription, CancellationToken.None);

                        // We're done here.
                        return null;
                    }

                    await _receiveStream.WriteAsync(_receiveBuffer.Slice(0, result.Count), cancellationToken);

                } while (!result.EndOfMessage);

                return new Packet(_receiveStream.ToArray(), result.MessageType.Map());
            }
        }

        public async Task Send(ReadOnlyMemory<byte> body, PacketType packetType, CancellationToken cancellationToken = default)
        {
            await _socket.SendAsync(body, packetType.Map(), true, cancellationToken);
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _receiveStream?.Dispose();
        }
    }
}
