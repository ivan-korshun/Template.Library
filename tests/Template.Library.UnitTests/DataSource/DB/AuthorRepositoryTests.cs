// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Template.Library.DataSource.Dapper;
using Template.Library.DataSource.DB;
using Template.Library.DataSource.DB.Records;
using Template.Library.Models;

namespace Template.Library.UnitTests.DataSource.DB;

[TestFixture]
public sealed class AuthorRepositoryTests
{
    private DatabaseOptions _options = null!;
    private IDbDataProvider _dataProvider = null!;
    private ILogger<AuthorRepository> _logger = null!;
    private AuthorRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new Fixture().Create<DatabaseOptions>();
        _dataProvider = Substitute.For<IDbDataProvider>();

        var factory = Substitute.For<IDbDataProviderFactory>();
        factory.CreateProvider(_options.ConnectionString).Returns(_dataProvider);

        _logger = Substitute.For<ILogger<AuthorRepository>>();

        _sut = new AuthorRepository(factory, _logger, Options.Create(_options));
    }

    [Test, ExtendedAutoData]
    public async Task GetAuthor_Always_CallsProvider(AuthorRecord authorRecord, CancellationToken token)
    {
        // Arrange
        _dataProvider.GetByIdAsync<AuthorRecord>("Authors", "Id", authorRecord.Id, token).Returns(authorRecord);

        // Act
        Author? result = await _sut.GetAuthor(authorRecord.Id, token);

        // Asssert
        result.Should().BeEquivalentTo(authorRecord.ToAuthor());
    }
}
