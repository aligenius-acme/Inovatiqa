using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Inovatiqa.Web.Controllers
{
    public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] _submitButtonNames;
        private readonly FormValueRequirement _requirement;
        private readonly bool _validateNameOnly;

        public FormValueRequiredAttribute(params string[] submitButtonNames):
            this(FormValueRequirement.Equal, submitButtonNames)
        {
        }
        public FormValueRequiredAttribute(FormValueRequirement requirement, params string[] submitButtonNames):
            this(requirement, true, submitButtonNames)
        {
        }
        public FormValueRequiredAttribute(FormValueRequirement requirement, bool validateNameOnly, params string[] submitButtonNames)
        {
            _submitButtonNames = submitButtonNames;
            _validateNameOnly = validateNameOnly;
            _requirement = requirement;
        }


        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (routeContext.HttpContext.Request.Method != WebRequestMethods.Http.Post)
                return false;

            foreach (var buttonName in _submitButtonNames)
            {
                try
                {
                    switch (_requirement)
                    {
                        case FormValueRequirement.Equal:
                            {
                                if (_validateNameOnly)
                                {
                                    if (routeContext.HttpContext.Request.Form.Keys.Any(x => x.Equals(buttonName, StringComparison.InvariantCultureIgnoreCase)))
                                        return true;
                                }
                                else
                                {
                                    string value = routeContext.HttpContext.Request.Form[buttonName];
                                    if (!string.IsNullOrEmpty(value))
                                        return true;
                                }
                            }
                            break;
                        case FormValueRequirement.StartsWith:
                            {
                                if (_validateNameOnly)
                                {
                                    if (routeContext.HttpContext.Request.Form.Keys.Any(x => x.StartsWith(buttonName, StringComparison.InvariantCultureIgnoreCase)))
                                        return true;
                                }
                                else
                                {
                                    foreach (var formValue in routeContext.HttpContext.Request.Form.Keys)
                                        if (formValue.StartsWith(buttonName, StringComparison.InvariantCultureIgnoreCase))
                                        { 
                                            var value = routeContext.HttpContext.Request.Form[formValue];
                                            if (!string.IsNullOrEmpty(value))
                                                return true;
                                        }
                                }
                            }
                            break;
                    }
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                }
            }
            return false;
        }
    }

    public enum FormValueRequirement
    {
        Equal,
        StartsWith
    }
}