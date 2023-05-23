using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIFindHelper
{
    /// <summary>
    /// 查找子节点
    /// </summary>
    /// <OtherParam name="_target"></OtherParam>
    /// <OtherParam name="_childName"></OtherParam>
    /// <returns></returns>
    public static Transform FindDeepChild(GameObject _target, string _childName)
    {
        Transform resultTrs = null;
        resultTrs = _target.transform.Find(_childName);
        if (resultTrs == null)
        {
            foreach (Transform trs in _target.transform)
            {
                resultTrs = UIFindHelper.FindDeepChild(trs.gameObject, _childName);
                if (resultTrs != null)
                    return resultTrs;
            }
        }
        return resultTrs;
    }

    /// <summary>
    /// 根据泛型查找子节点
    /// </summary>
    /// <param name="_target"></param>
    /// <param name="_childName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FindDeepChild<T>(GameObject _target, string _childName) where T : Component
    {
        Transform resultTrs = UIFindHelper.FindDeepChild(_target, _childName);
        if (resultTrs != null)
            return resultTrs.gameObject.GetComponent<T>();
        return (T)((object)null);
    }
}
