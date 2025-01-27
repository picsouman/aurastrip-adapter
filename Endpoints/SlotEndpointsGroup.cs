using aurastrip_adapter.Models;
using aurastrip_adapter.Services;

namespace aurastrip_adapter.Endpoints
{
    public static class SlotEndpointsGroup
    {
        public static RouteGroupBuilder MapSlotEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAll);
            group.MapGet("/{id}", GetById);
            group.MapPost("/", Update);
            group.MapPut("/", Create);
            group.MapDelete("/{id}", DeleteStrip);

            var slotGroup = group.MapGroup("{slotId}/strips");
            slotGroup.MapGet("/", GetStripsForSlot);
            slotGroup.MapDelete("/", DeleteStripsForSlot);

            return group;
        }

        private static async Task<IResult> DeleteStripsForSlot(Guid slotId, StripService service)
        {
            await service.DeleteAllAssignedToSlotAsync(slotId);
            return Results.Ok();
        }

        private static IResult GetStripsForSlot(Guid slotId, StripService service)
            => Results.Ok(service.GetAllForSlotId(slotId));

        public static IResult GetAll(SlotService service)
            => Results.Ok(service.GetAll());

        public static IResult GetById(Guid id, SlotService service)
        {
            var strip = service.GetById(id);
            if (strip is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(strip);
        }

        public static async Task<IResult> Create(Slot slot, SlotService service, CancellationToken cancellation)
        {
            var createdSlot = await service.Create(slot, cancellation);
            return Results.Ok(createdSlot);
        }

        public static async Task<IResult> Update(Slot slot, SlotService service, CancellationToken cancellation)
        {
            var updatedSlot = await service.Update(slot, cancellation);
            if(updatedSlot is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(updatedSlot);
        }

        private static async Task<IResult> DeleteStrip(Guid id, SlotService service)
        {
            await service.DeleteAsync(id);
            return Results.Ok();
        }
    }
}
