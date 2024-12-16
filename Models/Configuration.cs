using System.ComponentModel.DataAnnotations;

namespace aurastrip_adapter.Models
{
    public class Configuration : IGenericModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationUtc { get; set; }
    }
}
