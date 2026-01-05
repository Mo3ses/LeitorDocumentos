using UglyToad.PdfPig;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

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
            var nome = nomeBruto.Trim();

            var sujeiras = new[] { "CÓDIGO", "CODIGO", "MATRÍCULA", "MATRICULA", "NOME", "CC", "FAVORECIDO" };
            foreach (var s in sujeiras)
            {
                if (nome.Contains(s, StringComparison.OrdinalIgnoreCase))
                    nome = nome.Replace(s, "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            nome = RemoverAcentos(nome);

            return Regex.Replace(nome, @"[^a-zA-Z\s]", "").Trim();
        }

        private string RemoverAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;

            // Normaliza para separar as letras dos acentos (Ex: 'ç' vira 'c' + '¸')
            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                // Se não for um acento/sinal, adiciona na string final
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}