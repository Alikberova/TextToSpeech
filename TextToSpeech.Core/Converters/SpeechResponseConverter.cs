using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextToSpeech.Core.Models;

namespace TextToSpeech.Core.Converters;

public sealed class SpeechResponseJsonConverter : JsonConverter<SpeechResponseFormat>
{
    public override SpeechResponseFormat Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var str = reader.GetString();

        if (SpeechResponseFormat.TryParse(str, out var value))
        {
            return value;
        }

        throw new JsonException(SpeechResponseFormat.UnsupportedFormatError(str));
    }

    public override void Write(Utf8JsonWriter writer, SpeechResponseFormat value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public sealed class SpeechResponseTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str && SpeechResponseFormat.TryParse(str, out var valueResult))
        {
            return valueResult;
        }

        throw new FormatException();
    }
}
