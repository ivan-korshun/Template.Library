// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Common.Options;

namespace Template.Library.Common.Http.ClientCredentials.Models;

public class ClientCredentialsOptions : Dictionary<string, TokenRequest>, IOptionsBase
{
    public static string Key => "ClientCredentials";
}
