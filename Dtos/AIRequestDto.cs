namespace FlexiReportServer.Dtos;

public sealed record AIRequestDto(
    string ApiKey,
    string Prompt,
    string Model,
    string Schema);
