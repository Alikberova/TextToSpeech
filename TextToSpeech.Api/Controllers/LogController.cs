using TextToSpeech.Core.Dto;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;

    public LogController(ILogger<LogController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult LogMessage([FromBody] Log log)
    {
        _logger.Log(log.MappedLogLevel,
            "{Timestamp} {Message}" +
            "\nFileName: {FileName}, line {LineNumber}, column {ColumnNumber}" +
            "\nAdditional: {Additional}",
            log.Timestamp, log.Message, log.FileName, log.LineNumber, log.ColumnNumber, log.Additional);

        return Ok();
    }
}