using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEffect_Demo_PropertyControl : MonoBehaviour
{
    [SerializeField] private string m_PropertyName;
    [SerializeField] private Object[] m_Objects;

    public void ChangeValue(int value)
    {
        foreach (var o in m_Objects)
        {
            if (!o) continue;

            var p = o.GetType().GetProperty(m_PropertyName);
            Debug.LogFormat("{0} {1} {2}", o.GetType(), m_PropertyName, p);

            if (p == null) continue;
            p.SetValue(o, value, new object[0]);
        }
    }
}
