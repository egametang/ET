using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class UIPathComponent : Entity,IAwake,IDestroy
    {
        public static UIPathComponent Instance { get; set; }

        public  Dictionary<int, string> WindowPrefabPath = new Dictionary<int, string>();
        
        public  Dictionary<string,int> WindowTypeIdDict = new Dictionary<string, int>();
    }
}