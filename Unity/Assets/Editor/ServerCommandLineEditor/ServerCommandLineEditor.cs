using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ETModel
{
	enum StartConfigComponentType
	{
		ClientConfig,
		DBConfig,
		SceneConfig,
		HttpConfig,
		InnerConfig,
		OuterConfig,
		ProcessConfig,
		CopyConfig,
		MapConfig,
	}
	
	[ObjectSystem]
	class StartConfigDrawerAwakeSystem : AwakeSystem<StartConfigDrawer, int>
	{
		public override void Awake(StartConfigDrawer self, int level)
		{
			StartConfig startConfig = self.GetParent<StartConfig>();
			foreach (var childStartConfig in startConfig.List)
			{
				childStartConfig.AddComponentNoPool<StartConfigDrawer, int>(level + 1);
			}

			self.level = level;
		}
	}
	
#if !SERVER
	[HideInHierarchy]
#endif
	[NoObjectPool]
	public class StartConfigDrawer: Entity
	{
		public GUIStyle style = new GUIStyle();

		public int level;

		public bool isFold = true;

		private StartConfigComponentType st;

		public StartConfigDrawer()
		{
			this.Id = IdGenerater.GenerateId();
			
			this.style.normal.textColor = Color.red;
			this.style.alignment = TextAnchor.MiddleLeft;
			this.style.fixedHeight = 16;
		}

		public bool OnGUI()
		{
			StartConfig startConfig = this.GetParent<StartConfig>();
			GUILayout.BeginHorizontal(GUILayout.Height(16));

			if (this.level > 0)
			{
				string s = "";
				for (int i = 1; i < this.level; ++i)
				{
					s += $"    ";
				}
				GUILayout.Label(s, GUILayout.Width(20 * this.level));
			}
			
			{
				GUILayout.BeginHorizontal(GUILayout.Width(240), GUILayout.Height(16));
				this.isFold = EditorGUILayout.Foldout(isFold, $"子配置数量:{startConfig.List.Count}");
				
				if (GUILayout.Button("添加子配置", GUILayout.Height(16), GUILayout.Width(75)))
				{
					StartConfig s = new StartConfig();
					startConfig.Add(s);
					s.AddComponentNoPool<StartConfigDrawer, int>(this.level + 1);
					
					for (int i = 0; i < startConfig.List.Count; ++i)
					{
						startConfig.List[i].Id = i + 1;
					}

					this.isFold = true;
					return false;
				}
				if (GUILayout.Button("上", GUILayout.Height(16), GUILayout.Width(30)))
				{
					StartConfig parentStartConfig = startConfig.GetParent<StartConfig>();
					int index = parentStartConfig.List.IndexOf(startConfig);
					if (index == 0)
					{
						return true;
					}

					parentStartConfig.List.Remove(startConfig);
					parentStartConfig.List.Insert(index - 1, startConfig);
					for (int i = 0; i < parentStartConfig.List.Count; ++i)
					{
						parentStartConfig.List[i].Id = i + 1;
					}

					return false;
				}
				if (GUILayout.Button("下", GUILayout.Height(16), GUILayout.Width(30)))
				{
					StartConfig parentStartConfig = startConfig.GetParent<StartConfig>();
					int index = parentStartConfig.List.IndexOf(startConfig);
					if (index == parentStartConfig.List.Count - 1)
					{
						return true;
					}
					parentStartConfig.List.Remove(startConfig);
					parentStartConfig.List.Insert(index + 1, startConfig);
					for (int i = 0; i < parentStartConfig.List.Count; ++i)
					{
						parentStartConfig.List[i].Id = i + 1;
					}
					return false;
				}
				GUILayout.EndHorizontal();
			}

			{
				GUILayout.BeginHorizontal(GUILayout.Width(50));
				GUILayout.Label($"Id: ");
				startConfig.Id = EditorGUILayout.LongField(startConfig.Id, GUILayout.Width(30));
				GUILayout.EndHorizontal();
			}
			
			{
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				ProcessConfig processConfig = startConfig.GetComponent<ProcessConfig>();
				if (processConfig != null)
				{
					GUILayout.Label($"  ProcessConfig(", this.style);
					GUILayout.Label($"服务器IP: ");
					processConfig.ServerIP = EditorGUILayout.TextField(processConfig.ServerIP, GUILayout.Width(100));
					GUILayout.Label($"),", this.style);
				}
				GUILayout.EndHorizontal();
			}
			
			{
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				SceneConfig sceneConfig = startConfig.GetComponent<SceneConfig>();
				if (sceneConfig != null)
				{
					GUILayout.Label($"  SceneConfig(", this.style);
					GUILayout.Label($"SceneType: ");
					sceneConfig.SceneType = (SceneType)EditorGUILayout.EnumPopup(sceneConfig.SceneType, GUILayout.Width(100));
					GUILayout.Label($"Name: ");
					sceneConfig.Name = EditorGUILayout.TextField(sceneConfig.Name, GUILayout.Width(100));
					GUILayout.Label($"),", this.style);
				}
				GUILayout.EndHorizontal();
			}
			
			{
				GUILayout.BeginHorizontal(GUILayout.Width(150));
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				if (innerConfig != null)
				{
					GUILayout.Label($"  InnerConfig(", this.style);
					GUILayout.Label($"内网地址:");
					innerConfig.Address = EditorGUILayout.TextField(innerConfig.Address, GUILayout.Width(120));
					GUILayout.Label($"),", this.style);
				}

				GUILayout.EndHorizontal();
			}
			{
				GUILayout.BeginHorizontal(GUILayout.Width(350));
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				if (outerConfig != null)
				{
					GUILayout.Label($"  OuterConfig(", this.style);
					GUILayout.Label($"外网地址:");
					outerConfig.Address = EditorGUILayout.TextField(outerConfig.Address, GUILayout.Width(120));
					GUILayout.Label($"外网地址2:");
					outerConfig.Address2 = EditorGUILayout.TextField(outerConfig.Address2, GUILayout.Width(120));
					GUILayout.Label($"),", this.style);
				}

				GUILayout.EndHorizontal();
			}
			
			{
            	GUILayout.BeginHorizontal(GUILayout.Width(50));
                CopyConfig copyConfig = startConfig.GetComponent<CopyConfig>();
            	if (copyConfig != null)
            	{
            		GUILayout.Label($"  CopyConfig(", this.style);
            		GUILayout.Label($"),", this.style);
            	}
            
            	GUILayout.EndHorizontal();
            }
			
			{
				GUILayout.BeginHorizontal(GUILayout.Width(50));
				MapConfig mapConfig = startConfig.GetComponent<MapConfig>();
				if (mapConfig != null)
				{
					GUILayout.Label($"  MapConfig(", this.style);
					GUILayout.Label($"MapType: ");
					mapConfig.MapType = (MapType)EditorGUILayout.EnumPopup(mapConfig.MapType, GUILayout.Width(100));
					GUILayout.Label($"),", this.style);
				}
            
				GUILayout.EndHorizontal();
			}
			
			{
				GUILayout.BeginHorizontal(GUILayout.Width(350));
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				if (clientConfig != null)
				{
					GUILayout.Label($"  ClientConfig(", this.style);
					GUILayout.Label($"连接地址:");
					clientConfig.Address = EditorGUILayout.TextField(clientConfig.Address, GUILayout.Width(120));
					GUILayout.Label($"),", this.style);
				}

				DBConfig dbConfig = startConfig.GetComponent<DBConfig>();
				if (dbConfig != null)
				{
					GUILayout.Label($"  DBConfig(", this.style);
					GUILayout.Label($"连接串:");
					dbConfig.ConnectionString = EditorGUILayout.TextField(dbConfig.ConnectionString);

					GUILayout.Label($"DBName:");
					dbConfig.DBName = EditorGUILayout.TextField(dbConfig.DBName);
					GUILayout.Label($"),", this.style);
				}
				GUILayout.EndHorizontal();
			}

			{
				GUILayout.BeginHorizontal(GUILayout.Width(200), GUILayout.Height(16));
				this.st = (StartConfigComponentType) EditorGUILayout.EnumPopup(this.st, GUILayout.Width(100));

				if (GUILayout.Button("添加组件", GUILayout.Height(16)))
				{
					Assembly assembly = Assembly.GetAssembly(typeof(Init));
					Type type = assembly.GetType($"ETModel.{this.st.ToString()}");
					startConfig.AddComponent(type);
				}
				
				if (GUILayout.Button("删除组件", GUILayout.Height(16)))
				{
					Assembly assembly = Assembly.GetAssembly(typeof(Init));
					Type type = assembly.GetType($"ETModel.{this.st.ToString()}");
					startConfig.RemoveComponent(type);
				}
				
				if (GUILayout.Button("删除该行配置", GUILayout.Height(16)))
				{
					startConfig.GetParent<StartConfig>().Remove(startConfig);
					return false;
				}
				
				GUILayout.EndHorizontal();
			}

			GUILayout.EndHorizontal();
			
			if (this.isFold)
			{
				foreach (StartConfig child in startConfig.List)
				{
					if (child.GetComponent<StartConfigDrawer>()?.OnGUI() == false)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
	
	public class ServerCommandLineEditor: EditorWindow
	{
		private const string ConfigDir = @"../Config/StartConfig/";

		private List<string> files;

		private int selectedIndex;

		private string fileName;

		private string newFileName = "";

		private StartConfig startConfig;

		[MenuItem("Tools/命令行配置")]
		private static void ShowWindow()
		{
			Game.EventSystem.Add(DLLType.Editor, typeof(ServerCommandLineEditor).Assembly);
			GetWindow(typeof (ServerCommandLineEditor));
		}

		private void OnEnable()
		{
			Game.EventSystem.Add(DLLType.Editor, typeof(ServerCommandLineEditor).Assembly);
			this.files = this.GetConfigFiles();
			if (this.files.Count > 0)
			{
				this.fileName = this.files[this.selectedIndex];
				this.LoadConfig();
			}
		}

		public void ClearConfig()
		{
			startConfig?.Dispose();
		}

		private List<string> GetConfigFiles()
		{
			List<string> fs = Directory.GetFiles(ConfigDir).ToList();
			DirectoryInfo directoryInfo = new DirectoryInfo(ConfigDir);
			FileInfo[] fileInfo = directoryInfo.GetFiles();
			fs = fileInfo.Select(x => x.Name).ToList();
			return fs;
		}

		private void LoadConfig()
		{
			string filePath = this.GetFilePath();
			if (!File.Exists(filePath))
			{
				return;
			}

			string s2 = "";
			try
			{
				this.ClearConfig();
				startConfig = MongoHelper.FromJson<StartConfig>(File.ReadAllText(filePath));
				this.startConfig.AddComponentNoPool<StartConfigDrawer, int>(0);
			}
			catch (Exception e)
			{
				Log.Error($"加载配置失败! {s2} \n {e}");
			}
		}

		private string GetFilePath()
		{
			return Path.Combine(ConfigDir, this.fileName);
		}

		private void Save()
		{
			string path = this.GetFilePath();
			File.WriteAllText(path, MongoHelper.ToJson(startConfig));
		}

		private Vector2 scrollPos;
		
		private void OnGUI()
		{
			{
				GUILayout.BeginHorizontal();
				string[] filesArray = this.files.ToArray();
				this.selectedIndex = EditorGUILayout.Popup(this.selectedIndex, filesArray);

				string lastFile = this.fileName;
				this.fileName = this.files[this.selectedIndex];

				if (this.fileName != lastFile)
				{
					this.LoadConfig();
				}

				this.newFileName = EditorGUILayout.TextField("文件名", this.newFileName);

				if (GUILayout.Button("添加"))
				{
					this.ClearConfig();
					
					this.startConfig = new StartConfig();
					startConfig.AddComponent<StartConfigDrawer>();
					this.fileName = this.newFileName;
					this.newFileName = "";
					File.WriteAllText(this.GetFilePath(), MongoHelper.ToJson(this.startConfig));
					
					this.files = this.GetConfigFiles();
					this.selectedIndex = this.files.IndexOf(this.fileName);
					this.LoadConfig();
				}

				if (GUILayout.Button("复制"))
				{
					this.fileName = $"{this.fileName}-copy";
					this.Save();
					this.files = this.GetConfigFiles();
					this.selectedIndex = this.files.IndexOf(this.fileName);
					this.newFileName = "";
				}

				if (GUILayout.Button("重命名"))
				{
					if (this.newFileName == "")
					{
						Log.Debug("请输入新名字!");
					}
					else
					{
						File.Delete(this.GetFilePath());
						this.fileName = this.newFileName;
						this.Save();
						this.files = this.GetConfigFiles();
						this.selectedIndex = this.files.IndexOf(this.fileName);
						this.newFileName = "";
					}
				}

				if (GUILayout.Button("删除"))
				{
					File.Delete(this.GetFilePath());
					this.files = this.GetConfigFiles();
					this.selectedIndex = 0;
					this.newFileName = "";
				}

				GUILayout.EndHorizontal();
			}
			
			scrollPos = GUILayout.BeginScrollView(this.scrollPos, true, true);
			
			startConfig.GetComponent<StartConfigDrawer>()?.OnGUI();
			
			GUILayout.EndScrollView();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("保存"))
			{
				this.Save();
			}
			
			if (GUILayout.Button("启动"))
			{
				string arguments = $"--config={this.fileName}";
				ProcessHelper.Run("App.exe", arguments, "../Bin/");
			}
			if (GUILayout.Button("启动数据库"))
			{
				ProcessHelper.Run("mongod", @"--dbpath=db", "../Database/bin/");
			}
			GUILayout.EndHorizontal();
		}
		
		private void OnDestroy()
		{
			this.ClearConfig();
		}
	}
}