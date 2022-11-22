using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Extensions
{
    public static class CommonExtensions
    {
        public static bool SelectionIsNotPossible(this IList<SelectListItem> items, bool ignoreZeroValue = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            return items.Count(x => !ignoreZeroValue || !x.Value.ToString().Equals("0")) < 2;
        }
    }
}