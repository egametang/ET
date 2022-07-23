using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    internal static class TweenManager
    {
        static GTweener[] _activeTweens = new GTweener[30];
        static List<GTweener> _tweenerPool = new List<GTweener>(30);
        static int _totalActiveTweens = 0;
        static bool _inited = false;

        internal static GTweener CreateTween()
        {
            if (!_inited)
                Init();

            GTweener tweener;
            int cnt = _tweenerPool.Count;
            if (cnt > 0)
            {
                tweener = _tweenerPool[cnt - 1];
                _tweenerPool.RemoveAt(cnt - 1);
            }
            else
                tweener = new GTweener();
            tweener._Init();
            _activeTweens[_totalActiveTweens++] = tweener;

            if (_totalActiveTweens == _activeTweens.Length)
            {
                GTweener[] newArray = new GTweener[_activeTweens.Length + Mathf.CeilToInt(_activeTweens.Length * 0.5f)];
                _activeTweens.CopyTo(newArray, 0);
                _activeTweens = newArray;
            }

            return tweener;
        }

        internal static bool IsTweening(object target, TweenPropType propType)
        {
            if (target == null)
                return false;

            bool anyType = propType == TweenPropType.None;
            for (int i = 0; i < _totalActiveTweens; i++)
            {
                GTweener tweener = _activeTweens[i];
                if (tweener != null && tweener.target == target && !tweener._killed
                    && (anyType || tweener._propType == propType))
                    return true;
            }

            return false;
        }

        internal static bool KillTweens(object target, TweenPropType propType, bool completed)
        {
            if (target == null)
                return false;

            bool flag = false;
            int cnt = _totalActiveTweens;
            bool anyType = propType == TweenPropType.None;
            for (int i = 0; i < cnt; i++)
            {
                GTweener tweener = _activeTweens[i];
                if (tweener != null && tweener.target == target && !tweener._killed
                    && (anyType || tweener._propType == propType))
                {
                    tweener.Kill(completed);
                    flag = true;
                }
            }

            return flag;
        }

        internal static GTweener GetTween(object target, TweenPropType propType)
        {
            if (target == null)
                return null;

            int cnt = _totalActiveTweens;
            bool anyType = propType == TweenPropType.None;
            for (int i = 0; i < cnt; i++)
            {
                GTweener tweener = _activeTweens[i];
                if (tweener != null && tweener.target == target && !tweener._killed
                    && (anyType || tweener._propType == propType))
                {
                    return tweener;
                }
            }

            return null;
        }

        internal static void Update()
        {
            int cnt = _totalActiveTweens;
            int freePosStart = -1;
            for (int i = 0; i < cnt; i++)
            {
                GTweener tweener = _activeTweens[i];
                if (tweener == null)
                {
                    if (freePosStart == -1)
                        freePosStart = i;
                }
                else if (tweener._killed)
                {
                    tweener._Reset();
                    _tweenerPool.Add(tweener);
                    _activeTweens[i] = null;

                    if (freePosStart == -1)
                        freePosStart = i;
                }
                else
                {
                    if ((tweener._target is GObject) && ((GObject)tweener._target)._disposed)
                        tweener._killed = true;
                    else if (!tweener._paused)
                        tweener._Update();

                    if (freePosStart != -1)
                    {
                        _activeTweens[freePosStart] = tweener;
                        _activeTweens[i] = null;
                        freePosStart++;
                    }
                }
            }

            if (freePosStart >= 0)
            {
                if (_totalActiveTweens != cnt) //new tweens added
                {
                    int j = cnt;
                    cnt = _totalActiveTweens - cnt;
                    for (int i = 0; i < cnt; i++)
                    {
                        _activeTweens[freePosStart++] = _activeTweens[j];
                        _activeTweens[j] = null;
                        j++;
                    }
                }
                _totalActiveTweens = freePosStart;
            }
        }

        internal static void Clean()
        {
            _tweenerPool.Clear();
        }

        static void Init()
        {
            _inited = true;
            if (Application.isPlaying)
            {
                GameObject gameObject = new GameObject("[FairyGUI.TweenManager]");
                gameObject.hideFlags = HideFlags.HideInHierarchy;
                gameObject.SetActive(true);
                Object.DontDestroyOnLoad(gameObject);

                gameObject.AddComponent<TweenEngine>();
            }
        }

        class TweenEngine : MonoBehaviour
        {
            void Update()
            {
                TweenManager.Update();
            }
        }
    }
}
