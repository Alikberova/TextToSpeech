using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services.FileProcessing;

public sealed class FileProcessorFactory : IFileProcessorFactory
{
    private readonly IEnumerable<IFileProcessor> _processors;

    public FileProcessorFactory(IEnumerable<IFileProcessor> processors)
    {
        _processors = processors;
    }

    public IFileProcessor? GetProcessor(string fileType)
    {
        return _processors.FirstOrDefault(p => p.CanProcess(fileType));
    }
}

