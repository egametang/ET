using System;
using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public interface LoopScrollDataSource
    {
        void ProvideData(Transform transform, int idx);
    }
    
    public class LoopScrollDataSourceInstance: LoopScrollDataSource
    {
        public Action<Transform,int> scrollMoveEvent;
        public void ProvideData(Transform transform, int idx)
        {
            scrollMoveEvent?.Invoke(transform,idx);
        }
    }
}