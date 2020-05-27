using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebSocketTest.Server.Controllers
{
    [Route("ws")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger _logger;

        public WebSocketController(ILogger<WebSocketController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task Get()
        {
            try
            {
                var context = ControllerContext.HttpContext;
                var isSocketRequest = context.WebSockets.IsWebSocketRequest;

                if (!isSocketRequest)
                {
                    _logger.LogWarning("A non-websocket request was received.");

                    context.Response.StatusCode = 400;
                    return;
                }

                int counter = 0;

                using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                using (var serverConnection = new ServerWebSocketConnection(webSocket))
                {
                    while(true)
                    {
                        counter++;

                        if (counter >= 4)
                        {
                            _logger.LogInformation("Disconnecting...");

                            await serverConnection.Disconnect();

                            return;
                        }

                        var packet = await serverConnection.Receive();

                       if (packet == null)
                            return;

                        var message = Encoding.UTF8.GetString(packet.Body);

                        _logger.LogInformation(message);

                        await serverConnection.Send(packet.Body, packet.Type);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in the WebSocketController.");

                return;
            }
        }
    }
}