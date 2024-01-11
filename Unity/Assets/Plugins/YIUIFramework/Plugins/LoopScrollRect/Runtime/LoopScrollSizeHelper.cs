using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    // optional class for better scroll support
    public interface LoopScrollSizeHelper
    {
        Vector2 GetItemsSize(int itemsCount);
    }
}
