using Spectre.Console;
using System.Net;

namespace FindingAvailableVersions
{
    class Program
    {
        static async Task Main(/*string[] args*/)
        {
            // 基础 URL 和文件路径前缀
            string baseUrl = "基础链接";

            // 使用 HttpClient 来发送请求
            using HttpClient client = new();
            // 请求头
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

            // 当前最新版本号 e.g. 2.6.1
            var latestVersion = new Version(2, 6, 1);

            // 存储所有版本号和状态码
            var versionStatusCodes = new List<(string version, HttpStatusCode statusCode)>();

            // 计算总的版本数
            int totalVersions = (latestVersion.Major + 1) * (latestVersion.Minor + 1) * (latestVersion.Build + 1);
            int currentVersionCount = 0;

            // 使用进度条显示遍历进度
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[green]Checking versions[/]");
                    task.MaxValue = totalVersions;

                    // 遍历版本号从 0.0.0 到最新版本号
                    for (int major = 0; major <= latestVersion.Major; major++)
                    {
                        for (int minor = 0; minor <= latestVersion.Minor; minor++)
                        {
                            for (int patch = 0; patch <= latestVersion.Build; patch++)
                            {
                                var currentVersion = new Version(major, minor, patch);
                                if (currentVersion > latestVersion)
                                {
                                    break;
                                }

                                string version = $"{major}.{minor}.{patch}";
                                string url = $"{baseUrl}{version}.exe";

                                // 获取文件状态码
                                var statusCode = await GetFileStatusCodeAsync(client, url);

                                // 记录版本号和状态码
                                versionStatusCodes.Add((version, statusCode));

                                // 更新进度
                                currentVersionCount++;
                                task.Increment(1);
                                task.Description = $"[green]Checking versions ({currentVersionCount}/{totalVersions})[/]";
                            }
                        }
                    }
                });

            // 使用 Spectre.Console 输出表格
            var table = new Table();
            table.AddColumn("版本号");
            table.AddColumn("状态码");

            foreach (var version in versionStatusCodes)
            {
                var statusCode = version.statusCode;
                var color = statusCode switch
                {
                    HttpStatusCode.OK => "green",
                    _ when (int)statusCode >= 300 && (int)statusCode < 400 => "blue",
                    _ when (int)statusCode >= 400 && (int)statusCode < 500 => "yellow",
                    _ when (int)statusCode >= 500 => "red",
                    _ => "grey"
                };

                var versionColor = statusCode == HttpStatusCode.OK ? "white" : "dim";

                table.AddRow($"[{versionColor}]{version.version}[/]", $"[{color}]{statusCode}[/]");
            }
            
            AnsiConsole.Write(table);
        }

        // 获取指定 URL 的状态码
        static async Task<HttpStatusCode> GetFileStatusCodeAsync(HttpClient client, string url)
        {
            try
            {
                // 发送 HEAD 请求来获取文件状态码
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.StatusCode;
            }
            catch
            {
                try
                {
                    // 如果 HEAD 请求失败，发送 GET 请求来获取文件状态码
                    var response = await client.GetAsync(url);
                    return response.StatusCode;
                }
                catch
                {
                    // 如果出现异常，返回 0 表示未知状态
                    return 0;
                }
            }
        }
    }
}
