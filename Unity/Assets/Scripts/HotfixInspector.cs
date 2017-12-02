using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class HotfixInspector : MonoBehaviour
    {
        public Dictionary<Type, Properties> Components = new Dictionary<Type, Properties>();
    }
}
