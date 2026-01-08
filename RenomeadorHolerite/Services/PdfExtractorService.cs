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

                // --- USANDO O NOVO LOGGER ---
                InMemoryLogger.Log($"----------------------------------------");
                InMemoryLogger.Log($"[PROCESSANDO] Tipo: {tipoDocumento}");
                InMemoryLogger.Log($"[PREVIEW] {text.Substring(0, Math.Min(text.Length, 100)).Replace("\n", " ")}...");

                string nomeEncontrado = "";

                if (tipoDocumento == "comprovante")
                {
                    nomeEncontrado = ExtrairDeComprovante(text);
                }
                else if (tipoDocumento == "recibo")
                {
                    nomeEncontrado = ExtrairDeRecibo(text);
                }
                else
                {
                    nomeEncontrado = ExtrairDeHolerite(text);
                }

                if (!string.IsNullOrEmpty(nomeEncontrado))
                    InMemoryLogger.Log($"[SUCESSO] Nome extraído: {nomeEncontrado}");
                else
                    InMemoryLogger.Log($"[FALHA] Não foi possível identificar o nome.");

                return nomeEncontrado;
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
            var padraoRecebi = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]+)Recebi da firma";
            var match = Regex.Match(text, padraoRecebi);
            if (match.Success)
            {
                var nome = match.Groups[1].Value.Trim();
                if (nome.Length > 5) return LimparNome(nome);
            }

            var padraoOng = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]+)ONG\s?-";
            match = Regex.Match(text, padraoOng);
            if (match.Success)
            {
                var nome = match.Groups[1].Value.Trim();
                if (nome.Length > 5) return LimparNome(nome);
            }

            var padraoGeral = @"Nome do empregado.*?([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]{10,})";
            match = Regex.Match(text, padraoGeral);
            if (match.Success)
            {
                var nomeSujo = match.Groups[1].Value;
                var matchLimpo = Regex.Match(nomeSujo, @"[A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]+$");
                if (matchLimpo.Success) return LimparNome(matchLimpo.Value);
            }

            return "";
        }

        private string LimparNome(string nomeBruto)
        {
            var nome = nomeBruto.Trim();
            var sujeiras = new[] { "CÓDIGO", "CODIGO", "MATRÍCULA", "MATRICULA", "NOME", "CC", "FAVORECIDO", "DO EMPREGADO", "EMPREGADO" };
            foreach (var s in sujeiras)
            {
                if (nome.Contains(s, StringComparison.OrdinalIgnoreCase))
                    nome = nome.Replace(s, "", StringComparison.OrdinalIgnoreCase).Trim();
            }
            nome = RemoverAcentos(nome);
            nome = Regex.Replace(nome, @"[^a-zA-Z\s]", "").Trim();
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
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}