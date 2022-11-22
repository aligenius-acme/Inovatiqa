using System;

namespace Inovatiqa.Database.Models
{
    public partial class MigrationVersionInfo
    {
        public long Version { get; set; }
        public DateTime? AppliedOn { get; set; }
        public string Description { get; set; }
    }
}
