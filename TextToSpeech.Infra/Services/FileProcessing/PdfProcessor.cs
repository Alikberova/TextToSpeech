using System.Text;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;
using TextToSpeech.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TextToSpeech.Infra.Services;

public sealed class PdfProcessor : IFileProcessor
{
    public bool CanProcess(string fileType) => fileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractContentAsync(IFormFile file)
    {
        var stringBuilder = new StringBuilder();

        using var pdfReader = PdfDocument.Open(file.OpenReadStream());

        foreach (var page in pdfReader.GetPages())
        {
            stringBuilder.AppendLine(ContentOrderTextExtractor.GetText(page, addDoubleNewline: true));
        }

        return await Task.FromResult(stringBuilder.ToString());
    }
}
