using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands;

public record GetControllerDataCommand : IRequest<ControllerData>;

public class GetControllerDataHandler : IRequestHandler<GetControllerDataCommand, ControllerData>
{
    private readonly AuroraService auroraService;

    public GetControllerDataHandler(AuroraService auroraService)
    {
        this.auroraService = auroraService;
    }

    public async Task<ControllerData> Handle(GetControllerDataCommand request, CancellationToken cancellationToken)
    {
        return await auroraService.ExecuteTransaction(async (auroraTcp) =>
        {
            var dataBytes = System.Text.Encoding.ASCII.GetBytes("#CONN" + Environment.NewLine);
            await auroraTcp.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);

            var streamReader = new StreamReader(auroraTcp.GetStream(), System.Text.Encoding.ASCII);
            var auroraResponse = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;

            var response = auroraResponse.Split(';');
            if (response.Length <= 1)
            {
                return new ControllerData();
            }
            return new ControllerData(
                PositionName: response[1]
            );
        });
    }
}

public record ControllerData(
    string PositionName = ""
);