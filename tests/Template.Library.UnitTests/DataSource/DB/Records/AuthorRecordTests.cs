// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NUnit.Framework;
using Template.Library.DataSource.DB.Records;
using Template.Library.Models;

namespace Template.Library.UnitTests.DataSource.DB.Records;

[TestFixture]
public sealed class AuthorRecordTests
{
    [Test, ExtendedAutoData]
    public async Task ToAuthor_Always_Author(AuthorRecord authorRecord)
    {
        authorRecord.ToAuthor().Should().BeEquivalentTo(new Author
        {
            Id = authorRecord.Id,
            FullName = authorRecord.FullName,
            Country = authorRecord.Country,
            Birthday = authorRecord.Birthday,
            Famous = authorRecord.Famous,
            Version = Convert.ToBase64String(authorRecord.RowVersion)
        });
    }
}
