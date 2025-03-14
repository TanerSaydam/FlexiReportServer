namespace FlexiReportServer.Dtos;

public sealed record ReportDto(
    Guid? Id,
    string Content,
    string Endpoint,
    string Name,
    string PageSize,
    string PageOrientation,
    string FontFamily,
    string SqlQuery,
    string BackgroundColor,
    List<RequestElementDto> RequestElements);

public sealed record RequestElementDto(
    int Index,
    string Label,
    string Type,
    string Name,
    string? Endpoint);