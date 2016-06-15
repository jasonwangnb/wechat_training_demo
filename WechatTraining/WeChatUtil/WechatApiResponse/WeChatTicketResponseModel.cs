namespace WeChatTraining.WeChatUtil.Model.WechatApiResponse
{
    public class WeChatTicketResponseModel
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }
        public string ticket { get; set; }
        public int expires_in { get; set; }
    }
}
