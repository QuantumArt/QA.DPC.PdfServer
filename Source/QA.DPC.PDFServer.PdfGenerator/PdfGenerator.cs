using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Wkhtmltopdf.NetCore;
using Wkhtmltopdf.NetCore.Options;


namespace QA.DPC.PDFServer.PdfGenerator
{
    public static class PdfGenerator
    {
        public static string GeneratePdf(string html, PdfSettings pdfSettings, IServiceProvider serviceProvider, string outputDir)
        {
            try
            {
                EnsureOutputDirExists(outputDir);
                var guid = Guid.NewGuid();
                var fileName = $"{guid}.pdf";

                var pdfBytes = GeneratePdf(html, pdfSettings, serviceProvider);
                using (var stream = new FileStream(Path.Combine(outputDir, fileName), FileMode.Create))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(pdfBytes);
                    }
                }
                return fileName;
            }
            catch (PdfGenerationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new PdfGenerationException("Error while generating pdf", e);
            }
        }

        public static byte[] GeneratePdf(string html, PdfSettings pdfSettings, IServiceProvider serviceProvider)
        {
            try
            {
                var pdfGeneratorWk = GetWkConverter(serviceProvider, pdfSettings);
                return pdfGeneratorWk.GetPDF(html);
            }
            catch (Exception e)
            {
                throw new PdfGenerationException("Error while generating pdf", e);
            }
        }

        private static GeneratePdf GetWkConverter(IServiceProvider serviceProvider, PdfSettings pdfSettings)
        {
            const int dpi = 96;
            var pdfGeneratorWk = serviceProvider.GetService<IGeneratePdf>() as GeneratePdf;
            if (pdfGeneratorWk != null)
            {
                var opts = new ConvertOptions()
                {
                    PageMargins = new Margins()
                    {
                        Bottom = (int) PixelsToMm(pdfSettings.MarginBottom, dpi),
                        Top = (int) PixelsToMm(pdfSettings.MarginTop, dpi),
                        Left = (int) PixelsToMm(pdfSettings.MarginLeft, dpi),
                        Right = (int) PixelsToMm(pdfSettings.MarginRight, dpi)
                    },
                    PageSize = MapWkPdfPageSize(pdfSettings.PageSize)
                };
                pdfGeneratorWk.SetConvertOptions(opts);              
            }
            return pdfGeneratorWk;
        }

        private static Wkhtmltopdf.NetCore.Options.Size MapWkPdfPageSize(PageSize pdfSettingsPageSize)
        {
            switch (pdfSettingsPageSize)
            {
                case PageSize.A4:
                    return Wkhtmltopdf.NetCore.Options.Size.A4;
                case PageSize.A5:
                    return Wkhtmltopdf.NetCore.Options.Size.A5;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pdfSettingsPageSize), pdfSettingsPageSize, null);
            }
        }

        private static void EnsureOutputDirExists(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }

        private static float PixelsToMm(float pixels, int dpi)
        {
            const float onepixel_onedpi_mm = 25.4f;
            return pixels * onepixel_onedpi_mm / dpi;
        }
    }
}
