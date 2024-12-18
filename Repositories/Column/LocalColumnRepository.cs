using aurastrip_adapter.Contexts;
using Microsoft.EntityFrameworkCore;

namespace aurastrip_adapter.Repositories.Column
{
    public class LocalColumnRepository : IColumnRepository
    {
        private readonly ConfigurationDbContext context;

        public LocalColumnRepository(ConfigurationDbContext context)
        {
            this.context = context;
        }

        public Models.Column? GetById(Guid id)
            => context.Columns.FirstOrDefault(column => column.Id == id);

        public IEnumerable<Models.Column> GetAll()
            => context.Columns.AsEnumerable();

        public IEnumerable<Models.Column> GetAllAssignedToConfiguration(Guid configurationId)
            => context.Columns.Where(column => column.ConfigurationId.Equals(configurationId));

        public void Create(Models.Column model)
        {
            context.Columns.Add(model);
        }

        public void Update(Models.Column model)
        {
            context.Entry(model).State = EntityState.Modified;
        }

        public void Delete(Guid id)
        {
            var columns = context.Columns.FirstOrDefault(context => context.Id == id);
            if (columns is null)
            {
                return;
            }

            context.Columns.Remove(columns);
        }

        public void DeleteAllAssignToConfiguration(Guid configurationId)
        {
            var columns = context.Columns.Where(column => column.ConfigurationId == configurationId);
            context.Columns.RemoveRange(columns);
        }

        public void Save()
            => context.SaveChanges();

        public async Task SaveAsync(CancellationToken cancellationToken)
            => await context.SaveChangesAsync(cancellationToken);
    }
}
