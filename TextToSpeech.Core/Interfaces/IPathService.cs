namespace TextToSpeech.Core.Interfaces;

public interface IPathService
{
    string ResolveFilePathForStorage(string fileName);
    string GetFileStoragePath();
}