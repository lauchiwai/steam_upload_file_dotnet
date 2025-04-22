using Microsoft.AspNetCore.Mvc;
using System.IO;
using WebApplication1.Filters;
using WebApplication1.Services;
namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SteamUploadController : ControllerBase
{
    // 注入所需的服務和配置
    private readonly ISteamUploadServices _steamUploadServices;
    public SteamUploadController(ISteamUploadServices steamUploadServices)
    {
        _steamUploadServices = steamUploadServices;
    }

    [HttpPost]
    [Route("UploadFile")]
    [DisableFormValueModelBindingFilter]
    public async Task<IActionResult> UploadFile()
    {
        var result = await _steamUploadServices.UploadFileAsync(Request);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
        }
    }
}
