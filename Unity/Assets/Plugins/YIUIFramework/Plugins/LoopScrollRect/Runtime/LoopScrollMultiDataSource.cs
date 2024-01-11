using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public interface LoopScrollMultiDataSource
    {
        void ProvideData(Transform transform, int index);
    }
}