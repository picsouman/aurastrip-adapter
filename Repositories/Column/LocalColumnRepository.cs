using aurastrip_adapter.Contexts;

namespace aurastrip_adapter.Repositories.Column
{
    public class LocalColumnRepository : IColumnRepository
    {
        private readonly ConfigurationDbContext context;

        public LocalColumnRepository(ConfigurationDbContext context)
        {
            this.context = context;
        }

        public void Create(Models.Strip model)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.Strip> GetAll()
        {
            throw new NotImplementedException();
        }

        public Models.Strip? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Update(Models.Strip model)
        {
            throw new NotImplementedException();
        }
    }
}
