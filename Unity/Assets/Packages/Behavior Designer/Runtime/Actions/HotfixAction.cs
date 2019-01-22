using System;
using BDAction = BehaviorDesigner.Runtime.Tasks.Action;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class HotfixAction : BDAction
    {
        public int behaviorTreeConfigID;
        [NonSerialized]
        public Func<float> getPriority;
        [NonSerialized]
        public Func<float> getUtility;
        [NonSerialized]
        public System.Action onAwake;
        [NonSerialized]
        public System.Action onBehaviorComplete;
        [NonSerialized]
        public System.Action onBehaviorRestart;
        [NonSerialized]
        public System.Action onEnd;
        [NonSerialized]
        public System.Action onUpdate;
        [NonSerialized]
        public System.Action onFixedUpdate;
        [NonSerialized]
        public System.Action onLateUpdate;
        [NonSerialized]
        public System.Action onDrawGizmos;
        [NonSerialized]
        public Action<bool> onPause;
        [NonSerialized]
        public System.Action onReset;
        [NonSerialized]
        public System.Action onStart;
        [NonSerialized]
        public Func<TaskStatus> onTick;
        [NonSerialized]
        public Func<object, bool> equals;
        [NonSerialized]
        public Func<int> getHashCode;
        [NonSerialized]
        public Func<string> toString;
        
        public override float GetPriority()
        {
            if(getPriority != null)
            {
                return getPriority();
            }
            
            return base.GetPriority();
        }

        public override float GetUtility()
        {
            if (getUtility != null)
            {
                return getUtility();
            }

            return base.GetUtility();
        }

        public override void OnAwake()
        {
            onAwake?.Invoke();
        }

        public override void OnBehaviorComplete()
        {
            onBehaviorComplete?.Invoke();
        }

        public override void OnBehaviorRestart()
        {
            onBehaviorRestart?.Invoke();
        }

        public override void OnEnd()
        {
            onEnd?.Invoke();
        }

        public override void OnUpdate()
        {
            onUpdate?.Invoke();
        }

        public override void OnFixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        public override void OnLateUpdate()
        {
            onLateUpdate?.Invoke();
        }

        public override void OnDrawGizmos()
        {
            onDrawGizmos?.Invoke();
        }

        public override void OnPause(bool paused)
        {
            onPause?.Invoke(paused);
        }

        public override void OnReset()
        {
            onReset?.Invoke();
        }

        public override void OnStart()
        {
            onStart?.Invoke();
        }

        public override TaskStatus OnTick()
        {
            if(onTick != null)
            {
                return onTick();
            }

            return base.OnTick();
        }

        public override bool Equals(object obj)
        {
            if(equals != null)
            {
                return equals(obj);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if(getHashCode != null)
            {
                return getHashCode();
            }

            return base.GetHashCode();
        }

        public override string ToString()
        {
            if(toString != null)
            {
                return toString();
            }

            return base.ToString();
        }
    }
}