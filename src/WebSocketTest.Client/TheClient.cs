using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest.Client
{
    public class TheClient
    {
        private readonly string _url;

        public TheClient(string url)
        {
            _url = url;
        }

        public async Task Run(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                try
                {
                    using (var connection = new ClientWebSocketConnection(_url))
                    {
                        var connectCts = new CancellationTokenSource(5000);

                        token.Register(connectCts.Cancel);

                        await connection.Connect(connectCts.Token);

                        var session = new ClientSession(connection);

                        await session.Run(token);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Exception in TheClient.Run:" + ex.ToString());
                }
                finally
                {
                    //Retry timer (in real world we would fall off exponentially)
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }

                
            }
        }
    }
}
