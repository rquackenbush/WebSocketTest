using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (src, args) =>
            {
                args.Cancel = true;
                cts.Cancel();
            };

            //int counter = 0;

            var client = new TheClient("ws://localhost:5000/ws");

            try
            {
                await client.Run(cts.Token);
            }
            catch(OperationCanceledException)
            {
            }

            Console.WriteLine("Client is done.");

            //using (var connection = new ClientWebSocketConnection("ws://localhost:5000/ws"))
            //{
            //    await connection.Connect();

            //    try
            //    {

            //        while (!cts.Token.IsCancellationRequested)
            //        {
            //            var message = "Hello " + counter++;

            //            var messageBytes = Encoding.UTF8.GetBytes(message);

            //            await connection.Send(messageBytes, Common.PacketType.Binary, cts.Token);

            //            var receivedPacket = await connection.Receive(cts.Token);

            //            if (receivedPacket == null)
            //            {
            //                Console.WriteLine("The server gracefully closed the connection.");

            //                return;
            //            }

            //            var receivedMessage = Encoding.UTF8.GetString(receivedPacket.Body);

            //            Console.WriteLine($"Received: {receivedMessage}");

            //            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
            //        }
            //    }
            //    catch(OperationCanceledException)
            //    {
            //        Console.WriteLine("Operation cancelled.");
            //    }

            //    Console.WriteLine("Disconnecting...");

            //    await connection.Disconnect();

            //    Console.WriteLine("Disconnected.");
            //}   
        }
    }
}
