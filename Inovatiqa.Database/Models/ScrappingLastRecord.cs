using System;

namespace Inovatiqa.Database.Models
{
    public partial class ScrappingLastRecord
    {
        public int Id { get; set; }
        public string LastUrl { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? ThreadNo { get; set; }
    }
}
