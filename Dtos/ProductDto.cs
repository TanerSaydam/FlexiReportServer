namespace FlexiReportServer.Dtos;

public sealed class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public int StockEntry { get; set; }
    public int StockExit { get; set; }
    public int Stock { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal Total { get; set; }

}
