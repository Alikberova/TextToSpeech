﻿using EpubSharp;
using Microsoft.AspNetCore.Http;
using TextToSpeech.Infra.Services.FileProcessing;
using Xunit;

namespace TextToSpeech.UnitTests;

public class EpubProcessorTests
{
    private readonly EpubProcessor _epubProcessor;

    public EpubProcessorTests()
    {
        _epubProcessor = new EpubProcessor();
    }

    [Fact]
    public void CanProcess_ShouldReturnTrue_ForEpubFileType()
    {
        // Arrange
        string fileType = ".epub";

        // Act
        bool result = _epubProcessor.CanProcess(fileType);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanProcess_ShouldReturnFalse_ForNonEpubFileType()
    {
        // Arrange
        string fileType = ".txt";

        // Act
        bool result = _epubProcessor.CanProcess(fileType);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExtractContentAsync_ShouldExtractTextFromEpub()
    {
        // Arrange
        const string content = "Sample EPUB content";
        using var epubStream = CreateSimpleEpub(content);

        var file = new FormFile(epubStream, 0, epubStream.Length, "dummy", "test.epub")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/epub+zip"
        };

        // Act
        var result = await _epubProcessor.ExtractContentAsync(file);

        // Assert
        Assert.Equal("Sample EPUB content", result); // Ensure the extracted content matches
    }

    private static MemoryStream CreateSimpleEpub(string stringContent)
    {
        var stream = new MemoryStream();

        var writer = new EpubWriter();
        writer.SetTitle("Test EPUB");
        writer.AddAuthor("Author");
        writer.AddChapter("Chapter 1", $"<html><body>{stringContent}</body></html>");

        writer.Write(stream);

        stream.Position = 0;

        return stream;
    }
}
