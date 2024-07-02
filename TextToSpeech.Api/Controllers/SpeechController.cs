using TextToSpeech.Core.Dto;
using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;

    public SpeechController(ISpeechService speechService)
    {
        _speechService = speechService;
    }

    // POST api/<SpeechController>
    [HttpPost]
    public async Task<IActionResult> CreateSpeech([FromForm] SpeechRequest request)
    {
        var fileId = await _speechService.GetOrInitiateSpeech(request);

        Console.WriteLine("returning id " + fileId);

        return Ok(fileId);
    }

    [HttpPost("sample")]
    public async Task<IActionResult> GetSpeechSample([FromBody] SpeechRequest request)
    {
        var audioStream = await _speechService.CreateSpeechSample(request);

        return new FileStreamResult(audioStream, "audio/mpeg");
    }
}
