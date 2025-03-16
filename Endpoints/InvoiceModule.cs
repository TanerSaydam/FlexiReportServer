using Bogus;
using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexiReportServer.Endpoints;

public static class InvoiceModule
{
    public static IEndpointRouteBuilder MapInvoiceRoutes(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/invoices").WithTags("Invoices");

        app.MapGet("seed-data",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var salesmans = await dbContext.Salesmans.ToListAsync(cancellationToken);
                var products = await dbContext.Products.ToListAsync(cancellationToken);

                List<ProductDetail> productsDetails = new();

                for (int i = 0; i < 200; i++)
                {
                    int randomSalesmanIndex = new Random().Next(salesmans.Count);
                    Faker faker = new();
                    Invoice invoice = new()
                    {
                        SalesmanId = salesmans[randomSalesmanIndex].Id,
                        Date = GetRandomDate(new DateOnly(2025, 01, 01), new DateOnly(2025, 03, 31)),
                        CustomerName = faker.Company.CompanyName(),
                        InvoiceNumber = "EET2025" + DateTime.Now.ToFileTimeUtc().ToString()
                    };

                    List<InvoiceDetail> invoiceDetails = new();
                    for (int j = 0; j < new Random().Next(10); j++)
                    {
                        int randomProductIndex = new Random().Next(products.Count);
                        var product = products[randomProductIndex];
                        if (product.Stock <= 0) continue;
                        if (invoiceDetails.Any(i => i.ProductId == product.Id)) continue;

                        decimal price = j % 2 == 0 ? product.PurchasePrice : product.SellingPrice;
                        InvoiceDetail invoiceDetail = new()
                        {
                            Price = price,
                            Quantity = new Random().Next(product.Stock),
                            ProductId = product.Id,
                        };
                        invoiceDetails.Add(invoiceDetail);

                        if (i % 2 == 0)
                        {
                            product.Stock += invoiceDetail.Quantity;
                        }
                        else
                        {
                            product.Stock -= invoiceDetail.Quantity;
                        }


                        ProductDetail productDetail = new()
                        {
                            Date = invoice.Date,
                            Description = invoice.InvoiceNumber + " nolu fatura",
                            Price = price,
                            StockEntry = i % 2 == 0 ? invoiceDetail.Quantity : 0,
                            StockExit = i % 2 == 0 ? 0 : invoiceDetail.Quantity,
                            ProductId = product.Id
                        };

                        productsDetails.Add(productDetail);
                    }

                    invoice.InvoiceDetails = invoiceDetails;

                    dbContext.Invoices.Add(invoice);
                }

                dbContext.ProductDetails.AddRange(productsDetails);

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Created();
            });

        app.MapPost(string.Empty,
            async (RequestDto request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var invoices =
                await dbContext.Invoices
                .Include(i => i.InvoiceDetails!)
                .ThenInclude(i => i.Product)
                .Include(s => s.Salesman)
                .Where(i => i.Date >= request.StartDate && i.Date <= request.EndDate)
                .Where(i => request.Search != null ? i.InvoiceNumber.Contains(request.Search) : true)
                .Select(s => new InvoiceDto
                {
                    Id = s.Id,
                    InvoiceNumber = s.InvoiceNumber,
                    Date = s.Date,
                    CustomerName = s.CustomerName,
                    SalesmanName = s.Salesman!.Name,
                    Total = s.InvoiceDetails!.Sum(s => s.Quantity * s.Price)
                })
                .OrderBy(o => o.Date)
                .ToListAsync(cancellationToken);
                return Results.Ok(invoices);
            })
            .Produces<List<InvoiceDto>>();

        return builder;
    }

    public static DateOnly GetRandomDate(DateOnly start, DateOnly end)
    {
        Random random = new();
        int range = (end.DayNumber - start.DayNumber);
        return start.AddDays(random.Next(range));
    }
}