using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
	public static class Init
	{
		public static void Start()
		{
			try
			{
			    //关闭手机屏幕常亮
			    Screen.sleepTimeout = SleepTimeout.SystemSetting;

			    //设置为允许后台运行
			    Application.runInBackground = true;

                Game.Scene.ModelScene = ETModel.Game.Scene;

				// 注册热更层回调
				ETModel.Game.Hotfix.Update = () => { Update(); };
				ETModel.Game.Hotfix.LateUpdate = () => { LateUpdate(); };
				ETModel.Game.Hotfix.OnApplicationQuit = () => { OnApplicationQuit(); };
				
				Game.Scene.AddComponent<UIComponent>();
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

                // 客户端自定义全局组件, 用于保存本地数据:

			    // 加载Sqllite数据库
			    Game.Scene.AddComponent<SqlliteComponent>();

                // 资源读取组件
			    Game.Scene.AddComponent<ResourceComponent>();

                // 我的角色数据
                Game.Scene.AddComponent<ClientComponent>();

                // 场景内玩家数据
			    Game.Scene.AddComponent<GamePlayerComponent>();

			    // 场景内游戏物体
			    Game.Scene.AddComponent<SceneObjectComponent>();

                // 游戏地图组件
                Game.Scene.AddComponent<MapComponent>();

                // 射线检测组件
			    Game.Scene.AddComponent<RaycastComponent>();

                // 游戏物体对象池
                Game.Scene.AddComponent<GameObjectPoolComponent>();

                // 加载热更配置
                ETModel.Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
				Game.Scene.AddComponent<ConfigComponent>(); //Awake里会自动Load
				ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");

			    // 加载敏感字库
			    ConfigHelper.LoadMinGanText();
                
                // 测试读取配置表
                //UnitConfig unitConfig = (UnitConfig)Game.Scene.GetComponent<ConfigComponent>().Get(typeof(UnitConfig), 1001);
                //Log.Debug($"config {JsonHelper.ToJson(unitConfig)}");

                // 创造第一个UI界面
                Game.EventSystem.Run(EventIdType.InitSceneStart);
            }
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
        
	    public static void Update()
		{
			try
			{
				Game.EventSystem.Update();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void LateUpdate()
		{
			try
			{
				Game.EventSystem.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void OnApplicationQuit()
		{
		    PlayerPrefs.SetInt("GameConfigUpdate", 0);
            Game.Close();
		}
	}
}