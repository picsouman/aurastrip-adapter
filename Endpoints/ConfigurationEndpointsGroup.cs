using aurastrip_adapter.Models;
using aurastrip_adapter.Services;

namespace aurastrip_adapter.Controllers
{
    public static class ConfigurationEndpointsGroup
    {
        public static RouteGroupBuilder MapConfigurationEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAll);
            group.MapGet("/{id}", GetById);
            group.MapPost("/", Create);
            group.MapPut("/", Update);
            group.MapDelete("/{id}", DeleteStrip);

            var columnGroup = group.MapGroup("{configurationId}/columns");
            columnGroup.MapGet("/", GetSlotsForColumn);
            columnGroup.MapDelete("/", DeleteSlotsForColumn);

            return group;
        }

        private static async Task<IResult> DeleteSlotsForColumn(Guid configurationId, ColumnService service)
        {
            await service.DeleteAllAssignedToConfigurationAsync(configurationId);
            return Results.Ok();
        }

        private static IResult GetSlotsForColumn(Guid configurationId, ColumnService service)
            => Results.Ok(service.GetAllForConfigurationId(configurationId));

        public static IResult GetAll(ConfigurationService service)
            => Results.Ok(service.GetAll());

        public static IResult GetById(Guid id, ConfigurationService service)
        {
            var strip = service.GetById(id);
            if (strip is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(strip);
        }

        public static async Task<IResult> Create(Configuration configuration, ConfigurationService service, CancellationToken cancellation)
        {
            var createdSlot = await service.Create(configuration, cancellation);
            return Results.Ok(createdSlot);
        }

        public static async Task<IResult> Update(Configuration configuration, ConfigurationService service, CancellationToken cancellation)
        {
            var updatedSlot = await service.Update(configuration, cancellation);
            if (updatedSlot is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(updatedSlot);
        }

        private static async Task<IResult> DeleteStrip(Guid id, ConfigurationService service)
        {
            await service.DeleteAsync(id);
            return Results.Ok();
        }

    }
}
