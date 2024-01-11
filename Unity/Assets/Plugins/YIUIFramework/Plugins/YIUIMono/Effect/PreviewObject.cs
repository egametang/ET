#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 这是一个仅用于编辑器的脚本，用于在编辑模式下显示预览对象
    /// </summary>
    [ExecuteInEditMode]
    public sealed class PreviewObject : MonoBehaviour
    {
        private GameObject preview;
        private bool       simulateInEditMode = true; //记录播放时间
        private float      playingTime        = 0.0f;
        private double     lastTime           = -1.0;

        /// <summary>
        /// 获取或设置一个值，该值指示是否在编辑模式下模拟。
        /// </summary>
        public bool SimulateInEditMode
        {
            get { return this.simulateInEditMode; }
            set { this.simulateInEditMode = value; }
        }

        /// <summary>
        /// 清除预览对象。
        /// </summary>
        public void ClearPreview()
        {
            if (this.preview != null)
            {
                var deletePreview = this.preview;
                this.preview                =  null;
                EditorApplication.delayCall += () => { GameObject.DestroyImmediate(deletePreview); };
            }
        }

        /// <summary>
        /// 设置预览对象
        /// </summary>
        public void SetPreview(GameObject previewObj)
        {
            // Destroy the pre-preview.
            if (this.preview != null)
            {
                var deletePreview = this.preview;
                this.preview                =  null;
                EditorApplication.delayCall += () => { GameObject.DestroyImmediate(deletePreview); };
            }

            // Attach the preview object.
            this.preview     = previewObj;
            this.preview.tag = "EditorOnly";
            this.preview.transform.SetParent(this.transform, false);

            // Start the animation.
            if (this.simulateInEditMode)
            {
                var effectControl = this.preview.GetComponent<EffectControl>();
                if (effectControl != null)
                {
                    effectControl.SimulateInit();
                    effectControl.SimulateStart();
                }
                else
                {
                    var particelSystems = this.preview.GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particelSystems)
                    {
                        ps.Simulate(0.0f, false, true);
                    }
                }
            }

            // Hide this preview.
            this.SetHideFlags(this.preview, HideFlags.DontSave);
        }

        private void Awake()
        {
            this.hideFlags = HideFlags.DontSave;
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (EditorApplication.isPlaying ||
                    EditorApplication.isPlayingOrWillChangePlaymode ||
                    EditorApplication.isCompiling)
                {
                    if (this.preview != null)
                    {
                        GameObject.DestroyImmediate(this.preview);
                        this.preview = null;
                    }
                }
            };
        }

        private void OnDestroy()
        {
            if (this.preview != null)
            {
                GameObject.DestroyImmediate(this.preview);
                this.preview = null;
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorApplication.update += this.UpdatePreview;
            this.lastTime            =  EditorApplication.timeSinceStartup;
            if (this.preview != null)
            {
                this.preview.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorApplication.update -= this.UpdatePreview;
            if (this.preview != null)
            {
                this.preview.SetActive(false);
            }
        }

        private void UpdatePreview()
        {
            if (!this.simulateInEditMode)
            {
                return;
            }

            var timeSinceStartup = EditorApplication.timeSinceStartup;
            var deltaTime        = (float)(timeSinceStartup - this.lastTime);
            this.lastTime    =  timeSinceStartup;
            this.playingTime += deltaTime;

            if (this.preview == null)
            {
                return;
            }

            // Start the animation.
            var effectControl = this.preview.GetComponent<EffectControl>();
            if (effectControl != null)
            {
                effectControl.SimulateDelta(this.playingTime, deltaTime);
            }
            else
            {
                float playTime = 0.0f;
                var particelSystems =
                    this.preview.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particelSystems)
                {
                    if (playTime < ps.main.duration)
                    {
                        playTime = ps.main.duration;
                    }
                }

                foreach (var ps in particelSystems)
                {
                    ps.Simulate(deltaTime, false, false);
                }
            }
        }

        private void SetHideFlags(GameObject obj, HideFlags flags)
        {
            obj.hideFlags = flags;
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                var child = obj.transform.GetChild(i);
                this.SetHideFlags(child.gameObject, flags);
            }
        }
    }
}
#endif