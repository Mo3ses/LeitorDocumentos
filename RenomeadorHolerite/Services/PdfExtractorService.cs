using UglyToad.PdfPig;
using System.Text.RegularExpressions;

namespace RenomeadorHolerite.Services
{
    public class PdfExtractorService : IPdfExtractorService
    {
        public string ExtrairNome(Stream pdfStream, string tipoDocumento)
        {
            try
            {
                using var document = PdfDocument.Open(pdfStream);
                if (document.NumberOfPages == 0) return "";

                var text = document.GetPage(1).Text;

                if (tipoDocumento == "comprovante")
                {
                    return ExtrairDeComprovante(text);
                }
                else
                {
                    return ExtrairDeHolerite(text);
                }
            }
            catch
            {
                return "";
            }
        }

        private string ExtrairDeHolerite(string text)
        {
            var padrao = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ\s]+)(?=Nome do Funcionário)";
            var match = Regex.Match(text, padrao);

            if (match.Success)
            {
                return LimparNome(match.Groups[1].Value);
            }
            return "";
        }

        private string ExtrairDeComprovante(string text)
        {
            var padrao = @"FAVORECIDO[:\s]+(.*?)(?=CPF|\n|$)";
            var match = Regex.Match(text, padrao, RegexOptions.Singleline);

            if (match.Success)
            {
                return LimparNome(match.Groups[1].Value);
            }
            return "";
        }

        private string LimparNome(string nomeBruto)
        {
            var nomeEncontrado = nomeBruto.Trim();

            var sujeiras = new[] { "CÓDIGO", "CODIGO", "MATRÍCULA", "MATRICULA", "NOME", "CC", "FAVORECIDO" };
            foreach (var s in sujeiras)
            {
                if (nomeEncontrado.Contains(s, StringComparison.OrdinalIgnoreCase))
                    nomeEncontrado = nomeEncontrado.Replace(s, "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            return Regex.Replace(nomeEncontrado, @"[^a-zA-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ\s]", "").Trim();
        }
    }
}