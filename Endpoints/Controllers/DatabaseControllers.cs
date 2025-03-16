using FlexiReport;
using FlexiReport.Dtos;
using FlexiReportServer.Context;
using Microsoft.AspNetCore.Mvc;

namespace FlexiReportServer.Endpoints.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DatabaseControllers(
    ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("get-database-schema")]
    public async Task<IActionResult> GetDatabaseSchema(CancellationToken cancellationToken)
    {
        FlexiReportService flexiReportService = new();
        var result = await flexiReportService.GetDatabaseSchemaAsync(dbContext, cancellationToken);

        return Ok(result);
    }

    [HttpPost("execute-sql-query")]
    public async Task<IActionResult> ExcuteSqlQuery(QueryRequestDto request, CancellationToken cancellationToken)
    {
        FlexiReportService flexiReportService = new();
        var result = await flexiReportService.ExecuteQueryAsync(request, dbContext, cancellationToken);

        return Ok(result);
    }
}
