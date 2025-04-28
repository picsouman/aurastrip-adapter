using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record TransfertSendCommand(string Callsign, string PositionName) : IRequest<bool>;

    public class TransfertSendCommandHandler : IRequestHandler<TransfertSendCommand, bool>
    {
        private readonly AuroraService auroraService;

        public TransfertSendCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(TransfertSendCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);
                
                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRTR;{request.Callsign};{request.PositionName};{Environment.NewLine}");
                await auroraTcp.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                
                _ = await streamReader.ReadLineAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);
                
                return true;
            });
        }
    }
}
