using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands;


public record GetAllPositionCommand : IRequest<IEnumerable<TrafficPosition>>;

public class GetAllPositionCommandHandler : IRequestHandler<GetAllPositionCommand, IEnumerable<TrafficPosition>>
{
    private readonly AuroraService auroraService;

    public GetAllPositionCommandHandler(AuroraService auroraService)
    {
        this.auroraService = auroraService;
    }

    public async Task<IEnumerable<TrafficPosition>> Handle(GetAllPositionCommand request, CancellationToken cancellationToken)
    {
        return await auroraService.ExecuteTransaction(async (auroraTcp) =>
        {
            var dataBytes = System.Text.Encoding.ASCII.GetBytes("#TR" + Environment.NewLine);
            await auroraTcp.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length, cancellationToken);

            var streamReader = new StreamReader(auroraTcp.GetStream(), System.Text.Encoding.ASCII);
            var auroraResponse = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;

            var callsigns = auroraResponse.Split(';');
            if (callsigns.Length <= 1)
            {
                return Enumerable.Empty<TrafficPosition>();
            }

            var trafficPositionList = new List<TrafficPosition>(callsigns.Length - 1);
            foreach (var callsign in callsigns.Skip(1))
            {
                if(callsign == string.Empty)
                {
                    continue;
                }
                await auroraTcp.GetStream().FlushAsync(cancellationToken);
                dataBytes = System.Text.Encoding.ASCII.GetBytes($"#TRPOS;{callsign};{Environment.NewLine}");
                auroraTcp.GetStream().Write(dataBytes, 0, dataBytes.Length);
                auroraResponse = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;
                var positionData = auroraResponse.Split(';');

                if(positionData.Count() <= 1)
                {
                    continue;
                }
                trafficPositionList.Add(TrafficPosition.ParseFromString(positionData.Skip(1).ToArray()));
            }
            return trafficPositionList;
        });
    }
}


public record TrafficPosition(
    string Callsign,
    string Heading,
    string Track,
    string Altitude,
    string Speed,
    string Latitude,
    string Longitude,
    string SSRSet,
    string SSRLabel,
    string WaypointLabel,
    string AltitudeLabel,
    string SpeedLabel,
    string AssumedStation,
    string NextStation,
    string OnGround,
    string IsSelected,
    string WasSelected,
    string CurrentGate,
    string Voice
)
{
    public enum PositionIndexes
    {
        Callsign = 0,
        Heading = 1,
        Track = 2,
        Altitude = 3,
        Speed = 4,
        Latitude = 5,
        Longitude = 6,
        SSRSet = 7,
        SSRLabel = 8,
        WaypointLabel = 9,
        AltitudeLabel = 10,
        SpeedLabel = 11,
        AssumedStation = 12,
        NextStation = 13,
        OnGround = 14,
        IsSelected = 15,
        WasSelected = 16,
        CurrentGate = 17,
        Voice = 18
    }

    public static TrafficPosition ParseFromString(string[] values)
    {
        if (values.Length <= (int)PositionIndexes.Voice)
        {
            throw new FormatException("The values array is too short to be parsed as a TrafficPosition");
        }
        return new TrafficPosition(
            Callsign: values[(int)PositionIndexes.Callsign],
            Heading: values[(int)PositionIndexes.Heading],
            Track: values[(int)PositionIndexes.Track],
            Altitude: values[(int)PositionIndexes.Altitude],
            Speed: values[(int)PositionIndexes.Speed],
            Latitude: values[(int)PositionIndexes.Latitude],
            Longitude: values[(int)PositionIndexes.Longitude],
            SSRSet: values[(int)PositionIndexes.SSRSet],
            SSRLabel: values[(int)PositionIndexes.SSRLabel],
            WaypointLabel: values[(int)PositionIndexes.WaypointLabel],
            AltitudeLabel: values[(int)PositionIndexes.AltitudeLabel],
            SpeedLabel: values[(int)PositionIndexes.SpeedLabel],
            AssumedStation: values[(int)PositionIndexes.AssumedStation],
            NextStation: values[(int)PositionIndexes.NextStation],
            OnGround: values[(int)PositionIndexes.OnGround],
            IsSelected: values[(int)PositionIndexes.IsSelected],
            WasSelected: values[(int)PositionIndexes.WasSelected],
            CurrentGate: values[(int)PositionIndexes.CurrentGate],
            Voice: values[(int)PositionIndexes.Voice]
        );
    }
}