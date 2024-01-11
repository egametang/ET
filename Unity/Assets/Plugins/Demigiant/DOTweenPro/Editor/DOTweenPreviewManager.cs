// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 16:03

using System;
using System.Collections.Generic;
using DG.DemiEditor;
using DG.DemiLib;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DG.DOTweenEditor
{
    public static class DOTweenPreviewManager
    {
        static bool _previewOnlyIfSetToAutoPlay = true;
        static readonly Dictionary<DOTweenAnimation,TweenInfo> _AnimationToTween = new Dictionary<DOTweenAnimation,TweenInfo>();
        static readonly List<DOTweenAnimation> _TmpKeys = new List<DOTweenAnimation>();

        #region Public Methods & GUI

        /// <summary>
        /// Returns TRUE if its actually previewing animations
        /// </summary>
        public static bool PreviewGUI(DOTweenAnimation src)
        {
            if (EditorApplication.isPlaying) return false;

            Styles.Init();

            bool isPreviewing = _AnimationToTween.Count > 0;
            bool isPreviewingThis = isPreviewing && _AnimationToTween.ContainsKey(src);

            // Preview in editor
            GUI.backgroundColor = isPreviewing
                ? new DeSkinColor(new Color(0.49f, 0.8f, 0.86f), new Color(0.15f, 0.26f, 0.35f))
                : new DeSkinColor(Color.white, new Color(0.13f, 0.13f, 0.13f));
            GUILayout.BeginVertical(Styles.previewBox);
            DeGUI.ResetGUIColors();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preview Mode - Experimental", Styles.previewLabel);
            _previewOnlyIfSetToAutoPlay = DeGUILayout.ToggleButton(
                _previewOnlyIfSetToAutoPlay,
                new GUIContent("AutoPlay only", "If toggled only previews animations that have AutoPlay turned ON"),
                Styles.btOption
            );
            GUILayout.EndHorizontal();
            GUILayout.Space(1);
            // Preview - Play
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(
                isPreviewingThis || src.animationType == DOTweenAnimation.AnimationType.None
                || !src.isActive || _previewOnlyIfSetToAutoPlay && !src.autoPlay
            );
            if (GUILayout.Button("► Play", Styles.btPreview)) {
                if (!isPreviewing) StartupGlobalPreview();
                AddAnimationToGlobalPreview(src);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(isPreviewing);
            if (GUILayout.Button("► Play All <i>on GameObject</i>", Styles.btPreview)) {
                if (!isPreviewing) StartupGlobalPreview();
                DOTweenAnimation[] anims = src.gameObject.GetComponents<DOTweenAnimation>();
                foreach (DOTweenAnimation anim in anims) AddAnimationToGlobalPreview(anim);
            }
            if (GUILayout.Button("► Play All <i>in Scene</i>", Styles.btPreview)) {
                if (!isPreviewing) StartupGlobalPreview();
                DOTweenAnimation[] anims = Object.FindObjectsOfType<DOTweenAnimation>();
                foreach (DOTweenAnimation anim in anims) AddAnimationToGlobalPreview(anim);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            // Preview - Stop
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isPreviewingThis);
            if (GUILayout.Button("■ Stop", Styles.btPreview)) {
                if (_AnimationToTween.ContainsKey(src)) StopPreview(_AnimationToTween[src].tween);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!isPreviewing);
            if (GUILayout.Button("■ Stop All <i>on GameObject</i>", Styles.btPreview)) {
                StopPreview(src.gameObject);
            }
            if (GUILayout.Button("■ Stop All <i>in Scene</i>", Styles.btPreview)) {
                StopAllPreviews();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            if (isPreviewing) {
                int playingTweens = 0;
                int completedTweens = 0;
                int pausedTweens = 0;
                foreach (KeyValuePair<DOTweenAnimation, TweenInfo> kvp in _AnimationToTween) {
                    Tween t = kvp.Value.tween;
                    if (t.IsPlaying()) playingTweens++;
                    else if (t.IsComplete()) completedTweens++;
                    else pausedTweens++;
                }
                GUILayout.Label("Playing Tweens: " + playingTweens, Styles.previewStatusLabel);
                GUILayout.Label("Completed Tweens: " + completedTweens, Styles.previewStatusLabel);
//                GUILayout.Label("Paused Tweens: " + playingTweens);
            }
            GUILayout.EndVertical();

            return isPreviewing;
        }

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5)
        public static void StopAllPreviews(PlayModeStateChange state)
        {
            StopAllPreviews();
        }
#endif

        public static void StopAllPreviews()
        {
            _TmpKeys.Clear();
            foreach (KeyValuePair<DOTweenAnimation,TweenInfo> kvp in _AnimationToTween) {
                _TmpKeys.Add(kvp.Key);
            }
            StopPreview(_TmpKeys);
            _TmpKeys.Clear();
            _AnimationToTween.Clear();

            DOTweenEditorPreview.Stop();
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
            UnityEditor.EditorApplication.playmodeStateChanged -= StopAllPreviews;
#else
            UnityEditor.EditorApplication.playModeStateChanged -= StopAllPreviews;
#endif
//            EditorApplication.playmodeStateChanged -= StopAllPreviews;

            InternalEditorUtility.RepaintAllViews();
        }

#endregion

#region Methods

        static void StartupGlobalPreview()
        {
            DOTweenEditorPreview.Start();
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
            UnityEditor.EditorApplication.playmodeStateChanged += StopAllPreviews;
#else
            UnityEditor.EditorApplication.playModeStateChanged += StopAllPreviews;
#endif
//            EditorApplication.playmodeStateChanged += StopAllPreviews;
        }

        static void AddAnimationToGlobalPreview(DOTweenAnimation src)
        {
            if (!src.isActive) return; // Ignore sources whose tweens have been set to inactive
            if (_previewOnlyIfSetToAutoPlay && !src.autoPlay) return;

            Tween t = src.CreateEditorPreview();
            _AnimationToTween.Add(src, new TweenInfo(src, t, src.isFrom));
            // Tween setup
            DOTweenEditorPreview.PrepareTweenForPreview(t);
        }

        static void StopPreview(GameObject go)
        {
            _TmpKeys.Clear();
            foreach (KeyValuePair<DOTweenAnimation,TweenInfo> kvp in _AnimationToTween) {
                if (kvp.Key.gameObject != go) continue;
                _TmpKeys.Add(kvp.Key);
            }
            StopPreview(_TmpKeys);
            _TmpKeys.Clear();

            if (_AnimationToTween.Count == 0) StopAllPreviews();
            else InternalEditorUtility.RepaintAllViews();
        }

        static void StopPreview(Tween t)
        {
            TweenInfo tInfo = null;
            foreach (KeyValuePair<DOTweenAnimation,TweenInfo> kvp in _AnimationToTween) {
                if (kvp.Value.tween != t) continue;
                tInfo = kvp.Value;
                _AnimationToTween.Remove(kvp.Key);
                break;
            }
            if (tInfo == null) {
                Debug.LogWarning("DOTween Preview ► Couldn't find tween to stop");
                return;
            }
            if (tInfo.isFrom) {
                int totLoops = tInfo.tween.Loops();
                if (totLoops < 0 || totLoops > 1) {
                    tInfo.tween.Goto(tInfo.tween.Duration(false));
                } else tInfo.tween.Complete();
            } else tInfo.tween.Rewind();
            tInfo.tween.Kill();
            EditorUtility.SetDirty(tInfo.animation); // Refresh views

            if (_AnimationToTween.Count == 0) StopAllPreviews();
            else InternalEditorUtility.RepaintAllViews();
        }

        // Stops while iterating inversely, which deals better with tweens that overwrite each other
        static void StopPreview(List<DOTweenAnimation> keys)
        {
            for (int i = keys.Count - 1; i > -1; --i) {
                DOTweenAnimation anim = keys[i];
                TweenInfo tInfo = _AnimationToTween[anim];
                if (tInfo.isFrom) {
                    int totLoops = tInfo.tween.Loops();
                    if (totLoops < 0 || totLoops > 1) {
                        tInfo.tween.Goto(tInfo.tween.Duration(false));
                    } else tInfo.tween.Complete();
                } else tInfo.tween.Rewind();
                tInfo.tween.Kill();
                EditorUtility.SetDirty(anim); // Refresh views
                _AnimationToTween.Remove(anim);
            }
        }

#endregion

        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
        // ███ INTERNAL CLASSES ████████████████████████████████████████████████████████████████████████████████████████████████
        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

        class TweenInfo
        {
            public DOTweenAnimation animation;
            public Tween tween;
            public bool isFrom;
            public TweenInfo(DOTweenAnimation animation, Tween tween, bool isFrom)
            {
                this.animation = animation;
                this.tween = tween;
                this.isFrom = isFrom;
            }
        }

        static class Styles
        {
            static bool _initialized;

            public static GUIStyle previewBox, previewLabel, btOption, btPreview, previewStatusLabel;

            public static void Init()
            {
                if (_initialized) return;

                _initialized = true;

                previewBox = new GUIStyle(GUI.skin.box).Clone().Padding(1, 1, 0, 3)
                    .Background(DeStylePalette.squareBorderCurved_darkBorders).Border(7, 7, 7, 7);
                previewLabel = new GUIStyle(GUI.skin.label).Clone(10, FontStyle.Bold).Padding(1, 0, 3, 0).Margin(3, 6, 0, 0).StretchWidth(false);
                btOption = DeGUI.styles.button.bBlankBorderCompact.MarginBottom(2).MarginRight(4);
                btPreview = EditorStyles.miniButton.Clone(Format.RichText);
                previewStatusLabel = EditorStyles.miniLabel.Clone().Padding(4, 0, 0, 0).Margin(0);
            }
        }
    }
}
