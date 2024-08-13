namespace MxbcRobOrderWinFormsApp.Dto;

public class VersionInfoDto
{
    // {
    //     "versions":2.0,
    //     "available":true,
    //     "url":"https://linux.do/u/yuaotian/"
    //     "msg":"你爱喝奶茶吗？"
    // }

    public double versions { get; set; }
    public bool available { get; set; }
    public string url { get; set; }
    public string msg { get; set; }
}