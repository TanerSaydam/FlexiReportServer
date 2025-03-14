using Bogus;
using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexiReportServer.Endpoints;

public static class ProductModule
{
    public static IEndpointRouteBuilder MapProductRoutes(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/products").WithTags("Products");

        app.MapGet("seed-data",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var categories = await dbContext.Categories.ToListAsync(cancellationToken);

                for (int i = 0; i < 200; i++)
                {
                    Faker faker = new();
                    string productName = faker.Commerce.ProductName();
                    int randomIndex = new Random().Next(categories.Count);
                    if (!dbContext.Products.Any(p => p.Name == productName))
                    {
                        decimal puchasePrice = Convert.ToDecimal(faker.Commerce.Price(1, 1000));
                        decimal sellingPrice = puchasePrice + Convert.ToDecimal(faker.Commerce.Price(1, 100));

                        Product product = new()
                        {
                            Name = productName,
                            CategoryId = categories[randomIndex].Id,
                            PurchasePrice = puchasePrice,
                            SellingPrice = sellingPrice,
                            Stock = new Random().Next(5000)
                        };
                        dbContext.Products.Add(product);
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }

                return Results.Created();
            });

        app.MapPost(string.Empty,
            async (RequestDto request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var products =
                await dbContext.Products
                .Where(p => request.Search != null ? p.Name.Contains(request.Search) : true)
                .Include(i => i.Category)
                .Include(i => i.ProductDetails)
                .Select(s => new ProductDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryName = s.Category!.Name,
                    StockEntry = s.ProductDetails!.Sum(i => i.StockEntry),
                    StockExit = s.ProductDetails!.Sum(i => i.StockExit),
                    Stock = s.Stock,
                    PurchasePrice = s.PurchasePrice,
                    SellingPrice = s.SellingPrice,
                    Total = s.Stock * s.PurchasePrice,
                })
                .ToListAsync(cancellationToken);
                return Results.Ok(products);
            })
            .Produces<List<ProductDto>>();

        return builder;
    }
}