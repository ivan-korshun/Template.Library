// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Template.Library.E2ETests.Api;

public class ApiTestsBase
{
    protected const int MaxPageNumber = 20_000_000;
    protected const int MaxPageSize = 100;

    protected const string String64 = "KindnessKindnessKindnessKindnessKindnessKindnessKindnessKindness";
    protected const string String256 = String64 + String64 + String64 + String64;

    protected string SearchKey { get; } = Guid.NewGuid().ToString("N");

    protected static JsonSerializerOptions SkipNulls { get; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected static HttpClient SutClient => GlobalSetup.SutClient;
}
