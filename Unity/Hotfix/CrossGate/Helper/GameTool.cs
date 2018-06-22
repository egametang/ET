using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ETHotfix
{
    public static class GameTool
    {
        public static void ShowPopMessage(string msg, bool playerrorsound = false)
        {
            UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.UIWarnningMessage);
            ui.GetComponent<UIWarnningMessageComponent>().ShowMessage(msg);
        }

        public static string GetMd5(string str)
        {
            byte[] strByte = Encoding.UTF8.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(strByte);
            return BitConverter.ToString(result).Replace("-", "");
        }

        public static bool CharacterDetection(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z0-9_\u4e00-\u9fa5]+$");
        }
    }
}
