using Inovatiqa.Database.Models;
namespace Inovatiqa.Services.Topics.Interfaces
{
    public partial interface ITopicService
    {
        Topic GetTopicBySystemName(string systemName, int storeId = 0, bool showHidden = false);
    }
}
