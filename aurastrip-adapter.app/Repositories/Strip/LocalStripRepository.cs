using aurastrip_adapter.Contexts;
using Microsoft.EntityFrameworkCore;

namespace aurastrip_adapter.Repositories.Strip
{
    public class LocalStripRepository : IStripRepository
    {
        private readonly ConfigurationDbContext context;

        public LocalStripRepository(ConfigurationDbContext context)
        {
            this.context = context;
        }

        public Models.Strip? GetById(Guid id)
            => context.Strips.FirstOrDefault(strip => strip.Id == id);

        public Models.Strip? GetBySlotAndCallsign(Guid slotId, string callsign)
            => context.Strips.FirstOrDefault(strip => strip.SlotId == slotId && strip.Callsign == callsign);

        public IEnumerable<Models.Strip> GetAll()
            => context.Strips.AsEnumerable();

        public IEnumerable<Models.Strip> GetAllAssignedToSlot(Guid slotId)
            => context.Strips.Where(strip => strip.SlotId.Equals(slotId));

        public IEnumerable<Models.Strip> GetAllAssignedToConfiguration(Guid configurationId)
            => context.Strips.Where(strip => strip.ConfigurationId.Equals(configurationId));

        public void Create(Models.Strip model)
        {
            context.Strips.Add(model);
        }

        public void Update(Models.Strip model)
        {
            var existingStrip = context.Strips.Find(model.Id);
            if (existingStrip is null)
            {
                return;
            }
            
            existingStrip.Callsign = model.Callsign;
            existingStrip.ConfigurationId = model.ConfigurationId;
            existingStrip.Comment = model.Comment;
            existingStrip.SlotId = model.SlotId;
            existingStrip.Index = model.Index;
            existingStrip.Gate = model.Gate;
            existingStrip.Language = model.Language;
            existingStrip.ClearedPush = model.ClearedPush;
            existingStrip.ClearedStart = model.ClearedStart;
            existingStrip.ClearedSSR = model.ClearedSSR;
            existingStrip.RunwayClearance = model.RunwayClearance;
        }

        public void Delete(Guid id)
        {
            var strip = context.Strips.FirstOrDefault(strip => strip.Id == id);
            if(strip is null)
            {
                return;
            }

            context.Strips.Remove(strip);
        }

        public void DeleteAllAssignedToSlot(Guid slotId)
        {
            var strips = context.Strips.Where(strip => strip.SlotId == slotId);
            context.Strips.RemoveRange(strips);
        }

        public void Save()
            => context.SaveChanges();

        public async Task SaveAsync(CancellationToken cancellationToken)
            => await context.SaveChangesAsync(cancellationToken);
    }
}
