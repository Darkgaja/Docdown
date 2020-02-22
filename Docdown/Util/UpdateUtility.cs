using Docdown.Net;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public static class UpdateUtility
    {
        public static async Task<Version> CheckNewVersion()
        {
            var version = await GetNewestVersion();
            var cur = typeof(UpdateUtility).Assembly.GetName().Version;
            return version > cur ? version : null;
        }

        public static async Task Update(IProgress<WebDownloadProgress> progress)
        {
            var bytes = await DownloadNewestVersion(progress);
            var tempFile = Path.GetTempFileName() + ".exe";
            var curFile = Assembly.GetExecutingAssembly().Location;
            File.WriteAllBytes(tempFile, bytes);

            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("timeout 2");
            sb.Append("copy /b/y \"").Append(tempFile).Append("\" \"").Append(curFile).AppendLine("\"");
            sb.Append("start \"\" \"").Append(curFile).AppendLine("\"");
            sb.AppendLine("exit");
            
            var tempBat = Path.GetTempFileName() + ".bat";
            File.WriteAllText(tempBat, sb.ToString());

            ProgramUtility.ExecuteNonWaiting(tempBat);
            Environment.Exit(0);
        }

        public static async Task<Version> GetNewestVersion()
        {
            const string Repo = "https://api.github.com/repos/Darkgaja/Docdown/releases/latest";

            try
            {
                var json = await WebUtility.SimpleJsonRequest(Repo);
                var text = json.SelectToken("tag_name").Value<string>();
                return new Version(text);
            }
            catch
            {
                return Version.Parse("0.0.0.0");
            }
        }

        public static async Task<byte[]> DownloadNewestVersion(IProgress<WebDownloadProgress> progress)
        {
            const string Repo = "https://api.github.com/repos/science-docs/Docdown/releases/latest";

            var json = await WebUtility.SimpleJsonRequest(Repo);
            var url = json.SelectToken("assets").First().SelectToken("browser_download_url").Value<string>();
            var content = await WebUtility.DownloadAsync(url, progress);
            return content;
        }
    }
}
