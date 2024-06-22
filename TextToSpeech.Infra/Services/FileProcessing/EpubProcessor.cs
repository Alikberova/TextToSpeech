using Microsoft.AspNetCore.Http;
using TextToSpeech.Core.Interfaces;
using EpubSharp;

namespace TextToSpeech.Infra.Services.FileProcessing;

public class EpubProcessor : IFileProcessor // another package: VersOne.Epub
{
    public bool CanProcess(string fileType) => fileType.Equals(".epub", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractContentAsync(IFormFile file)
    {
        var book = EpubReader.Read(file.OpenReadStream(), false);

        return await Task.FromResult(book.ToPlainText());
    }
}
