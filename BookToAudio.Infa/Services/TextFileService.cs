namespace BookToAudio.Infra.Services;

public interface ITextFileService
{
    List<string> SplitTextIfGreaterThan(string text, int limit);
}

public class TextFileService : ITextFileService
{
    public List<string> SplitTextIfGreaterThan(string text, int maxLength)
    {
        var chunks = new List<string>();

        if (text.Length <= maxLength)
        {
            chunks.Add(text);
            return chunks;
        }

        int start = 0;

        while (start < text.Length)
        {
            int end = start + maxLength > text.Length ?
                text.Length :
                start + maxLength;

            int lastSentenceEnd = FindLastSentenceEnd(text, start, end);

            if (lastSentenceEnd == -1)
            {
                // If no sentence end found within the limit, take the maximum length available.
                lastSentenceEnd = end;
            }

            var chunk = text.Substring(start, lastSentenceEnd - start).Trim();

            if (chunk != string.Empty)
            {
                chunks.Add(chunk);
            }
            
            start = lastSentenceEnd;
        }

        return chunks;
    }

    //Гадаєте, ми їх знайдемо? – запитує вона.
    //"?" here doesn't mean the sentence end.
    //TODO: if you met combination of: $"{char}? - {lowercase letter}" - do not treat this as the sentence end

    private int FindLastSentenceEnd(string text, int start, int end)
    {
        // Search for sentence-ending punctuation within the specified range
        for (int i = end - 1; i >= start; i--)
        {
            if (IsSentenceEnd(text, i))
            {
                return i + 1;
            }
        }

        return -1; // No sentence end found in the range
    }

    private static bool IsSentenceEnd(string text, int i)
    {
        return (text[i] is '.' or '?' or '!') &&
            (i + 1 == text.Length || char.IsWhiteSpace(text[i + 1]));
    }
}
