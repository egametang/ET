using System;
using XLua;

namespace ET
{
    public class LuaEntry: IEntry
    {
        private LuaEnv luaEnv;

        private Action update;
        private Action lateUpdate;
        private Action onApplicationQuit;

        private LuaTable scriptEnv;
        
        public void Start()
        {
            luaEnv = new LuaEnv();

            luaEnv.AddLoader(LuaLoader.Load);
            
            scriptEnv = luaEnv.NewTable();

            this.luaEnv.DoString("require 'Main.lua'()");

            scriptEnv.Get("Update", out this.update);
            scriptEnv.Get("LateUpdate", out this.lateUpdate);
            scriptEnv.Get("OnApplicationQuit", out onApplicationQuit);
        }

        public void Update()
        {
            this.update?.Invoke();
        }

        public void LateUpdate()
        {
            this.lateUpdate?.Invoke();
        }

        public void OnApplicationQuit()
        {
            this.onApplicationQuit?.Invoke();
            
            this.luaEnv.Dispose();
        }
    }
}