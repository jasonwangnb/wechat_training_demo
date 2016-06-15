using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WeChatTraining.WeChatUtil;
using WeChatTraining.WeChatUtil.Model.WechatApiResponse;

namespace WeChatTraining.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// WeChat entry action
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Index(WeChatRequestModel model)
        {
            string resultContent = string.Empty;
            try
            {
                if (WeChatHelper.ValidateWeChatSignature(model.signature, model.timestamp, model.nonce))
                {
                    if (!string.IsNullOrWhiteSpace(model.echostr))
                    {
                        // For wechat validating server, when put server url and token on Wechat management page
                        resultContent = model.echostr;
                    }
                    else
                    {
                        // Receive events or messages from webchat
                        var weChatPostModel = this.FetchWeChatPostInfo();
                        if (weChatPostModel != null)
                        {
                            weChatPostModel.ClientOpenId = model.openid;
                            // Handle wechat event, eg: subscribe,unsubscribe,click
                            if (weChatPostModel.MsgType.ToLower() == "event" &&
                                !string.IsNullOrWhiteSpace(weChatPostModel.Event))
                            {
                                switch (weChatPostModel.Event.ToLower())
                                {
                                    case "subscribe":
                                        resultContent = this.HandelUserSubscribe(weChatPostModel);
                                        break;
                                    case "click":
                                        resultContent = this.HandelWeChatMenuClick(weChatPostModel);
                                        break;
                                }
                            }

                            // Handle user sent message to wechat
                            else
                            {
                                resultContent = this.HandelUserSendMessage(weChatPostModel);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Log error message
            }

            return Content(resultContent);
        }

        /// <summary>
        /// WeChat JS SDK testing page
        /// </summary>
        /// <returns></returns>
        public ActionResult JSSDK()
        {
            return View(WeChatHelper.PrepareJsAPIConfiguration());
        }

        /// <summary>
        /// Prepare response for user sending message
        /// </summary>
        /// <param name="weChatPostModel"></param>
        /// <returns></returns>
        [NonAction]
        private string HandelUserSendMessage(WeChatPostRequestModel weChatPostModel)
        {
            NewsTypeModel weChatNewsTypeModel = new NewsTypeModel
            {
                FromUserName = weChatPostModel.ToUserName,
                ToUserName = weChatPostModel.FromUserName,
                ListNews = new List<SingleNewsModel> {
                    new SingleNewsModel
                    {
                        Title = "WeChat Training Response",
                        Description = "Some description, redirect to baidu search",
                        PicUrl = "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png",
                        Url = "https://www.baidu.com/"
                    },
                    new SingleNewsModel
                    {
                        Title = "微信培训，自动回复",
                        Description = "描述测试，跳转到搜搜",
                        PicUrl = "http://www.sogou.com/images/logo/new/search400x150.png?v=2",
                        Url = "http://www.sogou.com/?rfrom=soso"
                    }
                },
            };

            return WeChatHelper.TransmitNews(weChatNewsTypeModel);
        }

        /// <summary>
        /// Prepare response for user subscribe event
        /// </summary>
        /// <param name="weChatPostModel"></param>
        /// <returns></returns>
        [NonAction]
        private string HandelUserSubscribe(WeChatPostRequestModel weChatPostModel)
        {
            var model = new TextTypeModel
            {
                ToUserName = weChatPostModel.FromUserName,
                FromUserName = weChatPostModel.ToUserName,
                Content = "Hi, 这是关注微信公众号后的自动回复信息\n/害羞/色"
            };

            return WeChatHelper.TransmitText(model);
        }

        /// <summary>
        /// Prepare response for user menu click event
        /// </summary>
        /// <param name="weChatPostModel"></param>
        /// <returns></returns>
        [NonAction]
        private string HandelWeChatMenuClick(WeChatPostRequestModel weChatPostModel)
        {
            var result = string.Empty;
            //var lastUnderlineIndex = weChatPostModel.EventKey.LastIndexOf('_');
            //var eventKeyPrefix = weChatPostModel.EventKey.ToUpper().Substring(0, lastUnderlineIndex);
            var responsTxtMessageModel = new TextTypeModel
            {
                ToUserName = weChatPostModel.FromUserName,
                FromUserName = weChatPostModel.ToUserName
            };

            switch (weChatPostModel.EventKey)
            {
                case Constant.WeChat_MenuKey_GetMessage:
                    responsTxtMessageModel.Content = "Hi,这是一条通过菜单事件触发的自动回复";
                    break;
                case Constant.WeChat_MenuKey_GetUserInfo:
                    responsTxtMessageModel.Content = WeChatHelper.GetUserInfoByOpenId(weChatPostModel.ClientOpenId);
                    break;
            }
            result = WeChatHelper.TransmitText(responsTxtMessageModel);

            return result;
        }

        /// <summary>
        /// Fetch WeChat posted informtion into model
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private WeChatPostRequestModel FetchWeChatPostInfo()
        {
            WeChatPostRequestModel result = null;
            string postedXMLString = new StreamReader(Request.InputStream, System.Text.Encoding.UTF8).ReadToEnd();
            try
            {
                if (!string.IsNullOrWhiteSpace(postedXMLString))
                {
                    result = new WeChatPostRequestModel();
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(postedXMLString);
                    result.ToUserName = xmlDocument.GetElementsByTagName("ToUserName")[0].InnerText.Trim();
                    result.FromUserName = xmlDocument.GetElementsByTagName("FromUserName")[0].InnerText.Trim();
                    result.MsgType = xmlDocument.GetElementsByTagName("MsgType")[0].InnerText.Trim();

                    var megIdNode = xmlDocument.GetElementsByTagName("MsgId");
                    if (megIdNode != null && megIdNode.Count > 0)
                        result.MsgId = megIdNode[0].InnerText;

                    var eventNode = xmlDocument.GetElementsByTagName("Event");
                    if (eventNode != null && eventNode.Count > 0)
                        result.Event = eventNode[0].InnerText;

                    var eventKeyNode = xmlDocument.GetElementsByTagName("EventKey");
                    if (eventKeyNode != null && eventKeyNode.Count > 0)
                        result.EventKey = eventKeyNode[0].InnerText;

                    var contentNode = xmlDocument.GetElementsByTagName("Content");
                    if (contentNode != null && contentNode.Count > 0)
                        result.Content = contentNode[0].InnerText;
                }
            }
            catch
            {
                // Log error message
            }

            return result;
        }
    }
}