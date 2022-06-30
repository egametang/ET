using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace YooAsset.Editor
{
	[InitializeOnLoad]
	public static class ShaderVariantCollector
	{
		private const float WaitMilliseconds = 1000f;
		private static string _saveFilePath;
		private static bool _isStarted = false;
		private static readonly Stopwatch _elapsedTime = new Stopwatch();

		static ShaderVariantCollector()
		{
			EditorApplication.update += EditorUpdate;
		}
		private static void EditorUpdate()
		{
			// 注意：一定要延迟保存才会起效
			if (_isStarted && _elapsedTime.ElapsedMilliseconds > WaitMilliseconds)
			{
				_isStarted = false;
				_elapsedTime.Stop();

				// 保存结果
				SaveCurrentShaderVariantCollection();

				// 创建说明文件
				CreateReadme();
			}
		}
		private static void CreateReadme()
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_saveFilePath);
			if(svc != null)
			{
				var wrapper = ShaderVariantCollectionHelper.Extract(svc);
				string jsonContents = JsonUtility.ToJson(wrapper, true);
				string savePath = _saveFilePath.Replace(".shadervariants", ".json");
				File.WriteAllText(savePath, jsonContents);
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}

		/// <summary>
		/// 开始收集
		/// </summary>
		public static void Run(string saveFilePath)
		{
			if (_isStarted)
				return;

			if (Path.HasExtension(saveFilePath) == false)
				saveFilePath = $"{saveFilePath}.shadervariants";
			if (Path.GetExtension(saveFilePath) != ".shadervariants")
				throw new System.Exception("Shader variant file extension is invalid.");
			EditorTools.CreateFileDirectory(saveFilePath);
			_saveFilePath = saveFilePath;

			// 聚焦到游戏窗口
			EditorTools.FocusUnityGameWindow();

			// 清空旧数据
			ClearCurrentShaderVariantCollection();

			// 创建临时测试场景
			CreateTemperScene();

			// 收集着色器变种
			var materials = GetAllMaterials();
			CollectVariants(materials);

			_isStarted = true;
			_elapsedTime.Reset();
			_elapsedTime.Start();

			UnityEngine.Debug.LogWarning("已经启动着色器变种收集工作，该工具只支持在编辑器下人工操作！");
		}

		private static void CreateTemperScene()
		{
			// 创建临时场景
			EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
		}
		private static List<Material> GetAllMaterials()
		{
			int progressValue = 0;
			List<string> allAssets = new List<string>(1000);

			// 获取所有打包的资源
			List<CollectAssetInfo> allCollectInfos = AssetBundleCollectorSettingData.Setting.GetAllCollectAssets(EBuildMode.DryRunBuild);
			List<string> collectAssets = allCollectInfos.Select(t => t.AssetPath).ToList();
			foreach (var assetPath in collectAssets)
			{
				string[] depends = AssetDatabase.GetDependencies(assetPath, true);
				foreach (var depend in depends)
				{
					if (allAssets.Contains(depend) == false)
						allAssets.Add(depend);
				}
				EditorTools.DisplayProgressBar("获取所有打包资源", ++progressValue, collectAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 搜集所有材质球
			progressValue = 0;
			var shaderDic = new Dictionary<Shader, List<Material>>(100);
			foreach (var assetPath in allAssets)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Material))
				{
					var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
					var shader = material.shader;
					if (shader == null)
						continue;

					if (shaderDic.ContainsKey(shader) == false)
					{
						shaderDic.Add(shader, new List<Material>());
					}
					if (shaderDic[shader].Contains(material) == false)
					{
						shaderDic[shader].Add(material);
					}
				}
				EditorTools.DisplayProgressBar("搜集所有材质球", ++progressValue, allAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 返回结果
			var materials = new List<Material>(1000);
			foreach (var valuePair in shaderDic)
			{
				materials.AddRange(valuePair.Value);
			}
			return materials;
		}
		private static void CollectVariants(List<Material> materials)
		{
			Camera camera = Camera.main;
			if(camera == null)
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
				CreateSphere(material, position, i);
				if (x == xMax)
				{
					x = 0;
					y++;
				}
				else
				{
					x++;
				}
				EditorTools.DisplayProgressBar("测试所有材质球", ++progressValue, materials.Count);
			}
			EditorTools.ClearProgressBar();
		}
		private static void CreateSphere(Material material, Vector3 position, int index)
		{
			var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.GetComponent<Renderer>().material = material;
			go.transform.position = position;
			go.name = $"Sphere_{index}|{material.name}";
		}

		private static void ClearCurrentShaderVariantCollection()
		{
			EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
		}
		private static void SaveCurrentShaderVariantCollection()
		{
			EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", _saveFilePath);
		}
		public static int GetCurrentShaderVariantCollectionShaderCount()
		{
			return (int)EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
		}
		public static int GetCurrentShaderVariantCollectionVariantCount()
		{
			return (int)EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
		}
	}
}