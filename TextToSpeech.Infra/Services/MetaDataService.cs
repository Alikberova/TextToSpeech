using TextToSpeech.Core;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services;

public sealed class MetaDataService() : IMetaDataService
{
    public void AddMetaData(string pathFile, string title)
    {
        using var file = TagLib.File.Create(pathFile);

        file.Tag.Title = title;
        file.Tag.Comment = $"Website: {AppConstants.Domain}";
        file.Save();
    }
}