using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace System
{
    public static class GlobalConfiguration
    {

        /// <summary>
        /// 获取基于根目录的子文件。
        /// </summary>
        /// <param name="context">HTTP 上下文。</param>
        /// <param name="subpath">路径。</param>
        /// <returns>子文件。</returns>
        public static IFileInfo Content(this HttpContext context, string subpath)
        {
            return context.RequestServices.GetRequiredService<IHostingEnvironment>().ContentRootFileProvider.GetFileInfo(subpath);
        }

        /// <summary>
        /// 获取基于 wwwroot 的子文件。
        /// </summary>
        /// <param name="context">HTTP 上下文。</param>
        /// <param name="subpath">路径。</param>
        /// <returns>子文件。</returns>
        public static IFileInfo Web(this HttpContext context, string subpath)
        {
            return context.RequestServices.GetRequiredService<IHostingEnvironment>().WebRootFileProvider.GetFileInfo(subpath);
        }
    }
}
