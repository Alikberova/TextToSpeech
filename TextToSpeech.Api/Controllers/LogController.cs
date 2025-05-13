using TextToSpeech.Core.Dto;
using Microsoft.AspNetCore.Mvc;

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
            "\nAdditional: {Additional}",
            log.Timestamp, log.Message, log.FileName, log.LineNumber, log.ColumnNumber, 
            log.Additional?.Replace("\n", "").Replace("\r", ""));

        return Ok();
    }
}
