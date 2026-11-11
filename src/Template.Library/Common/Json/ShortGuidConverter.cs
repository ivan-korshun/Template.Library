// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Template.Library.Common.Json;

public class ShortGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Guid.Parse(reader.GetString().EnsureNotNull());
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("N"));
    }
}
