using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace ETFileServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDirectoryBrowser();  //开启目录浏览
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            
            string configDir = Program.mConfig["DirectoryPath"];
            
            configDir = new DirectoryInfo(configDir).FullName;
            Console.WriteLine(configDir);
            UseStaticFiles(app, configDir); 
            app.Run(async (context) => { await context.Response.WriteAsync("Welcome to the ET file server!"); });
        }

        private void UseStaticFiles(IApplicationBuilder app, string filePath)
        {
            var staticfile = new StaticFileOptions
            {
                ServeUnknownFileTypes = true, 
                FileProvider = new PhysicalFileProvider(filePath),
                DefaultContentType = "application/x-msdownload"
            };
            // 设置MIME类型类型
            staticfile.ContentTypeProvider = new FileExtensionContentTypeProvider
            {    
                Mappings =
                {
                    ["*"] = "application/x-msdownload"
                }
            };
            app.UseDirectoryBrowser(new DirectoryBrowserOptions(){ FileProvider = staticfile.FileProvider });
            app.UseStaticFiles(staticfile);
        }
    }
}