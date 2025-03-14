namespace FlexiReportServer.Models;

public sealed class RequestElement
{
    public RequestElement()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public int Index { get; set; }
    public string Label { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Endpoint { get; set; }
}
