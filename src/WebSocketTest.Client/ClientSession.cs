using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest.Client
{
    public class ClientSession
    {
        private readonly ClientWebSocketConnection _connection;
        private CancellationTokenSource _cts;

        public ClientSession(ClientWebSocketConnection connection)
        {
            _connection = connection;
        }

        public async Task Run(CancellationToken token)
        {
            Console.WriteLine("Starting a new session...");

            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            var sendTask = Task.Run(() => Send(_cts.Token));
            var listenTask = Task.Run(() => Listen(_cts.Token));

            await Task.WhenAll(sendTask, listenTask);

            await _connection.Disconnect();

            Console.WriteLine("Session complete.");
        }

        private async Task Send(CancellationToken token)
        {
            try
            {

                int counter = 0;

                while (!token.IsCancellationRequested)
                {
                    var message = "Hello " + counter++;

                    var messageBytes = Encoding.UTF8.GetBytes(message);

                    if (_connection.IsConnected)
                    {
                        await _connection.Send(messageBytes, Common.PacketType.Binary, token);
                    }
                    else
                    {
                        _cts.Cancel();
                        Console.WriteLine("Unable to send data - disconnected.");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task Listen(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var receivedPacket = await _connection.Receive(token);

                    if (receivedPacket == null)
                    {
                        Console.WriteLine("The server gracefully closed the connection.");

                        _cts.Cancel();

                        return;
                    }

                    var receivedMessage = Encoding.UTF8.GetString(receivedPacket.Body);

                    Console.WriteLine($"Received: {receivedMessage}");
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
