using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record TransfertCancelCommand(string Callsign) : IRequest<bool>;

    public class TransfertCancelCommandHandler : IRequestHandler<TransfertCancelCommand, bool>
    {
        private readonly AuroraService auroraService;

        public TransfertCancelCommandHandler(AuroraService auroraService)
        {
            this.auroraService = auroraService;
        }

        public async Task<bool> Handle(TransfertCancelCommand request, CancellationToken cancellationToken)
        {
            return await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);

                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRHC;{request.Callsign};{Environment.NewLine}");
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                _ = await streamReader.ReadLineAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);

                return true;
            });
        }
    }
}
