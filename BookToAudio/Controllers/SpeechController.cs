using BookToAudio.Infra.Dto;
using BookToAudio.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookToAudio.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpeechController : ControllerBase
{
    private readonly SpeechService _speechService;
    

    public SpeechController(SpeechService speechService)
    {
        _speechService = speechService;
    }

    // POST api/<SpeechController>
    [HttpPost]
    public async Task<IActionResult> CreateSpeech([FromForm] SpeechRequest request)
    {
        var fileId = await _speechService.CreateSpeechAsync(request);

        return Ok(fileId);
    }
}
