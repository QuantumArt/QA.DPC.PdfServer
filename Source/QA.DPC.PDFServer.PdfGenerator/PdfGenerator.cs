using System;
using System.IO;

#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;
using Wkhtmltopdf.NetCore;
#else
using EvoPdf;
#endif


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

#if NETCOREAPP
                var pdfGeneratorWk = GetWkConverter(serviceProvider, pdfSettings);
                return pdfGeneratorWk.GetPDF(html);
#else

                var pdfGeneratoEvo = GetEvoConverter(pdfSettings);
                var pdfDocument = pdfGeneratoEvo.ConvertHtmlToPdfDocumentObject(html, string.Empty);
                ApplyDigitalSignature(pdfGeneratoEvo);
                var pdfBytes = pdfDocument.Save();
                return pdfBytes;
#endif
            }
            catch (Exception e)
            {
                throw new PdfGenerationException("Error while generating pdf", e);
            }
        }


#if NETCOREAPP
        private static GeneratePdf GetWkConverter(IServiceProvider serviceProvider, PdfSettings pdfSettings)
        {
            const int dpi = 96;
            var pdfGeneratorWk = serviceProvider.GetService<IGeneratePdf>() as GeneratePdf;
            pdfGeneratorWk.PageMargins = new Wkhtmltopdf.NetCore.Options.Margins();
            pdfGeneratorWk.PageMargins.Bottom = (int)PixelsToMm(pdfSettings.MarginBottom, dpi);
            pdfGeneratorWk.PageMargins.Top = (int)PixelsToMm(pdfSettings.MarginTop, dpi);
            pdfGeneratorWk.PageMargins.Left = (int)PixelsToMm(pdfSettings.MarginLeft, dpi);
            pdfGeneratorWk.PageMargins.Right = (int)PixelsToMm(pdfSettings.MarginRight, dpi);
            pdfGeneratorWk.PageSize = MapWkPdfPageSize(pdfSettings.PageSize);
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
#else
        private static void ApplyDigitalSignature(HtmlToPdfConverter pdfGenerator)
        {
            var digitalSignatureMapping = pdfGenerator.HtmlElementsMappingOptions.HtmlElementsMappingResult.GetElementByMappingId("digital_signature_element");
            if (digitalSignatureMapping != null)
            {
                var digitalSignaturePage = digitalSignatureMapping.PdfRectangles[0].PdfPage;
                var digitalSignatureRectangle = digitalSignatureMapping.PdfRectangles[0].Rectangle;
                const string certificateFilePath = "Cert\\mycert.pfx";
                var certificates = DigitalCertificatesStore.GetCertificates(certificateFilePath, "qwerty");
                var signature =
                    new DigitalSignatureElement(digitalSignatureRectangle, certificates[0])
                    {
                        Reason = "Protect the document from unwanted changes",
                        ContactInfo = "The contact email is support@quantumart.ru",
                        Location = "Development server"
                    };
                digitalSignaturePage.AddElement(signature);
            }
        }


        private static HtmlToPdfConverter GetEvoConverter(PdfSettings pdfSettings)
        {
            var htmlToPdfConverter = new HtmlToPdfConverter {LicenseKey = "T8HSwNXQwNHXwNXO0MDT0c7R0s7Z2dnZ"};
            htmlToPdfConverter.PdfDocumentOptions.TopMargin = pdfSettings.MarginTop;
            htmlToPdfConverter.PdfDocumentOptions.BottomMargin = pdfSettings.MarginBottom;
            htmlToPdfConverter.PdfDocumentOptions.LeftMargin = pdfSettings.MarginLeft;
            htmlToPdfConverter.PdfDocumentOptions.RightMargin = pdfSettings.MarginRight;
            htmlToPdfConverter.PdfDocumentOptions.PdfPageSize = MapPdfPageSize(pdfSettings.PageSize);
            return htmlToPdfConverter;
        }

        private static PdfPageSize MapPdfPageSize(PageSize pdfSettingsPageSize)
        {
            switch (pdfSettingsPageSize)
            {
                case PageSize.A4:
                    return PdfPageSize.A4;
                case PageSize.A5:
                    return PdfPageSize.A5;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pdfSettingsPageSize), pdfSettingsPageSize, null);
            }
        }
#endif

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
