﻿using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands
{
    public record AssumeCommand(string Callsign, bool Force = false) : IRequest<bool>;

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
                var stream = auroraTcp.GetStream();
                var streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);

                var auroraAssumeCommand = request.Force ? "TRASF" : "TRAS";

                var dataBytes = System.Text.Encoding.ASCII.GetBytes($"#{auroraAssumeCommand};{request.Callsign};{Environment.NewLine}");
                await stream.WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);
                _ = await streamReader.ReadLineAsync(cancellationToken);
                await stream.FlushAsync(cancellationToken);
                
                return true;
            });
        }
    }
}
