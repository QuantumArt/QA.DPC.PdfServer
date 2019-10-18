namespace QA.DPC.PDFServer.PdfGenerator
{
    public class PdfSettings
    {
        public PageSize PageSize { get; set; }
        /// <summary>
        /// MarginTop (in pixels)
        /// </summary>
        public float MarginTop { get; set; }
        /// <summary>
        /// MarginBottom (in pixels)
        /// </summary>
        public float MarginBottom { get; set; }
        /// <summary>
        /// MarginLeft (in pixels)
        /// </summary>
        public float MarginLeft { get; set; }
        /// <summary>
        /// MarginRight (in pixels)
        /// </summary>
        public float MarginRight { get; set; }
    }

    public enum PageSize
    {
        A4,
        A5
    }
}
