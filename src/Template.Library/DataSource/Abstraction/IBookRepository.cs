// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Models;

namespace Template.Library.DataSource.Abstraction;

public interface IBookRepository
{
    Task<DataFragment<Book>> GetBooks(int? from, int limit, CancellationToken token = default);

    Task<DataFragment<Book>> GetBooks(int? from, int limit, string? title, CancellationToken token = default);

    Task<DataFragment<Book>> GetBooks(
        int pageNumber, int pageSize, bool includeTotal = false, CancellationToken token = default);

    Task<Book?> GetBook(Guid id, CancellationToken token);

    Task<Book> CreateBook(BookCreateDto book, CancellationToken token = default);

    Task<Book?> UpdateBook(Guid id, BookUpdateDto book, string? eTag, CancellationToken token = default);

    Task<bool> DeleteBook(Guid id, CancellationToken token = default);
}
