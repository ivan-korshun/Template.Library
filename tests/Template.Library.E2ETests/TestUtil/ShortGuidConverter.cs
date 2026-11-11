// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Template.Library.E2ETests.TestUtil;

public sealed class ShortGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var rawValue = reader.GetString();
        if (Guid.TryParse(rawValue, out Guid guid))
        {
            return guid;
        }

        throw new JsonException($"Invalid Guid: '{rawValue}'");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("N"));
    }
}
