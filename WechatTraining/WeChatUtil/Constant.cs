using System.Configuration;

namespace WeChatTraining.WeChatUtil
{
    public class Constant
    {
        public const int DefaultCacheTime = 60 * 60;
        public const int WeChatTokenCacheTime = 7000;
        
        public const string CacheKeyWeChatToken = "Training_WeChatToken";
        public const string CacheKeyWeChatJsApiTicket = "Training_WeChatJsApiTicket";
        
        public readonly static string WeChatApiGetUserByOpenIdRequestUrl = ConfigurationManager.AppSettings["WeChatApiGetUserByOpenIdRequestUrl"];
        public readonly static string WeChatApiGetUsersRequestUrl = ConfigurationManager.AppSettings["WeChatApiGetUsersRequestUrl"];
        public readonly static string WeChatAuthorizeGetCodeUrl = ConfigurationManager.AppSettings["WeChatAuthorizeGetCodeUrl"];
        public readonly static string WeChatAuthorizeGetTokenUrl = ConfigurationManager.AppSettings["WeChatAuthorizeGetTokenUrl"];
        public readonly static string WeChatSignatureToken = ConfigurationManager.AppSettings["WeChatSignatureToken"];
        public readonly static string WeChatApiTokenRequestUrl = ConfigurationManager.AppSettings["WeChatApiTokenRequestUrl"];
        public readonly static string WeChatJsApiTicketRequestUrl = ConfigurationManager.AppSettings["WeChatJsApiTicketRequestUrl"];
        public readonly static string WeChatAppId = ConfigurationManager.AppSettings["WeChatAppId"];
        public readonly static string WeChatAppSecret = ConfigurationManager.AppSettings["WeChatAppSecret"];
        
        public const string WeChat_MenuKey_GetMessage = "M001_GET_MESSAGE";
        public const string WeChat_MenuKey_GetUserInfo = "M002_GET_USER";
    }
}
