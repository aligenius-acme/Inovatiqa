using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Messages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Messages
{
    public partial class MessageTemplateService : IMessageTemplateService
    {
        #region Fields

        private readonly IRepository<MessageTemplate> _messageTemplateRepository;

        #endregion

        #region Ctor

        public MessageTemplateService(IRepository<MessageTemplate> messageTemplateRepository)
        {
            _messageTemplateRepository = messageTemplateRepository;
        }

        #endregion

        #region Methods

        public virtual IList<MessageTemplate> GetMessageTemplatesByName(string messageTemplateName, int? storeId = null)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException(nameof(messageTemplateName));

            var templates = _messageTemplateRepository.Query()
                    .Where(messageTemplate => messageTemplate.Name.Equals(messageTemplateName))
                    .OrderBy(messageTemplate => messageTemplate.Id).ToList();

            return templates;
        }

        #endregion
    }
}