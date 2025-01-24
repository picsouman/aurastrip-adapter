using System.Text.Json;
using aurastrip_adapter.Exceptions;
using aurastrip_adapter.WebSockets.Commands;
using Fleck;
using MediatR;
using Console = System.Console;

namespace aurastrip_adapter.WebSockets;

public class WebSocketRelayHostedService : IHostedService
{
    private readonly WebSocketServer _server;
    private readonly IMediator _mediator;
    private IWebSocketConnection? _currentConnection = null;
    
    public WebSocketRelayHostedService(IMediator mediator)
    {
        _mediator = mediator;
        _server = new WebSocketServer("ws://0.0.0.0:6969");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting WebSocketRelayHostedService");
        _server.Start(socket =>
        {
            socket.OnOpen = () => OnOpenHandler(socket);
            socket.OnClose = () => OnCloseHandler(socket);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _currentConnection?.Close();
        return Task.CompletedTask;
    }
    
    private void OnOpenHandler(IWebSocketConnection socket)
    {
        if (_currentConnection is not null && _currentConnection != socket)
        {
            socket.Close();
            return;
        }

        _currentConnection = socket;
        socket.OnMessage = (data) => OnMessageHandler(socket, data).RunSynchronously();
    }

    private void OnCloseHandler(IWebSocketConnection socket)
    {
        if (_currentConnection == socket)
        {
            _currentConnection = null;
        }
    }
    
    private async Task OnMessageHandler(IWebSocketConnection config, string data)
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