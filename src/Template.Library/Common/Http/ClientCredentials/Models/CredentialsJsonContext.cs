// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Template.Library.Common.Http.ClientCredentials.Models;

/// <summary>
/// Source-generated serialization: trim/AOT friendly, no reflection, faster startup.
/// </summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TokenResponse))]
public sealed partial class CredentialsJsonContext : JsonSerializerContext;
