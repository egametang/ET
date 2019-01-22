using System;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class HotfixComposite : Composite
    {
        #region Task Override
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
        #endregion

        #region ParentTask Override
        [NonSerialized]
        public Func<bool> canExecute;
        [NonSerialized]
        public Func<bool> canReevaluate;
        [NonSerialized]
        public Func<bool> canRunParallelChildren;
        [NonSerialized]
        public Func<int> currentChildIndex;
        [NonSerialized]
        public Func<TaskStatus, TaskStatus> decorate;
        [NonSerialized]
        public Func<int> maxChildren;
        [NonSerialized]
        public Action<int, TaskStatus> onChildExecuted1;
        [NonSerialized]
        public Action<TaskStatus> onChildExecuted2;
        [NonSerialized]
        public System.Action onChildStarted1;
        [NonSerialized]
        public Action<int> onChildStarted2;
        [NonSerialized]
        public Action<int> onConditionalAbort;
        [NonSerialized]
        public Action<TaskStatus> onReevaluationEnded;
        [NonSerialized]
        public Func<bool> onReevaluationStarted;
        [NonSerialized]
        public Func<TaskStatus> overrideStatus1;
        [NonSerialized]
        public Func<TaskStatus, TaskStatus> overrideStatus2;

        public override bool CanExecute()
        {
            if(canExecute != null)
            {
                return canExecute();
            }

            return base.CanExecute();
        }

        public override bool CanReevaluate()
        {
            if(canReevaluate != null)
            {
                return canReevaluate();
            }

            return base.CanReevaluate();
        }

        public override bool CanRunParallelChildren()
        {
            if (canRunParallelChildren != null)
            {
                return canRunParallelChildren();
            }

            return base.CanRunParallelChildren();
        }

        public override int CurrentChildIndex()
        {
            if (currentChildIndex != null)
            {
                return currentChildIndex();
            }

            return base.CurrentChildIndex();
        }

        public override TaskStatus Decorate(TaskStatus status)
        {
            if (decorate != null)
            {
                return decorate(status);
            }

            return base.Decorate(status);
        }

        public override int MaxChildren()
        {
            if(maxChildren != null)
            {
                return maxChildren();
            }

            return base.MaxChildren();
        }

        public override void OnChildExecuted(int childIndex, TaskStatus childStatus)
        {
            onChildExecuted1?.Invoke(childIndex, childStatus);
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            onChildExecuted2?.Invoke(childStatus);
        }

        public override void OnChildStarted()
        {
            onChildStarted1?.Invoke();
        }

        public override void OnChildStarted(int childIndex)
        {
            onChildStarted2?.Invoke(childIndex);
        }

        public override void OnConditionalAbort(int childIndex)
        {
            onConditionalAbort?.Invoke(childIndex);
        }

        public override void OnReevaluationEnded(TaskStatus status)
        {
            onReevaluationEnded?.Invoke(status);
        }

        public override bool OnReevaluationStarted()
        {
            if (onReevaluationStarted != null)
            {
                return onReevaluationStarted();
            }

            return base.OnReevaluationStarted();
        }

        public override TaskStatus OverrideStatus()
        {
            if (overrideStatus1 != null)
            {
                return overrideStatus1();
            }

            return base.OverrideStatus();
        }

        public override TaskStatus OverrideStatus(TaskStatus status)
        {
            if (overrideStatus2 != null)
            {
                return overrideStatus2(status);
            }

            return base.OverrideStatus(status);
        }
        #endregion
    }
}