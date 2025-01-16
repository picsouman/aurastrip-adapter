using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record TransfertCommand(string Callsign, string PositionName) : IRequest<bool>;

    public class TransfertCommandHandler : IRequestHandler<TransfertCommand, bool>
    {
        private readonly AuroraService auroraService;

        public TransfertCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(TransfertCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRTR;{request.Callsign};{request.PositionName};{Environment.NewLine}");
                await auroraTcp.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                return true;
            });
        }
    }
}
