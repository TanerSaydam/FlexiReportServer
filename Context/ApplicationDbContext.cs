using FlexiReport.Models;
using FlexiReportServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexiReportServer.Context;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductDetail> ProductDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Salesman> Salesmans { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<RequestElement> RequestElements { get; set; }
}
