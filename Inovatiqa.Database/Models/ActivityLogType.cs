using System.Collections.Generic;

namespace Inovatiqa.Database.Models
{
    public partial class ActivityLogType
    {
        public ActivityLogType()
        {
            ActivityLog = new HashSet<ActivityLog>();
        }

        public int Id { get; set; }
        public string SystemKeyword { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public virtual ICollection<ActivityLog> ActivityLog { get; set; }
    }
}
