namespace FlexiReportServer.Models;

public sealed class ProductDetail
{
    public ProductDetail()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = default!;
    public int StockEntry { get; set; }
    public int StockExit { get; set; }
    public decimal Price { get; set; }
}
