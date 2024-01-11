using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = YIUIFramework.Logger;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YIUIFramework
{
    [RequireComponent(typeof(Animation))]
    [DetailedInfoBox("注意动画组件的 自动播放不要勾选",
        @"根据名称播放动画 不存在则会提示
使用方法: 
1 挂上 Animation 组件
2 Ctrl + 6 动态创建一个动画片段
3 吧对应的动画片段拖到组件中
4 修改了组件后记得保存 触发一次刷新")]
    [LabelText("动画")]
    [AddComponentMenu("YIUIBind/Data/动画 【AnimationByName】 UIDataBindAnimationByName")]
    public sealed class UIDataBindAnimationByName : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("动画")]
        private Animation m_Animation;

        [SerializeField]
        [ReadOnly]
        [LabelText("当前所有可用动画")]
        private List<string> m_AllClipName = new List<string>();

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.String;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Animation ??= GetComponent<Animation>();
            if (m_Animation != null)
            {
                m_Animation.playAutomatically = false;
                #if UNITY_EDITOR
                m_AllClipName.Clear();

                var clips = AnimationUtility.GetAnimationClips(m_Animation.gameObject);

                foreach (var clip in clips)
                {
                    m_AllClipName.Add(clip.name);
                }
                #endif
            }
            else
            {
                #if UNITY_EDITOR
                m_AllClipName.Clear();
                #endif
            }
        }

        protected override void OnValueChanged()
        {
            if (m_Animation == null) return;

            var dataValue = GetFirstValue<string>();
            if (string.IsNullOrEmpty(dataValue)) return;

            if (!m_AllClipName.Contains(dataValue))
            {
                Logger.LogErrorContext(this, $"{name} 播放失败 请检查动画名称是否存在 {dataValue}");
                return;
            }

            m_Animation.Play(dataValue);
        }
    }
}