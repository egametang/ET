using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
	/// <summary>
	/// 
	/// </summary>
	public class EditorToolSet
	{
		public static GUIContent[] packagesPopupContents;

		static bool _loaded;

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
		[InitializeOnLoadMethod]
		static void Startup()
		{
			EditorApplication.update += EditorApplication_Update;
		}
#endif

		[MenuItem("GameObject/FairyGUI/UI Panel", false, 0)]
		static void CreatePanel()
		{
#if !(UNITY_5 || UNITY_5_3_OR_NEWER)
			EditorApplication.update -= EditorApplication_Update;
			EditorApplication.update += EditorApplication_Update;
#endif
			StageCamera.CheckMainCamera();

			GameObject panelObject = new GameObject("UIPanel");
			if (Selection.activeGameObject != null)
			{
				panelObject.transform.parent = Selection.activeGameObject.transform;
				panelObject.layer = Selection.activeGameObject.layer;
			}
			else
			{
				int layer = LayerMask.NameToLayer(StageCamera.LayerName);
				panelObject.layer = layer;
			}
			panelObject.AddComponent<FairyGUI.UIPanel>();
			Selection.objects = new Object[] { panelObject };
		}

		[MenuItem("GameObject/FairyGUI/UI Camera", false, 0)]
		static void CreateCamera()
		{
			StageCamera.CheckMainCamera();
			Selection.objects = new Object[] { StageCamera.main.gameObject };
		}

		[MenuItem("Window/FairyGUI - Refresh Packages And Panels")]
		static void RefreshPanels()
		{
			ReloadPackages();
		}

		static void EditorApplication_Update()
		{
			if (Application.isPlaying)
				return;

			if (_loaded || !EMRenderSupport.hasTarget)
				return;

			LoadPackages();
		}

		public static void ReloadPackages()
		{
			if (!Application.isPlaying)
			{
				_loaded = false;
				LoadPackages();
				Debug.Log("FairyGUI - Refresh Packages And Panels complete.");
			}
			else
				EditorUtility.DisplayDialog("FairyGUI", "Cannot run in play mode.", "OK");
		}

		public static void LoadPackages()
		{
			if (Application.isPlaying || _loaded)
				return;

#if !(UNITY_5 || UNITY_5_3_OR_NEWER)
			EditorApplication.update -= EditorApplication_Update;
			EditorApplication.update += EditorApplication_Update;
#endif
			_loaded = true;

			UIPackage.RemoveAllPackages();
			FontManager.Clear();
			NTexture.DisposeEmpty();
			UIObjectFactory.Clear();

			string[] ids = AssetDatabase.FindAssets("_fui t:textAsset");
			int cnt = ids.Length;
			for (int i = 0; i < cnt; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(ids[i]);
				int pos = assetPath.LastIndexOf("_fui");
				if (pos == -1)
					continue;

				assetPath = assetPath.Substring(0, pos);
				if (AssetDatabase.AssetPathToGUID(assetPath) != null)
					UIPackage.AddPackage(assetPath,
						(string name, string extension, System.Type type, out DestroyMethod destroyMethod) =>
						{
							destroyMethod = DestroyMethod.Unload;
							return AssetDatabase.LoadAssetAtPath(name + extension, type);
						}
					);
			}

			List<UIPackage> pkgs = UIPackage.GetPackages();
			pkgs.Sort(CompareUIPackage);

			cnt = pkgs.Count;
			packagesPopupContents = new GUIContent[cnt + 1];
			for (int i = 0; i < cnt; i++)
				packagesPopupContents[i] = new GUIContent(pkgs[i].name);
			packagesPopupContents[cnt] = new GUIContent("Please Select");

			UIConfig.ClearResourceRefs();
			UIConfig[] configs = GameObject.FindObjectsOfType<UIConfig>();
			foreach (UIConfig config in configs)
				config.Load();

			EMRenderSupport.Reload();
		}

		static int CompareUIPackage(UIPackage u1, UIPackage u2)
		{
			return u1.name.CompareTo(u2.name);
		}
	}

}
