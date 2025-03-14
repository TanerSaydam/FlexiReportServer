namespace FlexiReportServer.Dtos;

public sealed record ReportDto(
    Guid? Id,
    string Content,
    string Endpoint,
    string Name,
    string PageSize,
    string PageOrientation,
    string FontFamily,
    string SqlQuery);