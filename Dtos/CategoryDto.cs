namespace FlexiReportServer.Dtos;

public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int ProductCounts { get; set; }
}
