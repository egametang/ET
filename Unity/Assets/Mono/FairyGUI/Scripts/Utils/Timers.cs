using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace FairyGUI
{
    public delegate void TimerCallback(object param);

    /// <summary>
    /// 
    /// </summary>
    public class Timers
    {
        public static int repeat;
        public static float time;

        public static bool catchCallbackExceptions = false;

        Dictionary<TimerCallback, Anymous_T> _items;
        Dictionary<TimerCallback, Anymous_T> _toAdd;
        List<Anymous_T> _toRemove;
        List<Anymous_T> _pool;

        TimersEngine _engine;
        GameObject gameObject;

        private static Timers _inst;
        public static Timers inst
        {
            get
            {
                if (_inst == null)
                    _inst = new Timers();
                return _inst;
            }
        }

        public Timers()
        {
            _inst = this;
            gameObject = new GameObject("[FairyGUI.Timers]");
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            gameObject.SetActive(true);
            Object.DontDestroyOnLoad(gameObject);

            _engine = gameObject.AddComponent<TimersEngine>();

            _items = new Dictionary<TimerCallback, Anymous_T>();
            _toAdd = new Dictionary<TimerCallback, Anymous_T>();
            _toRemove = new List<Anymous_T>();
            _pool = new List<Anymous_T>(100);
        }

        public void Add(float interval, int repeat, TimerCallback callback)
        {
            Add(interval, repeat, callback, null);
        }

        /**
         * @interval in seconds
         * @repeat 0 indicate loop infinitely, otherwise the run count
         **/
        public void Add(float interval, int repeat, TimerCallback callback, object callbackParam)
        {
            if (callback == null)
            {
                Debug.LogWarning("timer callback is null, " + interval + "," + repeat);
                return;
            }

            Anymous_T t;
            if (_items.TryGetValue(callback, out t))
            {
                t.set(interval, repeat, callback, callbackParam);
                t.elapsed = 0;
                t.deleted = false;
                return;
            }

            if (_toAdd.TryGetValue(callback, out t))
            {
                t.set(interval, repeat, callback, callbackParam);
                return;
            }

            t = GetFromPool();
            t.interval = interval;
            t.repeat = repeat;
            t.callback = callback;
            t.param = callbackParam;
            _toAdd[callback] = t;
        }

        public void CallLater(TimerCallback callback)
        {
            Add(0.001f, 1, callback);
        }

        public void CallLater(TimerCallback callback, object callbackParam)
        {
            Add(0.001f, 1, callback, callbackParam);
        }

        public void AddUpdate(TimerCallback callback)
        {
            Add(0.001f, 0, callback);
        }

        public void AddUpdate(TimerCallback callback, object callbackParam)
        {
            Add(0.001f, 0, callback, callbackParam);
        }

        public void StartCoroutine(IEnumerator routine)
        {
            _engine.StartCoroutine(routine);
        }

        public bool Exists(TimerCallback callback)
        {
            if (_toAdd.ContainsKey(callback))
                return true;

            Anymous_T at;
            if (_items.TryGetValue(callback, out at))
                return !at.deleted;

            return false;
        }

        public void Remove(TimerCallback callback)
        {
            Anymous_T t;
            if (_toAdd.TryGetValue(callback, out t))
            {
                _toAdd.Remove(callback);
                ReturnToPool(t);
            }

            if (_items.TryGetValue(callback, out t))
                t.deleted = true;
        }

        private Anymous_T GetFromPool()
        {
            Anymous_T t;
            int cnt = _pool.Count;
            if (cnt > 0)
            {
                t = _pool[cnt - 1];
                _pool.RemoveAt(cnt - 1);
                t.deleted = false;
                t.elapsed = 0;
            }
            else
                t = new Anymous_T();
            return t;
        }

        private void ReturnToPool(Anymous_T t)
        {
            t.callback = null;
            _pool.Add(t);
        }

        public void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Dictionary<TimerCallback, Anymous_T>.Enumerator iter;

            if (_items.Count > 0)
            {
                iter = _items.GetEnumerator();
                while (iter.MoveNext())
                {
                    Anymous_T i = iter.Current.Value;
                    if (i.deleted)
                    {
                        _toRemove.Add(i);
                        continue;
                    }

                    i.elapsed += dt;
                    if (i.elapsed < i.interval)
                        continue;

                    i.elapsed -= i.interval;
                    if (i.elapsed < 0 || i.elapsed > 0.03f)
                        i.elapsed = 0;

                    if (i.repeat > 0)
                    {
                        i.repeat--;
                        if (i.repeat == 0)
                        {
                            i.deleted = true;
                            _toRemove.Add(i);
                        }
                    }
                    repeat = i.repeat;
                    if (i.callback != null)
                    {
                        if (catchCallbackExceptions)
                        {
                            try
                            {
                                i.callback(i.param);
                            }
                            catch (System.Exception e)
                            {
                                i.deleted = true;
                                Debug.LogWarning("FairyGUI: timer(internal=" + i.interval + ", repeat=" + i.repeat + ") callback error > " + e.Message);
                            }
                        }
                        else
                            i.callback(i.param);
                    }
                }
                iter.Dispose();
            }

            int len = _toRemove.Count;
            if (len > 0)
            {
                for (int k = 0; k < len; k++)
                {
                    Anymous_T i = _toRemove[k];
                    if (i.deleted && i.callback != null)
                    {
                        _items.Remove(i.callback);
                        ReturnToPool(i);
                    }
                }
                _toRemove.Clear();
            }

            if (_toAdd.Count > 0)
            {
                iter = _toAdd.GetEnumerator();
                while (iter.MoveNext())
                    _items.Add(iter.Current.Key, iter.Current.Value);
                iter.Dispose();
                _toAdd.Clear();
            }
        }
    }

    class Anymous_T
    {
        public float interval;
        public int repeat;
        public TimerCallback callback;
        public object param;

        public float elapsed;
        public bool deleted;

        public void set(float interval, int repeat, TimerCallback callback, object param)
        {
            this.interval = interval;
            this.repeat = repeat;
            this.callback = callback;
            this.param = param;
        }
    }

    class TimersEngine : MonoBehaviour
    {
        void Update()
        {
            Timers.inst.Update();
        }
    }
}
