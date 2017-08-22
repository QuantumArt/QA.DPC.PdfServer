using System;

namespace QA.DPC.PDFServer.Services.DataContract.DpcApi
{
    public class PdfScriptMapper
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public FileInfo PdfScriptMapperFile { get; set; }
        public DateTime Timestamp { get; set; }
    }
}