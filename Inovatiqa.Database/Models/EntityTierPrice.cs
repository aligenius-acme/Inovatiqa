using System;

namespace Inovatiqa.Database.Models
{
    public partial class EntityTierPrice
    {
        public int Id { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public int? CustomerId { get; set; }
        public decimal? Rate { get; set; }
        public DateTime? StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }
    }
}
