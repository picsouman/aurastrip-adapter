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
    private readonly JsonSerializerOptions _jsonOptions;
    private IWebSocketConnection? _currentConnection = null;
    private CancellationTokenSource _cancellationTokenSourceForAutoSenders = new();
    
    public WebSocketRelayHostedService(IMediator mediator, JsonSerializerOptions jsonOptions)
    {
        _mediator = mediator;
        _jsonOptions = jsonOptions;
        _server = new WebSocketServer("ws://0.0.0.0:6969");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[WS] Starting server");
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

        Console.WriteLine($"[WS] Client connected with ip {socket.ConnectionInfo.ClientIpAddress} at {DateTime.Now:HH:mm:ss}");
        
        _currentConnection = socket;
        _cancellationTokenSourceForAutoSenders = new CancellationTokenSource();
        StartAutomaticPositionDataSender(_cancellationTokenSourceForAutoSenders.Token);
        StartAutomaticFlightPlanDataSender(_cancellationTokenSourceForAutoSenders.Token);
        socket.OnMessage = (data) => OnMessageHandler(socket, data).GetAwaiter().GetResult();
    }

    private void OnCloseHandler(IWebSocketConnection socket)
    {
        if (_currentConnection == socket)
        {
            Console.WriteLine($"[WS] Client disconnected with ip {socket.ConnectionInfo.ClientIpAddress} at {DateTime.Now:HH:mm:ss}");
            _cancellationTokenSourceForAutoSenders.Cancel();
            _currentConnection = null;
        }
    }
    
    private async Task OnMessageHandler(IWebSocketConnection config, string data)
    {
        var request = JsonSerializer.Deserialize<WebSocketRequest>(data, _jsonOptions)!;
        WebSocketResponse? response = null;

        try
        {
            Console.WriteLine($"[WS] Received request: {request.Method}");
            response = new WebSocketResponse(
                RequestId: request.RequestId,
                ReturnCode: "OK",
                Data: request.Method switch
                {
                    "GETALL_POS" => await _mediator.Send(new GetAllPositionCommand()),
                    "GETALL_FP" => await _mediator.Send(new GetAllFlightPlanCommand()),
                    "ASSUME" => await _mediator.Send(new AssumeCommand(request.Data.ToString() ?? string.Empty)),
                    "RELEASE" => await _mediator.Send(new ReleaseCommand(request.Data.ToString() ?? string.Empty)),
                    "TRANSFERT" => await _mediator.Send(request.GetData<TransfertCommand>(_jsonOptions)),
                    "SET_TRAFFIC" => await _mediator.Send(request.GetData<SetTrafficCommand>(_jsonOptions)),
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

        await config.Send(JsonSerializer.Serialize(response, _jsonOptions));
    }

    private async Task StartAutomaticPositionDataSender(CancellationToken cts)
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                var wsResponse = new WebSocketResponse(
                    RequestId: String.Empty,
                    ReturnCode: "AUTO_POS",
                    Data: JsonSerializer.Serialize(await _mediator.Send(new GetAllPositionCommand()), _jsonOptions)
                );
                await _currentConnection!.Send(JsonSerializer.Serialize(wsResponse, _jsonOptions));
                await Task.Delay(1000, cts);
                
            } catch(Exception) {}
        }
    }
    
    private async Task StartAutomaticFlightPlanDataSender(CancellationToken cts)
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                var wsResponse = new WebSocketResponse(
                    RequestId: String.Empty,
                    ReturnCode: "AUTO_FP",
                    Data: JsonSerializer.Serialize(await _mediator.Send(new GetAllFlightPlanCommand()), _jsonOptions)
                );
                await _currentConnection!.Send(JsonSerializer.Serialize(wsResponse, _jsonOptions));
                await Task.Delay(15000, cts);
                
            } catch(Exception) {}
        }
    } 
}