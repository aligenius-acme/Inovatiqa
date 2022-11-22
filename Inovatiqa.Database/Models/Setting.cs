namespace Inovatiqa.Database.Models
{
    public partial class Setting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int StoreId { get; set; }
        public int? TypeId { get; set; }
    }
}
