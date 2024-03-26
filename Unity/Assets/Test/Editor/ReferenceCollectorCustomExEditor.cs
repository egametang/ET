using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ET;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[CustomEditor(typeof (ReferenceCollector))]
public class ReferenceCollectorCustomExEditor : ReferenceCollectorEditor
{
	private GlobalConfig globalCfg => AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Resources/GlobalConfig.asset");
	
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		//选择的对象
		var go = referenceCollector.gameObject;
		
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Label("跳转");

		string path = string.Empty;
		if (GUILayout.Button("Hotfix"))
		{
			path = $"Assets/Scripts/Hotfix/Client/{globalCfg.AppType.ToString()}/UI/{go.name}";
			LocationDir(path);
		}
		if (GUILayout.Button("HotfixView"))
		{
			path = $"Assets/Scripts/HotfixView/Client/{globalCfg.AppType.ToString()}/UI/{go.name}";
			LocationDir(path);
		}
		if (GUILayout.Button("Model"))
		{
			path = $"Assets/Scripts/Model/Client/{globalCfg.AppType.ToString()}/UI/{go.name}";
			LocationDir(path);
        }
        if (GUILayout.Button("ModelView"))
        {
	        path = $"Assets/Scripts/ModelView/Client/{globalCfg.AppType.ToString()}/UI/{go.name}";
	        LocationDir(path);
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        //用于代码生成时是否添加IUpdate
        bool isUpdate = referenceCollector.IsUpdate;
        isUpdate = GUILayout.Toggle(isUpdate, "IsUpdate");
        referenceCollector.IsUpdate = isUpdate;
        //用于代码生成是否添加IDestroy
        bool isDestroy = referenceCollector.IsDestroy;
        isDestroy = GUILayout.Toggle(isDestroy, "IsDestroy");
        referenceCollector.IsDestroy = isDestroy;
        //代码生成
        if (GUILayout.Button("代码生成"))
        {
	        GenerateUI();
        }
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
	}

	private void LocationDir(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			Log.Error($"目录名为空");
			return;
		}

		if (!Directory.Exists(path))
		{
			Log.Error($"不存在文件夹===>{path}");
			return;
		}

		var dir = AssetDatabase.LoadAssetAtPath<Object>(path);
		if (dir != null)
		{
			// 选中目标文件夹
			Selection.activeObject = dir;
			        
			// 在Project窗口中高亮显示该文件夹
			EditorGUIUtility.PingObject(dir);
		}
	}
	
	private void GenerateUI()
	{
		var go = referenceCollector.gameObject;
		if (go == null || !(go.name.StartsWith("UI")))
		{
			Debug.LogError("未选中UI！");
			return;
		}

		//寻找到当前obj的路径
		var hotfixViewPath = $"Assets/Scripts/HotfixView/Client/{globalCfg.AppType.ToString()}/UI/{go.name}/";
		var modelViewPath = $"Assets/Scripts/ModelView/Client/{globalCfg.AppType.ToString()}/UI/{go.name}/";
		//创建目录
		if (!Directory.Exists(hotfixViewPath))
		{
			Directory.CreateDirectory(hotfixViewPath);
		}
		if (!Directory.Exists(modelViewPath))
		{
			Directory.CreateDirectory(modelViewPath);
		}
            
		CreateUIComponent(go, modelViewPath);
		CreateUIComponentSystem(go, hotfixViewPath);

		Debug.Log(go.name + "生成完毕！");
		//回收资源
		System.GC.Collect();
		//刷新编辑器
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// 生成UI组件Model，每次覆盖
	/// </summary>
	/// <param name="go"></param>
	/// <param name="path"></param>
	private void CreateUIComponent(GameObject go, string path)
	{
		string temp = @"/*********************************************
 * 
 * 脚本名：#ComponentName.cs
 * 创建时间：#Time
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class #ComponentName: Entity, IAwake #OtherInterface
	{
#GameObjectList
	}
}
";

		var sb = new StringBuilder();
		//替换UI名和时间
		var componentName = go.name + "Component";
		temp = temp.Replace("#ComponentName", componentName);
		temp = temp.Replace("#Time", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

		//替换生命周期接口
		sb.Clear();
		if (referenceCollector.IsUpdate)
			sb.Append(", IUpdate");
		if (referenceCollector.IsDestroy)
			sb.Append(", IDestroy");
		temp = temp.Replace("#OtherInterface", sb.ToString());
            
		//替换组件
		sb.Clear();
		foreach (var item in referenceCollector.data)
		{
			sb.AppendLine($"        public {item.type.ToString()} {item.key};");
		}
		temp = temp.Replace("#GameObjectList", sb.ToString());

		//输出文本
		File.WriteAllText(path + componentName + ".cs", temp, Encoding.UTF8);
	}

	private void CreateUIComponentSystem(GameObject go, string path)
	{
		var componentName = go.name + "Component";
		var systemName = go.name + "ComponentSystem";
		var codeSavePath = path + systemName + ".cs";
		//不覆盖逻辑代码
		if (File.Exists(codeSavePath))
		{
			return;
		}
            
		string temp = @"/*********************************************
 * 
 * 脚本名：#SystemName.cs
 * 创建时间：#Time
 *********************************************/
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(#ComponentName))]
	[FriendOf(typeof(#ComponentName))]
	public static partial class #SystemName
	{#InterfaceFunc#ButtonOnClick
	}
}
";
		
		
		temp = temp.Replace("#ComponentName", componentName);
		temp = temp.Replace("#SystemName", systemName);
		temp = temp.Replace("#Time", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
		
		//添加接口方法
		var interfaceTemp = @"
        [EntitySystem]
        private static void #Interface(this #ComponentName self)
		{
#Content
		}";

		//组件绑定
		var sbBindComponent = new StringBuilder();
		var sbButtonFunc = new StringBuilder();
		sbBindComponent.AppendLine("            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();");
		foreach (var item in referenceCollector.data)
		{
			sbBindComponent.AppendLine($"            self.{item.key} = rc.Get<GameObject>(\"{item.key}\").GetComponent<{item.type.ToString()}>();");
			//按钮类型绑定方法
			if (item.type == E_Type.Button)
			{
				var funcStr = $"On{item.key.Replace("Btn","")}";
				sbBindComponent.AppendLine($"            self.{item.key}.onClick.AddListener(() => {{ self.{funcStr}();}});");
				sbButtonFunc.AppendLine($"\n        public static void {funcStr}(this {componentName} self)\n        {{\n            Debug.Log(\"Click {item.key}\");\n        }}");
			}
		}
		//替换按钮方法
		temp = temp.Replace("#ButtonOnClick", sbButtonFunc.ToString());
		
		//生命周期
		var sbInterface = new StringBuilder();
		var awakeStr = interfaceTemp.Replace("#Interface", "Awake").Replace("#Content",sbBindComponent.ToString());
		sbInterface.AppendLine(awakeStr);
		if (referenceCollector.IsUpdate)
			sbInterface.AppendLine(interfaceTemp.Replace("#Interface","Update").Replace("#Content",""));
		if (referenceCollector.IsDestroy)
			sbInterface.AppendLine(interfaceTemp.Replace("#Interface","Destroy").Replace("#Content",""));
		interfaceTemp = sbInterface.ToString().Replace("#ComponentName", componentName);
		temp = temp.Replace("#InterfaceFunc", interfaceTemp);
		
		//输出文本
		File.WriteAllText(codeSavePath, temp, Encoding.UTF8);
	}
}