
namespace aurastrip_adapter.Repositories.Slot
{
    public interface ISlotRepository : IGenericRepository<Models.Slot>
    {
        IEnumerable<Models.Slot> GetAllAssignedToColumn(Guid columnId);
        void DeleteAllAssignToColumn(Guid columnId);
    }
}
