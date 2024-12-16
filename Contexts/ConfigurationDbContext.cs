using aurastrip_adapter.Models;
using Microsoft.EntityFrameworkCore;

namespace aurastrip_adapter.Contexts
{
    public class ConfigurationDbContext : DbContext
    {
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Strip> Strips { get; set; }

        private readonly string DbPath;

        public ConfigurationDbContext() {
            //var folder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(folder);
            DbPath = "configuration.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"DataSource={DbPath}");
        }
    }
}
