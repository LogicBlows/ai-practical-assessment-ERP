namespace SmeErp.Application.DTOs;

public class GlobalSearchResultDto
{
    public string Keyword { get; set; } = string.Empty;

    public bool KeywordTooShort { get; set; }

    public IReadOnlyList<SearchResultItemDto> Products { get; set; } = Array.Empty<SearchResultItemDto>();

    public IReadOnlyList<SearchResultItemDto> Customers { get; set; } = Array.Empty<SearchResultItemDto>();

    public IReadOnlyList<SearchResultItemDto> Quotations { get; set; } = Array.Empty<SearchResultItemDto>();

    public bool HasResults =>
        Products.Count > 0 || Customers.Count > 0 || Quotations.Count > 0;
}
