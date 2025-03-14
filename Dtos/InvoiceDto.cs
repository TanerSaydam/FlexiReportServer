namespace FlexiReportServer.Dtos;

public sealed class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string SalesmanName { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public decimal Total { get; set; }
}
