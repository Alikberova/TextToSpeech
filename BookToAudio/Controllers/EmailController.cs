using Microsoft.AspNetCore.Mvc;
using BookToAudio.Core.Dto;
using BookToAudio.Core.Services.Interfaces;

namespace BookToAudio.Controllers;

    [Route("api/[controller]")]
    [ApiController]
public class EmailController: ControllerBase
{
    private readonly IEmailService _emailService;
    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
  public void  SendMessage([FromBody] EmailRequest request)
   {
        _emailService.FeedbackMessage(request);
   }
}