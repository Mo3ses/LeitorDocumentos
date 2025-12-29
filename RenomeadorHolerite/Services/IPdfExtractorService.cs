namespace RenomeadorHolerite.Services
{
    public interface IPdfExtractorService
    {
        string ExtrairNome(Stream pdfStream, string tipoDocumento);
    }
}