using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class AnimationComponentAwakeSystem: AwakeSystem<AnimationComponent, GameObject>
    {
        protected override void Awake(AnimationComponent self, GameObject go)
        {
            self.AnimancerCom = go.GetComponent<Animancer.AnimancerComponent>();
            self.AnimancerCom.Animator.fireEvents = false;
           

            self.AnimancerCom.States.CreateIfNew(self.IdleAnimation);
            self.AnimancerCom.States.CreateIfNew(self.RunAnimation);
        }
    }

    [FriendOf(typeof (AnimationComponent))]
    public static class AnimationComponentSystem
    {
        public static AnimationClip GetClip(this AnimationComponent self, string clipName)
        {
            var clips = self.AnimancerCom.Animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clipName == clips[i].name)
                {
                    self.AnimancerCom.States.CreateIfNew(clips[i]);
                    return clips[i];
                }
            }
            return null;
        }
        
        
        public static void Play(this AnimationComponent self, string clipName)
        {
            AnimationClip clip = self.GetClip(clipName);
            if (clip == null)
            {
                Debug.Log($"No clip:{clipName}");
                return;
            }
            self.Play(clip);
        }
        public static void PlayFade(this AnimationComponent self, string clipName)
        {
            AnimationClip clip = self.GetClip(clipName);
            if (clip == null)
            {
                Debug.Log($"No clip:{clipName}");
                return;
            }
            self.PlayFade(clip);
        }
        public static void TryPlayFade(this AnimationComponent self, string clipName)
        {
            AnimationClip clip = self.GetClip(clipName);
            if (clip == null)
            {
                Debug.Log($"No clip:{clipName}");
                return;
            }
            self.TryPlayFade(clip);
        }
        public static void Play(this AnimationComponent self, AnimationClip clip)
        {
            var state = self.AnimancerCom.States.GetOrCreate(clip);
            state.Speed = 1;
            self.AnimancerCom.Play(state);
        }

        public static void PlayFade(this AnimationComponent self, AnimationClip clip)
        {
            var state = self.AnimancerCom.States.GetOrCreate(clip);
            state.Speed = 1;
            self.AnimancerCom.Play(state, 0.25f);
        }

        public static void TryPlayFade(this AnimationComponent self, AnimationClip clip)
        {
            var state = self.AnimancerCom.States.GetOrCreate(clip);
            state.Speed = 1;
            if (self.AnimancerCom.IsPlaying(clip))
            {
                return;
            }

            self.AnimancerCom.Play(state, 0.25f);
        }
    }
}