using System;
using System.Text;

namespace FairyGUI.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class XMLUtils
    {
        public static string DecodeString(string aSource)
        {
            int len = aSource.Length;
            StringBuilder sb = new StringBuilder();
            int pos1 = 0, pos2 = 0;

            while (true)
            {
                pos2 = aSource.IndexOf('&', pos1);
                if (pos2 == -1)
                {
                    sb.Append(aSource.Substring(pos1));
                    break;
                }
                sb.Append(aSource.Substring(pos1, pos2 - pos1));

                pos1 = pos2 + 1;
                pos2 = pos1;
                int end = Math.Min(len, pos2 + 10);
                for (; pos2 < end; pos2++)
                {
                    if (aSource[pos2] == ';')
                        break;
                }
                if (pos2 < end && pos2 > pos1)
                {
                    string entity = aSource.Substring(pos1, pos2 - pos1);
                    int u = 0;
                    if (entity[0] == '#')
                    {
                        if (entity.Length > 1)
                        {
                            if (entity[1] == 'x')
                                u = Convert.ToInt16(entity.Substring(2), 16);
                            else
                                u = Convert.ToInt16(entity.Substring(1));
                            sb.Append((char)u);
                            pos1 = pos2 + 1;
                        }
                        else
                            sb.Append('&');
                    }
                    else
                    {
                        switch (entity)
                        {
                            case "amp":
                                u = 38;
                                break;

                            case "apos":
                                u = 39;
                                break;

                            case "gt":
                                u = 62;
                                break;

                            case "lt":
                                u = 60;
                                break;

                            case "nbsp":
                                u = 32;
                                break;

                            case "quot":
                                u = 34;
                                break;
                        }
                        if (u > 0)
                        {
                            sb.Append((char)u);
                            pos1 = pos2 + 1;
                        }
                        else
                            sb.Append('&');
                    }
                }
                else
                {
                    sb.Append('&');
                }
            }

            return sb.ToString();
        }

        private static string[] ESCAPES = new string[] {
            "&", "&amp;",
            "<", "&lt;",
            ">", "&gt;",

            "'", "&apos;",
            "\"", "&quot;",
            "\t", "&#x9;",
            "\n", "&#xA;",
            "\r", "&#xD;"
        };
        public static void EncodeString(StringBuilder sb, int start, bool isAttribute = false)
        {
            int count;
            int len = isAttribute ? ESCAPES.Length : 6;
            for (int i = 0; i < len; i += 2)
            {
                count = sb.Length - start;
                sb.Replace(ESCAPES[i], ESCAPES[i + 1], start, count);
            }
        }

        public static string EncodeString(string str, bool isAttribute = false)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            else
            {
                StringBuilder sb = new StringBuilder(str);
                EncodeString(sb, 0);
                return sb.ToString();
            }
        }
    }
}
