using TextToSpeech.Core.Config;

namespace TextToSpeech.Infra.Services;

public interface IMetaDataService
{
    public void AddMetaData(string pathFile, string title);
}

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