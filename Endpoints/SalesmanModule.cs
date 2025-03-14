using Bogus;
using Microsoft.EntityFrameworkCore;
using FlexiReportServer.Context;
using FlexiReportServer.Models;

namespace FlexiReportServer.Endpoints;

public static class SalesmanModule
{
    public static IEndpointRouteBuilder MapSalesmanRoutes(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/salesmans").WithTags("Salesmans");

        app.MapGet("seed-data",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var salesmans = await dbContext.Salesmans.ToListAsync(cancellationToken);

                for (int i = 0; i < 10; i++)
                {
                    Faker faker = new();
                    var name = faker.Person.FullName;

                    if (!dbContext.Salesmans.Any(s => s.Name == name))
                    {
                        Salesman salesman = new()
                        {
                            Name = name,
                        };

                        dbContext.Add(salesman);
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }

                return Results.Created();
            });

        app.MapGet(string.Empty,
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var salesmans = await dbContext.Salesmans.ToListAsync(cancellationToken);
                return Results.Ok(salesmans);
            })
            .Produces<List<Salesman>>();

        return builder;
    }
}