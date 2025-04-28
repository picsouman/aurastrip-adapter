using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record TransfertRejectCommand(string Callsign) : IRequest<bool>;

    public class TransfertRejectCommandHandler : IRequestHandler<TransfertRejectCommand, bool>
    {
        private readonly AuroraService auroraService;

        public TransfertRejectCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(TransfertRejectCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);

                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRHR;{request.Callsign};{Environment.NewLine}");
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                _ = await streamReader.ReadLineAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);

                return true;
            });
        }
    }
}
