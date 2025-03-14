using Microsoft.EntityFrameworkCore;
using TS.Result;
using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Models;

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
                    report = await dbContext.Reports.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
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
                        dbContext.Update(report);
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
                        SqlQuery = request.SqlQuery
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
                var reports = await dbContext.Reports.ToListAsync(cancellationToken);
                return Results.Ok(reports);
            })
            .Produces<List<Report>>();

        app.MapGet("{id}",
            async (Guid id, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var report = await dbContext.Reports.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                return Results.Ok(report);
            })
            .Produces<Report>();

        app.MapDelete("{id}",
            async (Guid id, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var report = await dbContext.Reports.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
                if (report is not null)
                {
                    dbContext.Remove(report);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                return Results.Ok();
            });
        return builder;
    }
}