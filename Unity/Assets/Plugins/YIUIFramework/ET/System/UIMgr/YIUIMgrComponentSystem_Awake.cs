using I2.Loc;
using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        public static void InitAllBind(this YIUIMgrComponent self)
        {
            self.BindInit = YIUIBindHelper.InitAllBind();
        }

        //添加component的地方 调用 方便异步等待
        //因为UI初始化需要动态加载UI根节点
        public static async ETTask Initialize(this YIUIMgrComponent self)
        {
            //初始化其他UI框架中的管理器
            self.AddComponent<CountDownMgr>();
            await MgrCenter.Inst.Register(I2LocalizeMgr.Inst);
            await MgrCenter.Inst.Register(RedDotMgr.Inst);

            //初始化UIRoot
            await self.InitRoot();
            self.InitSafeArea();
        }
    }
}