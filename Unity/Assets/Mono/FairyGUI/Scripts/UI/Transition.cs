using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void PlayCompleteCallback();

    /// <summary>
    /// 
    /// </summary>
    public delegate void TransitionHook();

    /// <summary>
    /// 
    /// </summary>
    public class Transition : ITweenListener
    {
        /// <summary>
        /// 动效的名称。在编辑器里设定。
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// 当你启动了自动合批，动效里有涉及到XY、大小、旋转等的改变，如果你观察到元件的显示深度在播放过程中有错误，可以开启这个选项。
        /// </summary>
        public bool invalidateBatchingEveryFrame;

        GComponent _owner;
        TransitionItem[] _items;
        int _totalTimes;
        int _totalTasks;
        bool _playing;
        bool _paused;
        float _ownerBaseX;
        float _ownerBaseY;
        PlayCompleteCallback _onComplete;
        int _options;
        bool _reversed;
        float _totalDuration;
        bool _autoPlay;
        int _autoPlayTimes;
        float _autoPlayDelay;
        float _timeScale;
        bool _ignoreEngineTimeScale;
        float _startTime;
        float _endTime;
        GTweenCallback _delayedCallDelegate;
        GTweenCallback _checkAllDelegate;
        GTweenCallback1 _delayedCallDelegate2;

        const int OPTION_IGNORE_DISPLAY_CONTROLLER = 1;
        const int OPTION_AUTO_STOP_DISABLED = 2;
        const int OPTION_AUTO_STOP_AT_END = 4;

        public Transition(GComponent owner)
        {
            _owner = owner;
            _timeScale = 1;
            _ignoreEngineTimeScale = true;

            _delayedCallDelegate = OnDelayedPlay;
            _delayedCallDelegate2 = OnDelayedPlayItem;
            _checkAllDelegate = CheckAllComplete;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Play()
        {
            _Play(1, 0, 0, -1, null, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onComplete"></param>
        public void Play(PlayCompleteCallback onComplete)
        {
            _Play(1, 0, 0, -1, onComplete, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        /// <param name="delay"></param>
        /// <param name="onComplete"></param>
        public void Play(int times, float delay, PlayCompleteCallback onComplete)
        {
            _Play(times, delay, 0, -1, onComplete, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        /// <param name="delay"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="onComplete"></param>
        public void Play(int times, float delay, float startTime, float endTime, PlayCompleteCallback onComplete)
        {
            _Play(times, delay, startTime, endTime, onComplete, false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayReverse()
        {
            _Play(1, 0, 0, -1, null, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onComplete"></param>
        public void PlayReverse(PlayCompleteCallback onComplete)
        {
            _Play(1, 0, 0, -1, onComplete, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        /// <param name="delay"></param>
        /// <param name="onComplete"></param>
        public void PlayReverse(int times, float delay, PlayCompleteCallback onComplete)
        {
            _Play(times, delay, 0, -1, onComplete, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void ChangePlayTimes(int value)
        {
            _totalTimes = value;
        }

        /// <summary>
        /// 设置动效是否自动播放。
        /// </summary>
        /// <param name="autoPlay"></param>
        /// <param name="times"></param>
        /// <param name="delay"></param>
        public void SetAutoPlay(bool autoPlay, int times, float delay)
        {
            if (_autoPlay != autoPlay)
            {
                _autoPlay = autoPlay;
                _autoPlayTimes = times;
                _autoPlayDelay = delay;
                if (_autoPlay)
                {
                    if (_owner.onStage)
                        Play(times, delay, null);
                }
                else
                {
                    if (!_owner.onStage)
                        Stop(false, true);
                }
            }
        }

        void _Play(int times, float delay, float startTime, float endTime, PlayCompleteCallback onComplete, bool reverse)
        {
            Stop(true, true);

            _totalTimes = times;
            _reversed = reverse;
            _startTime = startTime;
            _endTime = endTime;
            _playing = true;
            _paused = false;
            _onComplete = onComplete;

            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.target == null)
                {
                    if (item.targetId.Length > 0)
                        item.target = _owner.GetChildById(item.targetId);
                    else
                        item.target = _owner;
                }
                else if (item.target != _owner && item.target.parent != _owner) //maybe removed
                    item.target = null;

                if (item.target != null && item.type == TransitionActionType.Transition)
                {
                    TValue_Transition value = (TValue_Transition)item.value;
                    Transition trans = ((GComponent)item.target).GetTransition(value.transName);
                    if (trans == this)
                        trans = null;
                    if (trans != null)
                    {
                        if (value.playTimes == 0) //stop
                        {
                            int j;
                            for (j = i - 1; j >= 0; j--)
                            {
                                TransitionItem item2 = _items[j];
                                if (item2.type == TransitionActionType.Transition)
                                {
                                    TValue_Transition value2 = (TValue_Transition)item2.value;
                                    if (value2.trans == trans)
                                    {
                                        value2.stopTime = item.time - item2.time;
                                        break;
                                    }
                                }
                            }
                            if (j < 0)
                                value.stopTime = 0;
                            else
                                trans = null; //no need to handle stop anymore
                        }
                        else
                            value.stopTime = -1;
                    }
                    value.trans = trans;
                }
            }

            if (delay == 0)
                OnDelayedPlay();
            else
                GTween.DelayedCall(delay).SetTarget(this).OnComplete(_delayedCallDelegate);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            Stop(true, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setToComplete"></param>
        /// <param name="processCallback"></param>
        public void Stop(bool setToComplete, bool processCallback)
        {
            if (!_playing)
                return;

            _playing = false;
            _totalTasks = 0;
            _totalTimes = 0;
            PlayCompleteCallback func = _onComplete;
            _onComplete = null;

            GTween.Kill(this);//delay start

            int cnt = _items.Length;
            if (_reversed)
            {
                for (int i = cnt - 1; i >= 0; i--)
                {
                    TransitionItem item = _items[i];
                    if (item.target == null)
                        continue;

                    StopItem(item, setToComplete);
                }
            }
            else
            {
                for (int i = 0; i < cnt; i++)
                {
                    TransitionItem item = _items[i];
                    if (item.target == null)
                        continue;

                    StopItem(item, setToComplete);
                }
            }

            if (processCallback && func != null)
                func();

        }

        void StopItem(TransitionItem item, bool setToComplete)
        {
            if (item.displayLockToken != 0)
            {
                item.target.ReleaseDisplayLock(item.displayLockToken);
                item.displayLockToken = 0;
            }

            if (item.tweener != null)
            {
                item.tweener.Kill(setToComplete);
                item.tweener = null;

                if (item.type == TransitionActionType.Shake && !setToComplete) //震动必须归位，否则下次就越震越远了。
                {
                    item.target._gearLocked = true;
                    item.target.SetXY(item.target.x - ((TValue_Shake)item.value).lastOffset.x, item.target.y - ((TValue_Shake)item.value).lastOffset.y);
                    item.target._gearLocked = false;

                    _owner.InvalidateBatchingState(true);
                }
            }

            if (item.type == TransitionActionType.Transition)
            {
                TValue_Transition value = (TValue_Transition)item.value;
                if (value.trans != null)
                    value.trans.Stop(setToComplete, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paused"></param>
        public void SetPaused(bool paused)
        {
            if (!_playing || _paused == paused)
                return;

            _paused = paused;
            GTweener tweener = GTween.GetTween(this);
            if (tweener != null)
                tweener.SetPaused(paused);

            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.target == null)
                    continue;

                if (item.type == TransitionActionType.Transition)
                {
                    if (((TValue_Transition)item.value).trans != null)
                        ((TValue_Transition)item.value).trans.SetPaused(paused);
                }
                else if (item.type == TransitionActionType.Animation)
                {
                    if (paused)
                    {
                        ((TValue_Animation)item.value).flag = ((IAnimationGear)item.target).playing;
                        ((IAnimationGear)item.target).playing = false;
                    }
                    else
                        ((IAnimationGear)item.target).playing = ((TValue_Animation)item.value).flag;
                }

                if (item.tweener != null)
                    item.tweener.SetPaused(paused);
            }
        }

        public void Dispose()
        {
            if (_playing)
                GTween.Kill(this);//delay start

            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.tweener != null)
                {
                    item.tweener.Kill();
                    item.tweener = null;
                }

                item.target = null;
                item.hook = null;
                if (item.tweenConfig != null)
                    item.tweenConfig.endHook = null;
            }

            _playing = false;
            _onComplete = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool playing
        {
            get { return _playing; }
        }

        /// <summary>
        /// transition's total duration, maybe zero when the transition only has one frame
        /// </summary>
        public float totalDuration
        {
            get { return _totalDuration; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="aParams"></param>
        public void SetValue(string label, params object[] aParams)
        {
            int cnt = _items.Length;
            object value;
            bool found = false;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.label == label)
                {
                    if (item.tweenConfig != null)
                        value = item.tweenConfig.startValue;
                    else
                        value = item.value;
                    found = true;
                }
                else if (item.tweenConfig != null && item.tweenConfig.endLabel == label)
                {
                    value = item.tweenConfig.endValue;
                    found = true;
                }
                else
                    continue;

                switch (item.type)
                {
                    case TransitionActionType.XY:
                    case TransitionActionType.Size:
                    case TransitionActionType.Pivot:
                    case TransitionActionType.Scale:
                    case TransitionActionType.Skew:
                        {
                            TValue tvalue = (TValue)value;
                            tvalue.b1 = true;
                            tvalue.b2 = true;
                            tvalue.f1 = Convert.ToSingle(aParams[0]);
                            tvalue.f2 = Convert.ToSingle(aParams[1]);
                        }
                        break;

                    case TransitionActionType.Alpha:
                        ((TValue)value).f1 = Convert.ToSingle(aParams[0]);
                        break;

                    case TransitionActionType.Rotation:
                        ((TValue)value).f1 = Convert.ToSingle(aParams[0]);
                        break;

                    case TransitionActionType.Color:
                        ((TValue)value).color = (Color)aParams[0];
                        break;

                    case TransitionActionType.Animation:
                        {
                            TValue_Animation tvalue = (TValue_Animation)value;
                            tvalue.frame = Convert.ToInt32(aParams[0]);
                            if (aParams.Length > 1)
                                tvalue.playing = Convert.ToBoolean(aParams[1]);
                            if (aParams.Length > 2)
                                tvalue.animationName = (string)aParams[2];
                            if (aParams.Length > 3)
                                tvalue.skinName = (string)aParams[3];
                        }
                        break;

                    case TransitionActionType.Visible:
                        ((TValue_Visible)value).visible = Convert.ToBoolean(aParams[0]);
                        break;

                    case TransitionActionType.Sound:
                        {
                            TValue_Sound tvalue = (TValue_Sound)value;
                            tvalue.sound = (string)aParams[0];
                            if (aParams.Length > 1)
                                tvalue.volume = Convert.ToSingle(aParams[1]);
                        }
                        break;

                    case TransitionActionType.Transition:
                        {
                            TValue_Transition tvalue = (TValue_Transition)value;
                            tvalue.transName = (string)aParams[0];
                            if (aParams.Length > 1)
                                tvalue.playTimes = Convert.ToInt32(aParams[1]);
                        }
                        break;

                    case TransitionActionType.Shake:
                        {
                            ((TValue_Shake)value).amplitude = Convert.ToSingle(aParams[0]);
                            if (aParams.Length > 1)
                                ((TValue_Shake)value).duration = Convert.ToSingle(aParams[1]);
                        }
                        break;

                    case TransitionActionType.ColorFilter:
                        {
                            TValue tvalue = (TValue)value;
                            tvalue.f1 = Convert.ToSingle(aParams[0]);
                            tvalue.f2 = Convert.ToSingle(aParams[1]);
                            tvalue.f3 = Convert.ToSingle(aParams[2]);
                            tvalue.f4 = Convert.ToSingle(aParams[3]);
                        }
                        break;

                    case TransitionActionType.Text:
                    case TransitionActionType.Icon:
                        ((TValue_Text)value).text = (string)aParams[0];
                        break;
                }
            }

            if (!found)
                throw new Exception("label not exists");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="callback"></param>
        public void SetHook(string label, TransitionHook callback)
        {
            int cnt = _items.Length;
            bool found = false;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.label == label)
                {
                    item.hook = callback;
                    found = true;
                    break;
                }
                else if (item.tweenConfig != null && item.tweenConfig.endLabel == label)
                {
                    item.tweenConfig.endHook = callback;
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new Exception("label not exists");
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearHooks()
        {
            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                item.hook = null;
                if (item.tweenConfig != null)
                    item.tweenConfig.endHook = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="newTarget"></param>
        public void SetTarget(string label, GObject newTarget)
        {
            int cnt = _items.Length;
            bool found = false;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.label == label)
                {
                    item.targetId = (newTarget == _owner || newTarget == null) ? string.Empty : newTarget.id;
                    if (_playing)
                    {
                        if (item.targetId.Length > 0)
                            item.target = _owner.GetChildById(item.targetId);
                        else
                            item.target = _owner;
                    }
                    else
                        item.target = null;
                    found = true;
                }
            }
            if (!found)
                throw new Exception("label not exists");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        public void SetDuration(string label, float value)
        {
            int cnt = _items.Length;
            bool found = false;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.tweenConfig != null && item.label == label)
                {
                    item.tweenConfig.duration = value;
                    found = true;
                }
            }

            if (!found)
                throw new Exception("label not exists or not a tween label");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public float GetLabelTime(string label)
        {
            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.label == label)
                    return item.time;
                else if (item.tweenConfig != null && item.tweenConfig.endLabel == label)
                    return item.time + item.tweenConfig.duration;
            }

            return float.NaN;
        }

        /// <summary>
        /// 
        /// </summary>
        public float timeScale
        {
            get { return _timeScale; }
            set
            {
                if (_timeScale != value)
                {
                    _timeScale = value;

                    int cnt = _items.Length;
                    for (int i = 0; i < cnt; i++)
                    {
                        TransitionItem item = _items[i];
                        if (item.tweener != null)
                            item.tweener.SetTimeScale(value);
                        else if (item.type == TransitionActionType.Transition)
                        {
                            if (((TValue_Transition)item.value).trans != null)
                                ((TValue_Transition)item.value).trans.timeScale = value;
                        }
                        else if (item.type == TransitionActionType.Animation)
                        {
                            if (item.target != null)
                                ((IAnimationGear)item.target).timeScale = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ignoreEngineTimeScale
        {
            get { return _ignoreEngineTimeScale; }
            set
            {
                if (_ignoreEngineTimeScale != value)
                {
                    _ignoreEngineTimeScale = value;

                    int cnt = _items.Length;
                    for (int i = 0; i < cnt; i++)
                    {
                        TransitionItem item = _items[i];
                        if (item.tweener != null)
                            item.tweener.SetIgnoreEngineTimeScale(value);
                        else if (item.type == TransitionActionType.Transition)
                        {
                            if (((TValue_Transition)item.value).trans != null)
                                ((TValue_Transition)item.value).trans.ignoreEngineTimeScale = value;
                        }
                        else if (item.type == TransitionActionType.Animation)
                        {
                            if (item.target != null)
                                ((IAnimationGear)item.target).ignoreEngineTimeScale = value;
                        }
                    }
                }
            }
        }

        internal void UpdateFromRelations(string targetId, float dx, float dy)
        {
            int cnt = _items.Length;
            if (cnt == 0)
                return;

            for (int i = 0; i < cnt; i++)
            {
                TransitionItem item = _items[i];
                if (item.type == TransitionActionType.XY && item.targetId == targetId)
                {
                    if (item.tweenConfig != null)
                    {
                        if (!item.tweenConfig.startValue.b3)
                        {
                            item.tweenConfig.startValue.f1 += dx;
                            item.tweenConfig.startValue.f2 += dy;
                        }
                        if (!item.tweenConfig.endValue.b3)
                        {
                            item.tweenConfig.endValue.f1 += dx;
                            item.tweenConfig.endValue.f2 += dy;
                        }
                    }
                    else
                    {
                        if (!((TValue)item.value).b3)
                        {
                            ((TValue)item.value).f1 += dx;
                            ((TValue)item.value).f2 += dy;
                        }
                    }
                }
            }
        }

        internal void OnOwnerAddedToStage()
        {
            if (_autoPlay && !_playing)
                Play(_autoPlayTimes, _autoPlayDelay, null);
        }

        internal void OnOwnerRemovedFromStage()
        {
            if ((_options & OPTION_AUTO_STOP_DISABLED) == 0)
                Stop((_options & OPTION_AUTO_STOP_AT_END) != 0 ? true : false, false);
        }

        void OnDelayedPlay()
        {
            InternalPlay();

            _playing = _totalTasks > 0;
            if (_playing)
            {
                if ((_options & OPTION_IGNORE_DISPLAY_CONTROLLER) != 0)
                {
                    int cnt = _items.Length;
                    for (int i = 0; i < cnt; i++)
                    {
                        TransitionItem item = _items[i];
                        if (item.target != null && item.target != _owner)
                            item.displayLockToken = item.target.AddDisplayLock();
                    }
                }
            }
            else if (_onComplete != null)
            {
                PlayCompleteCallback func = _onComplete;
                _onComplete = null;
                func();
            }
        }

        void InternalPlay()
        {
            _ownerBaseX = _owner.x;
            _ownerBaseY = _owner.y;

            _totalTasks = 1; //prevent to complete inside the loop

            bool needSkipAnimations = false;
            int cnt = _items.Length;
            if (!_reversed)
            {
                for (int i = 0; i < cnt; i++)
                {
                    TransitionItem item = _items[i];
                    if (item.target == null)
                        continue;

                    if (item.type == TransitionActionType.Animation && _startTime != 0 && item.time <= _startTime)
                    {
                        needSkipAnimations = true;
                        ((TValue_Animation)item.value).flag = false;
                    }
                    else
                        PlayItem(item);
                }
            }
            else
            {
                for (int i = cnt - 1; i >= 0; i--)
                {
                    TransitionItem item = _items[i];
                    if (item.target == null)
                        continue;

                    PlayItem(item);
                }
            }

            if (needSkipAnimations)
                SkipAnimations();

            _totalTasks--;
        }

        void PlayItem(TransitionItem item)
        {
            float time;
            if (item.tweenConfig != null)
            {
                if (_reversed)
                    time = (_totalDuration - item.time - item.tweenConfig.duration);
                else
                    time = item.time;

                if (_endTime == -1 || time <= _endTime)
                {
                    TValue startValue;
                    TValue endValue;

                    if (_reversed)
                    {
                        startValue = item.tweenConfig.endValue;
                        endValue = item.tweenConfig.startValue;
                    }
                    else
                    {
                        startValue = item.tweenConfig.startValue;
                        endValue = item.tweenConfig.endValue;
                    }

                    ((TValue)item.value).b1 = startValue.b1 || endValue.b1;
                    ((TValue)item.value).b2 = startValue.b2 || endValue.b2;

                    switch (item.type)
                    {
                        case TransitionActionType.XY:
                        case TransitionActionType.Size:
                        case TransitionActionType.Scale:
                        case TransitionActionType.Skew:
                            item.tweener = GTween.To(startValue.vec2, endValue.vec2, item.tweenConfig.duration);
                            break;

                        case TransitionActionType.Alpha:
                        case TransitionActionType.Rotation:
                            item.tweener = GTween.To(startValue.f1, endValue.f1, item.tweenConfig.duration);
                            break;

                        case TransitionActionType.Color:
                            item.tweener = GTween.To(startValue.color, endValue.color, item.tweenConfig.duration);
                            break;

                        case TransitionActionType.ColorFilter:
                            item.tweener = GTween.To(startValue.vec4, endValue.vec4, item.tweenConfig.duration);
                            break;
                    }

                    item.tweener.SetDelay(time)
                        .SetEase(item.tweenConfig.easeType, item.tweenConfig.customEase)
                        .SetRepeat(item.tweenConfig.repeat, item.tweenConfig.yoyo)
                        .SetTimeScale(_timeScale)
                        .SetIgnoreEngineTimeScale(_ignoreEngineTimeScale)
                        .SetTarget(item)
                        .SetListener(this);

                    if (_endTime >= 0)
                        item.tweener.SetBreakpoint(_endTime - time);

                    _totalTasks++;
                }
            }
            else if (item.type == TransitionActionType.Shake)
            {
                TValue_Shake value = (TValue_Shake)item.value;

                if (_reversed)
                    time = (_totalDuration - item.time - value.duration);
                else
                    time = item.time;

                if (_endTime == -1 || time <= _endTime)
                {
                    value.lastOffset.Set(0, 0);
                    value.offset.Set(0, 0);
                    item.tweener = GTween.Shake(Vector3.zero, value.amplitude, value.duration)
                        .SetDelay(time)
                        .SetTimeScale(_timeScale)
                        .SetIgnoreEngineTimeScale(_ignoreEngineTimeScale)
                        .SetTarget(item)
                        .SetListener(this);

                    if (_endTime >= 0)
                        item.tweener.SetBreakpoint(_endTime - item.time);

                    _totalTasks++;
                }
            }
            else
            {
                if (_reversed)
                    time = (_totalDuration - item.time);
                else
                    time = item.time;

                if (time <= _startTime)
                {
                    ApplyValue(item);
                    CallHook(item, false);
                }
                else if (_endTime == -1 || time <= _endTime)
                {
                    _totalTasks++;
                    item.tweener = GTween.DelayedCall(time)
                        .SetTimeScale(_timeScale)
                        .SetIgnoreEngineTimeScale(_ignoreEngineTimeScale)
                        .SetTarget(item)
                        .OnComplete(_delayedCallDelegate2);
                }
            }

            if (item.tweener != null)
                item.tweener.Seek(_startTime);
        }

        void SkipAnimations()
        {
            int frame;
            float playStartTime;
            float playTotalTime;
            TValue_Animation value;
            IAnimationGear target;
            TransitionItem item;

            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                item = _items[i];
                if (item.type != TransitionActionType.Animation || item.time > _startTime)
                    continue;

                value = (TValue_Animation)item.value;
                if (value.flag)
                    continue;

                target = (IAnimationGear)item.target;
                frame = target.frame;
                playStartTime = target.playing ? 0 : -1;
                playTotalTime = 0;

                for (int j = i; j < cnt; j++)
                {
                    item = _items[j];
                    if (item.type != TransitionActionType.Animation || item.target != target || item.time > _startTime)
                        continue;

                    value = (TValue_Animation)item.value;
                    value.flag = true;

                    if (value.frame != -1)
                    {
                        frame = value.frame;
                        if (value.playing)
                            playStartTime = item.time;
                        else
                            playStartTime = -1;
                        playTotalTime = 0;
                    }
                    else
                    {
                        if (value.playing)
                        {
                            if (playStartTime < 0)
                                playStartTime = item.time;
                        }
                        else
                        {
                            if (playStartTime >= 0)
                                playTotalTime += (item.time - playStartTime);
                            playStartTime = -1;
                        }
                    }

                    CallHook(item, false);
                }

                if (playStartTime >= 0)
                    playTotalTime += (_startTime - playStartTime);

                target.playing = playStartTime >= 0;
                target.frame = frame;
                if (playTotalTime > 0)
                    target.Advance(playTotalTime);
            }
        }

        void OnDelayedPlayItem(GTweener tweener)
        {
            TransitionItem item = (TransitionItem)tweener.target;
            item.tweener = null;
            _totalTasks--;

            ApplyValue(item);
            CallHook(item, false);

            CheckAllComplete();
        }

        public void OnTweenStart(GTweener tweener)
        {
            TransitionItem item = (TransitionItem)tweener.target;

            if (item.type == TransitionActionType.XY || item.type == TransitionActionType.Size) //位置和大小要到start才最终确认起始值
            {
                TValue startValue;
                TValue endValue;

                if (_reversed)
                {
                    startValue = item.tweenConfig.endValue;
                    endValue = item.tweenConfig.startValue;
                }
                else
                {
                    startValue = item.tweenConfig.startValue;
                    endValue = item.tweenConfig.endValue;
                }

                if (item.type == TransitionActionType.XY)
                {
                    if (item.target != _owner)
                    {
                        if (!startValue.b1)
                            tweener.startValue.x = item.target.x;
                        else if (startValue.b3) //percent
                            tweener.startValue.x = startValue.f1 * _owner.width;

                        if (!startValue.b2)
                            tweener.startValue.y = item.target.y;
                        else if (startValue.b3) //percent
                            tweener.startValue.y = startValue.f2 * _owner.height;

                        if (!endValue.b1)
                            tweener.endValue.x = tweener.startValue.x;
                        else if (endValue.b3)
                            tweener.endValue.x = endValue.f1 * _owner.width;

                        if (!endValue.b2)
                            tweener.endValue.y = tweener.startValue.y;
                        else if (endValue.b3)
                            tweener.endValue.y = endValue.f2 * _owner.height;
                    }
                    else
                    {
                        if (!startValue.b1)
                            tweener.startValue.x = item.target.x - _ownerBaseX;
                        if (!startValue.b2)
                            tweener.startValue.y = item.target.y - _ownerBaseY;

                        if (!endValue.b1)
                            tweener.endValue.x = tweener.startValue.x;
                        if (!endValue.b2)
                            tweener.endValue.y = tweener.startValue.y;
                    }
                }
                else
                {
                    if (!startValue.b1)
                        tweener.startValue.x = item.target.width;
                    if (!startValue.b2)
                        tweener.startValue.y = item.target.height;

                    if (!endValue.b1)
                        tweener.endValue.x = tweener.startValue.x;
                    if (!endValue.b2)
                        tweener.endValue.y = tweener.startValue.y;
                }

                if (item.tweenConfig.path != null)
                {
                    ((TValue)item.value).b1 = ((TValue)item.value).b2 = true;
                    tweener.SetPath(item.tweenConfig.path);
                }
            }

            CallHook(item, false);
        }

        public void OnTweenUpdate(GTweener tweener)
        {
            TransitionItem item = (TransitionItem)tweener.target;
            switch (item.type)
            {
                case TransitionActionType.XY:
                case TransitionActionType.Size:
                case TransitionActionType.Scale:
                case TransitionActionType.Skew:
                    ((TValue)item.value).vec2 = tweener.value.vec2;
                    if (item.tweenConfig.path != null)
                    {
                        ((TValue)item.value).f1 += tweener.startValue.x;
                        ((TValue)item.value).f2 += tweener.startValue.y;
                    }
                    break;

                case TransitionActionType.Alpha:
                case TransitionActionType.Rotation:
                    ((TValue)item.value).f1 = tweener.value.x;
                    break;

                case TransitionActionType.Color:
                    ((TValue)item.value).color = tweener.value.color;
                    break;

                case TransitionActionType.ColorFilter:
                    ((TValue)item.value).vec4 = tweener.value.vec4;
                    break;

                case TransitionActionType.Shake:
                    ((TValue_Shake)item.value).offset = tweener.deltaValue.vec2;
                    break;
            }
            ApplyValue(item);
        }

        public void OnTweenComplete(GTweener tweener)
        {
            TransitionItem item = (TransitionItem)tweener.target;
            item.tweener = null;
            _totalTasks--;

            if (tweener.allCompleted) //当整体播放结束时间在这个tween的中间时不应该调用结尾钩子
                CallHook(item, true);

            CheckAllComplete();
        }

        void OnPlayTransCompleted(TransitionItem item)
        {
            _totalTasks--;

            CheckAllComplete();
        }

        void CallHook(TransitionItem item, bool tweenEnd)
        {
            if (tweenEnd)
            {
                if (item.tweenConfig != null && item.tweenConfig.endHook != null)
                    item.tweenConfig.endHook();
            }
            else
            {
                if (item.time >= _startTime && item.hook != null)
                    item.hook();
            }
        }

        void CheckAllComplete()
        {
            if (_playing && _totalTasks == 0)
            {
                if (_totalTimes < 0)
                {
                    InternalPlay();
                    if (_totalTasks == 0)
                        GTween.DelayedCall(0).SetTarget(this).OnComplete(_checkAllDelegate);
                }
                else
                {
                    _totalTimes--;
                    if (_totalTimes > 0)
                    {
                        InternalPlay();
                        if (_totalTasks == 0)
                            GTween.DelayedCall(0).SetTarget(this).OnComplete(_checkAllDelegate);
                    }
                    else
                    {
                        _playing = false;

                        int cnt = _items.Length;
                        for (int i = 0; i < cnt; i++)
                        {
                            TransitionItem item = _items[i];
                            if (item.target != null && item.displayLockToken != 0)
                            {
                                item.target.ReleaseDisplayLock(item.displayLockToken);
                                item.displayLockToken = 0;
                            }
                        }

                        if (_onComplete != null)
                        {
                            PlayCompleteCallback func = _onComplete;
                            _onComplete = null;
                            func();
                        }
                    }
                }
            }
        }

        void ApplyValue(TransitionItem item)
        {
            item.target._gearLocked = true;

            switch (item.type)
            {
                case TransitionActionType.XY:
                    {
                        TValue value = (TValue)item.value;
                        if (item.target == _owner)
                        {
                            if (value.b1 && value.b2)
                                item.target.SetXY(value.f1 + _ownerBaseX, value.f2 + _ownerBaseY);
                            else if (value.b1)
                                item.target.x = value.f1 + _ownerBaseX;
                            else
                                item.target.y = value.f2 + _ownerBaseY;
                        }
                        else
                        {
                            if (value.b3) //position in percent
                            {
                                if (value.b1 && value.b2)
                                    item.target.SetXY(value.f1 * _owner.width, value.f2 * _owner.height);
                                else if (value.b1)
                                    item.target.x = value.f1 * _owner.width;
                                else if (value.b2)
                                    item.target.y = value.f2 * _owner.height;
                            }
                            else
                            {
                                if (value.b1 && value.b2)
                                    item.target.SetXY(value.f1, value.f2);
                                else if (value.b1)
                                    item.target.x = value.f1;
                                else if (value.b2)
                                    item.target.y = value.f2;
                            }
                        }
                        if (invalidateBatchingEveryFrame)
                            _owner.InvalidateBatchingState(true);
                    }
                    break;

                case TransitionActionType.Size:
                    {
                        TValue value = (TValue)item.value;
                        if (!value.b1)
                            value.f1 = item.target.width;
                        if (!value.b2)
                            value.f2 = item.target.height;
                        item.target.SetSize(value.f1, value.f2);
                        if (invalidateBatchingEveryFrame)
                            _owner.InvalidateBatchingState(true);
                    }
                    break;

                case TransitionActionType.Pivot:
                    item.target.SetPivot(((TValue)item.value).f1, ((TValue)item.value).f2, item.target.pivotAsAnchor);
                    if (invalidateBatchingEveryFrame)
                        _owner.InvalidateBatchingState(true);
                    break;

                case TransitionActionType.Alpha:
                    item.target.alpha = ((TValue)item.value).f1;
                    break;

                case TransitionActionType.Rotation:
                    item.target.rotation = ((TValue)item.value).f1;
                    if (invalidateBatchingEveryFrame)
                        _owner.InvalidateBatchingState(true);
                    break;

                case TransitionActionType.Scale:
                    item.target.SetScale(((TValue)item.value).f1, ((TValue)item.value).f2);
                    if (invalidateBatchingEveryFrame)
                        _owner.InvalidateBatchingState(true);
                    break;

                case TransitionActionType.Skew:
                    item.target.skew = ((TValue)item.value).vec2;
                    if (invalidateBatchingEveryFrame)
                        _owner.InvalidateBatchingState(true);
                    break;

                case TransitionActionType.Color:
                    ((IColorGear)item.target).color = ((TValue)item.value).color;
                    break;

                case TransitionActionType.Animation:
                    {
                        TValue_Animation value = (TValue_Animation)item.value;
                        if (value.frame >= 0)
                            ((IAnimationGear)item.target).frame = value.frame;
                        ((IAnimationGear)item.target).playing = value.playing;
                        ((IAnimationGear)item.target).timeScale = _timeScale;
                        ((IAnimationGear)item.target).ignoreEngineTimeScale = _ignoreEngineTimeScale;
                        if (value.animationName != null)
                            ((GLoader3D)item.target).animationName = value.animationName;
                        if (value.skinName != null)
                            ((GLoader3D)item.target).skinName = value.skinName;
                    }
                    break;

                case TransitionActionType.Visible:
                    item.target.visible = ((TValue_Visible)item.value).visible;
                    break;

                case TransitionActionType.Shake:
                    {
                        TValue_Shake value = (TValue_Shake)item.value;
                        item.target.SetXY(item.target.x - value.lastOffset.x + value.offset.x, item.target.y - value.lastOffset.y + value.offset.y);
                        value.lastOffset = value.offset;

                        if (invalidateBatchingEveryFrame)
                            _owner.InvalidateBatchingState(true);
                    }
                    break;

                case TransitionActionType.Transition:
                    if (_playing)
                    {
                        TValue_Transition value = (TValue_Transition)item.value;
                        if (value.trans != null)
                        {
                            _totalTasks++;

                            float startTime = _startTime > item.time ? (_startTime - item.time) : 0;
                            float endTime = _endTime >= 0 ? (_endTime - item.time) : -1;
                            if (value.stopTime >= 0 && (endTime < 0 || endTime > value.stopTime))
                                endTime = value.stopTime;
                            value.trans.timeScale = _timeScale;
                            value.trans.ignoreEngineTimeScale = _ignoreEngineTimeScale;
                            value.trans._Play(value.playTimes, 0, startTime, endTime, value.playCompleteDelegate, _reversed);
                        }
                    }
                    break;

                case TransitionActionType.Sound:
                    if (_playing && item.time >= _startTime)
                    {
                        TValue_Sound value = (TValue_Sound)item.value;
                        if (value.audioClip == null)
                        {
                            if (UIConfig.soundLoader == null || value.sound.StartsWith(UIPackage.URL_PREFIX))
                                value.audioClip = UIPackage.GetItemAssetByURL(value.sound) as NAudioClip;
                            else
                                value.audioClip = UIConfig.soundLoader(value.sound);
                        }

                        if (value.audioClip != null && value.audioClip.nativeClip != null)
                            Stage.inst.PlayOneShotSound(value.audioClip.nativeClip, value.volume);
                    }
                    break;

                case TransitionActionType.ColorFilter:
                    {
                        TValue value = (TValue)item.value;
                        ColorFilter cf = item.target.filter as ColorFilter;
                        if (cf == null)
                        {
                            cf = new ColorFilter();
                            item.target.filter = cf;
                        }
                        else
                            cf.Reset();

                        cf.AdjustBrightness(value.f1);
                        cf.AdjustContrast(value.f2);
                        cf.AdjustSaturation(value.f3);
                        cf.AdjustHue(value.f4);
                    }
                    break;

                case TransitionActionType.Text:
                    item.target.text = ((TValue_Text)item.value).text;
                    break;

                case TransitionActionType.Icon:
                    item.target.icon = ((TValue_Text)item.value).text;
                    break;
            }

            item.target._gearLocked = false;
        }

        public void Setup(ByteBuffer buffer)
        {
            this.name = buffer.ReadS();
            _options = buffer.ReadInt();
            _autoPlay = buffer.ReadBool();
            _autoPlayTimes = buffer.ReadInt();
            _autoPlayDelay = buffer.ReadFloat();

            int cnt = buffer.ReadShort();
            _items = new TransitionItem[cnt];
            for (int i = 0; i < cnt; i++)
            {
                int dataLen = buffer.ReadShort();
                int curPos = buffer.position;

                buffer.Seek(curPos, 0);

                TransitionItem item = new TransitionItem((TransitionActionType)buffer.ReadByte());
                _items[i] = item;

                item.time = buffer.ReadFloat();
                int targetId = buffer.ReadShort();
                if (targetId < 0)
                    item.targetId = string.Empty;
                else
                    item.targetId = _owner.GetChildAt(targetId).id;
                item.label = buffer.ReadS();

                if (buffer.ReadBool())
                {
                    buffer.Seek(curPos, 1);

                    item.tweenConfig = new TweenConfig();
                    item.tweenConfig.duration = buffer.ReadFloat();
                    if (item.time + item.tweenConfig.duration > _totalDuration)
                        _totalDuration = item.time + item.tweenConfig.duration;
                    item.tweenConfig.easeType = (EaseType)buffer.ReadByte();
                    item.tweenConfig.repeat = buffer.ReadInt();
                    item.tweenConfig.yoyo = buffer.ReadBool();
                    item.tweenConfig.endLabel = buffer.ReadS();

                    buffer.Seek(curPos, 2);

                    DecodeValue(item, buffer, item.tweenConfig.startValue);

                    buffer.Seek(curPos, 3);

                    DecodeValue(item, buffer, item.tweenConfig.endValue);

                    if (buffer.version >= 2)
                    {
                        var pts = buffer.ReadPath();
                        if (pts.Count > 0)
                        {
                            item.tweenConfig.path = new GPath();
                            item.tweenConfig.path.Create(pts);
                        }
                    }

                    if (buffer.version >= 4 && item.tweenConfig.easeType == EaseType.Custom)
                    {
                        var pts = buffer.ReadPath();
                        if (pts.Count > 0)
                        {
                            item.tweenConfig.customEase = new CustomEase();
                            item.tweenConfig.customEase.Create(pts);
                        }
                    }
                }
                else
                {
                    if (item.time > _totalDuration)
                        _totalDuration = item.time;

                    buffer.Seek(curPos, 2);

                    DecodeValue(item, buffer, item.value);
                }

                buffer.position = curPos + dataLen;
            }
        }

        void DecodeValue(TransitionItem item, ByteBuffer buffer, object value)
        {
            switch (item.type)
            {
                case TransitionActionType.XY:
                case TransitionActionType.Size:
                case TransitionActionType.Pivot:
                case TransitionActionType.Skew:
                    {
                        TValue tvalue = (TValue)value;
                        tvalue.b1 = buffer.ReadBool();
                        tvalue.b2 = buffer.ReadBool();
                        tvalue.f1 = buffer.ReadFloat();
                        tvalue.f2 = buffer.ReadFloat();

                        if (buffer.version >= 2 && item.type == TransitionActionType.XY)
                            tvalue.b3 = buffer.ReadBool(); //percent
                    }
                    break;

                case TransitionActionType.Alpha:
                case TransitionActionType.Rotation:
                    ((TValue)value).f1 = buffer.ReadFloat();
                    break;

                case TransitionActionType.Scale:
                    ((TValue)value).f1 = buffer.ReadFloat();
                    ((TValue)value).f2 = buffer.ReadFloat();
                    break;

                case TransitionActionType.Color:
                    ((TValue)value).color = buffer.ReadColor();
                    break;

                case TransitionActionType.Animation:
                    ((TValue_Animation)value).playing = buffer.ReadBool();
                    ((TValue_Animation)value).frame = buffer.ReadInt();
                    if (buffer.version >= 6)
                    {
                        ((TValue_Animation)value).animationName = buffer.ReadS();
                        ((TValue_Animation)value).skinName = buffer.ReadS();
                    }
                    break;

                case TransitionActionType.Visible:
                    ((TValue_Visible)value).visible = buffer.ReadBool();
                    break;

                case TransitionActionType.Sound:
                    ((TValue_Sound)value).sound = buffer.ReadS();
                    ((TValue_Sound)value).volume = buffer.ReadFloat();
                    break;

                case TransitionActionType.Transition:
                    ((TValue_Transition)value).transName = buffer.ReadS();
                    ((TValue_Transition)value).playTimes = buffer.ReadInt();
                    ((TValue_Transition)value).playCompleteDelegate = () => { OnPlayTransCompleted(item); };
                    break;

                case TransitionActionType.Shake:
                    ((TValue_Shake)value).amplitude = buffer.ReadFloat();
                    ((TValue_Shake)value).duration = buffer.ReadFloat();
                    break;

                case TransitionActionType.ColorFilter:
                    {
                        TValue tvalue = (TValue)value;
                        tvalue.f1 = buffer.ReadFloat();
                        tvalue.f2 = buffer.ReadFloat();
                        tvalue.f3 = buffer.ReadFloat();
                        tvalue.f4 = buffer.ReadFloat();
                    }
                    break;

                case TransitionActionType.Text:
                case TransitionActionType.Icon:
                    ((TValue_Text)value).text = buffer.ReadS();
                    break;
            }
        }
    }

    class TransitionItem
    {
        public float time;
        public string targetId;
        public TransitionActionType type;
        public TweenConfig tweenConfig;
        public string label;
        public object value;
        public TransitionHook hook;

        //running properties
        public GTweener tweener;
        public GObject target;
        public uint displayLockToken;

        public TransitionItem(TransitionActionType type)
        {
            this.type = type;

            switch (type)
            {
                case TransitionActionType.XY:
                case TransitionActionType.Size:
                case TransitionActionType.Scale:
                case TransitionActionType.Pivot:
                case TransitionActionType.Skew:
                case TransitionActionType.Alpha:
                case TransitionActionType.Rotation:
                case TransitionActionType.Color:
                case TransitionActionType.ColorFilter:
                    value = new TValue();
                    break;

                case TransitionActionType.Animation:
                    value = new TValue_Animation();
                    break;

                case TransitionActionType.Shake:
                    value = new TValue_Shake();
                    break;

                case TransitionActionType.Sound:
                    value = new TValue_Sound();
                    break;

                case TransitionActionType.Transition:
                    value = new TValue_Transition();
                    break;

                case TransitionActionType.Visible:
                    value = new TValue_Visible();
                    break;

                case TransitionActionType.Text:
                case TransitionActionType.Icon:
                    value = new TValue_Text();
                    break;
            }
        }
    }

    class TweenConfig
    {
        public float duration;
        public EaseType easeType;
        public CustomEase customEase;
        public int repeat;
        public bool yoyo;

        public TValue startValue;
        public TValue endValue;

        public string endLabel;
        public TransitionHook endHook;

        public GPath path;

        public TweenConfig()
        {
            easeType = EaseType.QuadOut;
            startValue = new TValue();
            endValue = new TValue();
        }
    }

    class TValue_Visible
    {
        public bool visible;
    }

    class TValue_Animation
    {
        public int frame;
        public bool playing;
        public bool flag;
        public string animationName;
        public string skinName;
    }

    class TValue_Sound
    {
        public string sound;
        public float volume;
        public NAudioClip audioClip;
    }

    class TValue_Transition
    {
        public string transName;
        public int playTimes;
        public Transition trans;
        public PlayCompleteCallback playCompleteDelegate;
        public float stopTime;
    }

    class TValue_Shake
    {
        public float amplitude;
        public float duration;
        public Vector2 lastOffset;
        public Vector2 offset;
    }

    class TValue_Text
    {
        public string text;
    }

    class TValue
    {
        public float f1;
        public float f2;
        public float f3;
        public float f4;
        public bool b1;
        public bool b2;
        public bool b3;

        public TValue()
        {
            b1 = true;
            b2 = true;
        }

        public void Copy(TValue source)
        {
            this.f1 = source.f1;
            this.f2 = source.f2;
            this.f3 = source.f3;
            this.f4 = source.f4;
            this.b1 = source.b1;
            this.b2 = source.b2;
        }

        public Vector2 vec2
        {
            get { return new Vector2(f1, f2); }
            set
            {
                f1 = value.x;
                f2 = value.y;
            }
        }

        public Vector4 vec4
        {
            get { return new Vector4(f1, f2, f3, f4); }
            set
            {
                f1 = value.x;
                f2 = value.y;
                f3 = value.z;
                f4 = value.w;
            }
        }

        public Color color
        {
            get { return new Color(f1, f2, f3, f4); }
            set
            {
                f1 = value.r;
                f2 = value.g;
                f3 = value.b;
                f4 = value.a;
            }
        }
    }
}
