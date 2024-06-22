namespace TextToSpeech.Core.Interfaces;

public interface IPathService
{
    string GetFilePathInFileStorage(string fileName);
    string GetFileStoragePath();
}