using System.Data;
using System.Reflection;
using System.Text.Json;
using aurastrip_adapter.Exceptions;
using aurastrip_adapter.WebSockets.Commands;
using aurastrip_adapter.WebSockets.Dtos;
using Fleck;
using MediatR;

namespace aurastrip_adapter.WebSockets;

public class WebSocketRelayHostedService : IHostedService
{
    private readonly WebSocketServer _server;
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger _logger;
    private IWebSocketConnection? _currentConnection = null;
    private CancellationTokenSource _cancellationTokenSourceForAutoSenders = new();
    private Version _buildVersion;

    public WebSocketRelayHostedService(IMediator mediator, JsonSerializerOptions jsonOptions, ILogger<WebSocketRelayHostedService> logger)
    {
        _mediator = mediator;
        _jsonOptions = jsonOptions;
        _buildVersion = Assembly.GetExecutingAssembly().GetName().Version ?? throw new VersionNotFoundException("No version");
        _server = new WebSocketServer("ws://0.0.0.0:6969");
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[WS] Starting server");
        _server.Start(socket =>
        {
            socket.OnOpen = () => OnOpenHandler(socket);
            socket.OnClose = () => OnCloseHandler(socket);
            socket.OnError = (e) => OnErrorHandler(socket, e);
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

        _logger.LogInformation("[WS] Client disconnected with ip {0} at {1}", socket.ConnectionInfo.ClientIpAddress, DateTime.Now.ToString("HH:mm:ss"));
        
        _currentConnection = socket;
        _cancellationTokenSourceForAutoSenders = new CancellationTokenSource();

        // TODO : convertir en background task
        _ = StartAutomaticControllerDataSender(_cancellationTokenSourceForAutoSenders.Token);
        _ = StartAutomaticPositionDataSender(_cancellationTokenSourceForAutoSenders.Token);
        _ = StartAutomaticFlightPlanDataSender(_cancellationTokenSourceForAutoSenders.Token);
        socket.OnMessage = (data) => OnMessageHandler(socket, data).GetAwaiter().GetResult();
    }

    private void OnCloseHandler(IWebSocketConnection socket)
    {
        if (_currentConnection == socket)
        {
            CloseCurrentConnexion();
        }
    }
    
    private void OnErrorHandler(IWebSocketConnection socket, Exception e)
    {
        _logger.LogError("WebSocket Error : {0}", e.Message);
        if (_currentConnection == socket)
        {
            CloseCurrentConnexion();
        }
    }
    
    private async Task OnMessageHandler(IWebSocketConnection config, string data)
    {
        var request = JsonSerializer.Deserialize<WebSocketRequest>(data, _jsonOptions)!;
        WebSocketResponse? response = null;

        try
        {
            _logger.LogInformation("[WS] Received request at {0}: {1}, Data : {2}", DateTime.Now.ToString("HH:mm:ss"), request.Method, data);
            
            CancellationTokenSource timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(2));
            response = new WebSocketResponse(
                RequestId: request.RequestId,
                ReturnCode: "OK",
                Data: request.Method switch
                {
                    "GETALL_POS" => await _mediator.Send(new GetAllPositionCommand(), timeoutCancellationTokenSource.Token),
                    "GETALL_FP" => await _mediator.Send(new GetAllFlightPlanCommand(), timeoutCancellationTokenSource.Token),
                    "ASSUME" => await _mediator.Send(request.GetData<AssumeCommand>(_jsonOptions), timeoutCancellationTokenSource.Token),
                    "RELEASE" => await _mediator.Send(new ReleaseCommand(request.Data.ToString() ?? string.Empty), timeoutCancellationTokenSource.Token),
                    "TRANSFERT_SEND" => await _mediator.Send(request.GetData<TransfertSendCommand>(_jsonOptions), timeoutCancellationTokenSource.Token),
                    "TRANSFERT_REJECT" => await _mediator.Send(new TransfertRejectCommand(request.Data.ToString() ?? string.Empty), timeoutCancellationTokenSource.Token),
                    "TRANSFERT_CANCEL" => await _mediator.Send(new TransfertCancelCommand(request.Data.ToString() ?? string.Empty), timeoutCancellationTokenSource.Token),
                    "TRANSFERT_ACCEPT" => await _mediator.Send(new TransfertAcceptCommand(request.Data.ToString() ?? string.Empty), timeoutCancellationTokenSource.Token),
                    "SET_TRAFFIC" => await _mediator.Send(request.GetData<SetTrafficCommand>(_jsonOptions), timeoutCancellationTokenSource.Token),
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

    private void CloseCurrentConnexion()
    {
        _logger.LogInformation("[WS] Client disconnected with ip {0} at {1}", _currentConnection?.ConnectionInfo.ClientIpAddress, DateTime.Now.ToString("HH:mm:ss"));
        _cancellationTokenSourceForAutoSenders.Cancel();
        try
        {
            _currentConnection?.Close();
        } catch(Exception) {}
        _currentConnection = null;
    }
    
    private async Task StartAutomaticControllerDataSender(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                ControllerData? controllerData = null;
                try
                {
                    controllerData = await _mediator.Send(new GetControllerDataCommand(), token);
                }
                catch(Exception) {}
                
                var generalDataToSend = new GeneralDatasDto(
                    ApiVersion: _buildVersion.ToString(),
                    PositionName: controllerData?.PositionName ?? string.Empty
                );
                
                var wsResponse = new WebSocketResponse(
                    RequestId: String.Empty,
                    ReturnCode: "AUTO_GENERAL_DATA",
                    Data: JsonSerializer.Serialize(generalDataToSend, _jsonOptions)
                );
                await _currentConnection!.Send(JsonSerializer.Serialize(wsResponse, _jsonOptions));
                await Task.Delay(5000, token);
            }
            catch(Exception) {}
        }
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