using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [RequireComponent(typeof(Animation))]
    [DetailedInfoBox("注意动画组件的 自动播放不要勾选",
        @"会根据配置播放当前配置上默认的Animation  如果要实现播放不同的动画还需要搭配另外一个传入名称的参数
使用方法: 
1 挂上 Animation 组件
2 Ctrl + 6 动态创建一个动画片段
3 吧对应的动画片段拖到组件中
4 默认会使用第一个动画")]
    [LabelText("动画")]
    [AddComponentMenu("YIUIBind/Data/动画 【Animation】 UIDataBindAnimation")]
    public sealed class UIDataBindAnimation : UIDataBindBool
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("动画")]
        private Animation m_Animation;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Animation ??= GetComponent<Animation>();
            if (m_Animation != null)
            {
                m_Animation.playAutomatically = false;
            }
        }

        protected override void OnValueChanged()
        {
            if (m_Animation == null) return;

            if (m_Animation.clip == null)
            {
                return;
            }

            var result = GetResult();
            if (result)
            {
                m_Animation.Play(m_Animation.clip.name);
            }
            else
            {
                m_Animation.Stop(m_Animation.clip.name);
            }
        }
    }
}