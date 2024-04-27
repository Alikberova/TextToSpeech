using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Infra.Services.Factories;

public sealed class FileProcessorFactory
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

