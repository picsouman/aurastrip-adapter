using aurastrip_adapter.Contexts;
using aurastrip_adapter.Models;
using aurastrip_adapter.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace aurastrip_adapter.Controllers
{
    public static class StripEndpointsGroup
    {
        public static RouteGroupBuilder MapStripEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAll);
            group.MapGet("/{id}", GetById);
            group.MapPost("/", SetStrip);

            return group;
        }

        public static IResult GetAll(StripService service)
        {
            return Results.Ok(service.GetAll());
        }

        public static IResult GetById(Guid id, StripService service)
        {
            var strip = service.GetById(id);
            if (strip is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(strip);
        }

        public static async Task<IResult> SetStrip(Strip strip, StripService service, CancellationToken cancellation)
        {
            return Results.Ok(await service.SetStripAsync(strip, cancellation));
        }
    }
}
