using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger(
                "Log my stuff",
                new IndentableWriter("  "),
                LogLevel.Debug
            );

            string url = "ws://localhost:22475/";
            var websocket = new ClientWebSocket();
            websocket.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(1);

            await websocket.ConnectAsync(new Uri(url), default);

            // Continuously consume ReadAsync to make it process the ping getting
            // sent by the server
            var receiveBuffer = new byte[1];
            // try
            // {
                while (true)
                    await websocket.ReceiveAsync(receiveBuffer, default);
            // }
            // catch (Exception e)
            // {
            //     // // Log the exception the same way CloudSync would.
            //     // // Does it cut that last bit of the stack trace off?
            //     // logger.LogError(e, "msg");
            // }

            // while (true)
            // {
            //     var a = Task.Run(() => SendText(websocket, "aaaaaaaaaaaaaaa"));
            //     var b = Task.Run(() => SendText(websocket, "bbbbbbbbbbbb"));
            //     await Task.WhenAll(a, b);
            // }
        }

        static async Task SendText(ClientWebSocket webSocket, string message)
        {
            var sendBuffer = Encoding.ASCII.GetBytes(message);
            await webSocket.SendAsync(
                sendBuffer,
                ((byte)WebSocketMessageType.Text),
                true,
                default
            );
        }
    }
}
