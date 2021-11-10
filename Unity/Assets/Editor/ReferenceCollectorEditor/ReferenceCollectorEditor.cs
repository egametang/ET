using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;


//自定义ReferenceCollector类在界面中的显示与功能
[CustomEditor(typeof (ReferenceCollector))]
//没有该属性的编辑器在选中多个物体时会提示“Multi-object editing not supported”
[CanEditMultipleObjects]
public class ReferenceCollectorEditor: Editor
{
    //输入在textfield中的字符串
    private string searchKey
	{
		get
		{
			return _searchKey;
		}
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				heroPrefab = referenceCollector.Get<Object>(searchKey);
			}
		}
	}

	private ReferenceCollector referenceCollector;

	private Object heroPrefab;

	private string _searchKey = "";

	private static Dictionary<RCComponentType,Type> _typeDict => ReferenceCollectorExtension.TypeDict;

    private void OnEnable()
    {
        referenceCollector = target as ReferenceCollector;
    }
    private void DelNullReference()
	{
		List<ReferenceCollectorData> datas = referenceCollector.data;
		List<int> delList = new List<int>();

        for (int i = 0; i < datas.Count; i++)
        {
            if (!datas[i].isArray)
            {
				if(datas[i].gameObject == null)
                {
					delList.Add(i);
                }
            }
            else
            {
				List<int> delList1 = new List<int>();
                for (int j = 0; j < datas[i].gameObjects.Count; j++)
                {
					if(datas[i].gameObjects[j] == null)
                    {
						delList1.Add(j);
                    }
                }
                for (int index = datas[i].gameObjects.Count -1; index >= 0; index--)
                {
                    if (delList1.Contains(index))
                    {
						datas[i].gameObjects.RemoveAt(index);
                    }
                }
			}
		}

		for (int index = datas.Count - 1; index >= 0; index--)
		{
			if (delList.Contains(index))
			{
				datas.RemoveAt(index);
			}
		}
	}

    public override void OnInspectorGUI()
	{
        //使ReferenceCollector支持撤销操作，还有Redo，不过没有在这里使用
        Undo.RecordObject(referenceCollector, "Changed Settings");
		List<ReferenceCollectorData> datas = referenceCollector.data;
		//开始水平布局，如果是比较新版本学习U3D的，可能不知道这东西，这个是老GUI系统的知识，除了用在编辑器里，还可以用在生成的游戏中
		GUILayout.Label("选中物体再按Ctrl+E后可以点击复制按钮快速引用，支持批量选中。",GUILayout.Height(35));
		GUILayout.BeginHorizontal();
        //下面几个if都是点击按钮就会返回true调用里面的东西
		if (GUILayout.Button("添加引用"))
		{
            //添加新的元素，具体的函数注释
            // Guid.NewGuid().GetHashCode().ToString() 就是新建后默认的key
            AddReference(datas, Guid.NewGuid().GetHashCode().ToString(), null);
		}
		if (GUILayout.Button("全部删除"))
		{
			referenceCollector.Clear();
		}
		if (GUILayout.Button("删除空引用"))
		{
			DelNullReference();
		}
		if (GUILayout.Button("排序"))
		{
			referenceCollector.Sort();
		}
		if (GUILayout.Button("复制"))
		{
            foreach (GameObject gameObjectToPaste in _gameObjectsToPaste)
            {
				AddReference(datas, gameObjectToPaste.name, gameObjectToPaste);
            }
			_gameObjectsToPaste.Clear();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		var delList = new List<ReferenceCollectorData>();
        //遍历ReferenceCollector中data list的所有元素，显示在编辑器中
        foreach(var data in datas)
		{
			GUILayout.Space(15);
			GUILayout.BeginHorizontal();

			//这里的知识点在ReferenceCollector中有说
			data.key = EditorGUILayout.TextField(data.key, GUILayout.Width(150));

			EditorGUILayout.LabelField("数组",GUILayout.Width(30));
			data.isArray = EditorGUILayout.ToggleLeft("", data.isArray, GUILayout.Width(20));
            if (!data.isArray)
			{
				data.gameObject = EditorGUILayout.ObjectField(data.gameObject, typeof(Object), true);
            }
            else
            {

				if (GUILayout.Button("复制"))
				{
					foreach (GameObject gameObjectToPaste in _gameObjectsToPaste)
					{
                        if (CheckArrayItemValid(data, gameObjectToPaste))
                        {
							data.gameObjects.Add(gameObjectToPaste);
                        }
					}
					_gameObjectsToPaste.Clear();
				}
				GUILayout.BeginVertical();
				if(data.gameObjects.Count == 0)
                {
					data.gameObjects.Add(data.gameObject);
                }
				data.gameObject = data.gameObjects[0];
                for (int j = 0; j < data.gameObjects.Count; j++)
                {
					GUILayout.BeginHorizontal();
					data.gameObjects[j] = EditorGUILayout.ObjectField(data.gameObjects[j], typeof(Object), true);
                    if (!CheckArrayItemValid(data, data.gameObjects[j]))
                    {
						data.gameObjects[j] = null;
					}
					if (GUILayout.Button("-"))
					{
						data.gameObjects.RemoveAt(j);
						if (data.gameObjects.Count == 0)
						{
							data.gameObjects.Add(null);
						}
						break;
					}
					if (GUILayout.Button("+"))
					{
						data.gameObjects.Add(null);
						break;
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
            }

			if (GUILayout.Button("X"))
			{
                //将元素添加进删除list
				delList.Add(data);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("类型",GUILayout.Width(50));
			GameObject gob = data.gameObject as GameObject;
			string temp = data.componentTypeStr;
			if(gob != null)
			{
				data.componentTypeStr = EditorGUILayout.EnumPopup(ProcessAndGetRCComponent(gob, data.componentTypeStr), GUILayout.Width(100)).ToString();
            }
            else
            {
				data.componentTypeStr = EditorGUILayout.EnumPopup((RCComponentType)Enum.Parse(typeof(RCComponentType), data.componentTypeStr), GUILayout.Width(100)).ToString();
			}
			if (temp != data.componentTypeStr && (data.gameObject != null || data.gameObjects.Count > 0))
			{
				data.gameObject = null;
				data.gameObjects.Clear();
			}
			GUILayout.EndHorizontal();
		}
		var eventType = Event.current.type;
        //在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			// Show a copy icon on the drag
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if (eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach (var o in DragAndDrop.objectReferences)
				{
					AddReference(datas, o.name, o);
				}
			}

			Event.current.Use();
		}

        //遍历删除list，将其删除掉
		foreach (var d in delList)
		{
			datas.Remove(d);
		}
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

    //添加元素，具体知识点在ReferenceCollector中说了
    private void AddReference(List<ReferenceCollectorData> datas, string key, Object obj)
	{
		ReferenceCollectorData data = new ReferenceCollectorData();
		data.key = key;
		data.gameObject = obj;
		data.componentTypeStr = RCComponentType.Gameobject.ToString();
		datas.Add(data);
	}
	private RCComponentType ProcessAndGetRCComponent(GameObject gob, string typeStr)
    {
		if(gob == null)
        {
			return RCComponentType.None;
        }
		RCComponentType e =  RCComponentType.None;
		if(!Enum.TryParse(typeStr, out e))
        {
			return RCComponentType.Gameobject;
        }
        switch (e)
        {
            case RCComponentType.Gameobject:
				return e;
        }
        if(_typeDict.TryGetValue(e,out Type type))
		{
			Object obj = gob.GetComponent(type);
			if (obj != null)
			{
				return e;
			}
		}
		return RCComponentType.Gameobject;
    }
	private bool CheckArrayItemValid(ReferenceCollectorData data, Object obj)
	{
		RCComponentType e = RCComponentType.None;
		Enum.TryParse(data.componentTypeStr, out e);
		if (e == RCComponentType.None || e == RCComponentType.Gameobject)
		{
			return true;
		}
		GameObject gob = obj as GameObject;
		Type type = null;
		_typeDict.TryGetValue(e, out type);
		if (gob == null || type == null)
		{
			return false;
		}
		var comp = gob.GetComponent(type);
		return comp != null;
	}

	private static List<GameObject> _gameObjectsToPaste = new List<GameObject>();
	[MenuItem("Tools/复制选中物体 %E")]
	public static void CopyGameObjects()
    {
		if(Selection.gameObjects.Length > 0)
        {
			_gameObjectsToPaste.Clear();
			_gameObjectsToPaste = Selection.gameObjects.OrderBy(gob => gob.name).ToList();
        }
	}
}
