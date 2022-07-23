// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/07/19 14:11
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php
// 
// =============================================================
// Contains Daniele Giardini's C# port of the easing equations created by Robert Penner
// (all easing equations except for Flash, InFlash, OutFlash, InOutFlash,
// which use some parts of Robert Penner's equations but were created by Daniele Giardini)
// http://robertpenner.com/easing, see license below:
// =============================================================
//
// TERMS OF USE - EASING EQUATIONS
//
// Open source under the BSD License.
//
// Copyright ? 2001 Robert Penner
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// - Neither the name of the author nor the names of contributors may be used to endorse
// or promote products derived from this software without specific prior written permission.
// - THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using UnityEngine;

#pragma warning disable 1591
namespace FairyGUI
{
    public static class EaseManager
    {
        const float _PiOver2 = Mathf.PI * 0.5f;
        const float _TwoPi = Mathf.PI * 2;

        /// <summary>
        /// Returns a value between 0 and 1 (inclusive) based on the elapsed time and ease selected
        /// </summary>
        public static float Evaluate(EaseType easeType, float time, float duration,
            float overshootOrAmplitude = 1.70158f,
            float period = 0,
            CustomEase customEase = null)
        {
            if (duration <= 0)
                return 1;

            switch (easeType)
            {
                case EaseType.Linear:
                    return time / duration;
                case EaseType.SineIn:
                    return -(float)Math.Cos(time / duration * _PiOver2) + 1;
                case EaseType.SineOut:
                    return (float)Math.Sin(time / duration * _PiOver2);
                case EaseType.SineInOut:
                    return -0.5f * ((float)Math.Cos(Mathf.PI * time / duration) - 1);
                case EaseType.QuadIn:
                    return (time /= duration) * time;
                case EaseType.QuadOut:
                    return -(time /= duration) * (time - 2);
                case EaseType.QuadInOut:
                    if ((time /= duration * 0.5f) < 1) return 0.5f * time * time;
                    return -0.5f * ((--time) * (time - 2) - 1);
                case EaseType.CubicIn:
                    return (time /= duration) * time * time;
                case EaseType.CubicOut:
                    return ((time = time / duration - 1) * time * time + 1);
                case EaseType.CubicInOut:
                    if ((time /= duration * 0.5f) < 1) return 0.5f * time * time * time;
                    return 0.5f * ((time -= 2) * time * time + 2);
                case EaseType.QuartIn:
                    return (time /= duration) * time * time * time;
                case EaseType.QuartOut:
                    return -((time = time / duration - 1) * time * time * time - 1);
                case EaseType.QuartInOut:
                    if ((time /= duration * 0.5f) < 1) return 0.5f * time * time * time * time;
                    return -0.5f * ((time -= 2) * time * time * time - 2);
                case EaseType.QuintIn:
                    return (time /= duration) * time * time * time * time;
                case EaseType.QuintOut:
                    return ((time = time / duration - 1) * time * time * time * time + 1);
                case EaseType.QuintInOut:
                    if ((time /= duration * 0.5f) < 1) return 0.5f * time * time * time * time * time;
                    return 0.5f * ((time -= 2) * time * time * time * time + 2);
                case EaseType.ExpoIn:
                    return (time == 0) ? 0 : (float)Math.Pow(2, 10 * (time / duration - 1));
                case EaseType.ExpoOut:
                    if (time == duration) return 1;
                    return (-(float)Math.Pow(2, -10 * time / duration) + 1);
                case EaseType.ExpoInOut:
                    if (time == 0) return 0;
                    if (time == duration) return 1;
                    if ((time /= duration * 0.5f) < 1) return 0.5f * (float)Math.Pow(2, 10 * (time - 1));
                    return 0.5f * (-(float)Math.Pow(2, -10 * --time) + 2);
                case EaseType.CircIn:
                    return -((float)Math.Sqrt(1 - (time /= duration) * time) - 1);
                case EaseType.CircOut:
                    return (float)Math.Sqrt(1 - (time = time / duration - 1) * time);
                case EaseType.CircInOut:
                    if ((time /= duration * 0.5f) < 1) return -0.5f * ((float)Math.Sqrt(1 - time * time) - 1);
                    return 0.5f * ((float)Math.Sqrt(1 - (time -= 2) * time) + 1);
                case EaseType.ElasticIn:
                    float s0;
                    if (time == 0) return 0;
                    if ((time /= duration) == 1) return 1;
                    if (period == 0) period = duration * 0.3f;
                    if (overshootOrAmplitude < 1)
                    {
                        overshootOrAmplitude = 1;
                        s0 = period / 4;
                    }
                    else s0 = period / _TwoPi * (float)Math.Asin(1 / overshootOrAmplitude);
                    return -(overshootOrAmplitude * (float)Math.Pow(2, 10 * (time -= 1)) * (float)Math.Sin((time * duration - s0) * _TwoPi / period));
                case EaseType.ElasticOut:
                    float s1;
                    if (time == 0) return 0;
                    if ((time /= duration) == 1) return 1;
                    if (period == 0) period = duration * 0.3f;
                    if (overshootOrAmplitude < 1)
                    {
                        overshootOrAmplitude = 1;
                        s1 = period / 4;
                    }
                    else s1 = period / _TwoPi * (float)Math.Asin(1 / overshootOrAmplitude);
                    return (overshootOrAmplitude * (float)Math.Pow(2, -10 * time) * (float)Math.Sin((time * duration - s1) * _TwoPi / period) + 1);
                case EaseType.ElasticInOut:
                    float s;
                    if (time == 0) return 0;
                    if ((time /= duration * 0.5f) == 2) return 1;
                    if (period == 0) period = duration * (0.3f * 1.5f);
                    if (overshootOrAmplitude < 1)
                    {
                        overshootOrAmplitude = 1;
                        s = period / 4;
                    }
                    else s = period / _TwoPi * (float)Math.Asin(1 / overshootOrAmplitude);
                    if (time < 1) return -0.5f * (overshootOrAmplitude * (float)Math.Pow(2, 10 * (time -= 1)) * (float)Math.Sin((time * duration - s) * _TwoPi / period));
                    return overshootOrAmplitude * (float)Math.Pow(2, -10 * (time -= 1)) * (float)Math.Sin((time * duration - s) * _TwoPi / period) * 0.5f + 1;
                case EaseType.BackIn:
                    return (time /= duration) * time * ((overshootOrAmplitude + 1) * time - overshootOrAmplitude);
                case EaseType.BackOut:
                    return ((time = time / duration - 1) * time * ((overshootOrAmplitude + 1) * time + overshootOrAmplitude) + 1);
                case EaseType.BackInOut:
                    if ((time /= duration * 0.5f) < 1) return 0.5f * (time * time * (((overshootOrAmplitude *= (1.525f)) + 1) * time - overshootOrAmplitude));
                    return 0.5f * ((time -= 2) * time * (((overshootOrAmplitude *= (1.525f)) + 1) * time + overshootOrAmplitude) + 2);
                case EaseType.BounceIn:
                    return Bounce.EaseIn(time, duration);
                case EaseType.BounceOut:
                    return Bounce.EaseOut(time, duration);
                case EaseType.BounceInOut:
                    return Bounce.EaseInOut(time, duration);

                case EaseType.Custom:
                    return customEase != null ? customEase.Evaluate(time / duration) : (time / duration);

                default:
                    return -(time /= duration) * (time - 2);
            }
        }

        /// <summary>
        /// This class contains a C# port of the easing equations created by Robert Penner (http://robertpenner.com/easing).
        /// </summary>
        static class Bounce
        {
            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
            /// </summary>
            /// <param name="time">
            /// Current time (in frames or seconds).
            /// </param>
            /// <param name="duration">
            /// Expected easing duration (in frames or seconds).
            /// </param>
            /// <returns>
            /// The eased value.
            /// </returns>
            public static float EaseIn(float time, float duration)
            {
                return 1 - EaseOut(duration - time, duration);
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
            /// </summary>
            /// <param name="time">
            /// Current time (in frames or seconds).
            /// </param>
            /// <param name="duration">
            /// Expected easing duration (in frames or seconds).
            /// </param>
            /// <returns>
            /// The eased value.
            /// </returns>
            public static float EaseOut(float time, float duration)
            {
                if ((time /= duration) < (1 / 2.75f))
                {
                    return (7.5625f * time * time);
                }
                if (time < (2 / 2.75f))
                {
                    return (7.5625f * (time -= (1.5f / 2.75f)) * time + 0.75f);
                }
                if (time < (2.5f / 2.75f))
                {
                    return (7.5625f * (time -= (2.25f / 2.75f)) * time + 0.9375f);
                }
                return (7.5625f * (time -= (2.625f / 2.75f)) * time + 0.984375f);
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="time">
            /// Current time (in frames or seconds).
            /// </param>
            /// <param name="duration">
            /// Expected easing duration (in frames or seconds).
            /// </param>
            /// <returns>
            /// The eased value.
            /// </returns>
            public static float EaseInOut(float time, float duration)
            {
                if (time < duration * 0.5f)
                {
                    return EaseIn(time * 2, duration) * 0.5f;
                }
                return EaseOut(time * 2 - duration, duration) * 0.5f + 0.5f;
            }
        }
    }
}