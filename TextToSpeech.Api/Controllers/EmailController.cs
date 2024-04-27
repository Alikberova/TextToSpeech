using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Services.Interfaces;

namespace TextToSpeech.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public void Send([FromBody] EmailRequest request)
    {
        _emailService.SendEmail(request);
    }
}