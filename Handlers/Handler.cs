using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Libs.UEditor.Handlers
{
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public abstract class Handler
    {
        public HttpRequest Request { get; }
        public HttpResponse Response { get; }
        public HttpContext Context { get; }

        public Handler(HttpContext context)
        {
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
        }

        public abstract Task ProcessAsync();

        protected virtual Task WriteJsonAsync(object response)
        {
            string jsonpCallback = Request.Query["callback"],
                json = JsonConvert.SerializeObject(response),
                contentType = null;

            if(!string.IsNullOrWhiteSpace(jsonpCallback))
            {
                contentType = "text/javascript";
                json = string.Format("{0}({1});", jsonpCallback, json);
            }
            return this.WriteAsync(json, contentType);
        }

        protected virtual Task WriteAsync(string content, string type = null)
        {
            Response.Headers.Add("Content-Type", type ?? "text/plain");
            var bytes = Encoding.UTF8.GetBytes(content);
            return Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

    }
}
