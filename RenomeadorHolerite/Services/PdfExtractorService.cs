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

                // --- DEBUG NO CONSOLE (Aparece no Portainer) ---
                Console.WriteLine("========================================");
                Console.WriteLine($"[DEBUG] Processando Tipo: {tipoDocumento}");
                Console.WriteLine($"[DEBUG] Texto Bruto (Primeiros 2000 chars): {text.Substring(0, Math.Min(text.Length, 2000)).Replace("\n", " [NL] ")}");
                Console.WriteLine("========================================");
                // ------------------------------------------------

                if (tipoDocumento == "comprovante")
                {
                    return ExtrairDeComprovante(text);
                }
                else if (tipoDocumento == "recibo")
                {
                    return ExtrairDeRecibo(text);
                }
                else
                {
                    return ExtrairDeHolerite(text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO FATAL] {ex.Message}");
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

        // --- LÓGICA DO RECIBO REFEITA ---
        // --- SUBSTITUA ESSA FUNÇÃO NO Services/PdfExtractorService.cs ---
        private string ExtrairDeRecibo(string text)
        {
            // ESTRATÉGIA "CIRÚRGICA": Usar a frase padrão "Recebi da firma" como âncora.
            // O texto vem colado assim: "2.107,40ARILENE...OLIVEIRARecebi da firma"
            // A regex procura letras maiúsculas e espaços ([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]+) 
            // que aparecem logo antes de "Recebi da firma".
            var padraoRecebi = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]+)Recebi da firma";
            var match = Regex.Match(text, padraoRecebi);

            if (match.Success)
            {
                var nome = match.Groups[1].Value.Trim();
                // Retorna apenas se tiver um tamanho razoável (evita pegar só "A")
                if (nome.Length > 5) return LimparNome(nome);
            }

            // ESTRATÉGIA RESERVA (RODAPÉ):
            // O texto no final tem: "ARILENE ... OLIVEIRAONG"
            // Procura letras maiúsculas antes de "ONG -" ou "ONG - INSTITUTO"
            var padraoOng = @"([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]+)ONG\s?-";
            match = Regex.Match(text, padraoOng);

            if (match.Success)
            {
                var nome = match.Groups[1].Value.Trim();
                if (nome.Length > 5) return LimparNome(nome);
            }

            // ESTRATÉGIA "DESESPERO" (Busca por "Nome do empregado" e tenta limpar o lixo numérico)
            // Se o texto for "...Nome do empregado... 2.107,40NOME DA PESSOA..."
            var padraoGeral = @"Nome do empregado.*?([A-ZÁÀÂÃÉÈÍÏÓÔÕÖÚÇÑ ]{10,})";
            match = Regex.Match(text, padraoGeral);
            if (match.Success)
            {
                // Aqui removemos qualquer coisa que tenha sobrado de números
                var nomeSujo = match.Groups[1].Value;
                // Pega a última sequência de letras puras (caso tenha pego "40NOME")
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

            // --- CORTE DE SEGURANÇA (MÁXIMO 30 CARACTERES) ---
            if (nome.Length > 30)
            {
                nome = nome.Substring(0, 30).Trim();
            }

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
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}