using Spectre.Console;

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

            // 存储所有可用的版本号和链接
            var availableVersions = new List<(string version, string url)>();

            // 遍历版本号从 0.0.0 到 x.x.x
            for (int major = 0; major <= 2; major++)
            {
                for (int minor = 0; minor <= 6; minor++)
                {
                    for (int patch = 0; patch <= 1; patch++)
                    {
                        string version = $"{major}.{minor}.{patch}";
                        string url = $"{baseUrl}{version}.exe";

                        // 尝试发送 HTTP 请求检查文件是否存在
                        bool fileExists = await CheckFileExistsAsync(client, url);

                        if (fileExists)
                        {
                            // 如果文件存在，记录版本号和 URL
                            availableVersions.Add((version, url));
                        }
                    }
                }
            }

            // 使用 Spectre.Console 输出表格
            var table = new Table();
            table.AddColumn("版本号");
            table.AddColumn("下载链接");

            foreach (var version in availableVersions)
            {
                table.AddRow(version.version, version.url);
            }
            
            AnsiConsole.Write(table);
        }

        // 检查指定的 URL 是否存在
        static async Task<bool> CheckFileExistsAsync(HttpClient client, string url)
        {
            try
            {
                // 发送 HEAD 请求来检查文件是否存在（不下载文件内容）
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                // 如果状态码是 200 OK，则文件存在
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // 如果出现异常，说明文件不存在
                return false;
            }
        }
    }
}
