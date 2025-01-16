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
                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRRE;{request.Callsign};{Environment.NewLine}");
                await auroraTcp.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);

                var buffer = new byte[1];
                var _ = await auroraTcp.GetStream().ReadAsync(buffer, 0, 1, cancellationToken).WaitAsync(TimeSpan.FromSeconds(2));

                return true;
            });
        }
    }
}
