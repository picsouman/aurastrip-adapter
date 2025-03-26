using aurastrip_adapter.Models;

namespace aurastrip_adapter.Repositories
{
    public interface IGenericRepository<T> where T : IGenericModel
    {
        void Create(T model);
        T? GetById(Guid id);
        IEnumerable<T> GetAll();
        void Update(T model);
        void Delete(Guid id);
        void Save();
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
