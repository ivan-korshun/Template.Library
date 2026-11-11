// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Template.Library.WebApi.Attributes;

/// <summary>
/// Identifies an action that supports the HTTP QUERY method.
/// </summary>
public class HttpQueryAttribute : HttpMethodAttribute
{
    private static readonly IEnumerable<string> s_methods = ["QUERY"];

    public HttpQueryAttribute() : base(s_methods)
    {
    }

    public HttpQueryAttribute([StringSyntax("Route")] string template) : base(s_methods, template)
    {
    }
}
