using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Libs.UEditor
{
    public class UEditorOptions
    {
        public JObject Configuration { get; }

        public IPathFormatter Formatter { get; }

        public UEditorOptions(JObject configuration, IPathFormatter formatter)
        {
            this.Configuration = configuration;
            this.Formatter = formatter;
        }

        public T GetValue<T>(string key) => this.Configuration.GetValue(key).ToObject<T>();

        public string[] GetStringList(string key) => this.GetValue<string[]>(key);

        public string GetString(string key) => this.GetValue<string>(key);

        public int GetInt(string key) => this.GetValue<int>(key);
    }
}
