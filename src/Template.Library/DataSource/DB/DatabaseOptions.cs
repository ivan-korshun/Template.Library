// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Template.Library.Common.Options;

namespace Template.Library.DataSource.DB;

public sealed class DatabaseOptions : IOptionsBase
{
    public static string Key => "DB";

    [Required]
    public required string ConnectionString { get; set; }
}
