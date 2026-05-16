namespace HRSystem.API.Services.Pdf;

public interface IPdfTemplateRenderer
{
    byte[] RenderPayslip(PayslipPdfModel model);
    byte[] RenderEmploymentLetter(EmploymentLetterModel model);
    byte[] RenderSalaryCertificate(SalaryCertificateModel model);
}
