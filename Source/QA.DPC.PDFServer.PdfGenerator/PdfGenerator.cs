using EvoPdf;

namespace QA.DPC.PDFServer.PdfGenerator
{
    public class PdfGenerator   
    {
        public static byte[] GeneratePdf(string html)
        {
            var pdfGenerator = new HtmlToPdfConverter();
            byte[] outPdfBuffer = null;
            outPdfBuffer = pdfGenerator.ConvertHtml(html, string.Empty);
            
            return outPdfBuffer;
        }
    }
}
