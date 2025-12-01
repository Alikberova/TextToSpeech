using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Core.Extensions;
using TextToSpeech.Infra.Dto;

namespace TextToSpeech.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LogController(ILogger<LogController> logger) : ControllerBase
{
    [HttpPost]
    public IActionResult LogMessage([FromBody] Log log)
    {
        logger.Log(log.MappedLogLevel,
            "{Timestamp} {Message}" +
            "\nFileName: {FileName}, line {LineNumber}, column {ColumnNumber}" +
            "\nhas Additional: {HasAdditional}",
            log.Timestamp, log.Message.Sanitize(), log.FileName.Sanitize(), log.LineNumber, log.ColumnNumber,
            log.Additional.Count != 0);

        return Ok();
    }
}
