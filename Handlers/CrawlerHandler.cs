using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Libs.UEditor.Handlers
{
    /// <summary>
    /// Crawler 的摘要说明
    /// </summary>
    public class CrawlerHandler : Handler
    {
        private string[] Sources;
        private Crawler[] Crawlers;

        private UEditorOptions Config { get; }

        public CrawlerHandler(HttpContext context, UEditorOptions config) : base(context)
        {
            this.Config = this.Config;
        }

        public override Task ProcessAsync()
        {
            this.Sources = this.Request.Form["source[]"].ToArray();
            if(this.Sources == null || this.Sources.Length == 0)
            {
                return this.WriteJsonAsync(new
                {
                    state = "参数错误：没有指定抓取源"
                });
            }

            this.Crawlers = this.Sources.Select(x => new Crawler(this.Context, this.Config, x).Fetch()).ToArray();
            return this.WriteJsonAsync(new
            {
                state = "SUCCESS",
                list = this.Crawlers.Select(x => new
                {
                    state = x.State,
                    source = x.SourceUrl,
                    url = x.ServerUrl
                })
            });
        }
    }
    public class Crawler
    {
        public string SourceUrl { get; set; }
        public string ServerUrl { get; private set; }
        public string State { get; private set; }

        private UEditorOptions Options { get; }

        private readonly HttpContext _context;

        public Crawler(HttpContext context, UEditorOptions options, string sourceUrl)
        {
            this.Options = this.Options;
            this.SourceUrl = sourceUrl;
            this._context = context;
        }

        public Crawler Fetch()
        {
            if(!this.IsExternalIPAddress(this.SourceUrl))
            {
                this.State = "INVALID_URL";
                return this;
            }
            var request = WebRequest.Create(this.SourceUrl) as HttpWebRequest;
            using(var response = request.GetResponse() as HttpWebResponse)
            {
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    this.State = "Url returns " + response.StatusCode + ", " + response.StatusDescription;
                    return this;
                }
                if(response.ContentType.IndexOf("image") == -1)
                {
                    this.State = "Url is not an image";
                    return this;
                }
                this.ServerUrl = this.Options.Formatter.Format(Path.GetFileName(this.SourceUrl), this.Options.GetString("catcherPathFormat"));
                var savePath = this._context.Web(this.ServerUrl).PhysicalPath;
                if(!Directory.Exists(Path.GetDirectoryName(savePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                }
                try
                {
                    var stream = response.GetResponseStream();
                    var reader = new BinaryReader(stream);
                    byte[] bytes;
                    using(var ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int count;
                        while((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, count);
                        }
                        bytes = ms.ToArray();
                    }
                    File.WriteAllBytes(savePath, bytes);
                    this.State = "SUCCESS";
                }
                catch(Exception e)
                {
                    this.State = "抓取错误：" + e.Message;
                }
                return this;
            }
        }

        private bool IsExternalIPAddress(string url)
        {
            var uri = new Uri(url);
            switch(uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                    foreach(IPAddress ipAddress in ipHostEntry.AddressList)
                    {
                        byte[] ipBytes = ipAddress.GetAddressBytes();
                        if(ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            if(!this.IsPrivateIP(ipAddress))
                            {
                                return true;
                            }
                        }
                    }
                    break;

                case UriHostNameType.IPv4:
                    return !this.IsPrivateIP(IPAddress.Parse(uri.DnsSafeHost));
            }
            return false;
        }

        private bool IsPrivateIP(IPAddress myIPAddress)
        {
            if(IPAddress.IsLoopback(myIPAddress)) return true;
            if(myIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                byte[] ipBytes = myIPAddress.GetAddressBytes();
                // 10.0.0.0/24 
                if(ipBytes[0] == 10)
                {
                    return true;
                }
                // 172.16.0.0/16
                else if(ipBytes[0] == 172 && ipBytes[1] == 16)
                {
                    return true;
                }
                // 192.168.0.0/16
                else if(ipBytes[0] == 192 && ipBytes[1] == 168)
                {
                    return true;
                }
                // 169.254.0.0/16
                else if(ipBytes[0] == 169 && ipBytes[1] == 254)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
