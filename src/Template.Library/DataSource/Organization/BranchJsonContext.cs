// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;
using Template.Library.Models;

namespace Template.Library.DataSource.Organization;

/// <summary>
/// Source-generated serialization: trim/AOT friendly, no reflection, faster startup.
/// </summary>
[JsonSourceGenerationOptions(
    JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(Branch))]
[JsonSerializable(typeof(DataFragment<Branch>))]
[JsonSerializable(typeof(BranchCreateDto))]
[JsonSerializable(typeof(BranchUpdateDto))]
public sealed partial class BranchJsonContext : JsonSerializerContext;
