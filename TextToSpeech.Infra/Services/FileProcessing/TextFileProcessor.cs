﻿using Microsoft.AspNetCore.Http;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services.FileProcessing;

public sealed class TextFileProcessor : IFileProcessor
{
    public bool CanProcess(string fileType) => fileType.Equals(".txt", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractTextAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return await Task.FromResult(string.Empty);
        }

        using var reader = new StreamReader(file.OpenReadStream());
        return await reader.ReadToEndAsync();
    }
}
