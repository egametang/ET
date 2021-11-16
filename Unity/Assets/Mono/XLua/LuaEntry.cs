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

            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);
            this.luaEnv.DoString("require 'Main.lua'(true)");

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