using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        /// <summary>
        /// 得到最顶层的面板
        /// 默认将返回所有面板的第一个
        /// 可能没有
        /// </summary>
        public static PanelInfo GetTopPanel(this YIUIMgrComponent self, EPanelLayer layer = EPanelLayer.Any,
                                            EPanelOption          ignoreOption = EPanelOption.Container)
        {
            const int layerCount = (int)EPanelLayer.Count;

            for (var i = 0; i < layerCount; i++)
            {
                var currentLayer = (EPanelLayer)i;

                //如果是任意层级则 从上到下找
                //否则只会在目标层级上找
                if (layer != EPanelLayer.Any && currentLayer != layer)
                {
                    continue;
                }

                var list = self.GetLayerPanelInfoList(currentLayer);

                foreach (var info in list)
                {
                    //禁止关闭的界面无法获取到
                    if (info.UIPanel.PanelDisClose)
                    {
                        continue;
                    }

                    //有忽略操作 且满足调节 则这个界面无法获取到
                    if (ignoreOption != EPanelOption.None &&
                        (info.UIPanel.PanelOption & ignoreOption) != 0)
                    {
                        continue;
                    }

                    if (layer == EPanelLayer.Any || info.UIPanel.Layer == layer)
                    {
                        return info;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 关闭这个层级上的最前面的一个UI 异步
        /// </summary>
        /// <param name="layer">层级</param>
        /// <param name="ignoreOption">忽略操作</param>
        public static async ETTask<bool> CloseLayerTopPanelAsync(this YIUIMgrComponent self, EPanelLayer layer,
                                                                 EPanelOption          ignoreOption = EPanelOption.Container)
        {
            var topPanel = self.GetTopPanel(layer, ignoreOption);
            if (topPanel == null)
            {
                return false;
            }

            return await self.ClosePanelAsync(topPanel.Name);
        }

        /// <summary>
        /// 关闭指定层级上的 最上层UI 同步
        /// </summary>
        /// <param name="layer">层级</param>
        /// <param name="ignoreOption">忽略操作</param>
        public static void CloseLayerTopPanel(this YIUIMgrComponent self, EPanelLayer layer, EPanelOption ignoreOption = EPanelOption.Container)
        {
            self.CloseLayerTopPanelAsync(layer, ignoreOption).Coroutine();
        }

        /// <summary>
        /// 关闭Panel层级上的最上层UI 异步
        /// </summary>
        public static async ETTask<bool> CloseTopPanelAsync(this YIUIMgrComponent self)
        {
            return await self.CloseLayerTopPanelAsync(EPanelLayer.Panel);
        }

        /// <summary>
        /// 关闭Panel层级上的最上层UI 同步
        /// </summary>
        public static void CloseTopPanel(this YIUIMgrComponent self)
        {
            self.CloseTopPanelAsync().Coroutine();
        }
    }
}