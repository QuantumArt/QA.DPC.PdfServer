using System;

namespace QA.DPC.PDFServer.Services.DataContract.DpcApi
{
    public class PdfTemplate : DpcProductBase
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public FileInfo PdfTemplateFile { get; set; }
        public string PdfTemplateEngine { get; set; }
        public PdfScriptMapper PdfScriptMapper { get; set; }
        public PdfTemplateCategory PdfTemplateCategory { get; set; }
    }
}
