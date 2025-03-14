using Bogus;
using Microsoft.EntityFrameworkCore;
using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Models;

namespace FlexiReportServer.Endpoints;

public static class CategoryModule
{
    public static IEndpointRouteBuilder MapCategoryRoutes(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/categories").WithTags("Categories");

        app.MapGet("seed-data",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var categories = await dbContext.Categories.ToListAsync(cancellationToken);

                Faker faker = new();
                var fakerCategories = faker.Commerce.Categories(20).ToList();

                foreach (var item in fakerCategories)
                {
                    Category category = new()
                    {
                        Name = item
                    };

                    if (!categories.Any(i => i.Name == item))
                    {
                        dbContext.Categories.Add(category);
                        categories.Add(category);
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created();
            });

        app.MapGet(string.Empty,
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var categories =
                await dbContext.Categories
                 .Join(dbContext.Products, c => c.Id, p => p.CategoryId, (c, p) => new { c, p })
                .GroupBy(g => g.c.Id)
                .Select(s => new CategoryDto
                {
                    Id = s.Key,
                    Name = s.First().c.Name,
                    ProductCounts = s.Count()
                })
                .ToListAsync(cancellationToken);
                return Results.Ok(categories);
            })
            .Produces<List<CategoryDto>>();

        return builder;
    }
}