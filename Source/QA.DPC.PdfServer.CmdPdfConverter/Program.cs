using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using QA.DPC.PDFServer.PdfGenerator;

namespace QA.DPC.PdfServer.CmdPdfConverter
{
    class Program
    {
        static void Main(string[] args)
        {

            var argsResult = new ApplicationArguments();
            argsResult.GetOptionSet().Parse(args);
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var pdfSettings = new PdfSettings();
            config.GetSection("PdfSettings").Bind(pdfSettings);
            try
            {
                using (var reader = File.OpenText(argsResult.InputFile))
                {
                    var html = reader.ReadToEnd();
                    var pdfDocument = PdfGenerator.GeneratePdf(html, pdfSettings, null);
                    using (var writeStream = File.OpenWrite(argsResult.OutputFile))
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

    }
}
