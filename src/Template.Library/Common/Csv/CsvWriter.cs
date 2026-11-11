// Copyright © Korshun Ivan. All rights reserved.

using System.IO;

namespace Template.Library.Common.Csv;

public sealed class CsvWriter(string filePath, bool ensureColumnCount) : IAsyncDisposable
{
    private readonly string _filePath = filePath;
    private readonly bool _ensureColumnCount = ensureColumnCount;

    private StreamWriter? _streamWriter;
    private int _columnCount;

    public async ValueTask DisposeAsync()
    {
        if (_streamWriter != null)
        {
            await _streamWriter.DisposeAsync();
            _streamWriter = null;
        }
    }

    public Task Start(ICollection<string> columnNames)
    {
        var newWriter = new StreamWriter(_filePath, false);
        if (Interlocked.CompareExchange(ref _streamWriter, newWriter, null) != null)
        {
            newWriter.Dispose();
            throw new InvalidOperationException(
                $"CSV writer has already been initialized via {nameof(Start)} method.");
        }

        if (_ensureColumnCount)
        {
            _columnCount = columnNames.Count;
        }

        return _streamWriter.WriteLineAsync(string.Join(',', columnNames));
    }

    public Task WriteDataRow(ICollection<object?> values)
    {
        if (_streamWriter == null)
        {
            throw new InvalidOperationException(
                $"CSV writer has not been initialized via {nameof(Start)} method.");
        }

        if (_ensureColumnCount && values.Count != _columnCount)
        {
            throw new InvalidOperationException(
                $"CSV writer expected {_columnCount} columns, but received {values.Count} columns.");
        }

        return _streamWriter.WriteLineAsync(string.Join(',', values.Select(EncodeValue)));
    }

    private static object? EncodeValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is not string)
        {
            return value;
        }

        return string.Concat("\"", value.ToString()?.Replace("\"", "\"\"").Replace("\n", "\\n"), "\"");
    }
}
