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
        var result = await _service.GetRawXmlAsync(Guid.Parse(refId));
        return Ok(result);
    }

    [HttpGet("demographics/{refId}")]
    public async Task<IActionResult> GetDemographics(string refId)
    {
        var result = await _service.GetDemographicsAsync(Guid.Parse(refId));
        return Ok(result);
    }
}