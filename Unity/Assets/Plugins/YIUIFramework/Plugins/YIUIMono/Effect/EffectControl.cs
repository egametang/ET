using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 效果控制
    /// </summary>
    [AddComponentMenu("YIUIBind/Effect/Effect Control 特效控制器")]
    public sealed class EffectControl : MonoBehaviour
    {
        [SerializeField]
        [LabelText("循环中")]
        private bool looping = false;

        [SerializeField]
        [LabelText("延迟时间")]
        private float delay = 0.0f;

        [SerializeField]
        [LabelText("效果持续时间")]
        private float duration = 5.0f;

        [SerializeField]
        [LabelText("随着时间的推移而逐渐消失")]
        private float fadeout = 1.0f;

        private PlayState            state = PlayState.Stopping;
        private float                timer;
        private List<ParticleSystem> particleSystems;
        private List<Animator>       animators;
        private List<Animation>      animations;
        private float                playbackSpeed = 1.0f;

        private bool releaseAfterFinish = false;

        public bool ReleaseAfterFinish
        {
            get { return releaseAfterFinish; }
            set { releaseAfterFinish = value; }
        }

        /// <summary>
        /// 淡出事件
        /// </summary>
        public event Action FadeoutEvent;

        /// <summary>
        /// 完成事件
        /// </summary>
        public event Action FinishEvent;

        /// <summary>
        /// 效果的播放状态
        /// </summary>
        private enum PlayState
        {
            Stopping,
            Pending,
            Playing,
            Pausing,
            Fadeouting,
        }

        /// <summary>
        /// 获取一个值，该值指示此效果是否正在循环
        /// </summary>
        public bool IsLooping
        {
            get { return this.looping; }
        }

        /// <summary>
        /// 获取一个值，该值指示是否暂停此效果。
        /// </summary>
        public bool IsPaused
        {
            get { return PlayState.Pausing == this.state; }
        }

        /// <summary>
        /// 获取一个值，该值指示是否停止此效果。
        /// </summary>
        public bool IsStopped
        {
            get { return PlayState.Stopping == this.state; }
        }

        /// <summary>
        /// 获取效果持续时间。
        /// </summary>
        public float Duration
        {
            get { return this.duration; }
        }

        /// <summary>
        /// 获取效果淡出时间。
        /// </summary>
        public float Fadeout
        {
            get { return this.fadeout; }
        }

        /// <summary>
        /// 获取或设置此效果的播放速度。
        /// </summary>
        public float PlaybackSpeed
        {
            get { return this.playbackSpeed; }

            set
            {
                this.playbackSpeed = value;

                foreach (var particleSystem in this.ParticleSystems)
                {
                    var main = particleSystem.main;
                    main.simulationSpeed = this.playbackSpeed;
                }

                foreach (var animator in this.Animators)
                {
                    animator.speed = this.playbackSpeed;
                }

                foreach (var animation in this.Animations)
                {
                    var clip = animation.clip;
                    if (clip != null)
                    {
                        animation[clip.name].speed = this.playbackSpeed;
                    }
                }
            }
        }

        /// <summary>
        /// 得到粒子系统。
        /// </summary>
        private List<ParticleSystem> ParticleSystems
        {
            get
            {
                if (this.particleSystems == null)
                {
                    this.particleSystems = ListPool<ParticleSystem>.Get();
                    this.GetComponentsInChildren(true, particleSystems);
                    foreach (var particleSystem in this.ParticleSystems)
                    {
                        var main = particleSystem.main;
                        main.simulationSpeed = this.playbackSpeed;
                    }
                }

                return this.particleSystems;
            }
        }

        /// <summary>
        /// 获取动画器
        /// </summary>
        private List<Animator> Animators
        {
            get
            {
                if (this.animators == null)
                {
                    this.animators = ListPool<Animator>.Get();
                    this.GetComponentsInChildren(true, this.animators);
                    foreach (var animator in this.animators)
                    {
                        animator.speed = this.playbackSpeed;
                    }
                }

                return this.animators;
            }
        }

        /// <summary>
        /// 获取动画
        /// </summary>
        private List<Animation> Animations
        {
            get
            {
                if (this.animations == null)
                {
                    this.animations = ListPool<Animation>.Get();
                    this.GetComponentsInChildren(true, this.animations);
                    foreach (var animation in this.animations)
                    {
                        var clip = animation.clip;
                        if (clip != null)
                        {
                            animation[clip.name].speed = this.playbackSpeed;
                        }
                    }
                }

                return this.animations;
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// 估计持续时间。
        /// </summary>
        public void EstimateDuration()
        {
            this.looping  = false;
            this.duration = 0.0f;
            this.fadeout  = 0.0f;

            foreach (var particleSystem in this.ParticleSystems)
            {
                if (particleSystem == null)
                {
                    continue;
                }

                if (particleSystem.main.loop)
                {
                    this.looping = true;
                }

                if (this.duration < particleSystem.main.duration)
                {
                    this.duration = particleSystem.main.duration;
                }

                if (this.fadeout < particleSystem.main.startLifetimeMultiplier)
                {
                    this.fadeout = particleSystem.main.startLifetimeMultiplier;
                }
            }

            foreach (var animation in this.Animations)
            {
                if (animation == null)
                {
                    continue;
                }

                var clip = animation.clip;
                if (clip == null)
                {
                    continue;
                }

                if (clip.isLooping)
                {
                    this.looping = true;
                }

                if (this.duration < clip.length)
                {
                    this.duration = clip.length;
                }
            }

            foreach (var animator in this.Animators)
            {
                if (animator == null)
                {
                    continue;
                }

                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.loop)
                {
                    this.looping = true;
                }

                if (this.duration < stateInfo.length)
                {
                    this.duration = stateInfo.length;
                }
            }
        }

        /// <summary>
        /// 刷新此效果。
        /// </summary>
        public void Refresh()
        {
            if (this.particleSystems != null)
            {
                ListPool<ParticleSystem>.Put(this.particleSystems);
                this.particleSystems = null;
            }

            if (this.animations != null)
            {
                ListPool<Animation>.Put(this.animations);
                this.animations = null;
            }

            if (this.animators != null)
            {
                ListPool<Animator>.Put(this.animators);
                this.animators = null;
            }
        }

        private void OnDestroy()
        {
            Refresh();
        }

        /// <summary>
        /// 开始模拟。
        /// </summary>
        public void SimulateInit()
        {
            // 烘焙所有动画师。
            var animators = this.Animators;
            foreach (var animator in animators)
            {
                if (animator == null)
                {
                    continue;
                }

                if (animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                const float FrameRate  = 30f;
                var         stateInfo  = animator.GetCurrentAnimatorStateInfo(0);
                int         frameCount = (int)((stateInfo.length * FrameRate) + 2);

                animator.Rebind();
                animator.StopPlayback();
                animator.recorderStartTime = 0;
                animator.StartRecording(frameCount);

                for (var i = 0; i < frameCount - 1; ++i)
                {
                    animator.Update(i / FrameRate);
                }

                animator.StopRecording();
                animator.StartPlayback();
            }
        }

        /// <summary>
        /// 开始模拟效果。
        /// </summary>
        public void SimulateStart()
        {
            var particleSystems = this.ParticleSystems;
            foreach (var ps in particleSystems)
            {
                if (ps == null)
                {
                    continue;
                }

                ps.Simulate(0, false, true);
                ps.time = 0;
                ps.Play();
            }

            var animators = this.Animators;
            foreach (var animator in animators)
            {
                if (animator == null)
                {
                    continue;
                }

                if (animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                animator.playbackTime = 0.0f;
                animator.Update(0.0f);
            }

            var animations = this.Animations;
            foreach (var animation in animations)
            {
                if (animation == null)
                {
                    continue;
                }

                var clip = animation.clip;
                if (clip == null)
                {
                    continue;
                }

                clip.SampleAnimation(animation.gameObject, 0.0f);
            }
        }

        /// <summary>
        /// 通过增量时间更新模拟的效果。
        /// </summary>
        public void SimulateDelta(float time, float deltaTime)
        {
            var particleSystems = this.ParticleSystems;
            foreach (var ps in particleSystems)
            {
                if (ps == null)
                {
                    continue;
                }

                ps.Simulate(deltaTime, false, false);
            }

            var animators = this.Animators;
            foreach (var animator in animators)
            {
                if (animator == null)
                {
                    continue;
                }

                if (animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                animator.playbackTime = time;
                animator.Update(0.0f);
            }

            var animations = this.Animations;
            foreach (var animation in animations)
            {
                if (animation == null)
                {
                    continue;
                }

                var clip = animation.clip;
                if (clip == null)
                {
                    continue;
                }

                clip.SampleAnimation(animation.gameObject, time);
            }
        }

        /// <summary>
        /// 模拟在编辑器模式下。
        /// </summary>
        public void Simulate(float time)
        {
            var randomKeeper    = new Dictionary<ParticleSystem, KeyValuePair<bool, uint>>();
            var particleSystems = this.ParticleSystems;
            foreach (var ps in particleSystems)
            {
                if (ps == null)
                {
                    continue;
                }

                ps.Stop(false);
                var pair = new KeyValuePair<bool, uint>(
                    ps.useAutoRandomSeed, ps.randomSeed);
                randomKeeper.Add(ps, pair);
                if (!ps.isPlaying)
                {
                    ps.useAutoRandomSeed = false;
                    ps.randomSeed        = 0;
                }

                ps.Simulate(0, false, true);
                ps.time = 0;
                ps.Play();
            }

            for (float i = 0.0f; i < time; i += 0.02f)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps == null)
                    {
                        continue;
                    }

                    ps.Simulate(0.02f, false, false);
                }
            }

            foreach (var ps in particleSystems)
            {
                if (ps == null)
                {
                    continue;
                }

                ps.Stop(false);
                var pair = randomKeeper[ps];
                ps.randomSeed        = pair.Value;
                ps.useAutoRandomSeed = pair.Key;
            }

            var animators = this.Animators;
            foreach (var animator in animators)
            {
                if (animator == null)
                {
                    continue;
                }

                if (animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                animator.playbackTime = time;
                animator.Update(0.0f);
            }

            var animations = this.Animations;
            foreach (var animation in animations)
            {
                if (animation == null)
                {
                    continue;
                }

                var clip = animation.clip;
                if (clip == null)
                {
                    continue;
                }

                clip.SampleAnimation(animation.gameObject, time);
            }
        }
        #endif

        /// <summary>
        /// 开始
        /// </summary>
        public void Play()
        {
            if (PlayState.Playing == this.state)
            {
                this.Stop();
            }

            this.state = PlayState.Pending;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (PlayState.Playing == this.state)
            {
                foreach (var particleSystem in this.ParticleSystems)
                {
                    particleSystem.Pause(false);
                }

                foreach (var animator in this.Animators)
                {
                    animator.speed = 0.0f;
                }

                foreach (var animation in this.Animations)
                {
                    var clip = animation.clip;
                    if (clip != null)
                    {
                        animation[clip.name].speed = 0.0f;
                    }
                }

                this.state = PlayState.Pausing;
            }
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        public void Resume()
        {
            if (PlayState.Pausing == this.state)
            {
                foreach (var particleSystem in this.ParticleSystems)
                {
                    particleSystem.Play(false);
                }

                foreach (var animator in this.Animators)
                {
                    animator.speed = this.playbackSpeed;
                }

                foreach (var animation in this.Animations)
                {
                    var clip = animation.clip;
                    if (clip != null)
                    {
                        animation[clip.name].speed = this.playbackSpeed;
                    }
                }

                this.state = PlayState.Playing;
            }
        }

        public void ForceCallFinishEvent()
        {
            if (this.FinishEvent != null)
            {
                this.FinishEvent();
            }
        }

        public void ClearFinishEvent()
        {
            this.FinishEvent        = null;
            this.ReleaseAfterFinish = false;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Stop()
        {
            if (this.state != PlayState.Stopping)
            {
                this.state = PlayState.Fadeouting;
                foreach (var particleSystem in this.ParticleSystems)
                {
                    particleSystem.Stop(false);
                }

                foreach (var animator in this.Animators)
                {
                    animator.gameObject.SetActive(false);
                }

                foreach (var animation in this.Animations)
                {
                    if (animation.playAutomatically)
                    {
                        animation.gameObject.SetActive(false);
                    }
                    else
                    {
                        animation.Stop();
                    }
                }

                if (this.FadeoutEvent != null)
                {
                    this.FadeoutEvent();
                    this.FadeoutEvent = null;
                }
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            this.timer = 0.0f;
            this.state = PlayState.Stopping;
        }

        private void Awake()
        {
            if (this.particleSystems == null)
            {
                this.particleSystems = ListPool<ParticleSystem>.Get();
                this.GetComponentsInChildren(true, this.particleSystems);
            }

            this.Reset();
        }

        private void LateUpdate()
        {
            if (PlayState.Stopping == this.state ||
                PlayState.Pausing == this.state)
            {
                return;
            }

            this.timer += Time.deltaTime * this.playbackSpeed;
            if (PlayState.Pending == this.state && this.timer >= this.delay)
            {
                foreach (var particleSystem in this.ParticleSystems)
                {
                    particleSystem.Play(false);
                }

                foreach (var animator in this.Animators)
                {
                    animator.gameObject.SetActive(false);
                    animator.gameObject.SetActive(true);
                }

                foreach (var animation in this.Animations)
                {
                    if (animation.playAutomatically)
                    {
                        animation.gameObject.SetActive(false);
                        animation.gameObject.SetActive(true);
                    }
                    else
                    {
                        animation.Stop();
                        animation.Play();
                    }
                }

                this.state = PlayState.Playing;
            }

            if (!this.looping)
            {
                if (PlayState.Playing == this.state &&
                    this.timer >= this.duration)
                {
                    this.Stop();
                }
            }

            if (PlayState.Fadeouting == this.state &&
                this.timer >= this.duration + this.fadeout)
            {
                this.state = PlayState.Stopping;
                if (this.FinishEvent != null)
                {
                    this.FinishEvent();
                    this.FinishEvent = null;
                    if (ReleaseAfterFinish)
                    {
                        ReleaseAfterFinish = false;

                        //Debug.LogError("TODO GameObjectPool");
                        //GameObjectPool.Instance.Free(gameObject);
                    }
                }
            }
        }
    }
}