using System;
using System.Text;

namespace I2.Loc
{
    // Simple String Obfucator 
    // (not particularly safe, but will stop most players from hacking your strings and its FAST)

	public class StringObfucator
	{
        // Change this for your projects if you need extra security
        public static char[] StringObfuscatorPassword = "ÝúbUu¸CÁÂ§*4PÚ©-á©¾@T6Dl±ÒWâuzÅm4GÐóØ$=Íg,¥Që®iKEßr¡×60Ít4öÃ~^«y:Èd1<QÛÝúbUu¸CÁÂ§*4PÚ©-á©¾@T6Dl±ÒWâuzÅm4GÐóØ$=Íg,¥Që®iKEßr¡×60Ít4öÃ~^«y:Èd".ToCharArray();

        public static string Encode(string NormalString)
        {
            try
            {
                var str = XoREncode(NormalString);
                return ToBase64(str);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string Decode(string ObfucatedString)
        {
            try
            {
                var str = FromBase64(ObfucatedString);
                return XoREncode(str);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static string ToBase64(string regularString)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(regularString);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        static string FromBase64(string base64string)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(base64string);
            return Encoding.UTF8.GetString(encodedDataAsBytes, 0, encodedDataAsBytes.Length);
        }

        static string XoREncode(string NormalString)
        {
            try
            {
                var pass = StringObfuscatorPassword;
                var buffer = NormalString.ToCharArray();

                var passlen = pass.Length;

                for (int i = 0, imax = buffer.Length; i < imax; ++i)
                    buffer[i] = (char)(buffer[i] ^ pass[i % passlen] ^ (byte)(i % 2 == 0 ? i * 23 : -i * 51));

                return new string(buffer);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}