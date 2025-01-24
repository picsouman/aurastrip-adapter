using aurastrip_adapter.WebSockets.Commands;
using Fleck;
using MediatR;
using System.Text.Json;
using aurastrip_adapter.Exceptions;

namespace aurastrip_adapter.WebSockets
{
    public record WebSocketRequest(string RequestId, string Method, object Data)
    {
        public T GetData<T>()
        {
            if (Data is T directCast)
            {
                return directCast;
            }

            if (Data is JsonElement jsonElement)
            {
                return jsonElement.Deserialize<T>() ?? throw new FormatException("The given data given as parameter is not compatible with the method");
            }

            var json = JsonSerializer.Serialize(Data);
            return JsonSerializer.Deserialize<T>(json) ?? throw new FormatException("The given data given as parameter is not compatible with the method");
        }
    }

    public record WebSocketResponse(string RequestId, string ReturnCode, object Data);

    public static class WebSocketRelayServer
    {
        private static IMediator _mediator = null!;

        public static void Start(IMediator mediator)
        {
            _mediator = mediator;
            var server = new WebSocketServer("ws://0.0.0.0:6969");
            server.Start(config =>
            {
                config.OnOpen = () => Console.WriteLine("Connexion ouverte" + config.ConnectionInfo.ClientIpAddress);
                config.OnClose = () => Console.WriteLine("Connexion fermée");
                config.OnMessage = (data) => OnMessageHandler(config, data);
            });
        }

        private async static void OnMessageHandler(IWebSocketConnection config, string data)
        {
            var request = JsonSerializer.Deserialize<WebSocketRequest>(data)!;
            WebSocketResponse? response = null;

            try
            {
                response = new WebSocketResponse(
                    RequestId: request.RequestId,
                    ReturnCode: "OK",
                    Data: request.Method switch
                    {
                        "GETALL_POS" => await _mediator.Send(new GetAllPositionCommand()),
                        "GETALL_FP" => await _mediator.Send(new GetAllFlightPlanCommand()),
                        "ASSUME" => await _mediator.Send(new AssumeCommand(request.Data.ToString() ?? string.Empty)),
                        "RELEASE" => await _mediator.Send(new ReleaseCommand(request.Data.ToString() ?? string.Empty)),
                        "TRANSFERT" => await _mediator.Send(request.GetData<TransfertCommand>()),
                        "SET_TRAFFIC" => await _mediator.Send(request.GetData<SetTrafficCommand>()),
                        _ => throw new CommandUnknownException()
                    }
                );
            }
            catch(CommandUnknownException)
            {
                response = new WebSocketResponse(
                    RequestId: request!.RequestId,
                    ReturnCode: "COMMAND_UNKNOWN",
                    Data: "The command does not exists"
                );
            }
            catch (Exception)
            {
                response = new WebSocketResponse(
                    RequestId: request!.RequestId,
                    ReturnCode: "NO_AURORA",
                    Data: "Aurora not available"
                );
            }

            await config.Send(JsonSerializer.Serialize(response));
        }
    }
}
