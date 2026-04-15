using Microsoft.AspNetCore.Mvc;
using EkycService.Application.Interfaces;
using EkycService.Application.DTOs.Request;
using EkycService.Application.DTOs.Response;

namespace EkycService.Api.Controllers;

[ApiController]
[Route("ekyc/v1")]
public class EkycController : ControllerBase
{
    private readonly IEkycService _service;

    public EkycController(IEkycService service)
    {
        _service = service;
    }

    [HttpPost("save")]
    public async Task<ActionResult<SaveEkycResponse>> Save([FromBody] SaveEkycRequest request)
    {
        var result = await _service.SaveAsync(request);
        return Ok(result);
    }

    [HttpGet("ekyc-xml/{refId}")]
    public async Task<IActionResult> GetXml(string refId)
    {
        if (!Guid.TryParse(refId, out var guid))
            return BadRequest(new { message = "Invalid refId format" });

        var result = await _service.GetRawXmlAsync(guid);

        if (result == null)
            return NotFound(new { message = "XML not found", refId });

        return Ok(result);
    }

    [HttpGet("demographics/{refId}")]
    public async Task<IActionResult> GetDemographics(string refId)
    {
        if (!Guid.TryParse(refId, out var guid))
            return BadRequest(new { message = "Invalid refId format" });

        var result = await _service.GetDemographicsAsync(guid);

        if (result == null)
            return NotFound(new { message = "Demographics not found", refId });

        return Ok(result);
    }
}