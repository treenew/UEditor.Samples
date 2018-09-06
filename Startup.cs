using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libs.UEditor;
using Libs.UEditor.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace UEditor.Samples
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            var configPath = "ueditor.json";
            var createdTime = DateTime.Now;

            var rawConfigJson = new System.IO.StreamReader(env.ContentRootFileProvider.GetFileInfo(configPath).CreateReadStream()).ReadToEnd();
            var pathFormatter = new PathFormatter();
            var options = new UEditorOptions(JObject.Parse(rawConfigJson), pathFormatter);
            app.Map(options.GetString("serverUrl"), builder =>
             {
                 builder.Run(async httpContext =>
                 {
                     Handler action = null;
                     switch(httpContext.Request.Query["action"])
                     {
                         case "config":
                             action = new ConfigHandler(httpContext, rawConfigJson, createdTime);
                             break;
                         case "uploadimage":
                             action = new UploadHandler(httpContext, options, new UploadConfig()
                             {
                                 AllowExtensions = options.GetStringList("imageAllowFiles"),
                                 PathFormat = options.GetString("imagePathFormat"),
                                 SizeLimit = options.GetInt("imageMaxSize"),
                                 UploadFieldName = options.GetString("imageFieldName")
                             });
                             break;
                         case "uploadvideo":
                             action = new UploadHandler(httpContext, options, new UploadConfig()
                             {
                                 AllowExtensions = options.GetStringList("videoAllowFiles"),
                                 PathFormat = options.GetString("videoPathFormat"),
                                 SizeLimit = options.GetInt("videoMaxSize"),
                                 UploadFieldName = options.GetString("videoFieldName")
                             });
                             break;
                         case "uploadfile":
                             action = new UploadHandler(httpContext, options, new UploadConfig()
                             {
                                 AllowExtensions = options.GetStringList("fileAllowFiles"),
                                 PathFormat = options.GetString("filePathFormat"),
                                 SizeLimit = options.GetInt("fileMaxSize"),
                                 UploadFieldName = options.GetString("fileFieldName")
                             });
                             break;
                         case "listimage":
                             action = new ListFileManager(httpContext, options, options.Formatter.Format(options.GetString("imageManagerListPath")), options.GetStringList("imageManagerAllowFiles"));
                             break;
                         case "listfile":
                             action = new ListFileManager(httpContext, options, options.Formatter.Format(options.GetString("fileManagerListPath")), options.GetStringList("fileManagerAllowFiles"));
                             break;
                         case "catchimage":
                             action = new CrawlerHandler(httpContext, options);
                             break;
                         default:
                             action = new NotSupportedHandler(httpContext);
                             break;
                     }
                     await action.ProcessAsync();
                 });
             });

        }
    }
}
