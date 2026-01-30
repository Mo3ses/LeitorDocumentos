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

                // Log para Debug
                InMemoryLogger.Log($"[PROCESSANDO] Tipo: {tipoDocumento}");
                if (tipoDocumento == "comprovante") return ExtrairDeComprovante(text);
                if (tipoDocumento == "recibo") return ExtrairDeRecibo(text);
                return ExtrairDeHolerite(text);
            }
            catch (Exception ex)
            {
                InMemoryLogger.Log($"[ERRO FATAL] {ex.Message}");
                return "";
            }
        }

        private string ExtrairDeHolerite(string text)
        {
            var padrao = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ\s]+)(?=Nome do Funcionário)";
            var match = Regex.Match(text, padrao);
            if (match.Success) return LimparNome(match.Groups[1].Value);
            return "";
        }

        private string ExtrairDeComprovante(string text)
        {
            var padrao = @"FAVORECIDO[:\s]+(.*?)(?=CPF|\n|$)";
            var match = Regex.Match(text, padrao, RegexOptions.Singleline);
            if (match.Success) return LimparNome(match.Groups[1].Value);
            return "";
        }

        private string ExtrairDeRecibo(string text)
        {
            // ESTRATÉGIA 1 (A SALVAÇÃO): Âncora no CPF
            // O texto vem sujo assim: "1.875,59ADRIANA...OLIVEIRACPF184..."
            // A regex busca letras ([A-Z...]+) que terminam exatamente onde começa "CPF"
            var padraoCpf = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]{5,})CPF";
            var match = Regex.Match(text, padraoCpf);
            if (match.Success)
            {
                var nome = match.Groups[1].Value.Trim();
                // Filtra falsos positivos (como nomes de colunas)
                if (!nome.Contains("BASE") && !nome.Contains("CÁLCULO"))
                {
                    return LimparNome(nome);
                }
            }

            // ESTRATÉGIA 2: Âncora no "Recebi da firma" (Caso Arilene)
            var padraoRecebi = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]{5,})(?:CPF|[\d\.\-\/\s]+)*Recebi da firma";
            match = Regex.Match(text, padraoRecebi);
            if (match.Success) return LimparNome(match.Groups[1].Value);

            // ESTRATÉGIA 3: Rodapé (ONG)
            var padraoOng = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]{5,})ONG\s?-";
            match = Regex.Match(text, padraoOng);
            if (match.Success) return LimparNome(match.Groups[1].Value);

            return "";
        }

        private string LimparNome(string nomeBruto)
        {
            var nome = nomeBruto.Trim();
            var sujeiras = new[] { "CÓDIGO", "CODIGO", "MATRÍCULA", "MATRICULA", "NOME", "CC", "FAVORECIDO", "DO EMPREGADO", "EMPREGADO", "TOTAL", "LÍQUIDO", "LIQUIDO" };
            foreach (var s in sujeiras)
            {
                if (nome.Contains(s, StringComparison.OrdinalIgnoreCase))
                    nome = nome.Replace(s, "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            nome = RemoverAcentos(nome);
            nome = Regex.Replace(nome, @"[^a-zA-Z\s]", "").Trim();

            // Corte de segurança
            if (nome.Length > 30) nome = nome.Substring(0, 30).Trim();

            return nome;
        }

        private string RemoverAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}