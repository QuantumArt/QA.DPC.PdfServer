using System;
using System.Collections.Generic;
using System.Text;

namespace QA.DPC.PDFServer.Services.DataContract
{
    public class DpcPdfTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DpcFileInfo PdfTemplateFile { get; set; }
        public string PdfTemplateEngine { get; set; }
        public DpcPdfScriptMapper PdfScriptMapper { get; set; }
        public DpcPdfTemplateCategory PdfTemplateCategory { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
