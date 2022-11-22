using Inovatiqa.Web.Models.Topics;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface ITopicModelFactory
    {
        TopicModel PrepareTopicModelBySystemName(string systemName);
    }
}
