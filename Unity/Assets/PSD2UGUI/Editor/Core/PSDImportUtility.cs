using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace PSDUIImporter
{
    public static class PSDImportUtility
    {
        public static string baseFilename;
        public static string baseDirectory;
        public static Canvas canvas;
        public static GameObject eventSys;
        public static readonly Dictionary<Transform, Transform> ParentDic = new Dictionary<Transform, Transform>();

        public static object DeserializeXml(string filePath, System.Type type)
        {
            object instance = null;
            StreamReader xmlFile = File.OpenText(filePath);
            if (xmlFile != null)
            {
                string xml = xmlFile.ReadToEnd();
                if ((xml != null) && (xml.ToString() != ""))
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    byte[] byteArray = encoding.GetBytes(xml);
                    MemoryStream memoryStream = new MemoryStream(byteArray);
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);
                    if (xmlTextWriter != null)
                    {
                        instance = xs.Deserialize(memoryStream);
                    }
                }
            }
            xmlFile.Close();
            return instance;
        }

//         public static T InstantiateItem<T>(string resourcePatn, string name,GameObject parent) where T : UnityEngine.Object
//         {
//             GameObject temp = Resources.Load(resourcePatn, typeof(GameObject)) as GameObject;
//             GameObject item = GameObject.Instantiate(temp) as GameObject;
//             item.name = name;
//             item.transform.SetParent(canvas.transform, false);
//             ParentDic[item.transform] =  parent.transform;
//             return item.GetComponent<T>();
//         }

        /// <summary>
        /// 加载并实例化prefab，编辑器下不用Resources.Load，否则这些预设会打到安装包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath">assets全路径，带后缀</param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T LoadAndInstant<T>(string assetPath, string name, GameObject parent) where T : UnityEngine.Object
        {
            GameObject temp = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            GameObject item = GameObject.Instantiate(temp) as GameObject;
            if(item == null)
            {
                Debug.LogError("LoadAndInstant asset failed : " + assetPath);
                return null;
            }
            item.name = name;
            //item.transform.SetParent(parent.transform);
            item.transform.SetParent(canvas.transform, false);
            ParentDic[item.transform] = parent.transform;
            item.transform.localScale = Vector3.one;
            return item.GetComponent<T>();
        }

        public static void SetAnchorMiddleCenter(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                Debug.LogWarning("rectTransform is null...");
                return;
            }
            rectTransform.offsetMin = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        }

    }
}