using TextToSpeech.Infra.Services;
using Xunit;

namespace TextToSpeech.UnitTests;

public class RedisCacheProviderTests
{
    [Fact]
    public async Task HandlesNullConnectionStringGracefully()
    {
        var provider = new RedisCacheProvider(null!);

        var result = await provider.GetCachedData<string>("missing");
        Assert.Null(result);

        await provider.SetCachedData("key", "value", TimeSpan.FromSeconds(1));
    }
}
