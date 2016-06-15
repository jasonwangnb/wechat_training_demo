using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatTraining.WeChatUtil
{
    public class WeChatPostRequestModel
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string CreateTime { get; set; }
        public string MsgType { get; set; }
        public string Content { get; set; }
        public string MsgId { get; set; }
        public string Event { get; set; }
        public string EventKey { get; set; }
        public int StoreId { get; set; }
        public string ClientOpenId { get; set; }
    }
}
