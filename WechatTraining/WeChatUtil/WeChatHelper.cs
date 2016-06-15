using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WeChatTraining.WeChatUtil.Model;
using WeChatTraining.WeChatUtil.Model.WechatApiResponse;

namespace WeChatTraining.WeChatUtil
{
    /// <summary>
    /// 
    /// </summary>
    public class WeChatHelper
    {
        private const string Response_Text_Template = @"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{3}]]></Content>
                                </xml>";
        private const string Response_News_Template = @"<xml>
                                <ToUserName><![CDATA[{0}]]></ToUserName>
                                <FromUserName><![CDATA[{1}]]></FromUserName>
                                <CreateTime>{2}</CreateTime>
                                <MsgType><![CDATA[news]]></MsgType>
                                <ArticleCount>{3}</ArticleCount>
                                <Articles>{4}</Articles>
                                </xml>";
        private const string Response_Single_News_Template = @"<item>
                                <Title><![CDATA[{0}]]></Title> 
                                <Description><![CDATA[{1}]]></Description>
                                <PicUrl><![CDATA[{2}]]></PicUrl>
                                <Url><![CDATA[{3}]]></Url>
                                </item>";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="weChatToken"></param>
        /// <returns></returns>
        public static bool ValidateWeChatSignature(string signature, string timestamp, string nonce)
        {
            if (string.IsNullOrWhiteSpace(Constant.WeChatSignatureToken))
            {
                return true;
            }
            else
            {
                string[] arr = { Constant.WeChatSignatureToken, timestamp, nonce };
                Array.Sort(arr);
                var calculatedSignature = WeChatHelper.GetSHA1String(string.Join("", arr)).ToLower();

                return calculatedSignature.Equals(signature);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string TransmitText(TextTypeModel model)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.ToUserName) && !string.IsNullOrWhiteSpace(model.FromUserName))
            {
                result = string.Format(Response_Text_Template,
                    model.ToUserName, model.FromUserName,
                    model.CreateTime, model.Content);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string TransmitNews(NewsTypeModel model)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.ToUserName) && !string.IsNullOrWhiteSpace(model.FromUserName))
            {
                var strAllNews = string.Empty;
                var intNewsCount = 0;
                if (model.ListNews != null && model.ListNews.Count > 0)
                {
                    intNewsCount = model.ListNews.Count;
                    foreach (var item in model.ListNews)
                    {
                        strAllNews += string.Format(Response_Single_News_Template,
                            item.Title, item.Description, item.PicUrl, item.Url);
                    }
                }

                result = string.Format(Response_News_Template,
                    model.ToUserName, model.FromUserName,
                    model.CreateTime, intNewsCount, strAllNews);
            }

            return result;
        }

        public static string GetUserInfoByOpenId(string openId)
        {
            var accessToken = GetWeChatAccessToken();
            if (string.IsNullOrWhiteSpace(openId) || string.IsNullOrWhiteSpace(accessToken))
            {
                return string.Empty;
            }
            else
            {
                return WeChatHelper.HttpRequest<string>(string.Format(Constant.WeChatApiGetUserByOpenIdRequestUrl, accessToken, openId), "GET");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static WeChatJsAPIModel PrepareJsAPIConfiguration()
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            CacheManager cacheManager = new CacheManager();
            var weChatJsApiTicket = cacheManager.Get<string>(Constant.CacheKeyWeChatJsApiTicket,
                () =>
                {
                    string weChatTicket = string.Empty;
                    var accessToken = GetWeChatAccessToken();

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        var resultTicket = WeChatHelper.HttpRequest<WeChatTicketResponseModel>(
                            string.Format(Constant.WeChatJsApiTicketRequestUrl, accessToken), "GET");
                        if (resultTicket != null && !string.IsNullOrWhiteSpace(resultTicket.ticket))
                            weChatTicket = resultTicket.ticket;
                    }

                    return weChatTicket;
                });

            var resultWeChatConfig = new WeChatJsAPIModel
            {
                AppId = Constant.WeChatAppId,
                Noncestr = Constant.WeChatSignatureToken,
                Url = HttpContext.Current.Request.Url.AbsoluteUri,
                Timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(),
                JsApiTicket = weChatJsApiTicket
            };
            var targetString = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}",
                resultWeChatConfig.JsApiTicket, resultWeChatConfig.Noncestr, resultWeChatConfig.Timestamp, resultWeChatConfig.Url);
            resultWeChatConfig.Signature = WeChatHelper.GetSHA1String(targetString).ToLower();

            return resultWeChatConfig;
        }

        /// <summary>
        /// Generate SHA1 encoded string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetSHA1String(string str)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            byte[] dataToHash = asciiEncoding.GetBytes(str);
            byte[] dataHashed = sha.ComputeHash(dataToHash);
            return BitConverter.ToString(dataHashed).Replace("-", "");
        }

        /// <summary>
        /// Execute HTTP Request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetPath"></param>
        /// <param name="methodType"></param>
        /// <param name="httpParams"></param>
        /// <returns>Convert returned json to provided type</returns>
        public static T HttpRequest<T>(string targetPath, string httpMethod, string httpParams = "") where T : class
        {
            T result = null;

            try
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                HttpWebRequest httpRequest = WebRequest.Create(targetPath) as HttpWebRequest;
                HttpWebResponse httpWebResponse = null;

                httpRequest.Method = httpMethod;
                if (!string.IsNullOrWhiteSpace(httpParams) && httpMethod == "POST")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(httpParams);
                    httpRequest.ContentType = "application/x-www-form-urlencoded";
                    httpRequest.ContentLength = bytes.Length;
                    Stream reqstream = httpRequest.GetRequestStream();
                    reqstream.Write(bytes, 0, bytes.Length);
                    reqstream.Close();
                }

                httpWebResponse = (HttpWebResponse)httpRequest.GetResponse();
                if (httpWebResponse == null)
                    return null;

                String responseBody = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (typeof(T).Equals(typeof(string)))
                        result = (T)Convert.ChangeType(responseBody, typeof(T));
                    else
                        result = JsonConvert.DeserializeObject<T>(responseBody, jsonSerializerSettings);
                }
            }
            catch
            {
                // Log error message
            }

            return result;
        }

        /// <summary>
        /// Fetch WeChat access token
        /// </summary>
        /// <returns></returns>
        private static string GetWeChatAccessToken()
        {
            CacheManager cacheManager = new CacheManager();
            WeChatTokenResponseModel weChatToken = cacheManager.Get<WeChatTokenResponseModel>(Constant.CacheKeyWeChatToken, Constant.WeChatTokenCacheTime, () =>
            {
                return WeChatHelper.HttpRequest<WeChatTokenResponseModel>(
                    string.Format(Constant.WeChatApiTokenRequestUrl, Constant.WeChatAppId, Constant.WeChatAppSecret), "GET");

            });

            if (weChatToken != null && !string.IsNullOrWhiteSpace(weChatToken.access_token))
            {
                return weChatToken.access_token;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
