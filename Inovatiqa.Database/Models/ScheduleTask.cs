using System;

namespace Inovatiqa.Database.Models
{
    public partial class ScheduleTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Seconds { get; set; }
        public bool Enabled { get; set; }
        public bool StopOnError { get; set; }
        public DateTime? LastStartUtc { get; set; }
        public DateTime? LastEndUtc { get; set; }
        public DateTime? LastSuccessUtc { get; set; }
    }
}
