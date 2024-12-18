using aurastrip_adapter.Models;
using aurastrip_adapter.Repositories.Strip;

namespace aurastrip_adapter.Services
{
    public class StripService
    {
        private readonly IStripRepository repository;

        public StripService(IStripRepository repository)
        {
            this.repository = repository;
        }

        public IEnumerable<Strip> GetAll()
            => repository.GetAll();

        public Strip? GetById(Guid id)
            => repository.GetById(id);

        public IEnumerable<Strip> GetAllForSlotId(Guid slotId)
            => repository.GetAllAssignedToSlot(slotId);

        public async Task<Strip> SetStripAsync(Strip model, CancellationToken cancellation)
        {
            var strip = repository.GetById(model.Id);
            if(strip is null)
            {
                model.Id = Guid.NewGuid();
                repository.Create(model);
            }
            else
            {
                repository.Update(model);
            }

            await repository.SaveAsync(cancellation);

            return model;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            repository.Delete(id);
            await repository.SaveAsync(cancellationToken);
        }

        public async Task DeleteAllAssignedToSlotAsync(Guid slotId, CancellationToken cancellationToken = default)
        {
            repository.DeleteAllAssignedToSlot(slotId);
            await repository.SaveAsync(cancellationToken);
        }
    }
}
