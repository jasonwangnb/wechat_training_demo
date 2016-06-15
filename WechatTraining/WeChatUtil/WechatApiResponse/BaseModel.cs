using System;

namespace WeChatTraining.WeChatUtil.Model.WechatApiResponse
{
    public class BaseModel
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public long CreateTime
        {
            get
            {
                return DateTime.Now.Ticks;
            }
        }
    }
}
