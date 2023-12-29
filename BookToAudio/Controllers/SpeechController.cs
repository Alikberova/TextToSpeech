using BookToAudio.Infa.Dto;
using BookToAudio.Infa.Services;
using BookToAudio.Services;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Audio;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookToAudio.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpeechController : ControllerBase
{
    private readonly SpeechService _speechService;
    private readonly IFileStorageService _fileStorageService;

    public SpeechController(SpeechService speechService, IFileStorageService fileStorageService)
    {
        _speechService = speechService;
        _fileStorageService = fileStorageService;
    }

    // GET api/<SpeechController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<SpeechController>
    [HttpPost]
    public async Task<IActionResult> CreateSpeech([FromBody] SpeechRequestDto request)
    {
        var fileContent = await _fileStorageService.RetrieveFileAsync(request.FileId.ToString());

        var req = new SpeechRequest(fileContent, request.Model, request.Voice, request.ResponseFormat, request.Speed);

        await _speechService.CreateSpeechAsync(req);

        //todo how to be informed that speech is ready
        return Ok("Speech creation initiated.");
    }

    // PUT api/<SpeechController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<SpeechController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
