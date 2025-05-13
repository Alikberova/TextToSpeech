namespace TextToSpeech.Infra.Services.Common;

public static class StringExtensions
{
    public static string RemoveLineBreaks(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.Replace("\r", "").Replace("\n", "");
    }
}
