// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;

namespace Template.Library.Common.Http;

public class HttpOperationException : Exception
{
    public HttpOperationException() : base("HTTP operation has failed.")
    {
    }

    public HttpOperationException(string message) : base(message)
    {
    }

    public HttpOperationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public HttpOperationException(string service, string entity, HttpMethod method, Exception innerException)
        : base($"The {method} {service} {entity} operation has failed.", innerException)
    {
    }
}
