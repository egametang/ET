namespace ET.Client
{
    /// <summary>
    /// GM命令参数信息
    /// </summary>
    public class GMParamInfo
    {
        public EGMParamType ParamType;   //类型
        public string       Desc; //描述
        public string       Value = "";       //参数值

        public GMParamInfo(EGMParamType paramType,string desc)
        {
            ParamType = paramType;
            Desc = desc;
        }
    }
}