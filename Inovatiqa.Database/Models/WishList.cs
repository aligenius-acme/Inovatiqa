namespace Inovatiqa.Database.Models
{
    public partial class WishList
    {
        public int Id { get; set; }
        public string ListName { get; set; }
        public bool IsSharedList { get; set; }
        public int CustomerId { get; set; }
    }
}
