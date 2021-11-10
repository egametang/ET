using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[AttributeUsage(AttributeTargets.Field)]
public class RCElementAttribute : Attribute
{
}


public enum RCComponentType
{
	None,
	Gameobject,
	Transform,
	RectTransform,
	Text,
	Image,
	RawImage,
	Button,
	Toggle,
	Slider,
	Scrollbar,
	Dropdown,
	InputField,
	Canvas,
	ScrollRect,
}
public static class ReferenceCollectorExtension
{
	public static Dictionary<RCComponentType, Type> TypeDict = new Dictionary<RCComponentType, Type>()
	{
		{RCComponentType.Gameobject, typeof(GameObject)},
		{RCComponentType.Transform, typeof(Transform)},
        {RCComponentType.RectTransform, typeof(RectTransform)},
		{RCComponentType.Text, typeof(Text)},
		{RCComponentType.Image, typeof(Image)},
		{RCComponentType.RawImage, typeof(RawImage)},
		{RCComponentType.Button, typeof(Button)},
        {RCComponentType.Toggle, typeof(Toggle)},
        {RCComponentType.Slider, typeof(Slider)},
        {RCComponentType.Scrollbar, typeof(Scrollbar)},
        {RCComponentType.Dropdown, typeof(Dropdown)},
		{RCComponentType.InputField, typeof(InputField)},
        {RCComponentType.Canvas, typeof(Canvas)},
        {RCComponentType.ScrollRect, typeof(ScrollRect)},
	};
	public static void BindToObject(this ReferenceCollector self, object obj)
	{
		Type type = obj.GetType();
		if (type == null)
		{
			return;
		}
		foreach (var fieldInfo in type.GetFields())
		{
			var attribute = fieldInfo.GetCustomAttributes(typeof(RCElementAttribute), false).FirstOrDefault();
			if (attribute == null)
			{
				continue;
			}
			var d = self.data.FirstOrDefault(d => d.key == fieldInfo.Name);
			if (d != null)
			{
				Type componentType = typeof(GameObject);
				RCComponentType eType = RCComponentType.Gameobject;
				Enum.TryParse(d.componentTypeStr, out eType);
				TypeDict.TryGetValue(eType, out componentType);


				if (!d.isArray)
                {
					if (fieldInfo.FieldType.GetInterface("IList") != null)
					{
						return;
					}
					if (fieldInfo.FieldType != componentType)
					{
						Debug.LogError($"类型{type.Name}找不到{componentType.Name} {fieldInfo.Name}字段");
					}
					else if (componentType == typeof(GameObject))
					{
						fieldInfo.SetValue(obj, d.gameObject);
					}
					else
					{
						GameObject gob = d.gameObject as GameObject;
						if (gob != null)
						{
							var component = gob.GetComponent(componentType);
							if (component != null)
							{
								fieldInfo.SetValue(obj, component);
							}
							else
							{
								Debug.LogError($"{self.name}找不到类型为{componentType.Name}的组件");
							}
						}
						else
						{
							Debug.LogError($"{self.name}不是GameObject");
						}
					}
                }
                else
				{
					if (fieldInfo.FieldType.GetInterface("IList") == null)
					{
						return;
					}
                    if (fieldInfo.FieldType.IsArray)
					{
						var array = Array.CreateInstance(fieldInfo.FieldType.GetElementType(), d.gameObjects.Count);							
						int index = 0;
						foreach (var o in d.gameObjects)
						{
							GameObject go = o as GameObject;
							Object component = go;
							if (componentType != typeof(GameObject))
							{
								component = go.GetComponent(componentType);
							}
							if (component != null)
							{
								array.SetValue(component, index);
								index++;
							}
						}
						fieldInfo.SetValue(obj, array);
					}
                    else
					{
						var list = Activator.CreateInstance(fieldInfo.FieldType);
						var add = fieldInfo.FieldType.GetMethod("Add");
						foreach (var o in d.gameObjects)
						{
							GameObject go = o as GameObject;
							Object component = go;
							if (componentType != typeof(GameObject))
							{
								component = go.GetComponent(componentType);
							}
							if (component != null)
							{
								add.Invoke(list, new object[] { component });
							}
						}
						fieldInfo.SetValue(obj, list);
					}
				}
				
			}
		}
	}
}