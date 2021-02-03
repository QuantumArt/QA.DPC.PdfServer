using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

namespace QA.DPC.PdfServer.CmdPdfConverter
{
    public class ApplicationArguments
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }

        public OptionSet GetOptionSet()
        {
            return new OptionSet
            {
                {"i|input=", "Input file path is required", i => InputFile = i},
                {"o|output", "Output file path is required", o => OutputFile = o},
            };
        }
    }
}
