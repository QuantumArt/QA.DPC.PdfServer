using System;

namespace QA.DPC.PDFServer.Services.DataContract
{
    public class DpcPdfScriptMapper
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DpcFileInfo PdfScriptMapperFile { get; set; }
        public DateTime Timestamp { get; set; }
    }
}