using Microsoft.AspNetCore.Mvc;
using BookToAudio.Core.Dto;
using BookToAudio.Core.Services.Interfaces;

namespace BookToAudio.Controllers;

    [Route("api/[controller]")]
    [ApiController]
public class EmailController: ControllerBase
{
    IEmailService _emailService;
    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
  public void  feadback([FromBody] FeedbackRequest request)
   {
        _emailService.FeedbackMessage(request);
   }

    [HttpGet]
   public string getting() {
        return "getting work";
    }
}

