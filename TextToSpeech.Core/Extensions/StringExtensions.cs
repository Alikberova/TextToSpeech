using System.Text;

namespace TextToSpeech.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Escapes characters that could enable code injection
    /// </summary>
    public static string Sanitize(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var builder = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            if (char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
            {
                builder.Append($"\\u{(int)c:x4}");
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
