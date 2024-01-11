#if UNITY_EDITOR
using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// UI反射动态生成模板
    /// </summary>
    [HideReferenceObjectPicker]
    public class CreateUIBindProviderModule : BaseCreateModule
    {
        [Button("UI自动生成绑定替代反射代码", 50), GUIColor(0.4f, 0.8f, 1)]
        public void Create()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            var codeData = GenCodeByType(typeof(YIUIBindProvider));
            if (codeData == null) return;
            new CreateUIBindProviderCode(out var result, YIUIAutoTool.Author, codeData);

            if (result)
            {
                UnityTipsHelper.CallBackOk("UI自动生成绑定替代反射代码 生成完毕", YIUIAutoTool.CloseWindowRefresh);
            }
        }

        private static UIBindProviderData GenCodeByType(Type type)
        {
            var codeGenObj = Activator.CreateInstance(type);
            var getFunInfo = type.GetMethod("Get");
            if (getFunInfo == null)
            {
                Debug.LogError($"没有这个方法 Get");
                return null;
            }

            var newFunInfo = type.GetMethod("NewCode");
            if (newFunInfo == null)
            {
                Debug.LogError($"没有这个方法 NewCode");
                return null;
            }

            var writeCodeFunInfo = type.GetMethod("WriteCode");
            if (writeCodeFunInfo == null)
            {
                Debug.LogError($"没有这个方法 WriteCode");
                return null;
            }

            var sb   = SbPool.Get();
            var list = (IList)getFunInfo.Invoke(codeGenObj, Array.Empty<object>());

            for (int i = 0; i < list.Count; i++)
            {
                var data = list[i];
                newFunInfo.Invoke(codeGenObj, new[] { data, sb });
                sb.AppendFormat("\r            list[{0}] = new {1}\r\n", i, data.GetType().Name);
                writeCodeFunInfo.Invoke(codeGenObj, new[] { data, sb });
            }

            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }

            var userCode = SbPool.PutAndToStr(sb);

            return new UIBindProviderData
            {
                Namespace = type.Namespace,
                FullName  = type.FullName,
                Name      = type.Name,
                Count     = list.Count,
                Content   = userCode,
            };
        }
    }
}
#endif