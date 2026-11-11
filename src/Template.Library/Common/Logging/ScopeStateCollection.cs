// Copyright © Korshun Ivan. All rights reserved.

using System.Text;

namespace Template.Library.Common.Logging;

/// <summary>
/// Passed as an argument to logger.BeginScope() calls for simple text logs
/// </summary>
public sealed class ScopeStateCollection(params KeyValuePair<string, object?>[] items)
    : IReadOnlyCollection<KeyValuePair<string, object?>>
{
    private readonly KeyValuePair<string, object?>[] _items = items;

    private string? _cachedToString;

    public int Count => _items.Length;

    public override string ToString()
    {
        if (_cachedToString == null)
        {
            var sb = new StringBuilder();

            sb.Append(_items[0].Key);
            sb.Append(':');
            sb.Append(_items[0].Value);

            for (int i = 1; i < _items.Length; i++)
            {
                sb.Append(", ");
                sb.Append(_items[i].Key);
                sb.Append(':');
                sb.Append(_items[i].Value);
            }

            _cachedToString = sb.ToString();
        }

        return _cachedToString;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            yield return _items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
