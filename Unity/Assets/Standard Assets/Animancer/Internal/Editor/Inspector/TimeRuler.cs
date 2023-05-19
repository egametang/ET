// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws a GUI box denoting a period of time.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TimeRuler
    /// 
    public sealed class TimeRuler
    {
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        private static readonly ConversionCache<float, string>
            G2Cache = new ConversionCache<float, string>((value) =>
            {
                if (Math.Abs(value) <= 99)
                    return value.ToString("G2");
                else
                    return ((int)value).ToString();
            });

        private static Texture _EventIcon;

        /// <summary>The icon used for events.</summary>
        public static Texture EventIcon => _EventIcon != null ?
            _EventIcon :
            (_EventIcon = EditorGUIUtility.IconContent("Animation.EventMarker").image);

        private static readonly Color
            FadeHighlightColor = new Color(0.35f, 0.5f, 1, 0.5f),
            SelectedEventColor = new Color(0.3f, 0.55f, 0.95f),
            UnselectedEventColor = new Color(0.9f, 0.9f, 0.9f),
            PreviewTimeColor = new Color(1, 0.25f, 0.1f),
            BaseTimeColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);

        private Rect _Area;
        private float _Speed, _Duration, _MinTime, _MaxTime, _StartTime, _FadeInEnd, _FadeOutEnd, _EndTime, _SecondsToPixels;
        private bool _HasEndTime;

        private readonly List<float>
            EventTimes = new List<float>();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Conversions
        /************************************************************************************************************************/

        /// <summary>Converts a number of seconds to a horizontal pixel position along the ruler.</summary>
        public float SecondsToPixels(float seconds) => (seconds - _MinTime) * _SecondsToPixels;

        /// <summary>Converts a horizontal pixel position along the ruler to a number of seconds.</summary>
        public float PixelsToSeconds(float pixels) => (pixels / _SecondsToPixels) + _MinTime;

        /// <summary>Converts a number of seconds to a normalized time value.</summary>
        public float SecondsToNormalized(float seconds) => seconds / _Duration;

        /// <summary>Converts a normalized time value to a number of seconds.</summary>
        public float NormalizedToSeconds(float normalized) => normalized * _Duration;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>
        /// Sets the `area` in which the ruler will be drawn and draws a <see cref="GUI.Box(Rect, string)"/> there.
        /// Must be followed by a call to <see cref="EndGUI"/>.
        /// </summary>
        public void BeginGUI(Rect area)
        {
            area = EditorGUI.IndentedRect(area);

            GUI.Box(area, "");

#if !UNITY_2019_3_OR_NEWER
            const float Padding = 1;
            area.x += Padding;
            area.y += Padding;
            area.width -= Padding * 2;
            area.height -= Padding * 2;
#endif

            GUI.BeginClip(area);

            area.x = area.y = 0;
            _Area = area;
        }

        /// <summary>
        /// Uses any unused <see cref="EventType.MouseDown"/> events in the area and ends the area started by
        /// <see cref="BeginGUI"/>.
        /// </summary>
        public void EndGUI()
        {
            GUI.EndClip();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the ruler GUI and handles input events for the specified `context`.
        /// </summary>
        public void DoGUI(Rect area, EventSequenceDrawer.Context context, out float addEventNormalizedTime)
        {
            BeginGUI(area);

            if (context.Property.hasMultipleDifferentValues)
            {
                GUI.Label(_Area, "Multi-editing is not supported");
                addEventNormalizedTime = float.NaN;
            }
            else
            {
                var transition = context.TransitionContext.Transition;

                _Speed = transition.Speed;
                var playDirection = _Speed < 0 ? -1 : 1;

                _Duration = context.TransitionContext.MaximumDuration;
                if (_Duration <= 0)
                    _Duration = 1;

                GatherEventTimes(context);

                _StartTime = transition.NormalizedStartTime;
                if (float.IsNaN(_StartTime))
                {
                    _StartTime = _Speed < 0 ? _Duration : 0;
                }
                else
                {
                    _StartTime *= _Duration;
                }

                _FadeInEnd = _StartTime + transition.FadeDuration * playDirection;

                _FadeOutEnd = GetFadeOutEnd(_Speed, _EndTime, _Duration);

                _MinTime = Mathf.Min(0, _StartTime);
                _MinTime = Mathf.Min(_MinTime, _FadeOutEnd);
                _MinTime = Mathf.Min(_MinTime, EventTimes[0]);

                _MaxTime = Mathf.Max(_StartTime, _FadeOutEnd);
                if (EventTimes.Count >= 2)
                    _MaxTime = Mathf.Max(_MaxTime, EventTimes[EventTimes.Count - 2]);

                if (_MaxTime < _Duration)
                    _MaxTime = _Duration;

                _SecondsToPixels = _Area.width / (_MaxTime - _MinTime);

                DoFadeHighlightGUI();
                DoEventsGUI(context, out addEventNormalizedTime);
                DoRulerGUI();

                if (_Speed > 0)
                {
                    if (_StartTime > _EndTime)
                        GUI.Label(_Area, "Start Time is after End Time");
                }
                else if (_Speed < 0)
                {
                    if (_StartTime < _EndTime)
                        GUI.Label(_Area, "Start Time is before End Time");
                }
            }

            EndGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the end time of the fade out.</summary>
        public static float GetFadeOutEnd(float speed, float endTime, float duration)
        {
            if (speed < 0)
                return endTime > 0 ? 0 : endTime - AnimancerPlayable.DefaultFadeDuration;
            else
                return endTime < duration ? duration : endTime + AnimancerPlayable.DefaultFadeDuration;
        }

        /************************************************************************************************************************/

        private static readonly Vector3[] QuadVerts = new Vector3[4];

        /// <summary>
        /// Draws a polygon describing the start, end, and fade details.
        /// </summary>
        public void DoFadeHighlightGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var color = Handles.color;
            Handles.color = FadeHighlightColor;
            QuadVerts[0] = new Vector3(SecondsToPixels(_StartTime), _Area.y);
            QuadVerts[1] = new Vector3(SecondsToPixels(_FadeInEnd), _Area.yMax + 1);
            QuadVerts[2] = new Vector3(SecondsToPixels(_FadeOutEnd), _Area.yMax + 1);
            QuadVerts[3] = new Vector3(SecondsToPixels(_EndTime), _Area.y);
            Handles.DrawAAConvexPolygon(QuadVerts);
            Handles.color = color;
        }

        /************************************************************************************************************************/
        #region Events
        /************************************************************************************************************************/

        private void GatherEventTimes(EventSequenceDrawer.Context context)
        {
            EventTimes.Clear();

            if (context.Times.Count > 0)
            {
                var depth = context.Times.Property.depth;
                var time = context.Times.GetElement(0);

                while (time.depth > depth)
                {
                    EventTimes.Add(time.floatValue * _Duration);
                    time.Next(false);
                }

                _EndTime = EventTimes[EventTimes.Count - 1];
                if (!float.IsNaN(_EndTime))
                {
                    _HasEndTime = true;
                    return;
                }
            }

            _EndTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(_Speed) * _Duration;
            _HasEndTime = false;
            if (EventTimes.Count == 0)
                EventTimes.Add(_EndTime);
            else
                EventTimes[EventTimes.Count - 1] = _EndTime;
        }

        /************************************************************************************************************************/

        private static readonly int EventHash = "Event".GetHashCode();
        private static readonly List<int> EventControlIDs = new List<int>();

        /// <summary>
        /// Draws the details of the <see cref="EventSequenceDrawer.Context.Callbacks"/>.
        /// </summary>
        public void DoEventsGUI(EventSequenceDrawer.Context context, out float addEventNormalizedTime)
        {
            addEventNormalizedTime = float.NaN;
            var currentGUIEvent = Event.current;

            EventControlIDs.Clear();
            var selectedEventControlID = -1;

            var baseControlID = GUIUtility.GetControlID(EventHash - 1, FocusType.Passive);

            for (int i = 0; i < EventTimes.Count; i++)
            {
                var controlID = GUIUtility.GetControlID(EventHash + i, FocusType.Keyboard);
                EventControlIDs.Add(controlID);
                if (context.SelectedEvent == i)
                    selectedEventControlID = controlID;
            }

            EventControlIDs.Add(baseControlID);

            switch (currentGUIEvent.type)
            {
                case EventType.Repaint:
                    RepaintEventsGUI(context);
                    break;

                case EventType.MouseDown:
                    if (_Area.Contains(currentGUIEvent.mousePosition))
                        OnMouseDown(currentGUIEvent, context, out addEventNormalizedTime);
                    break;

                case EventType.MouseUp:
                    if (currentGUIEvent.button == 1 &&
                        _Area.Contains(currentGUIEvent.mousePosition))
                    {
                        ShowContextMenu(currentGUIEvent, context);
                        currentGUIEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (_Duration <= 0)
                        break;

                    var hotControl = GUIUtility.hotControl;
                    if (hotControl == baseControlID)
                    {
                        SetPreviewTime(currentGUIEvent);
                        GUIUtility.ExitGUI();
                    }
                    else
                    {
                        for (int i = 0; i < EventTimes.Count; i++)
                        {
                            if (hotControl == EventControlIDs[i])
                            {
                                if (context.Times.Count < 1)
                                    context.Times.Count = 1;
                                var timeProperty = context.Times.GetElement(i);
                                var seconds = PixelsToSeconds(currentGUIEvent.mousePosition.x);
                                timeProperty.floatValue = seconds / _Duration;
                                timeProperty.serializedObject.ApplyModifiedProperties();
                                GUIUtility.hotControl = EventControlIDs[context.SelectedEvent];
                                GUI.changed = true;

                                SetPreviewTime(currentGUIEvent);
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                    break;

                case EventType.KeyUp:
                    if (GUIUtility.keyboardControl != selectedEventControlID)
                        break;

                    var exitGUI = false;

                    switch (currentGUIEvent.keyCode)
                    {
                        case KeyCode.Delete:
                        case KeyCode.Backspace:
                            EventSequenceDrawer.RemoveEvent(context, context.SelectedEvent);
                            exitGUI = true;
                            break;

                        case KeyCode.LeftArrow: NudgeEventTime(context, Event.current.shift ? -10 : -1); break;
                        case KeyCode.RightArrow: NudgeEventTime(context, Event.current.shift ? 10 : 1); break;

                        case KeyCode.Space: RoundEventTime(context); break;

                        default: return;// Don't call Use.
                    }

                    currentGUIEvent.Use();
                    GUI.changed = true;

                    if (exitGUI)
                        GUIUtility.ExitGUI();
                    break;
            }
        }

        /************************************************************************************************************************/

        private void RepaintEventsGUI(EventSequenceDrawer.Context context)
        {
            var color = GUI.color;

            for (int i = 0; i < EventTimes.Count; i++)
            {
                var currentColor = color;
                // Read Only: currentColor *= new Color(0.9f, 0.9f, 0.9f, 0.5f * alpha);
                if (context.SelectedEvent == i)
                {
                    currentColor *= SelectedEventColor;
                }
                else
                {
                    currentColor *= UnselectedEventColor;
                }

                if (i == EventTimes.Count - 1 && !_HasEndTime)
                    currentColor.a *= 0.65f;

                GUI.color = currentColor;

                var area = GetEventIconArea(i);
                GUI.DrawTexture(area, EventIcon);
            }

            GUI.color = color;
        }

        /************************************************************************************************************************/

        private void OnMouseDown(Event currentGUIEvent, EventSequenceDrawer.Context context, out float addEventNormalizedTime)
        {
            var selectedEventControlID = 0;
            var selectedEvent = -1;

            for (int i = 0; i < EventControlIDs.Count; i++)
            {
                var area = i < EventTimes.Count ? GetEventIconArea(i) : _Area;

                if (area.Contains(currentGUIEvent.mousePosition))
                {
                    selectedEventControlID = EventControlIDs[i];
                    selectedEvent = i;
                    break;
                }
            }

            GUIUtility.hotControl = GUIUtility.keyboardControl = selectedEventControlID;

            if (selectedEvent < 0 || selectedEvent >= EventTimes.Count)
            {
                SetPreviewTime(currentGUIEvent);
                selectedEvent = -1;
            }

            if (currentGUIEvent.type == EventType.MouseDown &&
                currentGUIEvent.clickCount == 2)
            {
                addEventNormalizedTime = PixelsToSeconds(currentGUIEvent.mousePosition.x);
                addEventNormalizedTime = SecondsToNormalized(addEventNormalizedTime);
            }
            else
            {
                addEventNormalizedTime = float.NaN;
            }

            context.SelectedEvent = selectedEvent;
            currentGUIEvent.Use();
        }

        /************************************************************************************************************************/

        private void ShowContextMenu(Event currentGUIEvent, EventSequenceDrawer.Context context)
        {
            context = context.Copy();
            var time = SecondsToNormalized(PixelsToSeconds(currentGUIEvent.mousePosition.x));
            var hasSelectedEvent = context.SelectedEvent >= 0;

            var menu = new GenericMenu();

            AddContextFunction(menu, context, "Add Event (Double Click)", true,
                () => EventSequenceDrawer.AddEvent(context, time));

            AddContextFunction(menu, context, "Remove Event (Delete)", hasSelectedEvent,
                () => EventSequenceDrawer.RemoveEvent(context, context.SelectedEvent));

            const string NudgePrefix = "Nudge Event Time/";
            AddContextFunction(menu, context, NudgePrefix + "Left 1 Pixel (Left Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, -1));
            AddContextFunction(menu, context, NudgePrefix + "Left 10 Pixels (Shift + Left Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, -10));
            AddContextFunction(menu, context, NudgePrefix + "Right 1 Pixel (Right Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, 1));
            AddContextFunction(menu, context, NudgePrefix + "Right 10 Pixels (Shift + Right Arrow)", hasSelectedEvent,
                () => NudgeEventTime(context, 10));

            var canRoundTime = hasSelectedEvent;
            if (canRoundTime)
            {
                time = context.Times.GetElement(context.SelectedEvent).floatValue;
                canRoundTime = TryRoundValue(ref time);
            }

            AddContextFunction(menu, context, $"Round Event Time to {time}x (Space)", canRoundTime,
                () => RoundEventTime(context));

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        private static void AddContextFunction(
            GenericMenu menu, EventSequenceDrawer.Context context, string label, bool enabled, Action function)
        {
            menu.AddFunction(label, enabled, () =>
            {
                using (context.SetAsCurrent())
                {
                    function();
                    GUI.changed = true;
                }
            });
        }

        /************************************************************************************************************************/

        private void SetPreviewTime(Event currentGUIEvent)
        {
            if (_Duration > 0)
            {
                var seconds = PixelsToSeconds(currentGUIEvent.mousePosition.x);
                TransitionPreviewWindow.PreviewNormalizedTime = seconds / _Duration;
            }
        }

        /************************************************************************************************************************/

        private Rect GetEventIconArea(int index)
        {
            var width = EventIcon.width;

            var x = SecondsToPixels(EventTimes[index]) - width * 0.5f;
            x = Mathf.Clamp(x, 0, _Area.width - width);

            return new Rect(x, _Area.y, width, EventIcon.height);
        }

        /************************************************************************************************************************/

        private void NudgeEventTime(EventSequenceDrawer.Context context, float offsetPixels)
        {
            var time = context.Times.GetElement(context.SelectedEvent);

            var value = time.floatValue;
            value = NormalizedToSeconds(value);
            value = SecondsToPixels(value);

            value += offsetPixels;

            value = PixelsToSeconds(value);
            value = SecondsToNormalized(value);
            time.floatValue = value;
        }

        /************************************************************************************************************************/

        private void RoundEventTime(EventSequenceDrawer.Context context)
        {
            var time = context.Times.GetElement(context.SelectedEvent);
            var value = time.floatValue;

            if (TryRoundValue(ref value))
                time.floatValue = value;
        }

        private static bool TryRoundValue(ref float value)
        {
            var format = System.Globalization.NumberFormatInfo.InvariantInfo;
            var text = value.ToString(format);
            var dot = text.IndexOf('.');
            if (dot < 0)
                return false;

            Round:
            var newValue = (float)Math.Round(value, text.Length - dot - 2, MidpointRounding.AwayFromZero);
            if (newValue == value)
            {
                dot--;
                if (dot > 0)
                    goto Round;
            }

            if (value != newValue)
            {
                value = newValue;
                return true;
            }
            else return false;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Ticks
        /************************************************************************************************************************/

        private const float TickHeight = 0.3f;

        private static readonly List<float> TickTimes = new List<float>();

        /// <summary>
        /// Draws ticks and labels for important times throughout the area.
        /// </summary>
        public void DoRulerGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var tickHeight = Mathf.Ceil(_Area.height * TickHeight);
            var area = new Rect(SecondsToPixels(0), _Area.yMax - tickHeight, 0, tickHeight)
            {
                xMax = SecondsToPixels(_Duration)
            };
#if !UNITY_2019_1_OR_NEWER
            area.height++;
#endif
            EditorGUI.DrawRect(area, BaseTimeColor);

            TickTimes.Clear();
            TickTimes.Add(0);
            TickTimes.Add(_StartTime);
            TickTimes.Add(_FadeInEnd);
            TickTimes.Add(_Duration);
            TickTimes.AddRange(EventTimes);
            TickTimes.Sort();

            var previousTime = float.NaN;
            area.x = float.NegativeInfinity;

            for (int i = 0; i < TickTimes.Count; i++)
            {
                var time = TickTimes[i];
                if (previousTime != time)
                {
                    previousTime = time;
                    DoRulerLabelGUI(ref area, time);
                }
            }

            var state = TransitionPreviewWindow.GetCurrentState();
            if (state != null)
            {
                var normalizedTime = TransitionPreviewWindow.PreviewNormalizedTime;
                DrawPreviewTime(normalizedTime, 1);

                if (state.IsLooping)
                {
                    if ((int)SecondsToPixels(0) <= (int)SecondsToPixels(_Duration) + 2)
                        return;

                    while (normalizedTime * _Duration >= _MinTime + _Duration)
                        normalizedTime -= 1;

                    while (normalizedTime * _Duration <= _MaxTime)
                    {
                        DrawPreviewTime(normalizedTime, 0.25f);
                        normalizedTime += 1;
                    }
                }
            }
        }

        /************************************************************************************************************************/

        private void DrawPreviewTime(float normalizedTime, float alpha)
        {
            var time = NormalizedToSeconds(normalizedTime);
            var x = SecondsToPixels(time);
            if (x >= 0 && x <= _Area.width)
            {
                var color = PreviewTimeColor;
                color.a = alpha;
                EditorGUI.DrawRect(new Rect(x - 1, _Area.y, 2, _Area.height), color);
            }
        }

        /************************************************************************************************************************/

        private static ConversionCache<string, float> _TimeLabelWidthCache;

        private void DoRulerLabelGUI(ref Rect previousArea, float time)
        {
            var style = ObjectPool.GetCachedResult(() => new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(),
                contentOffset = new Vector2(0, -2),
                alignment = TextAnchor.UpperLeft,
                fontSize = Mathf.CeilToInt(AnimancerGUI.LineHeight * 0.6f),
            });

            var text = G2Cache.Convert(time);

            if (_TimeLabelWidthCache == null)
                _TimeLabelWidthCache = AnimancerGUI.CreateWidthCache(style);

            var area = new Rect(
                SecondsToPixels(time),
                _Area.y,
                _TimeLabelWidthCache.Convert(text),
                _Area.height);

            if (area.x > _Area.x)
            {
                var tickHeight = Mathf.Ceil(_Area.height * TickHeight);
                var tickY = _Area.yMax - tickHeight;

                EditorGUI.DrawRect(new Rect(area.x, tickY, 1, tickHeight), AnimancerGUI.TextColor);
            }

            if (area.xMax > _Area.xMax)
                area.x = _Area.xMax - area.width;
            if (area.x < 0)
                area.x = 0;

            if (area.x > previousArea.xMax + 2)
            {
                GUI.Label(area, text, style);

                previousArea = area;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

