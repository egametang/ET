using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Model;

[CustomEditor(typeof(HotfixInspector))]
[CanEditMultipleObjects]
public class HotfixInspectorEditor : Editor
{
    private const string HOTFIXCODE_PATH = "Assets/Bundles/Code/Code.prefab";
    private HotfixInspector manager;
    private List<Type> componentTypes = new List<Type>();
    private Type unityInspectorAttribute;
    private Type unityInspectorPeopertyAttribute;
    private bool isOpen;
    private int selectIndex;

    private void OnEnable()
    {
        RefreshHotfixComponentType();
        manager = (HotfixInspector)target;
        selectIndex = 0;
        isOpen = false;
    }

    private void RefreshHotfixComponentType()
    {
        componentTypes.Clear();
        Assembly assembly = GetAssembly();

        InitAttribute(assembly);

        foreach (Type type in assembly.GetTypes())
        {
            object[] attributes = type.GetCustomAttributes(unityInspectorAttribute, true);
            foreach (object attribute in attributes)
            {
                if (attribute.GetType().Equals(unityInspectorAttribute))
                {
                    componentTypes.Add(type);
                }
            }
        }
    }

    private Assembly GetAssembly()
    {
        GameObject code = AssetDatabase.LoadAssetAtPath<GameObject>(@HOTFIXCODE_PATH);
        byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
        byte[] mdbBytes = code.Get<TextAsset>("Hotfix.mdb").bytes;
        Assembly assembly = Assembly.Load(assBytes, mdbBytes);

        return assembly;
    }

    private void InitAttribute(Assembly assembly)
    {
        unityInspectorAttribute = assembly.GetType("Hotfix.UnityInspectorAttribute");
        unityInspectorPeopertyAttribute = assembly.GetType("Hotfix.UnityInspectorPeopertyAttribute");
    }

    private void AddHotfixComponent(Type componentType)
    {
        Properties properties = new Properties();
        foreach (PropertyInfo property in componentType.GetProperties())
        {
            if(property.GetCustomAttribute(unityInspectorPeopertyAttribute,false) != null)
            {

            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("AddHotfixComponent",GUILayout.ExpandWidth(true)))
        {
            isOpen = !isOpen;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("MenuItem1"), false, null, "item 1");
            menu.AddItem(new GUIContent("MenuItem2"), false, null, "item 2");
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("SubMenu/MenuItem3"), false, null, "item 3");

            menu.DropDown(new Rect(0, 0, 100, 100));
        }

        if (isOpen)
        {
            string[] options = new string[componentTypes.Count];
            for (int i = 0; i < componentTypes.Count; i++)
            {
                options[i] = componentTypes[i].Name;
            }
        }
    }
}