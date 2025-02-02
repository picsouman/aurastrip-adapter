using aurastrip_adapter.Services;
using MediatR;
using System.IO;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record ReleaseCommand(string Callsign) : IRequest<bool>;

    public class ReleaseCommandHandler : IRequestHandler<ReleaseCommand, bool>
    {
        private readonly AuroraService auroraService;

        public ReleaseCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(ReleaseCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);
                
                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRRE;{request.Callsign};{Environment.NewLine}");
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);

                _ = await streamReader.ReadLineAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);
                
                return true;
            });
        }
    }
}
