using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Libs.UEditor.Handlers
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public class ConfigHandler : Handler
    {
        public string Json { get; }

        private readonly DateTime _createdTime;

        public ConfigHandler(HttpContext context, string json, DateTime createdTime) : base(context)
        {
            this.Json = json;
            this._createdTime = createdTime;
        }

        public override /*async*/ Task ProcessAsync()
        {
            //await this.Context.ResponseIfModifiedAsync(this._createdTime, 60 * 60, () =>
            //{
                return this.WriteAsync(this.Json);
            //});
        }
    }
}
