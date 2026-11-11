// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Template.Library.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequireQueryParameterAttribute(string parameterName) : Attribute, IActionConstraint
{
    private readonly string _parameterName = parameterName;

    public int Order => 0; // Set Order to 0 or higher so it runs during regular matching

    public bool Accept(ActionConstraintContext context)
    {
        return context.RouteContext.HttpContext.Request.Query.ContainsKey(_parameterName);
    }
}
