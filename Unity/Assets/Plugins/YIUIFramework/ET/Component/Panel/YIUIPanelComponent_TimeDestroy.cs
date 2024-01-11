using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 倒计时摧毁
    /// </summary>
    public partial class YIUIPanelComponent
    {
        public float CachePanelTime = 10;

        private ETCancellationToken m_Token;

        internal void CacheTimeCountDownDestroyPanel()
        {
            StopCountDownDestroyPanel();
            m_Token = new ETCancellationToken();
            DoCountDownDestroyPanel(m_Token).Coroutine();
        }

        internal void StopCountDownDestroyPanel()
        {
            if (m_Token == null) return;

            m_Token.Cancel();
            m_Token = null;
        }

        private async ETTask DoCountDownDestroyPanel(ETCancellationToken token)
        {
            try
            {
                await this.Fiber().Root.GetComponent<TimerComponent>().WaitAsync((long)(CachePanelTime * 1000), token);
                if (token.IsCancel()) //取消倒计时
                {
                    return;
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogError(e);
                return;
            }

            YIUIMgrComponent.Inst.RemoveUIReset(this.UIBase.UIName);
        }
    }
}