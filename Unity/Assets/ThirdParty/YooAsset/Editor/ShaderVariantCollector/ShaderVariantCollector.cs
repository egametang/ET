using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;

namespace YooAsset.Editor
{
	public static class ShaderVariantCollector
	{
		private enum ESteps
		{
			None,
			Prepare,
			CollectAllMaterial,
			CollectVariants,
			CollectSleeping,
			WaitingDone,
		}

		private const float WaitMilliseconds = 1000f;
		private const float SleepMilliseconds = 100f;
		private static string _savePath;
		private static string _packageName;
		private static int _processMaxNum;
		private static Action _completedCallback;

		private static ESteps _steps = ESteps.None;
		private static Stopwatch _elapsedTime;
		private static List<string> _allMaterials;
		private static List<GameObject> _allSpheres = new List<GameObject>(1000);


		/// <summary>
		/// 开始收集
		/// </summary>
		public static void Run(string savePath, string packageName, int processMaxNum, Action completedCallback)
		{
			if (_steps != ESteps.None)
				return;

			if (Path.HasExtension(savePath) == false)
				savePath = $"{savePath}.shadervariants";
			if (Path.GetExtension(savePath) != ".shadervariants")
				throw new System.Exception("Shader variant file extension is invalid.");
			if (string.IsNullOrEmpty(packageName))
				throw new System.Exception("Package name is null or empty !");

			// 注意：先删除再保存，否则ShaderVariantCollection内容将无法及时刷新
			AssetDatabase.DeleteAsset(savePath);
			EditorTools.CreateFileDirectory(savePath);
			_savePath = savePath;
			_packageName = packageName;
			_processMaxNum = processMaxNum;
			_completedCallback = completedCallback;

			// 聚焦到游戏窗口
			EditorTools.FocusUnityGameWindow();

			// 创建临时测试场景
			CreateTempScene();

			_steps = ESteps.Prepare;
			EditorApplication.update += EditorUpdate;
		}

		private static void EditorUpdate()
		{
			if (_steps == ESteps.None)
				return;

			if (_steps == ESteps.Prepare)
			{
				ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
				_steps = ESteps.CollectAllMaterial;
				return; //等待一帧
			}

			if (_steps == ESteps.CollectAllMaterial)
			{
				_allMaterials = GetAllMaterials();
				_steps = ESteps.CollectVariants;
				return; //等待一帧
			}
			
			if (_steps == ESteps.CollectVariants)
			{
				int count = Mathf.Min(_processMaxNum, _allMaterials.Count);
				List<string> range = _allMaterials.GetRange(0, count);
				_allMaterials.RemoveRange(0, count);
				CollectVariants(range);

				if (_allMaterials.Count > 0)
				{
					_elapsedTime = Stopwatch.StartNew();
					_steps = ESteps.CollectSleeping;
				}
				else
				{
					_elapsedTime = Stopwatch.StartNew();
					_steps = ESteps.WaitingDone;
				}
			}

			if (_steps == ESteps.CollectSleeping)
			{
				if (_elapsedTime.ElapsedMilliseconds > SleepMilliseconds)
				{
					DestroyAllSpheres();
					_elapsedTime.Stop();
					_steps = ESteps.CollectVariants;
				}
			}

			if (_steps == ESteps.WaitingDone)
			{
				// 注意：一定要延迟保存才会起效
				if (_elapsedTime.ElapsedMilliseconds > WaitMilliseconds)
				{
					_elapsedTime.Stop();
					_steps = ESteps.None;

					// 保存结果并创建清单
					ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(_savePath);
					CreateManifest();

					Debug.Log($"搜集SVC完毕！");
					EditorApplication.update -= EditorUpdate;
					_completedCallback?.Invoke();
				}
			}
		}
		private static void CreateTempScene()
		{
			EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
		}
		private static List<string> GetAllMaterials()
		{
			int progressValue = 0;
			List<string> allAssets = new List<string>(1000);

			// 获取所有打包的资源
			CollectResult collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(EBuildMode.DryRunBuild, _packageName);
			foreach (var assetInfo in collectResult.CollectAssets)
			{
				string[] depends = AssetDatabase.GetDependencies(assetInfo.AssetPath, true);
				foreach (var dependAsset in depends)
				{
					if (allAssets.Contains(dependAsset) == false)
						allAssets.Add(dependAsset);
				}
				EditorTools.DisplayProgressBar("获取所有打包资源", ++progressValue, collectResult.CollectAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 搜集所有材质球
			progressValue = 0;
			List<string> allMaterial = new List<string>(1000);
			foreach (var assetPath in allAssets)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Material))
				{
					allMaterial.Add(assetPath);
				}
				EditorTools.DisplayProgressBar("搜集所有材质球", ++progressValue, allAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 返回结果
			return allMaterial;
		}
		private static void CollectVariants(List<string> materials)
		{
			Camera camera = Camera.main;
			if (camera == null)
				throw new System.Exception("Not found main camera.");

			// 设置主相机
			float aspect = camera.aspect;
			int totalMaterials = materials.Count;
			float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
			float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
			float halfHeight = Mathf.CeilToInt(height / 2f);
			float halfWidth = Mathf.CeilToInt(width / 2f);
			camera.orthographic = true;
			camera.orthographicSize = halfHeight;
			camera.transform.position = new Vector3(0f, 0f, -10f);

			// 创建测试球体
			int xMax = (int)(width - 1);
			int x = 0, y = 0;
			int progressValue = 0;
			for (int i = 0; i < materials.Count; i++)
			{
				var material = materials[i];
				var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
				var go = CreateSphere(material, position, i);
				if (go != null)
					_allSpheres.Add(go);
				if (x == xMax)
				{
					x = 0;
					y++;
				}
				else
				{
					x++;
				}
				EditorTools.DisplayProgressBar("照射所有材质球", ++progressValue, materials.Count);
			}
			EditorTools.ClearProgressBar();
		}
		private static GameObject CreateSphere(string assetPath, Vector3 position, int index)
		{
			var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
			var shader = material.shader;
			if (shader == null)
				return null;

			var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.GetComponent<Renderer>().sharedMaterial = material;
			go.transform.position = position;
			go.name = $"Sphere_{index} | {material.name}";
			return go;
		}
		private static void DestroyAllSpheres()
		{
			foreach(var go in _allSpheres)
			{
				GameObject.DestroyImmediate(go);
			}
			_allSpheres.Clear();

			// 尝试释放编辑器加载的资源
			EditorUtility.UnloadUnusedAssetsImmediate(true);
		}
		private static void CreateManifest()
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_savePath);
			if (svc != null)
			{
				var wrapper = ShaderVariantCollectionManifest.Extract(svc);
				string jsonData = JsonUtility.ToJson(wrapper, true);
				string savePath = _savePath.Replace(".shadervariants", ".json");
				File.WriteAllText(savePath, jsonData);
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}
	}
}