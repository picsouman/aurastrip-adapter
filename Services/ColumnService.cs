using aurastrip_adapter.Models;
using aurastrip_adapter.Repositories.Column;

namespace aurastrip_adapter.Services
{
    public class ColumnService
    {
        private readonly IColumnRepository columnRepository;
        private readonly SlotService slotService;

        public ColumnService(
            IColumnRepository columnRepository,
            SlotService slotService)
        {
            this.columnRepository = columnRepository;
            this.slotService = slotService;
        }

        public IEnumerable<Column> GetAll()
            => columnRepository.GetAll();

        public IEnumerable<Column> GetAllForConfigurationId(Guid configurationId)
            => columnRepository.GetAllAssignedToConfiguration(configurationId);

        public Column? GetById(Guid id)
            => columnRepository.GetById(id);

        public async Task<Column> Create(Column model, CancellationToken cancellation)
        {
            var slot = columnRepository.GetById(model.Id);
            if (slot is not null)
            {
                columnRepository.Update(model);
            }
            else
            {
                model.Id = Guid.NewGuid();
                columnRepository.Create(model);
            }

            await columnRepository.SaveAsync(cancellation);

            return model;
        }

        public async Task<Column?> Update(Column slot, CancellationToken cancellation = default)
        {
            var columnToUpdate = columnRepository.GetById(slot.Id);
            if (columnToUpdate is null)
            {
                return null;
            }

            columnRepository.Update(slot);
            await columnRepository.SaveAsync(cancellation);
            return slot;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await slotService.DeleteAllAssignedToColumnAsync(id, cancellationToken);

            columnRepository.Delete(id);
            await columnRepository.SaveAsync(cancellationToken);
        }

        public async Task DeleteAllAssignedToConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
        {
            var columnToDelete = columnRepository.GetAllAssignedToConfiguration(configurationId);
            foreach (Column column in columnToDelete)
            {
                await slotService.DeleteAllAssignedToColumnAsync(column.Id, cancellationToken);
            }

            columnRepository.DeleteAllAssignToConfiguration(configurationId);
            await columnRepository.SaveAsync(cancellationToken);
        }
    }
}
