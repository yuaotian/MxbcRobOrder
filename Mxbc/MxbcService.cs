using System.Net;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Security;
using Microsoft.ClearScript.V8;
using MxbcRobOrderWinFormsApp.Dto;
using Newtonsoft.Json;

namespace MxbcRobOrderWinFormsApp
{
    public abstract class MxbcService
    {
        private static readonly V8ScriptEngine V8Js = new V8ScriptEngine();
        private static readonly string MarketingId = "1816854086004391938";

        private const string BaseUrl = "https://mxsa.mxbc.net";

        protected MxbcService()
        {
            //加载JS算法
            V8Js.Execute(Resources.Type1286);
        }


        public static SecretWordInfoDto? GetSecretWordInfo(string? proxyHostPort)
        {
            var stamp = DateTime.Now.GetTotalMilliseconds();
            var secretWordSign = GetSecretWordSign(stamp);
            var url =
                $"{BaseUrl}/api/v1/h5/marketing/secretword/info?marketingId={MarketingId}&sign={secretWordSign}&s=2&stamp={stamp}";
            var headers = new Dictionary<string, string>
            {
                { "Accept", " application/json, text/plain, */*" },
                { "Connection", " keep-alive" },
                { "Host", " mxsa.mxbc.net" },
                { "Origin: https", "//mxsa-h5.mxbc.net" },
                { "Referer: https", "//mxsa-h5.mxbc.net/" },
                {
                    "User-Agent",
                    " Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Mobile Safari/537.36 Edg/127.0.0.0"
                },
            };
            var response = HttpClientUtil.Get(url, headers,
                proxyHostPort != null ? new WebProxy(proxyHostPort, true) : null);
            return response != null ? JsonConvert.DeserializeObject<SecretWordInfoDto>(response) : null;
        }

        /// <summary>
        /// 开始抢单
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secretWord"></param>
        /// <param name="round"></param>
        /// <param name="proxyHostPort"></param>
        /// <returns></returns>
        public static string mxbcRobOrder(string token, string secretWord, string round, string? proxyHostPort)
        {
            var stamp = DateTime.Now.GetTotalMilliseconds();
            var sign = GetOrderSign(round, secretWord, stamp);
            Console.WriteLine($"sign: {sign}");
            //判断是否为null
            if (string.IsNullOrEmpty(sign))
            {
                return "Sign 生成出错。";
            }

            //{"marketingId":"1816854086004391938","round":"20:00","secretword":"热情点燃 蜜雪免单","sign":"222015f54b132a7ea10c9bfccf1b5ac2","s":2,"stamp":1722718413562}
            // 使用SortedDictionary来保证顺序
            var paramsJson = new Dictionary<string, object>
            {
                { "marketingId", MarketingId },
                { "round", round },
                { "secretword", secretWord },
                { "sign", sign },
                { "s", 2 },
                { "stamp", stamp }
            };
            var postJson = JsonConvert.SerializeObject(paramsJson);
            Console.WriteLine(postJson);

            var type1286 = GetType1286(postJson);
            Console.WriteLine(type1286);
            if (string.IsNullOrEmpty(type1286))
            {
                Console.WriteLine(@"生成出错。");
                return "Type1286,生成出错。";
            }

            var result = PostDataAsync(token, type1286, postJson, proxyHostPort);
            //判断是不是json数据
            if (result.Contains("安全威胁") || result.Contains("IP风控"))
            {
                return "IP风控";
            }

            if (result.Contains("已抢完"))
            {
                return "本轮【茉莉奶绿免单券】已抢完";
            }

            if (result.Contains("eaf382d757164d9295b0a136c9603188.gif"))
            {
                return "可能大概也许抢到了，快去看看!!!";
            }

            Console.WriteLine(result);
            return result;
        }

        private static string PostDataAsync(string accessToken, string type1286Param, string? postData,
            string? proxyHostPort)
        {
            var url = $"{BaseUrl}/api/v1/h5/marketing/secretword/confirm?type__1286={type1286Param}";
            var headers = new Dictionary<string, string>
            {
                { "Host", "mxsa.mxbc.net" },
                { "Content-Type", "application/json;charset=utf-8" },
                { "Origin", "https://mxsa-h5.mxbc.net" },
                { "Access-Token", "" + accessToken },
                { "Connection", " keep-alive" },
                { "Accept", "application/json, text/plain, */*" },
                { "Accept-Language", "zh-cn" },
                {
                    "User-Agent",
                    "Mozilla/5.0 (iPhone; CPU iPhone OS 16_1_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 MicroMessenger/8.0.47(0x18002f2c) NetType/WIFI Language/zh_CN miniProgram/wx7696c66d2245d107"
                },
                { "Referer", "https://mxsa-h5.mxbc.net/" }
            };
            Console.WriteLine(
                $"url: {url} \r\n headers: {JsonConvert.SerializeObject(headers)} \r\n postData: {postData}");

            var response = HttpClientUtil.Post(url, postData, headers,
                proxyHostPort != null ? new WebProxy(proxyHostPort, true) : null);
            return response;
        }

        /// <summary>
        ///   生成签名
        /// </summary>
        /// <param name="marketingId">活动Id</param>
        /// <param name="round">整点时间：13:00</param>
        /// <param name="secretWord">口令,如：热情点燃 蜜雪免单</param>
        /// <param name="stamp">时间戳,DateTime.Now.GetTotalMilliseconds().ToString()</param>
        /// <returns></returns>
        private static string? GetOrderSign(string round, string secretWord, long stamp)
        {
            //marketingId=1816854086004391938&round=20:00&s=2&secretword=热情点燃 蜜雪免单&stamp=13时间戳c274bac6493544b89d9c4f9d8d542b84
            var str =
                $"marketingId={MarketingId}&round={round}&s=2&secretword={secretWord}&stamp={stamp}c274bac6493544b89d9c4f9d8d542b84";
            Console.WriteLine($@"sign: {str}");
            return str.MDString();
        }


        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="stamp"></param>
        /// <returns></returns>
        private static string? GetSecretWordSign(long stamp)
        {
            //marketingId=1816854086004391938&round=20:00&s=2&secretword=热情点燃 蜜雪免单&stamp=13时间戳c274bac6493544b89d9c4f9d8d542b84
            var str = $"marketingId={MarketingId}&s=2&stamp={stamp}c274bac6493544b89d9c4f9d8d542b84";
            Console.WriteLine($@"sign: {str}");
            return str.MDString();
        }

        private static string? GetType1286(string postJson)
        {
            //加载JS算法
            V8Js.Execute(Resources.Type1286);
            return V8Js.Script.type__1286(postJson);
        }
    }
}