using Microsoft.AspNetCore.Mvc;
using BookToAudio.Core.Dto;
using BookToAudio.Core.Services.Interfaces;

namespace BookToAudio.Api.Controllers;

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