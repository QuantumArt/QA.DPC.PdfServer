using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Caching;

#if NETCOREAPP
using Wkhtmltopdf.NetCore;
#endif

namespace QA.DPC.PDFServer.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddHttpClient();
            services.Configure<ConfigurationServiceSettings>(Configuration.GetSection("ConfigurationService"));
            services.Configure<DpcApiSettings>(Configuration.GetSection("DPCApi"));
//            services.Configure<DpcDbApiSettings>(Configuration.GetSection("DPCDbApi"));
//            services.Configure<ImpactApiSettings>(Configuration.GetSection("ImpactApi"));
            services.Configure<PdfTemplateSelectorSettings>(Configuration.GetSection("PdfTemplateSelector"));
            services.Configure<NodeServerSettings>(Configuration.GetSection("NodeServer"));
            services.Configure<PdfStaticFilesSettings>(Configuration.GetSection("PdfStaticFiles"));
            services.Configure<PdfSettings>(Configuration.GetSection("PdfPageSettings"));
            services.Configure<CacheSettings>(Configuration.GetSection("CacheSettings"));

            services.AddSingleton<ICacheProvider, VersionedCacheCoreProvider>();
            services.AddTransient<IConfigurationServiceClient, ConfigurationServiceClient>();
            services.AddTransient<IPdfGenerationSettingsProvider, PdfGenerationSettingsProvider>();
            services.AddTransient<IDpcDbClient, DpcDbClient>();
            services.AddTransient<IDpcApiClient, DpcApiClient>();
//            services.AddTransient<IDpcDbApiClient, DpcDbApiClient>();
            services.AddTransient<IImpactApiClient, ImpactApiClient>();
            services.AddTransient<IPdfTemplateSelector, PdfTemplateSelector>();
            services.AddTransient<IHtmlGenerator, HtmlGenerator>();
            services.AddTransient<IProductJsonMapper, ProductJsonMapper>();
            services.AddTransient<IRegionTagsReplacer, RegionTagsReplacer>();
            
#if NETCOREAPP
            services.AddWkhtmltopdf();
#endif
            services.AddMemoryCache();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddNLog();
            app.AddNLogWeb();
            env.ConfigureNLog("nlog.config");
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseMvc();
            
            //app.UseStaticFiles();
            var staticFileSettings = Configuration.GetSection("PdfStaticFiles").Get<PdfStaticFilesSettings>();
            if (staticFileSettings.ServeStatic)
            {
                if (!Directory.Exists(staticFileSettings.RootOutputDirectory))
                    Directory.CreateDirectory(staticFileSettings.RootOutputDirectory);
                app.UseFileServer(new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider(staticFileSettings.RootOutputDirectory),
                    RequestPath = new PathString(staticFileSettings.DirectoryRelativePath)
                });
            }
            
        }
    }
}
