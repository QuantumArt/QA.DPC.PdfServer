using System;
using System.IO;
using EvoPdf;

namespace QA.DPC.PDFServer.PdfGenerator
{
    public class PdfGenerator 
    {
        public static string GeneratePdf(string html, string outputDir)
        {
            EnsureOutputDirExists(outputDir);
            var guid = Guid.NewGuid();
            var fileName = $"{guid}.pdf";
            var pdfGenerator = new HtmlToPdfConverter();
            //byte[] outPdfBuffer = null;
            //outPdfBuffer = pdfGenerator.ConvertHtml(html, string.Empty);
            pdfGenerator.ConvertHtmlToFile(html, string.Empty, Path.Combine(outputDir, fileName));
            return fileName;
        }

        private static void EnsureOutputDirExists(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }
    }
}
