using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public interface LoopScrollDataSource
    {
        void ProvideData(Transform transform, int index);
    }
}