using aurastrip_adapter.Services;
using MediatR;
using System.Runtime.CompilerServices;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record AssumeCommand(string Callsign) : IRequest<bool>;

    public class AssumeCommandHandler : IRequestHandler<AssumeCommand, bool>
    {
        private readonly AuroraService auroraService;

        public AssumeCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(AssumeCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRAS;{request.Callsign};{Environment.NewLine}");
                await auroraTcp.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                return true;
            });
        }
    }
}
