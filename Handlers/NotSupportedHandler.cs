using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Libs.UEditor.Handlers
{
    /// <summary>
    /// NotSupportedHandler 的摘要说明
    /// </summary>
    public class NotSupportedHandler : Handler
    {
        public NotSupportedHandler(HttpContext context)
            : base(context)
        {
        }

        public override Task ProcessAsync()
        {
            return WriteJsonAsync(new
            {
                state = "action 参数为空或者 action 不被支持。"
            });
        }
    }
}
