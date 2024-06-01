namespace TextToSpeech.Api.Services;

public static class HttpHeaderHelper
{
    public static void SetCacheControl(HttpResponse response, int maxAgeSeconds)
    {
        response.Headers.Append("Cache-Control", $"public, max-age={maxAgeSeconds}");
    }
}
