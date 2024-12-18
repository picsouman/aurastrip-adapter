using aurastrip_adapter.Models;
using aurastrip_adapter.Repositories.Configuration;

namespace aurastrip_adapter.Services
{
    public class ConfigurationService
    {
        private readonly IConfigurationRepository configurationRepository;
        private readonly ColumnService columnService;

        public ConfigurationService(
            IConfigurationRepository configurationRepository,
            ColumnService columnService)
        {
            this.configurationRepository = configurationRepository;
            this.columnService = columnService;
        }

        public IEnumerable<Configuration> GetAll()
            => configurationRepository.GetAll();

        public Configuration? GetById(Guid id)
            => configurationRepository.GetById(id);

        public async Task<Configuration> Create(Configuration model, CancellationToken cancellation)
        {
            var configuration = configurationRepository.GetById(model.Id);
            if (configuration is not null)
            {
                configurationRepository.Update(model);
            }
            else
            {
                model.Id = Guid.NewGuid();
                configurationRepository.Create(model);
            }

            await configurationRepository.SaveAsync(cancellation);

            return model;
        }

        public async Task<Configuration?> Update(Configuration configuration, CancellationToken cancellation = default)
        {
            var configurationToUpdate = configurationRepository.GetById(configuration.Id);
            if (configurationToUpdate is null)
            {
                return null;
            }

            configurationRepository.Update(configuration);
            await configurationRepository.SaveAsync(cancellation);
            return configuration;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await columnService.DeleteAllAssignedToConfigurationAsync(id, cancellationToken);

            configurationRepository.Delete(id);
            await configurationRepository.SaveAsync(cancellationToken);
        }
    }
}
