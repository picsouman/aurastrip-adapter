using aurastrip_adapter.Services;
using MediatR;

namespace aurastrip_adapter.WebSockets.Commands;

public class GetAllFlightPlanCommand : IRequest<IEnumerable<FlightPlanRecord>>
{
}

public class GetAllFlightPlanCommandHandler : IRequestHandler<GetAllFlightPlanCommand, IEnumerable<FlightPlanRecord>>
{
    private readonly AuroraService auroraService;

    public GetAllFlightPlanCommandHandler(AuroraService auroraService)
    {
        this.auroraService = auroraService;
    }

    public async Task<IEnumerable<FlightPlanRecord>> Handle(GetAllFlightPlanCommand request, CancellationToken cancellationToken)
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
                return Enumerable.Empty<FlightPlanRecord>();
            }

            var flightPlanList = new List<FlightPlanRecord>(callsigns.Length - 1);
            foreach (var callsign in callsigns.Skip(1))
            {
                if (callsign == string.Empty)
                {
                    continue;
                }
                await auroraTcp.GetStream().FlushAsync(cancellationToken);
                dataBytes = System.Text.Encoding.ASCII.GetBytes($"#FP;{callsign};{Environment.NewLine}");
                auroraTcp.GetStream().Write(dataBytes, 0, dataBytes.Length);
                auroraResponse = await streamReader.ReadLineAsync(cancellationToken) ?? string.Empty;
                var flightPlanData = auroraResponse.Split(';');

                if (flightPlanData.Count() <= 1)
                {
                    continue;
                }
                flightPlanData[0] = callsign; // replace the first slot by the callsign
                flightPlanList.Add(FlightPlanRecord.ParseFromString(flightPlanData.Skip(1).ToArray()));
            }
            return flightPlanList;
        });
    }
}


public enum FlightPlanIndexes
{
    Callsign = 0,
    DepartureICAO = 1,
    ArrivingICAO = 2,
    AlternateICAO = 3,
    EstimatedDepartureTime = 4,
    AircraftICAO = 5,
    WakeTurbulence = 6,
    FlightRules = 7,
    FlightType = 8,
    Equipment = 9,
    CruisingAltitude = 10,
    CruisingSpeed = 11,
    Endurance = 12,
    EstimatedFlightTime = 13,
    Route = 14,
    Remarks = 15
}

public record FlightPlanRecord(
    string Callsign,
    string DepartureICAO,
    string ArrivingICAO,
    string AlternateICAO,
    string EstimatedDepartureTime,
    string AircraftICAO,
    string WakeTurbulence,
    string FlightType,
    string FlightRules,
    string Equipment,
    string CruisingAltitude,
    string CruisingSpeed,
    string Endurance,
    string EstimatedFlightTime,
    string Route,
    string Remarks
)
{
    public static FlightPlanRecord ParseFromString(string[] values)
    {
        if (values.Length <= (int)FlightPlanIndexes.Remarks)
        {
            throw new FormatException("The values array is too short to be parsed as a FlightPlanRecord");
        }
        return new FlightPlanRecord(
            Callsign: values[(int)FlightPlanIndexes.Callsign],
            DepartureICAO: values[(int)FlightPlanIndexes.DepartureICAO],
            ArrivingICAO: values[(int)FlightPlanIndexes.ArrivingICAO],
            AlternateICAO: values[(int)FlightPlanIndexes.AlternateICAO],
            EstimatedDepartureTime: values[(int)FlightPlanIndexes.EstimatedDepartureTime],
            AircraftICAO: values[(int)FlightPlanIndexes.AircraftICAO],
            WakeTurbulence: values[(int)FlightPlanIndexes.WakeTurbulence],
            FlightType: values[(int)FlightPlanIndexes.FlightType],
            FlightRules: values[(int)FlightPlanIndexes.FlightRules],
            Equipment: values[(int)FlightPlanIndexes.Equipment],
            CruisingAltitude: values[(int)FlightPlanIndexes.CruisingAltitude],
            CruisingSpeed: values[(int)FlightPlanIndexes.CruisingSpeed],
            Endurance: values[(int)FlightPlanIndexes.Endurance],
            EstimatedFlightTime: values[(int)FlightPlanIndexes.EstimatedFlightTime],
            Route: values[(int)FlightPlanIndexes.Route],
            Remarks: values[(int)FlightPlanIndexes.Remarks]
        );
    }
}