using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using QA.DPC.PDFServer.PdfGenerator;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.Services;
using QA.DPC.PDFServer.Services.Interfaces;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Caching;
using QA.DPC.PdfServer.DevApi;
#if NETCOREAPP
using Wkhtmltopdf.NetCore;
#endif

namespace QA.DPC.PDFServer.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        private IConfiguration Configuration { get; }
        
        public IWebHostEnvironment CurrentEnvironment { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddHttpClient();
            services.Configure<ConfigurationServiceSettings>(Configuration.GetSection("ConfigurationService"));
            services.Configure<DpcApiSettings>(Configuration.GetSection("DPCApi"));
            services.Configure<ExtraSettings>(Configuration.GetSection("Extra"));
            services.Configure<PdfTemplateSelectorSettings>(Configuration.GetSection("PdfTemplateSelector"));
            services.Configure<NodeServerSettings>(Configuration.GetSection("NodeServer"));
            services.Configure<PdfStaticFilesSettings>(Configuration.GetSection("PdfStaticFiles"));
            services.Configure<PdfSettings>(Configuration.GetSection("PdfPageSettings"));
            services.Configure<CacheSettings>(Configuration.GetSection("CacheSettings"));
            
            var props = new ExtraSettings();
            Configuration.Bind("Extra", props);

            services.AddSingleton<ICacheProvider, VersionedCacheCoreProvider>();
            services.AddTransient<IConfigurationServiceClient, ConfigurationServiceClient>();
            services.AddTransient<IPdfGenerationSettingsProvider, PdfGenerationSettingsProvider>();
            services.AddTransient<IDpcDbClient, DpcDbClient>();
            services.AddTransient<IDpcApiClient, DpcApiClient>();
            services.AddTransient<IPdfTemplateSelector, PdfTemplateSelector>();
            services.AddTransient<IHtmlGenerator, HtmlGenerator>();
            services.AddTransient<IProductJsonMapper, ProductJsonMapper>();
            services.AddTransient<IRegionTagsReplacer, RegionTagsReplacer>();
            services.AddWkhtmltopdf();
            services.AddMemoryCache();
            services.AddSwaggerGen();
            services.AddControllers().ConfigureApplicationPartManager(apm =>
            {
                apm.ApplicationParts.Clear();
                apm.ApplicationParts.Add(new AssemblyPart(typeof(Startup).Assembly));
                if (CurrentEnvironment.IsDevelopment())
                {
                    apm.ApplicationParts.Add(new AssemblyPart(typeof(ConfigurationController).Assembly));
                }
                
                foreach (var library in props.Libraries)
                {
                    var assembly = Assembly.LoadFile(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, library + ".dll"
                    ));
                    apm.ApplicationParts.Add(new AssemblyPart(assembly));
                    var t = assembly.GetType($"{library}.Startup");
                    if (t != null)
                    {
                        var m = t.GetMethod("ConfigureServices");
                        if (m != null)
                        {
                            m.Invoke(null, new object[] {services, Configuration});
                        }
                    }

                }

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            
            app.UseRouting();
            app.UseEndpoints(routes =>
            {
                routes.MapControllers();
            });

            
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
