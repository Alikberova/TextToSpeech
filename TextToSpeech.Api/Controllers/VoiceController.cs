using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Api.Services;
using TextToSpeech.Infra.Interfaces;

namespace TextToSpeech.Api.Controllers;

[Route("api/voices")]
[ApiController]
public class VoiceController(INarakeetService narraketService) : ControllerBase
{
    [HttpGet("narakeet")]
    public async Task<IActionResult> Get()
    {
        var result = await narraketService.GetAvailableVoices();

        if (result is null)
        {
            return NotFound();
        }

        // for 2 weeks
        HttpHeaderHelper.SetCacheControl(Response, 1209600);

        return Ok(result);
    }
}
