namespace FlexiReportServer.Models;

public sealed class InvoiceDetail
{
    public InvoiceDetail()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
