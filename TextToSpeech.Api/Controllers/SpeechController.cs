using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;

    public SpeechController(ISpeechService speechService)
    {
        _speechService = speechService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpeech([FromForm] SpeechRequest request)
    {
        if (request.File is null)
        {
            return BadRequest("File is required for speech generation.");
        }

        using var ms = new MemoryStream();

        await request.File.CopyToAsync(ms);

        var fileId = await _speechService.GetOrInitiateSpeech(request.TtsRequestOptions, ms.ToArray(),
            request.File.FileName, request.LanguageCode ?? string.Empty, request.TtsApi);

        return Ok(fileId);
    }

    [HttpPost("sample")]
    public async Task<IActionResult> GetSpeechSample([FromBody] SpeechRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest("Input text is required for speech sample.");
        }

        var audioStream = await _speechService.CreateSpeechSample(request.TtsRequestOptions, request.Input,
            request.LanguageCode ?? string.Empty, request.TtsApi);

        return new FileStreamResult(audioStream, "audio/mpeg");
    }
}
