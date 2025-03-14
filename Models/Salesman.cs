namespace FlexiReportServer.Models;

public sealed class Salesman
{
    public Salesman()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}
