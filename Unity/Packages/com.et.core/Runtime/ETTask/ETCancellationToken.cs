using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ET
{
    public class ETCancellationToken
    {
        private HashSet<Action> actions = new HashSet<Action>();

        public void Add(Action callback)
        {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            this.actions.Add(callback);
        }
        
        public void Remove(Action callback)
        {
            this.actions?.Remove(callback);
        }

        public bool IsDispose()
        {
            return this.actions == null;
        }

        public void Cancel()
        {
            if (this.actions == null)
            {
                return;
            }

            this.Invoke();
        }

        private void Invoke()
        {
            HashSet<Action> runActions = this.actions;
            this.actions = null;
            try
            {
                foreach (Action action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                ETTask.ExceptionHandler.Invoke(e);
            }
        }
    }
}