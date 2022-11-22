using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Topics.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Topics;
using System;

namespace Inovatiqa.Web.Factories
{
    public partial class TopicModelFactory : ITopicModelFactory
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;
        private readonly ITopicService _topicService;

        #endregion

        #region Ctor

        public TopicModelFactory(ITopicService topicService,
            IUrlRecordService urlRecordService)
        {
            _topicService = topicService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        protected virtual TopicModel PrepareTopicModel(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            var model = new TopicModel
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IncludeInSitemap = topic.IncludeInSitemap,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? string.Empty : topic.Title,
                Body = topic.IsPasswordProtected ? string.Empty : topic.Body,
                MetaKeywords = topic.MetaKeywords,
                MetaDescription = topic.MetaDescription,
                MetaTitle = topic.MetaTitle,
                SeName = _urlRecordService.GetActiveSlug(topic.Id, InovatiqaDefaults.TopicSlugName, InovatiqaDefaults.LanguageId),
                TopicTemplateId = topic.TopicTemplateId
            };

            return model;
        }

        #endregion

        #region Methods

        public virtual TopicModel PrepareTopicModelBySystemName(string systemName)
        {
            var topic = _topicService.GetTopicBySystemName(systemName, InovatiqaDefaults.StoreId);
            if (topic == null)
                return null;

            return PrepareTopicModel(topic);
        }

        #endregion
    }
}