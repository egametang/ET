using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(RedPointComponent))]
    [FriendOf(typeof(RedPointComponent))]
    [FriendOf(typeof(RedPointNode))]
    public static partial class RedPointComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RedPointComponent self )
        {
            self.RedPointDic = new Dictionary<string, long>();
        }

        /// <summary>
        /// 添加或获取红点节点
        /// </summary>
        public static RedPointNode AddOrGetNode(this RedPointComponent self,string uiName)
        {
            RedPointNode node = null;
            //判断有无UI界面的节点
            if (!self.RedPointDic.TryGetValue(uiName, out var id))
            {
                node = self.AddChild<RedPointNode,string>(uiName,true);
                self.RedPointDic[uiName] = node.Id;
            }
            else
            {
                node = self.GetChild<RedPointNode>(id);
            }
            return node;
        }
        
        /// <summary>
        /// 移除指定红点
        /// </summary>
        public static void RemoveNode(this RedPointComponent self,string uiName)
        {
            if (!self.RedPointDic.TryGetValue(uiName, out var id))
            {
                Log.Error(uiName + "红点不存在！");
                return;
            }
            self.RemoveChild(id);
        }

        [EntitySystem]
        private static void Destroy(this RedPointComponent self)
        {
            
        }
    }
}