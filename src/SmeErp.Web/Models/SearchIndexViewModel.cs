using SmeErp.Application.DTOs;

namespace SmeErp.Web.Models;

public class SearchIndexViewModel
{
    public string? Keyword { get; set; }

    public GlobalSearchResultDto Results { get; set; } = new();
}
