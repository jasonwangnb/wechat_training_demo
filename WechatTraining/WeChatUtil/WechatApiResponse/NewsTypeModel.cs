using System.Collections.Generic;

namespace WeChatTraining.WeChatUtil.Model.WechatApiResponse
{
    public class NewsTypeModel : BaseModel
    {
        public List<SingleNewsModel> ListNews { get; set; }
    }
}
