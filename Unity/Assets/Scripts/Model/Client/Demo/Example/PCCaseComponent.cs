namespace ET.Client
{
    //该组件类属于指定的实体类，添加typeof指定，任意父实体就不需要。
    [ComponentOf(typeof(Computer))]
    public class PCCaseComponent : Entity,IAwake
    {
    
    }
}