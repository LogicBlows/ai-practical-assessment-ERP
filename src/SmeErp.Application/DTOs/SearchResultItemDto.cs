namespace SmeErp.Application.DTOs;

public class SearchResultItemDto
{
    public int Id { get; set; }

    public SearchResultType Type { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string Url { get; set; } = string.Empty;
}
