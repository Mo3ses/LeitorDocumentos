using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using RenomeadorHolerite.Services;
using UglyToad.PdfPig; // Precisamos disso aqui para o Debug

namespace RenomeadorHolerite.Controllers
{
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IPdfExtractorService _pdfService;

        public UploadController(IPdfExtractorService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> ProcessarArquivos()
        {
            var form = await Request.ReadFormAsync();
            var files = form.Files;
            var tipoDoc = form["tipoDoc"].ToString();

            if (files.Count == 0) return BadRequest("Nenhum arquivo enviado.");

            var relatorio = new List<object>();
            var nomesUsados = new HashSet<string>();

            using var zipMemoryStream = new MemoryStream();

            using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    if (file.ContentType != "application/pdf") continue;

                    string novoNome = "";
                    string status = "Renomeado";
                    string debugTexto = ""; // Variável para guardar o texto bruto

                    using (var fileMemoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(fileMemoryStream);
                        fileMemoryStream.Position = 0;

                        // 1. Tenta extrair o nome
                        novoNome = _pdfService.ExtrairNome(fileMemoryStream, tipoDoc);

                        // 2. DEBUG: Lê o texto cru para mostrar no site caso precise investigar
                        fileMemoryStream.Position = 0;
                        try
                        {
                            using var docDebug = PdfDocument.Open(fileMemoryStream);
                            if (docDebug.NumberOfPages > 0)
                            {
                                // Pega os primeiros 500 caracteres da pág 1 para não ficar gigante
                                string rawText = docDebug.GetPage(1).Text;
                                debugTexto = rawText.Length > 800 ? rawText.Substring(0, 800) + "..." : rawText;
                            }
                        }
                        catch { debugTexto = "Erro ao ler texto para debug."; }


                        if (string.IsNullOrWhiteSpace(novoNome))
                        {
                            novoNome = $"NAO_IDENTIFICADO_{file.FileName}";
                            status = "Falha (Clique em Ver Texto)";
                        }
                        else
                        {
                            string sufixo = "";
                            if (tipoDoc == "comprovante") sufixo = " COMPROVANTE.pdf";
                            else if (tipoDoc == "recibo") sufixo = " RECIBO.pdf";
                            else sufixo = " HOLERITE.pdf";

                            novoNome = $"{novoNome}{sufixo}";
                        }

                        // Lógica de Duplicatas
                        var nomeBase = Path.GetFileNameWithoutExtension(novoNome);
                        var ext = Path.GetExtension(novoNome);
                        var nomeFinal = novoNome;
                        int contador = 1;

                        while (nomesUsados.Contains(nomeFinal))
                        {
                            nomeFinal = $"{nomeBase}_{contador}{ext}";
                            status = "Renomeado (Duplicado)";
                            contador++;
                        }

                        nomesUsados.Add(nomeFinal);

                        // Adicionamos o campo 'debug' na resposta JSON
                        relatorio.Add(new
                        {
                            original = file.FileName,
                            novo = nomeFinal,
                            status = status,
                            debug = debugTexto
                        });

                        var entry = archive.CreateEntry(nomeFinal);
                        using var entryStream = entry.Open();
                        fileMemoryStream.Position = 0;
                        await fileMemoryStream.CopyToAsync(entryStream);
                    }
                }
            }

            zipMemoryStream.Position = 0;
            return Ok(new { lista = relatorio, arquivoZip = Convert.ToBase64String(zipMemoryStream.ToArray()) });
        }
    }
}