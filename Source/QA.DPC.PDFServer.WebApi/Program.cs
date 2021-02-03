using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace QA.DPC.PDFServer.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })                
                .UseNLog()
                .UseStartup<Startup>()
                .Build();
    }
}
