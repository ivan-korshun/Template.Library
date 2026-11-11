// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class SerializationSetupExtensions
{
    /// <summary>
    /// Remove JSON parser syntax from field names of validation errors.
    /// </summary>
    public static void TrimFieldNames(this ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            if (context.ProblemDetails is HttpValidationProblemDetails validationDetails)
            {
                validationDetails.Errors = validationDetails.Errors.ToDictionary(
                    kvp => kvp.Key.TrimStart('$').TrimStart('.'),
                    kvp => kvp.Value);
            }
        };
    }

    public static void SerializeEnumAsString(this JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static void SerializeEnumAsString(this Microsoft.AspNetCore.Http.Json.JsonOptions options)
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
