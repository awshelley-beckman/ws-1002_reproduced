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
            string url = "ws://localhost:22475/";
            var websocket = new ClientWebSocket();

            // Make it send a keepalive ping every millisecond, to speed up
            // the process.
            websocket.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(1);

            await websocket.ConnectAsync(new Uri(url), default);

            // Continuously consume ReadAsync to make it process the ping getting
            // sent by the server
            var receiveBuffer = new byte[1];
            while (true)
                await websocket.ReceiveAsync(receiveBuffer, default);

        }
    }
}
