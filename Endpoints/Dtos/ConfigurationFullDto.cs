namespace aurastrip_adapter.Endpoints.Dtos
{
    public class ConfigurationFullDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationUtc { get; set; }
        public ColumnFullDto[] Columns { get; set; } = Array.Empty<ColumnFullDto>();
    }

    public class  ColumnFullDto
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public SlotFullDto[] Slots { get; set; } = Array.Empty<SlotFullDto>();
    }

    public class SlotFullDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Index { get; set; }
        public int SizePercentage { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
