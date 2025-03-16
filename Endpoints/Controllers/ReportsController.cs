using FlexiReport.Dtos;
using FlexiReport.Models;
using FlexiReportServer.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace FlexiReportServer.Endpoints.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ReportsController(
    ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(ReportDto request, CancellationToken cancellationToken)
    {
        Report report = new()
        {
            Name = request.Name,
            Content = request.Content,
            BackgroundColor = request.BackgroundColor,
            CreatedAt = DateTime.Now,
            Endpoint = request.Endpoint,
            FontFamily = request.FontFamily,
            PageOrientation = request.PageOrientation,
            PageSize = request.PageSize,
            SqlQuery = request.SqlQuery,
        };

        if (request.RequestElements is not null)
        {
            report.RequestElements = request.RequestElements.Select(x => new RequestElement
            {
                Name = x.Name,
                Label = x.Label,
                Type = x.Type,
                Endpoint = x.Endpoint,
                Index = x.Index,
            }).ToList();
        }
        dbContext.Add(report);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(Result<Guid>.Succeed(report.Id));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ReportDto request, CancellationToken cancellationToken)
    {
        Report report = await dbContext.Reports.FirstAsync(p => p.Id == request.Id, cancellationToken);

        report.Name = request.Name;
        report.Content = request.Content;
        report.BackgroundColor = request.BackgroundColor;
        report.CreatedAt = DateTime.Now;
        report.Endpoint = request.Endpoint;
        report.FontFamily = request.FontFamily;
        report.PageOrientation = request.PageOrientation;
        report.PageSize = request.PageSize;
        report.SqlQuery = request.SqlQuery;

        dbContext.Update(report);

        if (request.RequestElements is not null)
        {
            var currentElements = dbContext.RequestElements.AsNoTracking().Where(x => x.ReportId == report.Id).ToList();
            if (currentElements.Any())
            {
                dbContext.RemoveRange(currentElements);
            }

            var newRequestElements = request.RequestElements.Select(x => new RequestElement
            {
                Name = x.Name,
                Label = x.Label,
                Type = x.Type,
                Endpoint = x.Endpoint,
                Index = x.Index,
                ReportId = report.Id
            }).ToList();

            dbContext.RequestElements.AddRange(newRequestElements);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(Result<string>.Succeed("Rapor başarıyla güncellendi"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var reports = await dbContext.Reports
            .Select(s => new
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync(cancellationToken);
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var report = await dbContext.Reports
            .Include(x => x.RequestElements)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (report is null)
        {
            return NotFound();
        }

        report.RequestElements = report.RequestElements?.OrderBy(i => i.Index).ToList();

        return Ok(report);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var report = await dbContext.Reports.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (report is null)
        {
            return NotFound();
        }

        dbContext.Remove(report);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Result<string>.Succeed("Rapor başarıyla silindi"));
    }
}
