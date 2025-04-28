using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record TransfertAcceptCommand(string Callsign) : IRequest<bool>;

    public class TransfertAcceptCommandHandler : IRequestHandler<TransfertAcceptCommand, bool>
    {
        private readonly AuroraService auroraService;
        private readonly IMediator mediator;

        public TransfertAcceptCommandHandler(AuroraService auroraService, IMediator mediator)
        {
            this.auroraService = auroraService;
            this.mediator = mediator;
        }

        public async Task<bool> Handle(TransfertAcceptCommand request, CancellationToken cancellationToken)
        {
            var transfertSuccess = await auroraService.ExecuteTransaction(async (auroraTcp) =>
            {
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);

                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRHA;{request.Callsign};{Environment.NewLine}");
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                var response = await streamReader.ReadLineAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);

                return response?.StartsWith("@ERR", StringComparison.Ordinal) ?? false;
            });

            if(!transfertSuccess)
            {
                return await mediator.Send(new AssumeCommand(request.Callsign, true), cancellationToken);
            }
            return true;
        }
    }
}
