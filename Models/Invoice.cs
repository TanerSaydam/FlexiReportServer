namespace FlexiReportServer.Models;

public sealed class Invoice
{
    public Invoice()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string CustomerName { get; set; } = default!;
    public Guid SalesmanId { get; set; }
    public Salesman? Salesman { get; set; }
    public List<InvoiceDetail>? InvoiceDetails { get; set; }
}
