// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/27 19:02
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php


#if false // MODULE_MARKER
using System;
using System.Globalization;
using System.Collections.Generic;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using TMPro;
using Object = UnityEngine.Object;

namespace DG.Tweening
{
    public enum TMPSkewSpanMode
    {
        /// <summary>Applies the skew as-is (like normal skew works): the longer the text-span the higher the last character will be</summary>
        Default,
        /// <summary>Applies the skew scaled by the size of the text-span: the max skew/displacement will be the given skew factor</summary>
        AsMaxSkewFactor
    }

    /// <summary>
    /// Methods that extend TMP_Text objects and allow to directly create and control tweens from their instances.
    /// </summary>
    public static class ShortcutExtensionsTMPText
    {
        #region Colors

        /// <summary>Tweens a TextMeshPro's color to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOColor(this TMP_Text target, Color endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's faceColor to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFaceColor(this TMP_Text target, Color32 endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => target.faceColor, x => target.faceColor = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's outlineColor to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOOutlineColor(this TMP_Text target, Color32 endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => target.outlineColor, x => target.outlineColor = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's glow color to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="useSharedMaterial">If TRUE will use the fontSharedMaterial instead than the fontMaterial</param>
        public static TweenerCore<Color, Color, ColorOptions> DOGlowColor(this TMP_Text target, Color endValue, float duration, bool useSharedMaterial = false)
        {
            TweenerCore<Color, Color, ColorOptions> t = useSharedMaterial
                ? target.fontSharedMaterial.DOColor(endValue, "_GlowColor", duration)
                : target.fontMaterial.DOColor(endValue, "_GlowColor", duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's alpha color to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFade(this TMP_Text target, float endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro faceColor's alpha to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Color, Color, ColorOptions> DOFaceFade(this TMP_Text target, float endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => target.faceColor, x => target.faceColor = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #endregion

        #region Other

        /// <summary>Tweens a TextMeshPro's scale to the given value (using correct uniform scale as TMP requires).
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOScale(this TMP_Text target, float endValue, float duration)
        {
            Transform trans = target.transform;
            Vector3 endValueV3 = new Vector3(endValue, endValue, endValue);
            TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => trans.localScale, x => trans.localScale = x, endValueV3, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>
        /// Tweens a TextMeshPro's text from one integer to another, with options for thousands separators
        /// </summary>
        /// <param name="fromValue">The value to start from</param>
        /// <param name="endValue">The end value to reach</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="addThousandsSeparator">If TRUE (default) also adds thousands separators</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use (InvariantCulture if NULL)</param>
        public static TweenerCore<int, int, NoOptions> DOCounter(
            this TMP_Text target, int fromValue, int endValue, float duration, bool addThousandsSeparator = true, CultureInfo culture = null
        ){
            int v = fromValue;
            CultureInfo cInfo = !addThousandsSeparator ? null : culture ?? CultureInfo.InvariantCulture;
            TweenerCore<int, int, NoOptions> t = DOTween.To(() => v, x => {
                v = x;
                target.text = addThousandsSeparator
                    ? v.ToString("N0", cInfo)
                    : v.ToString();
            }, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's fontSize to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<float, float, FloatOptions> DOFontSize(this TMP_Text target, float endValue, float duration)
        {
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.fontSize, x => target.fontSize = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's maxVisibleCharacters to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public static TweenerCore<int, int, NoOptions> DOMaxVisibleCharacters(this TMP_Text target, int endValue, float duration)
        {
            TweenerCore<int, int, NoOptions> t = DOTween.To(() => target.maxVisibleCharacters, x => target.maxVisibleCharacters = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens a TextMeshPro's text to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
        /// otherwise all tags will be considered as normal text</param>
        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
        /// Leave it to NULL (default) to use default ones</param>
        public static TweenerCore<string, string, StringOptions> DOText(this TMP_Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            TweenerCore<string, string, StringOptions> t = DOTween.To(() => target.text, x => target.text = x, endValue, duration);
            t.SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetTarget(target);
            return t;
        }

        #endregion
    }

    #region DOTweenTMPAnimator

    // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
    // ███ CLASS ███████████████████████████████████████████████████████████████████████████████████████████████████████████
    // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

    /// <summary>
    /// Wrapper for <see cref="TMP_Text"/> objects that enables per-character tweening
    /// (you don't need this if instead you want to animate the whole text object).
    /// It also contains various handy methods to simply deform text without animating it ;)
    /// <para><code>EXAMPLE:<para/>
    /// DOTweenTMPAnimator animator = new DOTweenTMPAnimator(myTextMeshProTextField);<para/>
    /// Tween tween = animator.DOCharScale(characterIndex, scaleValue, duration);
    /// </code></para>
    /// </summary>
    public class DOTweenTMPAnimator : IDisposable
    {
        static readonly Dictionary<TMP_Text,DOTweenTMPAnimator> _targetToAnimator = new Dictionary<TMP_Text,DOTweenTMPAnimator>();

        /// <summary><see cref="TMP_Text"/> that this animator is linked to</summary>
        public TMP_Text target { get; private set; }
        public TMP_TextInfo textInfo { get; private set; }
        readonly List<CharTransform> _charTransforms = new List<CharTransform>();
        TMP_MeshInfo[] _cachedMeshInfos;
        bool _ignoreTextChangedEvent;

        /// <summary>
        /// Creates a new instance of the <see cref="DOTweenTMPAnimator"/>, which is necessary to animate <see cref="TMP_Text"/> by single characters.<para/>
        /// If a <see cref="DOTweenTMPAnimator"/> already exists for the same <see cref="TMP_Text"/> object it will be disposed
        /// (but not its tweens, those you will have to kill manually).
        /// If you want to animate the whole text object you don't need this, and you can use direct <see cref="TMP_Text"/> DO shortcuts instead.<para/>
        /// IMPORTANT: the <see cref="TMP_Text"/> target must have been enabled/activated at least once before you can use it with this
        /// </summary>
        /// <param name="target">The <see cref="TMP_Text"/> that will be linked to this animator</param>
        public DOTweenTMPAnimator(TMP_Text target)
        {
            if (target == null) {
                Debugger.LogError("DOTweenTMPAnimator target can't be null");
                return;
            }
            if (!target.gameObject.activeInHierarchy) {
                Debugger.LogError("You can't create a DOTweenTMPAnimator if its target is disabled");
                return;
            }
            // Verify that there's no other animators for the same target, and in case dispose them
            if (_targetToAnimator.ContainsKey(target)) {
                if (Debugger.logPriority >= 2) {
                    Debugger.Log(string.Format(
                        "A DOTweenTMPAnimator for \"{0}\" already exists: disposing it because you can't have more than one DOTweenTMPAnimator" +
                        " for the same TextMesh Pro object. If you have tweens running on the disposed DOTweenTMPAnimator you should kill them manually",
                        target
                    ));
                }
                _targetToAnimator[target].Dispose();
                _targetToAnimator.Remove(target);
            }
            //
            this.target = target;
            _targetToAnimator.Add(target, this);
            Refresh();
            // Listeners
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        /// <summary>
        /// If a <see cref="DOTweenTMPAnimator"/> instance exists for the given target disposes it
        /// </summary>
        public static void DisposeInstanceFor(TMP_Text target)
        {
            if (!_targetToAnimator.ContainsKey(target)) return;
            _targetToAnimator[target].Dispose();
            _targetToAnimator.Remove(target);
        }

        /// <summary>
        /// Clears and disposes of this object
        /// </summary>
        public void Dispose()
        {
            target = null;
            _charTransforms.Clear();
            textInfo = null;
            _cachedMeshInfos = null;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        /// <summary>
        /// Refreshes the animator text data and resets all transformation data. Call this after you change the target <see cref="TMP_Text"/>
        /// </summary>
        public void Refresh()
        {
            _ignoreTextChangedEvent = true;
            target.ForceMeshUpdate(true);
            textInfo = target.textInfo;
            _cachedMeshInfos = textInfo.CopyMeshInfoVertexData();
            int totChars = textInfo.characterCount;
            int totCurrent = _charTransforms.Count;
            if (totCurrent > totChars) {
                _charTransforms.RemoveRange(totChars, totCurrent - totChars);
                totCurrent = totChars;
            }
            for (int i = 0; i < totCurrent; ++i) {
                CharTransform c = _charTransforms[i];
                c.ResetTransformationData();
                c.Refresh(textInfo, _cachedMeshInfos);
                _charTransforms[i] = c;
            }
            for (int i = totCurrent; i < totChars; ++i) _charTransforms.Add(new CharTransform(i, textInfo, _cachedMeshInfos));
            _ignoreTextChangedEvent = false;
        }

        /// <summary>
        /// Resets all deformations
        /// </summary>
        public void Reset()
        {
            int totCurrent = _charTransforms.Count;
            for (int i = 0; i < totCurrent; ++i) _charTransforms[i].ResetAll(target, textInfo.meshInfo, _cachedMeshInfos);
        }

        void OnTextChanged(Object obj)
        {
            if (_ignoreTextChangedEvent || target == null || obj != target) return;
            Refresh();
        }

        bool ValidateChar(int charIndex, bool isTween = true)
        {
            if (textInfo.characterCount <= charIndex) {
                Debugger.LogError(string.Format("CharIndex {0} doesn't exist", charIndex));
                return false;
            }
            if (!textInfo.characterInfo[charIndex].isVisible) {
                if (Debugger.logPriority > 1) {
                    if (isTween) {
                        Debugger.Log(string.Format(
                            "CharIndex {0} isn't visible, ignoring it and returning an empty tween (TextMesh Pro will behave weirdly if invisible chars are included in the animation)",
                            charIndex
                        ));
                    } else {
                        Debugger.Log(string.Format("CharIndex {0} isn't visible, ignoring it", charIndex));
                    }
                }
                return false;
            }
            return true;
        }

        bool ValidateSpan(int fromCharIndex, int toCharIndex, out int firstVisibleCharIndex, out int lastVisibleCharIndex)
        {
            firstVisibleCharIndex = -1; // First visible/existing charIndex from given index
            lastVisibleCharIndex = -1; // Last visible/existing charIndex backwards from given index
            int charCount = textInfo.characterCount;
            if (fromCharIndex >= charCount) return false;
            if (toCharIndex >= charCount) toCharIndex = charCount - 1;
            for (int i = fromCharIndex; i < toCharIndex + 1; ++i) {
                if (!_charTransforms[i].isVisible) continue;
                firstVisibleCharIndex = i;
                break;
            }
            if (firstVisibleCharIndex == -1) return false;
            for (int i = toCharIndex; i > firstVisibleCharIndex - 1; --i) {
                if (!_charTransforms[i].isVisible) continue;
                lastVisibleCharIndex = i;
                break;
            }
            if (lastVisibleCharIndex == -1) return false;
            return true;
        }

        #region Word Setters

        /// <summary>
        /// Skews a span of characters uniformly (like normal skew works in graphic applications)
        /// </summary>
        /// <param name="fromCharIndex">First char index of the span to skew</param>
        /// <param name="toCharIndex">Last char index of the span to skew</param>
        /// <param name="skewFactor">Skew factor</param>
        /// <param name="skewTop">If TRUE skews the top side of the span, otherwise the bottom one</param>
        public void SkewSpanX(int fromCharIndex, int toCharIndex, float skewFactor, bool skewTop = true)
        {
            int firstVisibleCharIndex, lastVisibleCharIndex;
            if (!ValidateSpan(fromCharIndex, toCharIndex, out firstVisibleCharIndex, out lastVisibleCharIndex)) return;
            for (int i = firstVisibleCharIndex; i < lastVisibleCharIndex + 1; ++i) {
                if (!_charTransforms[i].isVisible) continue;
                CharVertices v = _charTransforms[i].GetVertices();
                float skew = SkewCharX(i, skewFactor, skewTop);
            }
        }

        /// <summary>
        /// Skews a span of characters uniformly (like normal skew works in graphic applications)
        /// </summary>
        /// <param name="fromCharIndex">First char index of the span to skew</param>
        /// <param name="toCharIndex">Last char index of the span to skew</param>
        /// <param name="skewFactor">Skew factor</param>
        /// <param name="mode">Skew mode</param>
        /// <param name="skewRight">If TRUE skews the right side of the span, otherwise the left one</param>
        public void SkewSpanY(
            int fromCharIndex, int toCharIndex, float skewFactor,
            TMPSkewSpanMode mode = TMPSkewSpanMode.Default, bool skewRight = true
        ){
            int firstVisibleCharIndex, lastVisibleCharIndex;
            if (!ValidateSpan(fromCharIndex, toCharIndex, out firstVisibleCharIndex, out lastVisibleCharIndex)) return;
            if (mode == TMPSkewSpanMode.AsMaxSkewFactor) {
                CharVertices firstVisibleCharVertices = _charTransforms[firstVisibleCharIndex].GetVertices();
                CharVertices lastVisibleCharVertices = _charTransforms[lastVisibleCharIndex].GetVertices();
                float spanW = Mathf.Abs(lastVisibleCharVertices.bottomRight.x - firstVisibleCharVertices.bottomLeft.x);
                float spanH = Mathf.Abs(lastVisibleCharVertices.topRight.y - lastVisibleCharVertices.bottomRight.y);
                float ratio = spanH / spanW;
                skewFactor *= ratio;
            }
            float offsetY = 0;
            CharVertices prevCharVertices = new CharVertices();
            float prevCharSkew = 0;
            if (skewRight) {
                for (int i = firstVisibleCharIndex; i < lastVisibleCharIndex + 1; ++i) {
                    if (!_charTransforms[i].isVisible) continue;
                    CharVertices v = _charTransforms[i].GetVertices();
                    float skew = SkewCharY(i, skewFactor, skewRight);
                    if (i > firstVisibleCharIndex) {
                        float prevCharW = Mathf.Abs(prevCharVertices.bottomLeft.x - prevCharVertices.bottomRight.x);
                        float charsDist = Mathf.Abs(v.bottomLeft.x - prevCharVertices.bottomRight.x);
                        offsetY += prevCharSkew + (prevCharSkew * charsDist) / prevCharW;
                        SetCharOffset(i, new Vector3(0, _charTransforms[i].offset.y + offsetY, 0));
                    }
                    prevCharVertices = v;
                    prevCharSkew = skew;
                }
            } else {
                for (int i = lastVisibleCharIndex; i > firstVisibleCharIndex - 1; --i) {
                    if (!_charTransforms[i].isVisible) continue;
                    CharVertices v = _charTransforms[i].GetVertices();
                    float skew = SkewCharY(i, skewFactor, skewRight);
                    if (i < lastVisibleCharIndex) {
                        float prevCharW = Mathf.Abs(prevCharVertices.bottomLeft.x - prevCharVertices.bottomRight.x);
                        float charsDist = Mathf.Abs(v.bottomRight.x - prevCharVertices.bottomLeft.x);
                        offsetY += prevCharSkew + (prevCharSkew * charsDist) / prevCharW;
                        SetCharOffset(i, new Vector3(0, _charTransforms[i].offset.y + offsetY, 0));
                    }
                    prevCharVertices = v;
                    prevCharSkew = skew;
                }
            }
        }

        #endregion

        #region Char Getters

        /// <summary>
        /// Returns the current color of the given character, if it exists and is visible.
        /// </summary>
        /// <param name="charIndex">Character index</param>
        public Color GetCharColor(int charIndex)
        {
            if (!ValidateChar(charIndex)) return Color.white;
            return _charTransforms[charIndex].GetColor(textInfo.meshInfo);
        }

        /// <summary>
        /// Returns the current offset of the given character, if it exists and is visible.
        /// </summary>
        /// <param name="charIndex">Character index</param>
        public Vector3 GetCharOffset(int charIndex)
        {
            if (!ValidateChar(charIndex)) return Vector3.zero;
            return _charTransforms[charIndex].offset;
        }

        /// <summary>
        /// Returns the current rotation of the given character, if it exists and is visible.
        /// </summary>
        /// <param name="charIndex">Character index</param>
        public Vector3 GetCharRotation(int charIndex)
        {
            if (!ValidateChar(charIndex)) return Vector3.zero;
            return _charTransforms[charIndex].rotation.eulerAngles;
        }

        /// <summary>
        /// Returns the current scale of the given character, if it exists and is visible.
        /// </summary>
        /// <param name="charIndex">Character index</param>
        public Vector3 GetCharScale(int charIndex)
        {
            if (!ValidateChar(charIndex)) return Vector3.zero;
            return _charTransforms[charIndex].scale;
        }

        #endregion

        #region Char Setters

        /// <summary>
        /// Immediately sets the color of the given character.
        /// Will do nothing if the <see cref="charIndex"/> is invalid or the character isn't visible
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="color">Color to set</param>
        public void SetCharColor(int charIndex, Color32 color)
        {
            if (!ValidateChar(charIndex)) return;
            CharTransform c = _charTransforms[charIndex];
            c.UpdateColor(target, color, textInfo.meshInfo);
            _charTransforms[charIndex] = c;
        }

        /// <summary>
        /// Immediately sets the offset of the given character.
        /// Will do nothing if the <see cref="charIndex"/> is invalid or the character isn't visible
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="offset">Offset to set</param>
        public void SetCharOffset(int charIndex, Vector3 offset)
        {
            if (!ValidateChar(charIndex)) return;
            CharTransform c = _charTransforms[charIndex];
            c.UpdateGeometry(target, offset, c.rotation, c.scale, _cachedMeshInfos);
            _charTransforms[charIndex] = c;
        }

        /// <summary>
        /// Immediately sets the rotation of the given character.
        /// Will do nothing if the <see cref="charIndex"/> is invalid or the character isn't visible
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="rotation">Rotation to set</param>
        public void SetCharRotation(int charIndex, Vector3 rotation)
        {
            if (!ValidateChar(charIndex)) return;
            CharTransform c = _charTransforms[charIndex];
            c.UpdateGeometry(target, c.offset, Quaternion.Euler(rotation), c.scale, _cachedMeshInfos);
            _charTransforms[charIndex] = c;
        }

        /// <summary>
        /// Immediately sets the scale of the given character.
        /// Will do nothing if the <see cref="charIndex"/> is invalid or the character isn't visible
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="scale">Scale to set</param>
        public void SetCharScale(int charIndex, Vector3 scale)
        {
            if (!ValidateChar(charIndex)) return;
            CharTransform c = _charTransforms[charIndex];
            c.UpdateGeometry(target, c.offset, c.rotation, scale, _cachedMeshInfos);
            _charTransforms[charIndex] = c;
        }

        /// <summary>
        /// Immediately shifts the vertices of the given character by the given factor.
        /// Will do nothing if the <see cref="charIndex"/> is invalid or the character isn't visible
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="topLeftShift">Top left offset</param>
        /// <param name="topRightShift">Top right offset</param>
        /// <param name="bottomLeftShift">Bottom left offset</param>
        /// <param name="bottomRightShift">Bottom right offset</param>
        public void ShiftCharVertices(int charIndex, Vector3 topLeftShift, Vector3 topRightShift, Vector3 bottomLeftShift, Vector3 bottomRightShift)
        {
            if (!ValidateChar(charIndex)) return;
            CharTransform c = _charTransforms[charIndex];
            c.ShiftVertices(target, topLeftShift, topRightShift, bottomLeftShift, bottomRightShift);
            _charTransforms[charIndex] = c;
        }

        /// <summary>
        /// Skews the given character horizontally along the X axis and returns the skew amount applied (based on the character's size)
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="skewFactor">skew amount</param>
        /// <param name="skewTop">If TRUE skews the top side of the character, otherwise the bottom one</param>
        public float SkewCharX(int charIndex, float skewFactor, bool skewTop = true)
        {
            if (!ValidateChar(charIndex)) return 0;
            Vector3 skewV = new Vector3(skewFactor, 0, 0);
            CharTransform c = _charTransforms[charIndex];
            if (skewTop) c.ShiftVertices(target, skewV, skewV, Vector3.zero, Vector3.zero);
            else c.ShiftVertices(target, Vector3.zero, Vector3.zero, skewV, skewV);
            _charTransforms[charIndex] = c;
            return skewFactor;
        }

        /// <summary>
        /// Skews the given character vertically along the Y axis and returns the skew amount applied (based on the character's size)
        /// </summary>
        /// <param name="charIndex">Character index</param>
        /// <param name="skewFactor">skew amount</param>
        /// <param name="skewRight">If TRUE skews the right side of the character, otherwise the left one</param>
        /// <param name="fixedSkew">If TRUE applies exactly the given <see cref="skewFactor"/>,
        /// otherwise modifies it based on the aspectRation of the character</param>
        public float SkewCharY(int charIndex, float skewFactor, bool skewRight = true, bool fixedSkew = false)
        {
            if (!ValidateChar(charIndex)) return 0;
            float skew = fixedSkew ? skewFactor : skewFactor * textInfo.characterInfo[charIndex].aspectRatio;
            Vector3 skewV = new Vector3(0, skew, 0);
            CharTransform c = _charTransforms[charIndex];
            if (skewRight) c.ShiftVertices(target, Vector3.zero, skewV, Vector3.zero, skewV);
            else c.ShiftVertices(target, skewV, Vector3.zero, skewV, Vector3.zero);
            _charTransforms[charIndex] = c;
            return skew;
        }

        /// <summary>
        /// Resets the eventual vertices shift applied to the given character via <see cref="ShiftCharVertices"/>.
        /// Will do nothing if the <see cref="charIndex"/> is invalid or the character isn't visible
        /// </summary>
        /// <param name="charIndex">Character index</param>
        public void ResetVerticesShift(int charIndex)
        {
            if (!ValidateChar(charIndex)) return;
            CharTransform c = _charTransforms[charIndex];
            c.ResetVerticesShift(target);
            _charTransforms[charIndex] = c;
        }

        #endregion

        #region Char Tweens

        /// <summary>Tweens a character's alpha to the given value and returns the <see cref="Tween"/>.
        /// Will return NULL if the <see cref="charIndex"/> is invalid or the character isn't visible.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public TweenerCore<Color, Color, ColorOptions> DOFadeChar(int charIndex, float endValue, float duration)
        {
            if (!ValidateChar(charIndex)) return null;
            TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => _charTransforms[charIndex].GetColor(textInfo.meshInfo), x => {
                _charTransforms[charIndex].UpdateAlpha(target, x, textInfo.meshInfo);
            }, endValue, duration);
            return t;
        }

        /// <summary>Tweens a character's color to the given value and returns the <see cref="Tween"/>.
        /// Will return NULL if the <see cref="charIndex"/> is invalid or the character isn't visible.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public TweenerCore<Color, Color, ColorOptions> DOColorChar(int charIndex, Color endValue, float duration)
        {
            if (!ValidateChar(charIndex)) return null;
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => _charTransforms[charIndex].GetColor(textInfo.meshInfo), x => {
                _charTransforms[charIndex].UpdateColor(target, x, textInfo.meshInfo);
            }, endValue, duration);
            return t;
        }

        /// <summary>Tweens a character's offset to the given value and returns the <see cref="Tween"/>.
        /// Will return NULL if the <see cref="charIndex"/> is invalid or the character isn't visible.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public TweenerCore<Vector3, Vector3, VectorOptions> DOOffsetChar(int charIndex, Vector3 endValue, float duration)
        {
            if (!ValidateChar(charIndex)) return null;
            TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => _charTransforms[charIndex].offset, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, x, charT.rotation, charT.scale, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, endValue, duration);
            return t;
        }

        /// <summary>Tweens a character's rotation to the given value and returns the <see cref="Tween"/>.
        /// Will return NULL if the <see cref="charIndex"/> is invalid or the character isn't visible.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        /// <param name="mode">Rotation mode</param>
        public TweenerCore<Quaternion, Vector3, QuaternionOptions> DORotateChar(int charIndex, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
        {
            if (!ValidateChar(charIndex)) return null;
            TweenerCore<Quaternion, Vector3, QuaternionOptions> t = DOTween.To(() => _charTransforms[charIndex].rotation, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, charT.offset, x, charT.scale, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, endValue, duration);
            t.plugOptions.rotateMode = mode;
            return t;
        }

        /// <summary>Tweens a character's scale to the given value and returns the <see cref="Tween"/>.
        /// Will return NULL if the <see cref="charIndex"/> is invalid or the character isn't visible.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public TweenerCore<Vector3, Vector3, VectorOptions> DOScaleChar(int charIndex, float endValue, float duration)
        {
            return DOScaleChar(charIndex, new Vector3(endValue, endValue, endValue), duration);
        }
        /// <summary>Tweens a character's color to the given value and returns the <see cref="Tween"/>.
        /// Will return NULL if the <see cref="charIndex"/> is invalid or the character isn't visible.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="endValue">The end value to reach</param><param name="duration">The duration of the tween</param>
        public TweenerCore<Vector3, Vector3, VectorOptions> DOScaleChar(int charIndex, Vector3 endValue, float duration)
        {
            if (!ValidateChar(charIndex)) return null;
            TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => _charTransforms[charIndex].scale, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, charT.offset, charT.rotation, x, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, endValue, duration);
            return t;
        }

        /// <summary>Punches a character's offset towards the given direction and then back to the starting one
        /// as if it was connected to the starting position via an elastic.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="punch">The punch strength</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="vibrato">Indicates how much will the punch vibrate per second</param>
        /// <param name="elasticity">Represents how much (0 to 1) the vector will go beyond the starting size when bouncing backwards.
        /// 1 creates a full oscillation between the punch offset and the opposite offset,
        /// while 0 oscillates only between the punch offset and the start offset</param>
        public Tweener DOPunchCharOffset(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            if (!ValidateChar(charIndex)) return null;
            if (duration <= 0) {
                if (Debugger.logPriority > 0) Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
                return null;
            }
            return DOTween.Punch(() => _charTransforms[charIndex].offset, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, x, charT.rotation, charT.scale, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, punch, duration, vibrato, elasticity);
        }

        /// <summary>Punches a character's rotation towards the given direction and then back to the starting one
        /// as if it was connected to the starting position via an elastic.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="punch">The punch strength</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="vibrato">Indicates how much will the punch vibrate per second</param>
        /// <param name="elasticity">Represents how much (0 to 1) the vector will go beyond the starting size when bouncing backwards.
        /// 1 creates a full oscillation between the punch rotation and the opposite rotation,
        /// while 0 oscillates only between the punch rotation and the start rotation</param>
        public Tweener DOPunchCharRotation(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            if (!ValidateChar(charIndex)) return null;
            if (duration <= 0) {
                if (Debugger.logPriority > 0) Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
                return null;
            }
            return DOTween.Punch(() => _charTransforms[charIndex].rotation.eulerAngles, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, charT.offset, Quaternion.Euler(x), charT.scale, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, punch, duration, vibrato, elasticity);
        }

        /// <summary>Punches a character's scale towards the given direction and then back to the starting one
        /// as if it was connected to the starting position via an elastic.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="punch">The punch strength (added to the character's current scale)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="vibrato">Indicates how much will the punch vibrate per second</param>
        /// <param name="elasticity">Represents how much (0 to 1) the vector will go beyond the starting size when bouncing backwards.
        /// 1 creates a full oscillation between the punch scale and the opposite scale,
        /// while 0 oscillates only between the punch scale and the start scale</param>
        public Tweener DOPunchCharScale(int charIndex, float punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            return DOPunchCharScale(charIndex, new Vector3(punch, punch, punch), duration, vibrato, elasticity);
        }
        /// <summary>Punches a character's scale towards the given direction and then back to the starting one
        /// as if it was connected to the starting position via an elastic.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="punch">The punch strength (added to the character's current scale)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="vibrato">Indicates how much will the punch vibrate per second</param>
        /// <param name="elasticity">Represents how much (0 to 1) the vector will go beyond the starting size when bouncing backwards.
        /// 1 creates a full oscillation between the punch scale and the opposite scale,
        /// while 0 oscillates only between the punch scale and the start scale</param>
        public Tweener DOPunchCharScale(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1)
        {
            if (!ValidateChar(charIndex)) return null;
            if (duration <= 0) {
                if (Debugger.logPriority > 0) Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
                return null;
            }
            return DOTween.Punch(() => _charTransforms[charIndex].scale, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, charT.offset, charT.rotation, x, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, punch, duration, vibrato, elasticity);
        }

        /// <summary>Shakes a character's offset with the given values.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        public Tweener DOShakeCharOffset(int charIndex, float duration, float strength, int vibrato = 10, float randomness = 90, bool fadeOut = true)
        {
            return DOShakeCharOffset(charIndex, duration, new Vector3(strength, strength, strength), vibrato, randomness, fadeOut);
        }
        /// <summary>Shakes a character's offset with the given values.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        public Tweener DOShakeCharOffset(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true)
        {
            if (!ValidateChar(charIndex)) return null;
            if (duration <= 0) {
                if (Debugger.logPriority > 0) Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
                return null;
            }
            return DOTween.Shake(() => _charTransforms[charIndex].offset, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, x, charT.rotation, charT.scale, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, duration, strength, vibrato, randomness, fadeOut);
        }

        /// <summary>Shakes a character's rotation with the given values.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        public Tweener DOShakeCharRotation(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true)
        {
            if (!ValidateChar(charIndex)) return null;
            if (duration <= 0) {
                if (Debugger.logPriority > 0) Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
                return null;
            }
            return DOTween.Shake(() => _charTransforms[charIndex].rotation.eulerAngles, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, charT.offset, Quaternion.Euler(x), charT.scale, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, duration, strength, vibrato, randomness, fadeOut);
        }

        /// <summary>Shakes a character's scale with the given values.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        public Tweener DOShakeCharScale(int charIndex, float duration, float strength, int vibrato = 10, float randomness = 90, bool fadeOut = true)
        {
            return DOShakeCharScale(charIndex, duration, new Vector3(strength, strength, strength), vibrato, randomness, fadeOut);
        }
        /// <summary>Shakes a character's scale with the given values.</summary>
        /// <param name="charIndex">The index of the character to tween (will throw an error if it doesn't exist)</param>
        /// <param name="duration">The duration of the tween</param>
        /// <param name="strength">The shake strength</param>
        /// <param name="vibrato">Indicates how much will the shake vibrate</param>
        /// <param name="randomness">Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). 
        /// Setting it to 0 will shake along a single direction.</param>
        /// <param name="fadeOut">If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not</param>
        public Tweener DOShakeCharScale(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true)
        {
            if (!ValidateChar(charIndex)) return null;
            if (duration <= 0) {
                if (Debugger.logPriority > 0) Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
                return null;
            }
            return DOTween.Shake(() => _charTransforms[charIndex].scale, x => {
                CharTransform charT = _charTransforms[charIndex];
                charT.UpdateGeometry(target, charT.offset, charT.rotation, x, _cachedMeshInfos);
                _charTransforms[charIndex] = charT;
            }, duration, strength, vibrato, randomness, fadeOut);
        }

        #endregion

        // ███ INTERNAL CLASSES ████████████████████████████████████████████████████████████████████████████████████████████████

        struct CharVertices
        {
            public Vector3 bottomLeft, topLeft, topRight, bottomRight;

            public CharVertices(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
            {
                this.bottomLeft = bottomLeft;
                this.topLeft = topLeft;
                this.topRight = topRight;
                this.bottomRight = bottomRight;
            }
        }

        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

        // Vertices of each character are:
        // 0 : bottom left, 1 : top left, 2 : top right, 3 : bottom right
        struct CharTransform
        {
            public int charIndex;
            public bool isVisible { get; private set; } // FALSE both if it's invisible or if it's a space
            public Vector3 offset;
            public Quaternion rotation;
            public Vector3 scale;
            Vector3 _topLeftShift, _topRightShift, _bottomLeftShift, _bottomRightShift;
            Vector3 _charMidBaselineOffset;
            int _matIndex, _firstVertexIndex;
            TMP_MeshInfo _meshInfo;

            public CharTransform(int charIndex, TMP_TextInfo textInfo, TMP_MeshInfo[] cachedMeshInfos) : this()
            {
                this.charIndex = charIndex;
                offset = Vector3.zero;
                rotation = Quaternion.identity;
                scale = Vector3.one;
                Refresh(textInfo, cachedMeshInfos);
            }

            public void Refresh(TMP_TextInfo textInfo, TMP_MeshInfo[] cachedMeshInfos)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
                bool isSpaceChar = charInfo.character == ' ';
                isVisible = charInfo.isVisible && !isSpaceChar;
                _matIndex = charInfo.materialReferenceIndex;
                _firstVertexIndex = charInfo.vertexIndex;
                _meshInfo = textInfo.meshInfo[_matIndex];
                Vector3[] cachedVertices = cachedMeshInfos[_matIndex].vertices;
                _charMidBaselineOffset = isSpaceChar
                    ? Vector3.zero
                    : (cachedVertices[_firstVertexIndex] + cachedVertices[_firstVertexIndex + 2]) * 0.5f;
            }

            public void ResetAll(TMP_Text target, TMP_MeshInfo[] meshInfos, TMP_MeshInfo[] cachedMeshInfos)
            {
                ResetGeometry(target, cachedMeshInfos);
                ResetColors(target, meshInfos);
            }

            public void ResetTransformationData()
            {
                offset = Vector3.zero;
                rotation = Quaternion.identity;
                scale = Vector3.one;
                _topLeftShift = _topRightShift = _bottomLeftShift = _bottomRightShift = Vector3.zero;
            }

            public void ResetGeometry(TMP_Text target, TMP_MeshInfo[] cachedMeshInfos)
            {
                ResetTransformationData();
                Vector3[] destinationVertices = _meshInfo.vertices;
                Vector3[] cachedVertices = cachedMeshInfos[_matIndex].vertices;
                destinationVertices[_firstVertexIndex + 0] = cachedVertices[_firstVertexIndex + 0];
                destinationVertices[_firstVertexIndex + 1] = cachedVertices[_firstVertexIndex + 1];
                destinationVertices[_firstVertexIndex + 2] = cachedVertices[_firstVertexIndex + 2];
                destinationVertices[_firstVertexIndex + 3] = cachedVertices[_firstVertexIndex + 3];
                _meshInfo.mesh.vertices = _meshInfo.vertices;
                target.UpdateGeometry(_meshInfo.mesh, _matIndex);
            }

            public void ResetColors(TMP_Text target, TMP_MeshInfo[] meshInfos)
            {
                Color color = target.color;
                Color32[] vertexCols = meshInfos[_matIndex].colors32;
                vertexCols[_firstVertexIndex] = color;
                vertexCols[_firstVertexIndex + 1] = color;
                vertexCols[_firstVertexIndex + 2] = color;
                vertexCols[_firstVertexIndex + 3] = color;
                target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }

            public Color32 GetColor(TMP_MeshInfo[] meshInfos)
            {
                return meshInfos[_matIndex].colors32[_firstVertexIndex];
            }

            public CharVertices GetVertices()
            {
                return new CharVertices(
                    _meshInfo.vertices[_firstVertexIndex], _meshInfo.vertices[_firstVertexIndex + 1],
                    _meshInfo.vertices[_firstVertexIndex + 2], _meshInfo.vertices[_firstVertexIndex + 3]
                );
            }

            public void UpdateAlpha(TMP_Text target, Color alphaColor, TMP_MeshInfo[] meshInfos, bool apply = true)
            {
                byte alphaByte = (byte)(alphaColor.a * 255);
                Color32[] vertexCols = meshInfos[_matIndex].colors32;
                vertexCols[_firstVertexIndex].a = alphaByte;
                vertexCols[_firstVertexIndex + 1].a = alphaByte;
                vertexCols[_firstVertexIndex + 2].a = alphaByte;
                vertexCols[_firstVertexIndex + 3].a = alphaByte;
                if (apply) target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }

            public void UpdateColor(TMP_Text target, Color32 color, TMP_MeshInfo[] meshInfos, bool apply = true)
            {
                Color32[] vertexCols = meshInfos[_matIndex].colors32;
                vertexCols[_firstVertexIndex] = color;
                vertexCols[_firstVertexIndex + 1] = color;
                vertexCols[_firstVertexIndex + 2] = color;
                vertexCols[_firstVertexIndex + 3] = color;
                if (apply) target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }

            public void UpdateGeometry(TMP_Text target, Vector3 offset, Quaternion rotation, Vector3 scale, TMP_MeshInfo[] cachedMeshInfos, bool apply = true)
            {
                this.offset = offset;
                this.rotation = rotation;
                this.scale = scale;

                if (!apply) return;

                Vector3[] destinationVertices = _meshInfo.vertices;
                Vector3[] cachedVertices = cachedMeshInfos[_matIndex].vertices;
                destinationVertices[_firstVertexIndex] = cachedVertices[_firstVertexIndex + 0] - _charMidBaselineOffset;
                destinationVertices[_firstVertexIndex + 1] = cachedVertices[_firstVertexIndex + 1] - _charMidBaselineOffset;
                destinationVertices[_firstVertexIndex + 2] = cachedVertices[_firstVertexIndex + 2] - _charMidBaselineOffset;
                destinationVertices[_firstVertexIndex + 3] = cachedVertices[_firstVertexIndex + 3] - _charMidBaselineOffset;
                Matrix4x4 matrix = Matrix4x4.TRS(this.offset, this.rotation, this.scale);
                destinationVertices[_firstVertexIndex]
                    = matrix.MultiplyPoint3x4(destinationVertices[_firstVertexIndex + 0]) + _charMidBaselineOffset + _bottomLeftShift;
                destinationVertices[_firstVertexIndex + 1]
                    = matrix.MultiplyPoint3x4(destinationVertices[_firstVertexIndex + 1]) + _charMidBaselineOffset + _topLeftShift;
                destinationVertices[_firstVertexIndex + 2]
                    = matrix.MultiplyPoint3x4(destinationVertices[_firstVertexIndex + 2]) + _charMidBaselineOffset + _topRightShift;
                destinationVertices[_firstVertexIndex + 3]
                    = matrix.MultiplyPoint3x4(destinationVertices[_firstVertexIndex + 3]) + _charMidBaselineOffset + _bottomRightShift;
                _meshInfo.mesh.vertices = _meshInfo.vertices;
                target.UpdateGeometry(_meshInfo.mesh, _matIndex);
            }

            public void ShiftVertices(TMP_Text target, Vector3 topLeftShift, Vector3 topRightShift, Vector3 bottomLeftShift, Vector3 bottomRightShift)
            {
                _topLeftShift += topLeftShift;
                _topRightShift += topRightShift;
                _bottomLeftShift += bottomLeftShift;
                _bottomRightShift += bottomRightShift;
                Vector3[] destinationVertices = _meshInfo.vertices;
                destinationVertices[_firstVertexIndex] = destinationVertices[_firstVertexIndex] + _bottomLeftShift;
                destinationVertices[_firstVertexIndex + 1] = destinationVertices[_firstVertexIndex + 1] + _topLeftShift;
                destinationVertices[_firstVertexIndex + 2] = destinationVertices[_firstVertexIndex + 2] + _topRightShift;
                destinationVertices[_firstVertexIndex + 3] = destinationVertices[_firstVertexIndex + 3] + _bottomRightShift;
                _meshInfo.mesh.vertices = _meshInfo.vertices;
                target.UpdateGeometry(_meshInfo.mesh, _matIndex);
            }

            public void ResetVerticesShift(TMP_Text target)
            {
                Vector3[] destinationVertices = _meshInfo.vertices;
                destinationVertices[_firstVertexIndex] = destinationVertices[_firstVertexIndex] - _bottomLeftShift;
                destinationVertices[_firstVertexIndex + 1] = destinationVertices[_firstVertexIndex + 1] - _topLeftShift;
                destinationVertices[_firstVertexIndex + 2] = destinationVertices[_firstVertexIndex + 2] - _topRightShift;
                destinationVertices[_firstVertexIndex + 3] = destinationVertices[_firstVertexIndex + 3] - _bottomRightShift;
                _meshInfo.mesh.vertices = _meshInfo.vertices;
                target.UpdateGeometry(_meshInfo.mesh, _matIndex);
                _topLeftShift = _topRightShift = _bottomLeftShift = _bottomRightShift = Vector3.zero;
            }
        }
    }

    #endregion
}
#endif
