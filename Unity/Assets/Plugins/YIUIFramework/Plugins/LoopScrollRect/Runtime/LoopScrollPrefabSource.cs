using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public interface LoopScrollPrefabSource
    {
        GameObject GetObject(int index);

        void ReturnObject(Transform transform);
    }
}
