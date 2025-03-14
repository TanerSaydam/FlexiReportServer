namespace FlexiReportServer.Models;

public sealed class Report
{
    public Report()
    {
        Id = Guid.CreateVersion7();
    }
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string Endpoint { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string PageSize { get; set; } = default!;
    public string PageOrientation { get; set; } = default!;
    public string FontFamily { get; set; } = default!;
    public string SqlQuery { get; set; } = string.Empty!;
    public string BackgroundColor { get; set; } = string.Empty!;
    public List<RequestElement>? RequestElements { get; set; }
}