using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class AnimationComponent : Entity,IAwake<GameObject>
    {
        public Animancer.AnimancerComponent AnimancerCom{ get; set; }
        public AnimationClip IdleAnimation;
        public AnimationClip RunAnimation;
    }
}
