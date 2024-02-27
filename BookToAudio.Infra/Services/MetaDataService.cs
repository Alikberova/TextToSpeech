using BookToAudio.Core.Config;
using Microsoft.Extensions.Options;

namespace BookToAudio.Infra.Services;

public interface IMetaDataService
{
    public void AddMetaData(string pathFile, string title);
}

public class MetaDataService(IOptions<JwtConfig> jwtConfigOptions) : IMetaDataService
{
    private readonly JwtConfig _jwtConfig = jwtConfigOptions.Value;

    public void AddMetaData(string pathFile, string title)
    {
        //var file = TagLib.File.Create(pathFile);
        
        //file.Tag.Title = title;
        //file.Tag.Comment = $"Website: {_jwtConfig.Issuer}";
        //file.Save();
        //file.Dispose();
    }
}