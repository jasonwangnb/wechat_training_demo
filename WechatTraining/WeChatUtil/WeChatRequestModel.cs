namespace WeChatTraining.WeChatUtil
{
    public class WeChatRequestModel
    {
        public string signature { get; set; }
        public string timestamp { get; set; }
        public string nonce { get; set; }
        public string echostr { get; set; }
        public string openid { get; set; }
    }
}
