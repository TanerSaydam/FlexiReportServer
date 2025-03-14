namespace FlexiReportServer.Models;

public sealed class Category
{
    public Category()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}
