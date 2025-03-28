using System.ComponentModel.DataAnnotations;

namespace aurastrip_adapter.Models
{
    public class Slot : IGenericModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }= string.Empty;
        public int Index { get; set; }
        public Guid ColumnId { get; set; }
        public int SizePercentage { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
