using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public static class ResPathHelper 
    {
        public static string GetUIPath(string ui)
        {
            return "Assets/Bundles/UI/"+ui;
        }
        public static string GetUnitPath(string unit)
        {
            return "Assets/Bundles/Unit/"+unit;
        }
        public static string GetScenePath(string scene)
        {
            return "Assets/Scenes/"+scene;
        }
        
        public static string GetConfigPath(string config)
        {
            return "Assets/Config/"+config;
        }
        
        public static string GetSpriteAltasPath(string sa)
        {
            return "Assets/SpriteAltas/"+sa;
        }
    }
}
