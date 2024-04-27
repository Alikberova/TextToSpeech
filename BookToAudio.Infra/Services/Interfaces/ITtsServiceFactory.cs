using BookToAudio.Core.Services.Interfaces;

namespace BookToAudio.Infra.Services.Interfaces
{
    public interface ITtsServiceFactory
    {
        ITtsService Get(string key);
    }
}