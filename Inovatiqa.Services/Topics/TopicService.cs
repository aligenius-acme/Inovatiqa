using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Topics.Interfaces;
using System.Linq;

namespace Inovatiqa.Services.Topics
{
    public partial class TopicService : ITopicService
    {
        #region Fields

        private readonly IRepository<Topic> _topicRepository;

        #endregion

        #region Ctor

        public TopicService(IRepository<Topic> topicRepository)
        {
            _topicRepository = topicRepository;
        }

        #endregion

        #region Methods

        public virtual Topic GetTopicBySystemName(string systemName, int storeId = 0, bool showHidden = false)
        {
            var query = _topicRepository.Query();
            query = query.Where(t => t.SystemName == systemName);
            if (!showHidden)
                query = query.Where(c => c.Published);
            query = query.OrderBy(t => t.Id);
            var topics = query.ToList();

            return topics.FirstOrDefault();

        }

        #endregion
    }
}