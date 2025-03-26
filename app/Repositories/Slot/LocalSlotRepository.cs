using aurastrip_adapter.Contexts;
using Microsoft.EntityFrameworkCore;

namespace aurastrip_adapter.Repositories.Slot
{
    public class LocalSlotRepository : ISlotRepository
    {
        private readonly ConfigurationDbContext context;

        public LocalSlotRepository(ConfigurationDbContext context)
        {
            this.context = context;
        }

        public Models.Slot? GetById(Guid id)
            => context.Slots.FirstOrDefault(slot => slot.Id == id);

        public IEnumerable<Models.Slot> GetAll()
            => context.Slots.AsEnumerable();

        public IEnumerable<Models.Slot> GetAllAssignedToColumn(Guid columnId)
            => context.Slots.Where(slot => slot.ColumnId.Equals(columnId));

        public void Create(Models.Slot model)
        {
            context.Slots.Add(model);
        }

        public void Update(Models.Slot model)
        {
            var slotToUpdate = context.Slots.Find(model.Id);
            if(slotToUpdate is null)
            {
                return;
            }
            
            slotToUpdate.Name = model.Name;
            slotToUpdate.ColumnId = model.ColumnId;
            slotToUpdate.Index = model.Index;
            slotToUpdate.SizePercentage = model.SizePercentage;
            slotToUpdate.Type = model.Type;
            slotToUpdate.Data = model.Data;
        }

        public void Delete(Guid id)
        {
            var strip = context.Slots.FirstOrDefault(slot => slot.Id == id);
            if (strip is null)
            {
                return;
            }

            context.Slots.Remove(strip);
        }

        public void DeleteAllAssignToColumn(Guid columnId)
        {
            var slots = context.Slots.Where(slot => slot.ColumnId == columnId);
            context.Slots.RemoveRange(slots);
        }

        public void Save()
            => context.SaveChanges();

        public async Task SaveAsync(CancellationToken cancellationToken)
            => await context.SaveChangesAsync(cancellationToken);
    }
}
