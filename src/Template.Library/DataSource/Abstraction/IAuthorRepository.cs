// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Models;

namespace Template.Library.DataSource.Abstraction;

public interface IAuthorRepository
{
    Task<DataFragment<Author>> GetAuthors(int? from, int limit, CancellationToken token = default);

    Task<DataFragment<Author>> GetAuthors(int? from, int limit, string? fullName, CancellationToken token = default);

    Task<DataFragment<Author>> GetAuthors(
        int pageNumber, int pageSize, bool includeTotal = false, CancellationToken token = default);

    Task<Author?> GetAuthor(Guid id, CancellationToken token = default);

    Task<Author> CreateAuthor(AuthorCreateDto author, CancellationToken token = default);

    Task<Author?> UpdateAuthor(Guid id, AuthorUpdateDto author, string? eTag, CancellationToken token = default);

    Task<bool> DeleteAuthor(Guid id, CancellationToken token = default);
}
