using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Models;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace FlexiReportServer.Endpoints;

public static class ReportModule
{
    public static IEndpointRouteBuilder MapReportRoutes(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/reports").WithTags("Reports");

        app.MapPost(string.Empty,
            async (ReportDto request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                Report? report = new();

                if (request.Id is not null)
                {
                    report = await dbContext.Reports.Where(p => p.Id == request.Id).Include(i => i.RequestElements).FirstOrDefaultAsync(cancellationToken);
                    if (report is not null)
                    {
                        report.Content = request.Content;
                        report.CreatedAt = DateTime.UtcNow;
                        report.Endpoint = request.Endpoint;
                        report.Name = request.Name;
                        report.PageOrientation = request.PageOrientation;
                        report.PageSize = request.PageSize;
                        report.FontFamily = request.FontFamily;
                        report.SqlQuery = request.SqlQuery;
                        report.BackgroundColor = request.BackgroundColor;
                        dbContext.Update(report);

                        if (report.RequestElements!.Any())
                        {
                            dbContext.RemoveRange(report.RequestElements!);
                        }

                        var newElements = request.RequestElements.Select(s => new RequestElement()
                        {
                            ReportId = report.Id,
                            Index = s.Index,
                            Label = s.Label,
                            Name = s.Name,
                            Type = s.Type,
                            Endpoint = s.Endpoint,
                        }).ToList();
                        dbContext.AddRange(newElements);
                    }
                }
                else
                {
                    report = new()
                    {
                        Name = request.Name,
                        Endpoint = request.Endpoint,
                        CreatedAt = DateTime.UtcNow,
                        Content = request.Content,
                        PageSize = request.PageSize,
                        FontFamily = request.FontFamily,
                        PageOrientation = request.PageOrientation,
                        SqlQuery = request.SqlQuery,
                        BackgroundColor = request.BackgroundColor,
                        RequestElements = request.RequestElements.Select(s => new RequestElement()
                        {
                            Index = s.Index,
                            Label = s.Label,
                            Name = s.Name,
                            Type = s.Type,
                            Endpoint = s.Endpoint,
                        }).ToList()
                    };
                    dbContext.Add(report);
                }
                await dbContext.SaveChangesAsync(cancellationToken);
                return Results.Ok(Result<Guid>.Succeed(report!.Id));
            })
            .Produces<Result<Guid>>();

        app.MapGet(string.Empty,
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var reports = await dbContext.Reports.Select(s => new
                {
                    Id = s.Id,
                    Endpoint = s.Endpoint,
                    Name = s.Name
                }).ToListAsync(cancellationToken);
                return Results.Ok(reports);
            })
            .Produces<List<Report>>();

        app.MapGet("{id}",
            async (Guid id, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var report = await dbContext.Reports
                .Where(p => p.Id == id)
                .Include(i => i.RequestElements)
                .FirstOrDefaultAsync(cancellationToken);

                if (report != null)
                {
                    report.RequestElements = report.RequestElements!.OrderBy(p => p.Index).ToList();
                }
                return Results.Ok(report);
            })
            .Produces<Report>();

        app.MapDelete("{id}",
            async (Guid id, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var report = await dbContext.Reports.Where(p => p.Id == id).Include(i => i.RequestElements).FirstOrDefaultAsync(cancellationToken);
                if (report is not null)
                {
                    dbContext.Remove(report);
                    if (report.RequestElements!.Any())
                    {
                        dbContext.RemoveRange(report.RequestElements!);
                    }
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                return Results.Ok();
            });
        return builder;
    }
}