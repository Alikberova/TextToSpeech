using Google.Apis.Requests;
using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Core.Dto;
using TextToSpeech.Infra.Services;

namespace TextToSpeech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;

    public TranslationController(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        var translatedText = await _translationService.Translate(request.Text, request.SourceLanguage, request.TargetLanguage);

        return Ok(translatedText);
    }
}