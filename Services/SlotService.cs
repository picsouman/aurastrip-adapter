using aurastrip_adapter.Models;
using aurastrip_adapter.Repositories.Slot;
using aurastrip_adapter.Repositories.Strip;

namespace aurastrip_adapter.Services
{
    public class SlotService
    {
        private readonly ISlotRepository slotRepository;
        private readonly StripService stripService;

        public SlotService(
            ISlotRepository repository,
            StripService stripService)
        {
            this.slotRepository = repository;
            this.stripService = stripService;
        }

        public IEnumerable<Slot> GetAll()
            => slotRepository.GetAll();

        public IEnumerable<Slot> GetAllForColumnId(Guid columnId)
            => slotRepository.GetAllAssignedToColumn(columnId);

        public Slot? GetById(Guid id)
            => slotRepository.GetById(id);

        public async Task<Slot> Create(Slot model, CancellationToken cancellation)
        {
            var slot = slotRepository.GetById(model.Id);
            if (slot is not null)
            {
                slotRepository.Update(model);
            }
            else
            {
                model.Id = Guid.NewGuid();
                slotRepository.Create(model);
            }

            await slotRepository.SaveAsync(cancellation);

            return model;
        }

        public async Task<Slot?> Update(Slot slot, CancellationToken cancellation = default)
        {
            var slotToUpdate = slotRepository.GetById(slot.Id);
            if(slotToUpdate is null)
            {
                return null;
            }

            slotRepository.Update(slot);
            await slotRepository.SaveAsync(cancellation);
            return slot;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await stripService.DeleteAllAssignedToSlotAsync(id, cancellationToken);

            slotRepository.Delete(id);
            await slotRepository.SaveAsync(cancellationToken);
        }

        public async Task DeleteAllAssignedToColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
        {
            var slotToDelete = slotRepository.GetAllAssignedToColumn(columnId);
            foreach(Slot slot in slotToDelete)
            {
                await stripService.DeleteAllAssignedToSlotAsync(slot.Id, cancellationToken);
            }

            slotRepository.DeleteAllAssignToColumn(columnId);
            await slotRepository.SaveAsync(cancellationToken);
        }
    }
}
