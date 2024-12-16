using Fleck;
using System.Net.Sockets;

namespace aurastrip_adapter.WebSockets
{
    public static class WebSocketRelayServer
    {
        private static TcpClient auroraTcp = new TcpClient();

        public static void Start()
        {
            var server = new Fleck.WebSocketServer("ws://0.0.0.0:6969");
            server.Start(config =>
            {
                config.OnOpen = () => Console.WriteLine("Connexion ouverte");
                config.OnClose = () => Console.WriteLine("Connexion fermée");
                config.OnMessage = (data) => OnMessageHandler(config, data);
            });
        }

        private static void OnMessageHandler(IWebSocketConnection config, string data)
        {
            lock (auroraTcp)
            {
                if (auroraTcp.Connected is false)
                {
                    auroraTcp.Connect("127.0.0.1", 1130);
                    if (auroraTcp.Connected is false)
                    {
                        config.Send(System.Text.Encoding.ASCII.GetBytes("#NO_AURORA#"));
                        return;
                    }
                }

                var dataBytes = System.Text.Encoding.ASCII.GetBytes(data.Trim() + Environment.NewLine);
                auroraTcp.GetStream().Write(dataBytes, 0, dataBytes.Length);

                var streamReader = new StreamReader(auroraTcp.GetStream(), System.Text.Encoding.ASCII);
                var response = streamReader.ReadLine();
                config.Send(response);
            }
        }
    }
}
