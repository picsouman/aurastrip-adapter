using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record SetTrafficCommand(
        string? Callsign,
        string? Waypoint,
        string? FlightLevel,
        string? Speed,
        string? Squawk
    ) : IRequest<bool>;

    public class SetTrafficCommandHandler : IRequestHandler<SetTrafficCommand, bool>
    {
        private readonly AuroraService auroraService;

        public SetTrafficCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(SetTrafficCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream);
                
                if (request.Waypoint is not null)
                {
                    var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#LBWP;{request.Callsign};{request.Waypoint}{Environment.NewLine}");
                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                    _ = await streamReader.ReadLineAsync(cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                if (request.FlightLevel is not null)
                {
                    var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#LBALT;{request.Callsign};{request.FlightLevel}{Environment.NewLine}");
                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                    _ = await streamReader.ReadLineAsync(cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                if (request.Speed is not null)
                {
                    var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#LBSPD;{request.Callsign};{request.Speed}{Environment.NewLine}");
                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                    _ = await streamReader.ReadLineAsync(cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                if (request.Squawk is not null)
                {
                    var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#LBSQK;{request.Callsign};{request.Squawk}{Environment.NewLine}");
                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                    _ = await streamReader.ReadLineAsync(cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
                
                return true;
            });
        }
    }
}
