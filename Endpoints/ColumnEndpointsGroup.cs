using aurastrip_adapter.Models;
using aurastrip_adapter.Services;

namespace aurastrip_adapter.Controllers
{
    public static class ColumnEndpointsGroup
    {
        public static RouteGroupBuilder MapColumnEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAll);
            group.MapGet("/{id}", GetById);
            group.MapPost("/", Create);
            group.MapPut("/", Update);
            group.MapDelete("/{id}", Delete);

            var slotGroup = group.MapGroup("{columnId}/slots");
            slotGroup.MapGet("/", GetSlotsForColumn);
            slotGroup.MapDelete("/", DeleteSlotsForColumn);

            return group;
        }

        private static async Task<IResult> DeleteSlotsForColumn(Guid columnId, SlotService service, CancellationToken cancellation)
        {
            await service.DeleteAllAssignedToColumnAsync(columnId, cancellation);
            return Results.Ok();
        }

        private static IResult GetSlotsForColumn(Guid columnId, SlotService service)
            => Results.Ok(service.GetAllForColumnId(columnId));

        public static IResult GetAll(ColumnService service)
            => Results.Ok(service.GetAll());

        public static IResult GetById(Guid id, ColumnService service)
        {
            var strip = service.GetById(id);
            if (strip is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(strip);
        }

        public static async Task<IResult> Create(Column column, ColumnService service, CancellationToken cancellation)
        {
            var createdSlot = await service.Create(column, cancellation);
            return Results.Ok(createdSlot);
        }

        public static async Task<IResult> Update(Column column, ColumnService service, CancellationToken cancellation)
        {
            var updatedSlot = await service.Update(column, cancellation);
            if(updatedSlot is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(updatedSlot);
        }

        private static async Task<IResult> Delete(Guid id, ColumnService service)
        {
            await service.DeleteAsync(id);
            return Results.Ok();
        }
    }
}
