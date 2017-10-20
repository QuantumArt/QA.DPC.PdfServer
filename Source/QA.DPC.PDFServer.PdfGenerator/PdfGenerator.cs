using System;
using System.IO;
using EvoPdf;

namespace QA.DPC.PDFServer.PdfGenerator
{
    public static class PdfGenerator
    {
        public static string GeneratePdf(string html, PdfSettings pdfSettings, string outputDir)
        {
            try
            {
                EnsureOutputDirExists(outputDir);
                var guid = Guid.NewGuid();
                var fileName = $"{guid}.pdf";

                var pdfBytes = GeneratePdf(html, pdfSettings);
                using (var stream = new FileStream(Path.Combine(outputDir, fileName), FileMode.Create))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(pdfBytes);
                    }
                }
                return fileName;
            }
            catch (PdfGenerationException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new PdfGenerationException("Error while generating pdf", e);
            }
        }

        public static byte[] GeneratePdf(string html, PdfSettings pdfSettings)
        {
            try
            {
                var pdfGenerator = GetConverter(pdfSettings);
                var pdfDocument = pdfGenerator.ConvertHtmlToPdfDocumentObject(html, string.Empty);
                ApplyDigitalSignature(pdfGenerator);
                var pdfBytes = pdfDocument.Save();
                return pdfBytes;
            }
            catch (Exception e)
            {
                throw new PdfGenerationException("Error while generating pdf", e);
            }
        }

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


        private static HtmlToPdfConverter GetConverter(PdfSettings pdfSettings)
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

        private static void EnsureOutputDirExists(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }
    }
}
