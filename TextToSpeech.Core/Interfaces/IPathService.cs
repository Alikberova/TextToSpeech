namespace TextToSpeech.Core.Interfaces;

public interface IPathService
{
    string GetFileStorageFilePath(string fileName);
    string GetFileStoragePath();
}