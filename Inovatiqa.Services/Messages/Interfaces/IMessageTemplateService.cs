using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface IMessageTemplateService
    {
        IList<MessageTemplate> GetMessageTemplatesByName(string messageTemplateName, int? storeId = null);
    }
}
