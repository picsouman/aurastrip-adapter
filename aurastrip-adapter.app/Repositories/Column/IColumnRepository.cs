

namespace aurastrip_adapter.Repositories.Column
{
    public interface IColumnRepository : IGenericRepository<Models.Column>
    {
        void DeleteAllAssignToConfiguration(Guid configurationId);
        IEnumerable<Models.Column> GetAllAssignedToConfiguration(Guid configurationId);
    }
}
