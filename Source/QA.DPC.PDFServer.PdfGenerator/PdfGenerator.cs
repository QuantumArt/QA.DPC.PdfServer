using System;
using System.IO;
using EvoPdf;

namespace QA.DPC.PDFServer.PdfGenerator
{
    public static class PdfGenerator
    {
        public static string GeneratePdf(string html, string outputDir)
        {
            try
            {
                EnsureOutputDirExists(outputDir);
                var guid = Guid.NewGuid();
                var fileName = $"{guid}.pdf";

                var pdfBytes = GeneratePdf(html);
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

        public static byte[] GeneratePdf(string html)
        {
            try
            {
                var pdfGenerator = GetConverter();
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


        private static HtmlToPdfConverter GetConverter()
        {
            return new HtmlToPdfConverter { LicenseKey = "T8HSwNXQwNHXwNXO0MDT0c7R0s7Z2dnZ" };
        }

        private static void EnsureOutputDirExists(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }
    }
}
