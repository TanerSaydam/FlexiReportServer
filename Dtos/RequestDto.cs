namespace FlexiReportServer.Dtos;

public sealed record RequestDto(
    DateOnly StartDate,
    DateOnly EndDate,
    string? Search);
