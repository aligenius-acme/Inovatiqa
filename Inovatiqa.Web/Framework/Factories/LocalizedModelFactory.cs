using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Web.Framework.Factories.Interfaces;
using Inovatiqa.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Framework.Factories
{
    public partial class LocalizedModelFactory : ILocalizedModelFactory
    {
        #region Fields
        

        #endregion

        #region Ctor

        public LocalizedModelFactory()
        {
        }

        #endregion

        #region Methods

        public virtual IList<T> PrepareLocalizedModels<T>(Action<T, int> configure = null) where T : ILocalizedLocaleModel
        {
            var availableLanguages = new List<Language>();
            availableLanguages.Add(new Language { Id = InovatiqaDefaults.LanguageId}
            );

            var localizedModels = availableLanguages.Select(language =>
            {
                var localizedModel = Activator.CreateInstance<T>();

                localizedModel.LanguageId = language.Id;

                configure?.Invoke(localizedModel, localizedModel.LanguageId);

                return localizedModel;
            }).ToList();

            return localizedModels;
        }

        #endregion
    }
}