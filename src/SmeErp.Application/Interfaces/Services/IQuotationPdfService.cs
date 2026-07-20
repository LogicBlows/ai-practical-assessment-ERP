using SmeErp.Application.DTOs;

namespace SmeErp.Application.Interfaces.Services;

public interface IQuotationPdfService
{
    byte[] GeneratePdf(QuotationDetailDto quotation, CompanySettingsDto companySettings);
}
