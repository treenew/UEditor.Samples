using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Libs.UEditor.Handlers
{
    /// <summary>
    /// UploadHandler 的摘要说明
    /// </summary>
    public class UploadHandler : Handler
    {
        private UEditorOptions Options { get; }
        public UploadConfig UploadConfig { get; private set; }
        public UploadResult Result { get; private set; }

        public UploadHandler(HttpContext context, UEditorOptions options, UploadConfig uploadConfig)
            : base(context)
        {
            this.UploadConfig = uploadConfig;
            this.Result = new UploadResult() { State = UploadState.Unknown };
            this.Options = options;
        }

        public override async Task ProcessAsync()
        {
            string uploadFileName = null;

            var file = this.Request.Form.Files[this.UploadConfig.UploadFieldName];
            uploadFileName = file.FileName;

            if(!this.CheckFileType(uploadFileName))
            {
                this.Result.State = UploadState.TypeNotAllow;
                await this.WriteResultAsync();
                return;
            }
            if(!this.CheckFileSize(file.Length))
            {
                this.Result.State = UploadState.SizeLimitExceed;
                await this.WriteResultAsync();
                return;
            }

            this.Result.OriginFileName = uploadFileName;

            var savePath = this.Options.Formatter.Format(uploadFileName, this.UploadConfig.PathFormat);

            var localPath = this.Context.Web(savePath).PhysicalPath;
            try
            {
                if(!Directory.Exists(Path.GetDirectoryName(localPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                }

                using(var writer = File.Create(localPath))
                {
                    await file.OpenReadStream().CopyToAsync(writer);
                }

                this.Result.Url = savePath;
                this.Result.State = UploadState.Success;
            }
            catch(Exception e)
            {
                this.Result.State = UploadState.FileAccessError;
                this.Result.ErrorMessage = e.Message;
            }
            await this.WriteResultAsync();
        }

        private Task WriteResultAsync()
        {
            return this.WriteJsonAsync(new
            {
                state = this.GetStateMessage(this.Result.State),
                url = this.Result.Url,
                title = this.Result.OriginFileName,
                original = this.Result.OriginFileName,
                error = this.Result.ErrorMessage
            });
        }

        private string GetStateMessage(UploadState state)
        {
            switch(state)
            {
                case UploadState.Success:
                    return "SUCCESS";
                case UploadState.FileAccessError:
                    return "文件访问出错，请检查写入权限";
                case UploadState.SizeLimitExceed:
                    return "文件大小超出服务器限制";
                case UploadState.TypeNotAllow:
                    return "不允许的文件格式";
                case UploadState.NetworkError:
                    return "网络错误";
            }
            return "未知错误";
        }

        private bool CheckFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return this.UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        private bool CheckFileSize(long size)
        {
            return size < this.UploadConfig.SizeLimit;
        }
    }

    public class UploadConfig
    {
        /// <summary>
        /// 文件命名规则
        /// </summary>
        public string PathFormat { get; set; }

        /// <summary>
        /// 上传表单域名称
        /// </summary>
        public string UploadFieldName { get; set; }

        /// <summary>
        /// 上传大小限制
        /// </summary>
        public int SizeLimit { get; set; }

        /// <summary>
        /// 上传允许的文件格式
        /// </summary>
        public string[] AllowExtensions { get; set; }

        ///// <summary>
        ///// 文件是否以 Base64 的形式上传
        ///// </summary>
        //public bool Base64 { get; set; }

        ///// <summary>
        ///// Base64 字符串所表示的文件名
        ///// </summary>
        //public string Base64Filename { get; set; }
    }

    public class UploadResult
    {
        public UploadState State { get; set; }
        public string Url { get; set; }
        public string OriginFileName { get; set; }

        public string ErrorMessage { get; set; }
    }

    public enum UploadState
    {
        Success = 0,
        SizeLimitExceed = -1,
        TypeNotAllow = -2,
        FileAccessError = -3,
        NetworkError = -4,
        Unknown = 1,
    }
}
