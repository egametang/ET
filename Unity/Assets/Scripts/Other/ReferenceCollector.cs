using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class ReferenceCollectorData
{
	public string key;
	public Object gameObject;
}

public class ReferenceCollectorDataComparer: IComparer<ReferenceCollectorData>
{
	public int Compare(ReferenceCollectorData x, ReferenceCollectorData y)
	{
		return String.Compare(x.key, y.key, StringComparison.Ordinal);
	}
}

public class ReferenceCollector: MonoBehaviour, ISerializationCallbackReceiver
{
	public List<ReferenceCollectorData> data = new List<ReferenceCollectorData>();

	private readonly Dictionary<string, Object> dict = new Dictionary<string, Object>();

#if UNITY_EDITOR
	public void Add(string key, Object obj)
	{
		SerializedObject serializedObject = new SerializedObject(this);
		var dataProperty = serializedObject.FindProperty("data");
		int i;
		for (i = 0; i < data.Count; i++)
		{
			if (data[i].key == key)
			{
				break;
			}
		}
		if (i != data.Count)
		{
			var element = dataProperty.GetArrayElementAtIndex(i);
			element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
		}
		else
		{
			dataProperty.InsertArrayElementAtIndex(i);
			var element = dataProperty.GetArrayElementAtIndex(i);
			element.FindPropertyRelative("key").stringValue = key;
			element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
		}
		EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Remove(string key)
	{
		SerializedObject serializedObject = new SerializedObject(this);
		var dataProperty = serializedObject.FindProperty("data");
		int i;
		for (i = 0; i < data.Count; i++)
		{
			if (data[i].key == key)
			{
				break;
			}
		}
		if (i != data.Count)
		{
			dataProperty.DeleteArrayElementAtIndex(i);
		}
		EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Clear()
	{
		SerializedObject serializedObject = new SerializedObject(this);
		var dataProperty = serializedObject.FindProperty("data");
		dataProperty.ClearArray();
		EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Sort()
	{
		SerializedObject serializedObject = new SerializedObject(this);
		data.Sort(new ReferenceCollectorDataComparer());
		EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}
#endif

	public T Get<T>(string key) where T : class
	{
		Object dictGo;
		if (!dict.TryGetValue(key, out dictGo))
		{
			return null;
		}
		return dictGo as T;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		dict.Clear();
		foreach (var referenceCollectorData in data)
		{
			if (!dict.ContainsKey(referenceCollectorData.key))
			{
				dict.Add(referenceCollectorData.key, referenceCollectorData.gameObject);
			}
		}
	}
}