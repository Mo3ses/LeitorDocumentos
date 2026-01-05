using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using RenomeadorHolerite.Services;

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

                    using (var fileMemoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(fileMemoryStream);
                        fileMemoryStream.Position = 0;

                        // Chama o serviço para descobrir o nome da pessoa
                        novoNome = _pdfService.ExtrairNome(fileMemoryStream, tipoDoc);

                        if (string.IsNullOrWhiteSpace(novoNome))
                        {
                            novoNome = $"NAO_IDENTIFICADO_{file.FileName}";
                            status = "Não Identificado";
                        }
                        else
                        {
                            string sufixo = (tipoDoc == "comprovante")
                                ? " COMPROVANTE.pdf"
                                : " HOLERITE.pdf";

                            novoNome = $"{novoNome}{sufixo}";
                            // ----------------------
                        }

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
                        relatorio.Add(new { original = file.FileName, novo = nomeFinal, status = status });

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