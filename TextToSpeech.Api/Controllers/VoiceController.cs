using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Api.Controllers
{
    [Route("api/voices")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private readonly INarakeetService _narraketService;

        public VoiceController(INarakeetService narraketService)
        {
            _narraketService = narraketService;
        }

        [HttpGet("narakeet")]
        public async Task<IActionResult> Get()
        {
            var result = await _narraketService.GetAvailableVoices();

            if (result is null)
            {
                return NotFound();
            }


            Response.Headers.Append("Cache-Control", "public, max-age=3600");

            return Ok(result);
        }
    }
}
