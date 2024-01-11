using UnityEngine;
using System;
using System.Collections.Generic;

namespace Coffee.UIEffects
{
    /// <summary>
    /// Effect player.
    /// </summary>
    [Serializable]
    public class EffectPlayer
    {
        //################################
        // Public Members.
        //################################
        /// <summary>
        /// Gets or sets a value indicating whether is playing.
        /// </summary>
        [Header("Effect Player")] [Tooltip("Playing.")]
        public bool play = false;

        /// <summary>
        /// Gets or sets the delay before looping.
        /// </summary>
        [Tooltip("Initial play delay.")] [Range(0f, 10f)]
        public float initialPlayDelay = 0;

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        [Tooltip("Duration.")] [Range(0.01f, 10f)]
        public float duration = 1;

        /// <summary>
        /// Gets or sets a value indicating whether can loop.
        /// </summary>
        [Tooltip("Loop.")] public bool loop = false;

        /// <summary>
        /// Gets or sets the delay before looping.
        /// </summary>
        [Tooltip("Delay before looping.")] [Range(0f, 10f)]
        public float loopDelay = 0;

        /// <summary>
        /// Gets or sets the update mode.
        /// </summary>
        [Tooltip("Update mode")] public AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal;

        static List<Action> s_UpdateActions;

        /// <summary>
        /// Register player.
        /// </summary>
        public void OnEnable(Action<float> callback = null)
        {
            if (s_UpdateActions == null)
            {
                s_UpdateActions = new List<Action>();
                Canvas.willRenderCanvases += () =>
                {
                    var count = s_UpdateActions.Count;
                    for (int i = 0; i < count; i++)
                    {
                        s_UpdateActions[i].Invoke();
                    }
                };
            }

            s_UpdateActions.Add(OnWillRenderCanvases);

            if (play)
            {
                _time = -initialPlayDelay;
            }
            else
            {
                _time = 0;
            }

            _callback = callback;
        }

        /// <summary>
        /// Unregister player.
        /// </summary>
        public void OnDisable()
        {
            _callback = null;
            s_UpdateActions.Remove(OnWillRenderCanvases);
        }

        /// <summary>
        /// Start playing.
        /// </summary>
        public void Play(bool reset, Action<float> callback = null)
        {
            if (reset)
            {
                _time = 0;
            }

            play = true;
            if (callback != null)
            {
                _callback = callback;
            }
        }

        /// <summary>
        /// Stop playing.
        /// </summary>
        public void Stop(bool reset)
        {
            if (reset)
            {
                _time = 0;
                if (_callback != null)
                {
                    _callback(_time);
                }
            }

            play = false;
        }

        //################################
        // Private Members.
        //################################
        float _time = 0;
        Action<float> _callback;

        void OnWillRenderCanvases()
        {
            if (!play || !Application.isPlaying || _callback == null)
            {
                return;
            }

            _time += updateMode == AnimatorUpdateMode.UnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
            var current = _time / duration;

            if (duration <= _time)
            {
                play = loop;
                _time = loop ? -loopDelay : 0;
            }

            _callback(current);
        }
    }
}
