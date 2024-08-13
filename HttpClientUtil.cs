using System.Diagnostics;
using System.Net;
using System.Text;
using Masuit.Tools.Security;
using MxbcRobOrderWinFormsApp.Dto;
using Newtonsoft.Json;

namespace MxbcRobOrderWinFormsApp;

public static class HttpClientUtil
{
    public static string Post(string url, string jsonData, Dictionary<string, string>? headers, IWebProxy? proxy)
    {
        var handler = new HttpClientHandler();
        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
        };
        // 添加自定义头部
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        try
        {
            var response = client.Send(request);
            var httpStatusCode = response.StatusCode;
            Console.WriteLine($@"Response StatusCode: {httpStatusCode}");
            if (httpStatusCode == HttpStatusCode.MethodNotAllowed)
            {
                return "IP风控";
            }
            var result = response.Content.ReadAsStringAsync().Result;
            return string.IsNullOrEmpty(result) ? result : "接口异常，请稍后再试";
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($@"Error: {e.Message}");
            return $"接口异常:{e.Message}，请稍后再试";
        }
    }

    public static string? Get(string url, Dictionary<string, string>? headers, IWebProxy? proxy)
    {
        var handler = new HttpClientHandler();
        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        // 添加自定义头部
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        try
        {
            var response = client.Send(request);
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($@"Error: {e.Message}");
            return null;
        }
    }

    public static void CheckVersionInfo()
    {
        //1、get 访问 https://share.weiyun.com/iUhIgsUX
        //2、获取内容 "html_content": "<p>【mxbc&nbsp;V2.0】</p><p>【{ewogICAgInZlcnNpb25zIjoyLjAsCiAgICAiYXZhaWxhYmxlIjp0cnVlLAogICAgInVybCI6Imh0dHBzOi8vbGludXguZG8vdS95dWFvdGlhbi8iCiAgICAibXNnIjoi5L2g54ix5Zad5aW26Iy25ZCX77yfIgp9}】</p><p><br  /></p>",
        var headers = new Dictionary<string, string>
        {
            { "Host", " share.weiyun.com" },
            { "Connection", " keep-alive" },
            { "Cache-Control", " max-age=0" },
            { "Upgrade-Insecure-Requests", " 1" },
            {
                "User-Agent",
                " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36"
            },
            {
                "Accept",
                " text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
            },
            { "Sec-Fetch-Site", " none" },
            { "Sec-Fetch-Mode", " navigate" },
            { "Sec-Fetch-User", " ?1" },
            { "Sec-Fetch-Dest", " document" },
            { "Accept-Language", " en,zh-CN;q=0.9,zh;q=0.8" },
        };
        var tmpStr = Get("https://share.weiyun.com/iUhIgsUX", headers, null);
        //如果为空代表连接失败，退出软件
        if (string.IsNullOrEmpty(tmpStr))
        {
            Console.WriteLine("网络连接失败, 请检查网络连接!!!");
            MessageBox.Show(@"网络连接失败, 请检查网络连接!!!");
            return;
        }

        var textBetween = GetTextBetween(tmpStr, "p>【$", "$】\\u003C/p>");
        if (string.IsNullOrEmpty(textBetween))
        {
            Console.WriteLine("获取版本信息失败, 请检查网络连接!!!");
            MessageBox.Show(@"获取版本信息失败, 请检查网络连接!!!");
            return;
        }

        Console.WriteLine($@"当前版本信息：{textBetween}");
        var jsonStr = textBetween.Base64Decrypt();
        Console.WriteLine($@"解密后的版本信息：{jsonStr}");
        var versionInfoDto = JsonConvert.DeserializeObject<VersionInfoDto>(jsonStr);
        if (versionInfoDto == null)
        {
            Console.WriteLine("获取版本信息失败, 请检查网络连接!!!");
            MessageBox.Show(@"获取版本信息失败, 请检查网络连接!!!");
            OpenUrl("https://linux.do/u/yuaotian/");
            return;
        }

        //软件不可用
        if (versionInfoDto.available == false)
        {
            //软件不可用over
            MessageBox.Show(versionInfoDto.msg, @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OpenUrl("https://linux.do/u/yuaotian/");
            return;
        }

        //内部版本
        var internalVersion = ProgramMain.Version;
        //外部版本
        var externalVersion = versionInfoDto.versions;
        //版本不一致时弹出提示
        if (internalVersion == externalVersion) return;
        MessageBox.Show(versionInfoDto.msg, @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //url 不为空时，跳转
        if (string.IsNullOrEmpty(versionInfoDto.url))
        {
            OpenUrl("https://linux.do/u/yuaotian/");
            return;
        }
        OpenUrl(versionInfoDto.url);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            // 使用 ProcessStartInfo 类并设置 UseShellExecute 为 true
            var startInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true // 必须设置为 true，以允许启动默认浏览器
            };
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("无法打开网址: " + ex.Message);
        }
    }

    private static string GetTextBetween(string source, string start, string end)
    {
        var startIndex = source.IndexOf(start) + start.Length;
        if (startIndex < start.Length)
        {
            return ""; // 起始标识符未找到
        }

        var endIndex = source.IndexOf(end, startIndex);
        return endIndex < 0
            ? ""
            : // 结束标识符未找到
            source.Substring(startIndex, endIndex - startIndex);
    }
}