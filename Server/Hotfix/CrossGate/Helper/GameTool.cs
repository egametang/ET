using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ETHotfix
{
    public class GameTool
    {
        public static string GetMd5(string str)
        {
            byte[] strByte = Encoding.UTF8.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(strByte);
            return BitConverter.ToString(result).Replace("-", "");
        }

        public static string GuidTo8String() //获得8位的Guid
        {
            string KeleyiStr = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] rtn = new char[8];
            Guid gid = Guid.NewGuid();
            var ba = gid.ToByteArray();
            for (var i = 0; i < 8; i++)
            {
                rtn[i] = KeleyiStr[(ba[i] + ba[8 + i]) % 35];
            }
            return "" + rtn[0] + rtn[1] + rtn[2] + rtn[3] + rtn[4] + rtn[5] + rtn[6] + rtn[7];
        }

        public static bool CharacterDetection(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z0-9_\u4e00-\u9fa5]+$");
        }

        public static bool ContainCharacterId(int id)
        {
            if (id == 100002 || id == 100027 || id == 100052 || id == 100077 || id == 100102 || id == 100127 || id == 100152 || id == 106002 || id == 106027 ||
                id == 106052 || id == 106077 || id == 106102 || id == 106127 || id == 106152 || id == 100252 || id == 100277 || id == 100302 || id == 100327 ||
                id == 100352 || id == 100377 || id == 100402 || id == 106252 || id == 106277 || id == 106302 || id == 106327 || id == 106352 || id == 106377 || id == 106402)
            {
                return true;
            }
            return false;
        }
    }
}
