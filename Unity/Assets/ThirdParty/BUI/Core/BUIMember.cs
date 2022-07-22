using System;
using UnityEngine;

namespace BUI
{

    [DisallowMultipleComponent]
    public class BUIMember : MonoBehaviour
    {
        public string Name;
        public BUIType Type;

        public void SolveType()
        {
            if (Type == BUIType.GameObject)
            {
                return;
            }
            if(BUITypeCatagory.Types.TryGetValue(Type, out var t))
            {
                if (!GetComponent(t))
                {
                    Type = BUIType.Auto;
                }
            }
            else
            {
                Type = BUIType.Auto;
            }
            if (Type == BUIType.Auto)
            {
                foreach (var kvp in BUITypeCatagory.Types)
                {
                    if(kvp.Key == BUIType.GameObject)
                    {
                        Type = BUIType.GameObject;
                        return;
                    }
                    var com = GetComponent(kvp.Value);
                    if (com != null)
                    {
                        Type = kvp.Key;
                        return;
                    }
                }
            }
        }
    }
}
