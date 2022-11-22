using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Messages.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Messages
{
    public partial class EmailAccountService : IEmailAccountService
    {
        #region Fields

        private readonly IRepository<EmailAccount> _emailAccountRepository;

        #endregion

        #region Ctor

        public EmailAccountService(IRepository<EmailAccount> emailAccountRepository)
        {
            _emailAccountRepository = emailAccountRepository;
        }

        #endregion

        #region Methods

        public virtual EmailAccount GetEmailAccountById(int emailAccountId)
        {
            if (emailAccountId == 0)
                return null;

            return _emailAccountRepository.GetById(emailAccountId);
        }

        public virtual IList<EmailAccount> GetAllEmailAccounts()
        {
            var query = from ea in _emailAccountRepository.Query()
                        orderby ea.Id
                        select ea;

            var emailAccounts = query.ToList();

            return emailAccounts;
        }

        #endregion
    }
}