using System.ComponentModel.DataAnnotations;

namespace aurastrip_adapter.Models
{
    public class Column : IGenericModel
    {
        [Key]
        public Guid Id { get; set; }
        public int Index { get; set; }
        public Guid ConfigurationId { get; set; }
    }
}
