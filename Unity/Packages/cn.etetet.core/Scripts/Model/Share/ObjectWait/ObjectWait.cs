using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public static class WaitTypeError
    {
        public const int Success = 0;
        public const int Destroy = 1;
        public const int Cancel = 2;
        public const int Timeout = 3;
    }
    
    public interface IWaitType
    {
        int Error
        {
            get;
            set;
        }
    }
    
    
    public interface IDestroyRun
    {
        void SetResult();
    }
    
    public class ResultCallback<K>: Object, IDestroyRun where K : struct, IWaitType
    {
        private ETTask<K> tcs;

        public ResultCallback()
        {
            this.tcs = ETTask<K>.Create(true);
        }

        public bool IsDisposed
        {
            get
            {
                return this.tcs == null;
            }
        }

        public ETTask<K> Task => this.tcs;

        public void SetResult(K k)
        {
            var t = tcs;
            this.tcs = null;
            t.SetResult(k);
        }

        public void SetResult()
        {
            var t = tcs;
            this.tcs = null;
            t.SetResult(new K() { Error = WaitTypeError.Destroy });
        }
    }

    [ComponentOf]
    public class ObjectWait: Entity, IAwake, IDestroy
    {
        public Dictionary<Type, object> tcss = new Dictionary<Type, object>();
    }
}