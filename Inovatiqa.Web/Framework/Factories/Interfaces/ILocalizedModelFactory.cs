using Inovatiqa.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Framework.Factories.Interfaces
{
    public partial interface ILocalizedModelFactory
    {
        IList<T> PrepareLocalizedModels<T>(Action<T, int> configure = null) where T : ILocalizedLocaleModel;
    }
}