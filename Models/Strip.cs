﻿using System.ComponentModel.DataAnnotations;

namespace aurastrip_adapter.Models
{
    public class Strip : IGenericModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Callsign { get; set; } = string.Empty;
        public Guid SlotId {  get; set; }
        public string Language { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
        public bool ClearedPush { get; set; } = false;
    }
}