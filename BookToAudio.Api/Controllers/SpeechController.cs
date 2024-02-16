using BookToAudio.Core.Dto;
using BookToAudio.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookToAudio.Api.Controllers;

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
    public IActionResult CreateSpeech([FromForm] SpeechRequest request)
        {
        var fileId = _speechService.CreateSpeech(request);

        return Ok(fileId);
    }

    [HttpPost("sample")]
    public async Task<IActionResult> GetSpeechSample([FromBody] SpeechRequest request)
    {
        var audioStream = await _speechService.CreateSpeechSample(request);

        return new FileStreamResult(audioStream, "audio/mpeg");
    }
}
