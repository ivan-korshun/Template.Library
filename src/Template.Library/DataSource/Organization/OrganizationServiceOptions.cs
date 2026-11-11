// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Common.Http;
using Template.Library.Common.Options;

namespace Template.Library.DataSource.Organization;

public sealed class OrganizationServiceOptions : ServiceOptionsBase, IOptionsBase
{
    public static string Key => "OrganizationService";
}
