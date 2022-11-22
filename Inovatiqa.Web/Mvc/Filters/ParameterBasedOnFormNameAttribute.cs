using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inovatiqa.Web.Mvc.Filters
{
    public sealed class ParameterBasedOnFormNameAttribute : TypeFilterAttribute
    {
        #region Ctor

        public ParameterBasedOnFormNameAttribute(string formKeyName, string actionParameterName) : base(typeof(ParameterBasedOnFormNameFilter))
        {
            Arguments = new object[] { formKeyName, actionParameterName };
        }

        #endregion

        #region Nested filter

        private class ParameterBasedOnFormNameFilter : IActionFilter
        {
            #region Fields

            private readonly string _formKeyName;
            private readonly string _actionParameterName;

            #endregion

            #region Ctor

            public ParameterBasedOnFormNameFilter(string formKeyName, string actionParameterName)
            {
                _formKeyName = formKeyName;
                _actionParameterName = actionParameterName;
            }

            #endregion

            #region Methods

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (context.HttpContext.Request == null)
                    return;

                context.ActionArguments[_actionParameterName] = context.HttpContext.Request.Form.Keys.Any(key => key.Equals(_formKeyName));

            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            #endregion
        }

        #endregion
    }
}