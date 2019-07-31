namespace QA.DPC.PDFServer.Services.Settings
{
    public class PdfStaticFilesSettings
    {
        public string RootOutputDirectory { get; set; }
        public string DirectoryRelativePath { get; set; }
        public bool ServeStatic { get; set; }
    }
}
