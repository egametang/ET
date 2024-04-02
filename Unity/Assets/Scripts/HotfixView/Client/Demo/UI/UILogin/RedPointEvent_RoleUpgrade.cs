using System.Collections.Generic;

namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class RedPointEvent_RoleUpgrade: AEvent<Scene, RoleUpgrade>
    {
        protected override async ETTask Run(Scene root, RoleUpgrade args)
        {
            await UILoginComponentRedPoint(root,args);
            //其它UI的事件
            //await UILoginComponentRedPoint(root,args);
            //await UILoginComponentRedPoint(root,args);
            //await UILoginComponentRedPoint(root,args);
        }

        private async ETTask UILoginComponentRedPoint(Scene root, RoleUpgrade roleUpgrade)
        {
            var redPointComponent = root.GetComponent<RedPointComponent>();
            //UI对应的红点节点
            var node = redPointComponent.AddOrGetNode(UIType.UILogin);
            //设置所需对应类别的红点值
            node.SetValueByType(nameof(roleUpgrade), roleUpgrade.Level);
            //刷新UI红点
            var uiLoginComponent = root.GetComponent<UIComponent>().Get(UIType.UILogin).GetComponent<UILoginComponent>();
            uiLoginComponent.RefreshRedPoint(node);
            
            await ETTask.CompletedTask;
        }
    }
}