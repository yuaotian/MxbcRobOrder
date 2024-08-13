namespace MxbcRobOrderWinFormsApp.Dto;



public class SecretWordInfoDto
{
    /// <summary>
    /// 
    /// </summary>
    public int code { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string msg { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Data data { get; set; }
    
}






public class RoundListItem
{
    /// <summary>
    /// 
    /// </summary>
    public string startTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int status { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string leftSecond { get; set; }
    /// <summary>
    /// 8月4日
    /// </summary>
    public string date { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime localDateTime { get; set; }
}
 
public class Data
{
    /// <summary>
    /// 
    /// </summary>
    public string marketingId { get; set; }
    /// <summary>
    /// 年度重磅 新品免单
    /// </summary>
    public string marketingName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string marketingImageUrl { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string shareImageUrl { get; set; }
    /// <summary>
    /// 客服在线时间：上午9:30-11:50，下午13:30-17:50）。
    /// </summary>
    public string ruleDesc { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List <RoundListItem > roundList { get; set; }
    /// <summary>
    /// 本场口令：茉莉奶绿 白月光
    /// </summary>
    public string hintWord { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string bottomImg { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string bottomAppUrl { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string bottomMiniUrl { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime serverTime { get; set; }
    /// <summary>
    /// 170万张🧧新品免单券🧧来啦!
    /// </summary>
    public string shareText { get; set; }
}
 

