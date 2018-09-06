using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Libs.UEditor.Handlers
{    /// <summary>
     /// FileManager 的摘要说明
     /// </summary>
    public class ListFileManager : Handler
    {
        enum ResultState
        {
            Success,
            InvalidParam,
            AuthorizError,
            IOError,
            PathNotFound
        }

        private int Start;
        private int Size;
        private int Total;
        private ResultState State;
        private readonly String PathToList;
        private String[] FileList;
        private String[] SearchExtensions;
        private UEditorOptions Config { get; }

        public ListFileManager(HttpContext context, UEditorOptions config, string pathToList, string[] searchExtensions)
            : base(context)
        {
            this.Config = this.Config;
            this.PathToList = pathToList;
            this.SearchExtensions = searchExtensions.Select(x => x.ToLower()).ToArray();
        }

        public override Task ProcessAsync()
        {
            try
            {
                this.Start = String.IsNullOrEmpty(this.Request.Query["start"]) ? 0 : Convert.ToInt32(this.Request.Query["start"]);
                this.Size = String.IsNullOrEmpty(this.Request.Query["size"]) ? this.Config.GetInt("imageManagerListSize") : Convert.ToInt32(this.Request.Query["size"]);
            }
            catch(FormatException)
            {
                this.State = ResultState.InvalidParam;
                return this.WriteResultAsync();
            }
            var buildingList = new List<String>();
            try
            {
                var folder = this.Context.Web(this.PathToList);
                if(folder.Exists)
                {
                    var localPath = folder.PhysicalPath;
                    buildingList.AddRange(Directory.GetFiles(localPath, "*", SearchOption.AllDirectories)
                        .Where(x => this.SearchExtensions.Contains(Path.GetExtension(x).ToLower()))
                        .Select(x => this.PathToList + x.Substring(localPath.Length).Replace("\\", "/")));
                    this.Total = buildingList.Count;
                    this.FileList = buildingList.OrderBy(x => x).Skip(this.Start).Take(this.Size).ToArray();
                }
                else
                {
                    this.Total = 0;
                    this.FileList = Array.Empty<string>();
                }
            }
            catch(UnauthorizedAccessException)
            {
                this.State = ResultState.AuthorizError;
            }
            catch(DirectoryNotFoundException)
            {
                this.State = ResultState.PathNotFound;
            }
            catch(IOException)
            {
                this.State = ResultState.IOError;
            }

            return this.WriteResultAsync();
        }

        private Task WriteResultAsync()
        {
            return this.WriteJsonAsync(new
            {
                state = this.GetStateString(),
                list = this.FileList?.Select(x => new { url = x }),
                start = this.Start,
                size = this.Size,
                total = this.Total
            });
        }

        private string GetStateString()
        {
            switch(this.State)
            {
                case ResultState.Success:
                    return "SUCCESS";
                case ResultState.InvalidParam:
                    return "参数不正确";
                case ResultState.PathNotFound:
                    return "路径不存在";
                case ResultState.AuthorizError:
                    return "文件系统权限不足";
                case ResultState.IOError:
                    return "文件系统读取错误";
            }
            return "未知错误";
        }
    }
}
