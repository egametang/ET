using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ET
{
    public interface IStateMachineWrap
    {
        Action MoveNext { get; }
        void Recycle();
    }
    
    public class StateMachineWrap<T>: IStateMachineWrap where T: IAsyncStateMachine
    {
        private static readonly ConcurrentQueue<StateMachineWrap<T>> queue = new();

        public static StateMachineWrap<T> Fetch(ref T stateMachine)
        {
            if (!queue.TryDequeue(out var stateMachineWrap))
            {
                stateMachineWrap = new StateMachineWrap<T>();
            }
            stateMachineWrap.StateMachine = stateMachine;
            return stateMachineWrap;
        }
        
        public void Recycle()
        {
            if (queue.Count > 1000)
            {
                return;
            }
            queue.Enqueue(this);
        }

        private readonly Action moveNext;

        public Action MoveNext
        {
            get
            {
                return this.moveNext;
            }
        }

        private T StateMachine;

        private StateMachineWrap()
        {
            this.moveNext = this.Run;
        }

        private void Run()
        {
            this.StateMachine.MoveNext();
        }
    }
}