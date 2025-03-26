using aurastrip_adapter.Endpoints.Dtos;
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
            group.MapPost("/", Update);
            group.MapPut("/", Create);
            group.MapDelete("/{id}", DeleteStrip);
            group.MapGet("/{id}/full", GetByIdFullConfiguration);

            var columnGroup = group.MapGroup("{configurationId}/columns");
            columnGroup.MapGet("/", GetSlotsForColumn);
            columnGroup.MapDelete("/", DeleteSlotsForColumn);

            var stripGroup = group.MapGroup("{configurationId}/strips");
            stripGroup.MapGet("/", GetStripsForConfiguration);

            return group;
        }

        private static async Task<IResult> DeleteSlotsForColumn(Guid configurationId, ColumnService service)
        {
            await service.DeleteAllAssignedToConfigurationAsync(configurationId);
            return Results.Ok();
        }

        private static IResult GetSlotsForColumn(Guid configurationId, ColumnService service)
            => Results.Ok(service.GetAllForConfigurationId(configurationId));

        private static IResult GetStripsForConfiguration(Guid configurationId, StripService service)
            => Results.Ok(service.GetAllForConfigurationId(configurationId));

        public static IResult GetAll(ConfigurationService service)
            => Results.Ok(service.GetAll());

        public static IResult GetById(Guid id, ConfigurationService service)
        {
            var configuration = service.GetById(id);
            if (configuration is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(configuration);
        }

        private static IResult GetByIdFullConfiguration(
            Guid id,
            ConfigurationService configurationService,
            ColumnService columnService,
            SlotService slotService)
        {
            var configuration = configurationService.GetById(id);
            if (configuration is null)
            {
                return Results.NotFound();
            }

            var result = new ConfigurationFullDto()
            {
                Id = configuration.Id,
                Name = configuration.Name,
                CreationUtc = configuration.CreationUtc,
                Columns = columnService
                    .GetAllForConfigurationId(id)
                    .OrderBy(column => column.Index)
                    .Select(column => new ColumnFullDto()
                    {
                        Id = column.Id,
                        Slots = slotService
                            .GetAllForColumnId(column.Id)
                            .OrderBy(slot => slot.Index)
                            .Select(slot => new SlotFullDto()
                            {
                                Id = slot.Id,
                                Name = slot.Name,
                                SizePercentage = slot.SizePercentage,
                                Type = slot.Type,
                                Data = slot.Data
                            })
                            .ToArray(),
                    })
                    .ToArray(),
            };

            var columns = columnService.GetAllForConfigurationId(id);

            return Results.Ok(result);
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
