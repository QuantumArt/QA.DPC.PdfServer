using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fclp;
using QA.DPC.PDFServer.PdfGenerator;

namespace QA.DPC.PdfServer.CmdPdfConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = ParseArguments(args);
            if (parsedArgs == null) return;
            try
            {
                using (var reader = File.OpenText(parsedArgs.InputFile))
                {
                    var html = reader.ReadToEnd();
                    var pdfDocument = PdfGenerator.GeneratePdf(html, GetPdfSettins());
                    using (var writeStream = File.OpenWrite(parsedArgs.OutputFile))
                    {
                        using (var writer = new BinaryWriter(writeStream))
                        {
                            writer.Write(pdfDocument);
                        }
                    }
                }
                Console.WriteLine("Done!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static PdfSettings GetPdfSettins()
        {
            return new PdfSettings
            {
                MarginBottom = float.Parse(ConfigurationManager.AppSettings["MarginBottom"]),
                MarginTop = float.Parse(ConfigurationManager.AppSettings["MarginTop"]),
                MarginLeft = float.Parse(ConfigurationManager.AppSettings["MarginLeft"]),
                MarginRight = float.Parse(ConfigurationManager.AppSettings["MarginRight"]),
            };
        }

        private static ApplicationArguments ParseArguments(string[] args)
        {
            var p = new FluentCommandLineParser<ApplicationArguments>();
            p.Setup(arg => arg.InputFile)
                .As('i', "input")
                .Required()
                .WithDescription("Input file path is required");

            p.Setup(arg => arg.OutputFile)
                .As('o', "output")
                .Required()
                .WithDescription("Output file path is required");

            p.SetupHelp("?", "help")
                .Callback(text => Console.WriteLine(text));

            var commandLineParserResult = p.Parse(args);
            if (!commandLineParserResult.HasErrors)
            {
                return p.Object;
            }
            else
            {
                Console.WriteLine(commandLineParserResult.ErrorText);
                p.HelpOption.ShowHelp(p.Options);
                return null;
            }

        }
    }
}
