using System.Collections.Generic;

namespace ET.Client
{
    [ChildOf(typeof(RedPointComponent))]
    public class RedPointNode: Entity, IAwake<string>, IDestroy
    {
        /// <summary>
        /// UI名
        /// </summary>
        public string Key;
        /// <summary>
        /// 类型和数量
        /// </summary>
        public Dictionary<string,int> TypeRedPointDic;
    }
}