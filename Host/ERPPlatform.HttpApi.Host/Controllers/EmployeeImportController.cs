using System;
using System.IO;
using System.Threading.Tasks;
using ERPPlatform.Application.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace ERPPlatform.Controllers;

[Authorize]
[ApiController]
[Route("api/app/employee-import")]
public class EmployeeImportController : AbpControllerBase
{
    private readonly IEmployeeImportAppService _employeeImportAppService;

    public EmployeeImportController(IEmployeeImportAppService employeeImportAppService)
    {
        _employeeImportAppService = employeeImportAppService;
    }

    [HttpPost("import-employees")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<EmployeeImportStartResultDto> ImportEmployeesAsync([FromForm] EmployeeImportFormDto input)
    {
        if (input?.File == null || input.File.Length == 0)
        {
            throw new Volo.Abp.UserFriendlyException("Please select a valid Excel file.");
        }

        var memoryStream = new MemoryStream();
        await input.File.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var remoteStream = new RemoteStreamContent(memoryStream, input.File.FileName, input.File.ContentType);
        return await _employeeImportAppService.ImportEmployeesAsync(new EmployeeImportInput
        {
            File = remoteStream
        });
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTemplateAsync()
    {
        var streamContent = await _employeeImportAppService.GetTemplateAsync();
        return File(streamContent.GetStream(), streamContent.ContentType, streamContent.FileName);
    }

    [HttpGet("{id}/file")]
    [Authorize]
    public async Task<IActionResult> GetImportFileAsync(Guid id)
    {
        var streamContent = await _employeeImportAppService.GetImportFileAsync(id);
        return File(streamContent.GetStream(), streamContent.ContentType, streamContent.FileName);
    }
}

public class EmployeeImportFormDto
{
    public IFormFile File { get; set; } = default!;
}
