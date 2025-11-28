using Google.Cloud.Translation.V2;
using Moq;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Infra.Services;
using Xunit;

namespace TextToSpeech.UnitTests;

public sealed class TranslationServiceTests
{
    private readonly Mock<ITranslationRepository> _mockTranslationRepository;
    private readonly Mock<ITranslationClientWrapper> _mockTranslationClientWrapper;
    private readonly TranslationService _translationService;

    private const string TextRequest = "Hello";
    private const string TextResponse = "Bonjour";
    private const string SourceLanguage = "en";
    private const string TargetLanguage = "fr";

    public TranslationServiceTests()
    {
        _mockTranslationRepository = new Mock<ITranslationRepository>();
        _mockTranslationClientWrapper = new Mock<ITranslationClientWrapper>();
        _translationService = new TranslationService(_mockTranslationClientWrapper.Object, _mockTranslationRepository.Object);
    }

    [Fact]
    public async Task Translate_ReturnsDbTranslation_WhenTranslationExists()
    {
        // Arrange
        var dbTranslation = new Translation
        {
            TranslatedText = TextResponse
        };

        _mockTranslationRepository
            .Setup(repo => repo.GetTranslationAsync(SourceLanguage, TargetLanguage, TextRequest))
            .ReturnsAsync(dbTranslation);

        // Act
        var result = await _translationService.Translate(TextRequest, SourceLanguage, TargetLanguage);

        // Assert
        Assert.Equal(TextResponse, result);

        _mockTranslationClientWrapper.Verify(client => client.TranslateTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockTranslationRepository.Verify(repo => repo.AddTranslationAsync(It.IsAny<Translation>()), Times.Never);
    }

    [Fact]
    public async Task Translate_CallsApiAndSavesTranslation_WhenTranslationDoesNotExist()
    {
        // Arrange

        var apiTranslation = new TranslationResult(string.Empty, TextResponse, string.Empty, string.Empty, string.Empty, null);

        _mockTranslationRepository
            .Setup(repo => repo.GetTranslationAsync(SourceLanguage, TargetLanguage, TextRequest))
            .ReturnsAsync((Translation)null!);

        _mockTranslationClientWrapper
            .Setup(client => client.TranslateTextAsync(TextRequest, TargetLanguage, SourceLanguage))
            .ReturnsAsync(apiTranslation);

        // Act
        var result = await _translationService.Translate(TextRequest, SourceLanguage, TargetLanguage);

        // Assert
        Assert.Equal(TextResponse, result);

        _mockTranslationClientWrapper.Verify(client => client.TranslateTextAsync(TextRequest, TargetLanguage, SourceLanguage), Times.Once);
        _mockTranslationRepository.Verify(repo =>
            repo.AddTranslationAsync(It.Is<Translation>(t => t.OriginalText == TextRequest && t.TranslatedText == TextResponse)), Times.Once);
    }
}
