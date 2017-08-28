﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.DPC.PDFServer.Services.Settings;
using QA.DPC.PDFServer.Services;

namespace QA.DPC.PDFServer.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<DpcApiSettings>(Configuration.GetSection("DPCApi"));
            services.Configure<PdfTemplateSelectorSettings>(Configuration.GetSection("PdfTemplateSelector"));
            services.Configure<NodeServerSettings>(Configuration.GetSection("NodeServer"));
            services.Configure<PdfStaticFilesSettings>(Configuration.GetSection("PdfStaticFiles"));
            services.AddTransient<IDpcApiClient, DpcApiClient>();
            services.AddTransient<IPdfTemplateSelector, PdfTemplateSelector>();
            services.AddTransient<IHtmlGenerator, HtmlGenerator>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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