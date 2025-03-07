﻿using aurastrip_adapter.Contexts;
using aurastrip_adapter.Repositories.Configuration;
using Microsoft.EntityFrameworkCore;

namespace aurastrip_adapter.Services.Repositories.Configuration
{
    public class LocalConfigurationRespository : IConfigurationRepository
    {
        private readonly ConfigurationDbContext context;

        public LocalConfigurationRespository(ConfigurationDbContext context)
        {
            this.context = context;
        }

        public Models.Configuration? GetById(Guid id)
            => context.Configurations.FirstOrDefault(configuration => configuration.Id == id);

        public IEnumerable<Models.Configuration> GetAll()
            => context.Configurations.AsEnumerable();

        public void Create(Models.Configuration model)
            => context.Configurations.Add(model);

        public void Update(Models.Configuration model)
        {
            var configuration = context.Configurations.Find(model.Id);
            if (configuration is null)
            {
                return;
            }
            
            configuration.Id = model.Id;
            configuration.Name = model.Name;
            configuration.CreationUtc = model.CreationUtc;
        }

        public void Delete(Guid id)
        {
            var configuration = context.Configurations.FirstOrDefault(configuration => configuration.Id == id);
            if (configuration is null)
            {
                return;
            }

            context.Configurations.Remove(configuration);
        }

        public void Save()
            => context.SaveChanges();

        public async Task SaveAsync(CancellationToken cancellationToken)
            => await context.SaveChangesAsync(cancellationToken);
    }
}
