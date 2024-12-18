using aurastrip_adapter.Models;

namespace aurastrip_adapter.Repositories.Strip
{
    public interface IStripRepository : IGenericRepository<Models.Strip>
    {
        Models.Strip? GetBySlotAndCallsign(Guid slotId, string callsign);
        IEnumerable<Models.Strip> GetAllAssignedToSlot(Guid slotId);

        void DeleteAllAssignedToSlot(Guid slotId);
    }
}
