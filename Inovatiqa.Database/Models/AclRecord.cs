namespace Inovatiqa.Database.Models
{
    public partial class AclRecord
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public int CustomerRoleId { get; set; }
        public int EntityId { get; set; }

        public virtual CustomerRole CustomerRole { get; set; }
    }
}
