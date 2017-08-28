using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.DPC.PDFServer.Services.Settings
{
    public class PdfStaticFilesSettings
    {
        public string RootOutputDirectory { get; set; }
        public string DirectoryRelativePath { get; set; }
        public bool ServeStatic { get; set; }
    }
}
