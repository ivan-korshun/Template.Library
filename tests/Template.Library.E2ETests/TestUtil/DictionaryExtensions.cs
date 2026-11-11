// Copyright © Korshun Ivan. All rights reserved.

using System.Net;

namespace Template.Library.E2ETests.TestUtil;

public static class DictionaryExtensions
{
    extension(Dictionary<string, object?> dictionary)
    {
        public string ToQueryString()
        {
            return string.Join("&", dictionary
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value?.ToString())}"));
        }
    }
}
