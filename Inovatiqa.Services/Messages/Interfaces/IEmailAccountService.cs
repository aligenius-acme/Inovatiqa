using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Messages.Interfaces
{
    public partial interface IEmailAccountService
    {
        EmailAccount GetEmailAccountById(int emailAccountId);

        IList<EmailAccount> GetAllEmailAccounts();
    }
}
