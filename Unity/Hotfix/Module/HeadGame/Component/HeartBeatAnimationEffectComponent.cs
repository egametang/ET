using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ETHotfix
{
    [ObjectSystem]
    public class HG_HeartBeatMCComponentAwakeSystem : AwakeSystem<HeartBeatAnimationEffectComponent>
    {
        public override void Awake(HeartBeatAnimationEffectComponent self)
        {
            self.Awake();
        }
    }

    /// <summary>
    /// 在图片上添加的心跳组件，其实直接在控制层用DoTween 就好。这里是为了做一个添加自定义组件的演示流程;
    /// </summary>
    public class HeartBeatAnimationEffectComponent : Component
    {
        public void Awake()
        {
            Log.Info("heat component awake");

            GameObject gameObject = this.GetParent<UI>().GameObject;
            gameObject.GetComponent<RawImage>();

            Tweener tweener = gameObject.transform.DOScale(0.3f, 0.7f).SetRelative();
            //设置这个Tween不受Time.scale影响
//        tweener.SetUpdate(true);
//        //设置移动类型
            tweener.SetLoops(-1);
            tweener.SetEase(Ease.InBounce);
            tweener.OnComplete(() => { Debug.Log("end add one "); });
        }
    }
}