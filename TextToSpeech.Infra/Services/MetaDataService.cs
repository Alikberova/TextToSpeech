using TextToSpeech.Core.Config;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services;

public class MetaDataService() : IMetaDataService
{
    public void AddMetaData(string pathFile, string title)
    {
        var file = TagLib.File.Create(pathFile);

        file.Tag.Title = title;
        file.Tag.Comment = $"Website: {SharedConstants.Domain}";
        file.Save();
        file.Dispose();
    }
}