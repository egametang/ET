/********************************************************************
				Copyright (c) 2018, Tadpole Studio
					All rights reserved
 
	文件名称：	RTL.cs
	说	明：		RTL字符转换
    
	版	本：		1.00
	时	间：		2018/2/24 9:02:12
	作	者：		AQ		QQ:355010366
	概	述：		新建

*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace FairyGUI
{
    public class RTLSupport
    {
        internal enum CharState
        {
            isolated,
            final,
            lead,
            middle,
        }

        // Bidirectional Character Types
        public enum DirectionType
        {
            UNKNOW = 0,
            LTR,
            RTL,
            NEUTRAL,
        }

#if RTL_TEXT_SUPPORT
        public static DirectionType BaseDirection = DirectionType.RTL;    // 主体语言是否是RTL的语言
#else
        public static DirectionType BaseDirection = DirectionType.UNKNOW;
#endif
        private static bool isCharsInitialized = false;
        private static Dictionary<int, char[]> mapping = new Dictionary<int, char[]>();

        private static List<char> listFinal = new List<char>();
        private static List<string> listRep = new List<string>();
        private static StringBuilder sbRep = new StringBuilder();
        private static StringBuilder sbN = new StringBuilder();
        private static StringBuilder sbFinal = new StringBuilder();
        private static StringBuilder sbReverse = new StringBuilder();

        public static bool IsArabicLetter(char ch)
        {
            if (ch >= 0x600 && ch <= 0x6ff)
            {
                if ((ch >= 0x660 && ch <= 0x66D) || (ch >= 0x6f0 && ch <= 0x6f9)) // 标准阿拉伯语数字和东阿拉伯语数字 [2019/3/1/ 17:45:18 by aq_1000]
                {
                    return false;
                }
                return true;
            }
            else if (ch >= 0x750 && ch <= 0x77f)
                return true;
            else if (ch >= 0xfb50 && ch <= 0xfc3f)
                return true;
            else if (ch >= 0xfe70 && ch <= 0xfefc)
                return true;
            return false;
        }

        public static string ConvertNumber(string strNumber)
        {
            sbRep.Length = 0;
            foreach (char ch in strNumber)
            {
                int un = ch;
                if (un == 0x66c || ch == ',')   // 去掉逗号
                    continue;
                else if (un == 0x660 || un == 0x6f0)
                    sbRep.Append('0');
                else if (un == 0x661 || un == 0x6f1)
                    sbRep.Append('1');
                else if (un == 0x662 || un == 0x6f2)
                    sbRep.Append('2');
                else if (un == 0x663 || un == 0x6f3)
                    sbRep.Append('3');
                else if (un == 0x664 || un == 0x6f4)
                    sbRep.Append('4');
                else if (un == 0x665 || un == 0x6f5)
                    sbRep.Append('5');
                else if (un == 0x666 || un == 0x6f6)
                    sbRep.Append('6');
                else if (un == 0x667 || un == 0x6f7)
                    sbRep.Append('7');
                else if (un == 0x668 || un == 0x6f8)
                    sbRep.Append('8');
                else if (un == 0x669 || un == 0x6f9)
                    sbRep.Append('9');
                else
                    sbRep.Append(ch);
            }
            return sbRep.ToString();
        }

        public static bool ContainsArabicLetters(string text)
        {
            foreach (char character in text)
            {
                if (character >= 0x600 && character <= 0x6ff)
                    return true;

                if (character >= 0x750 && character <= 0x77f)
                    return true;

                if (character >= 0xfb50 && character <= 0xfc3f)
                    return true;

                if (character >= 0xfe70 && character <= 0xfefc)
                    return true;
            }
            return false;
        }

        // 检测文本主方向
        public static DirectionType DetectTextDirection(string text)
        {
            bool isContainRTL = false;
            bool isContainLTR = false;
            foreach (char ch in text)
            {
                if (IsArabicLetter(ch))
                {
                    isContainRTL = true;
                    if (isContainLTR)
                        break;
                }
                else if (char.IsLetter(ch))
                {
                    isContainLTR = true;
                    if (isContainRTL)
                        break;
                }
            }
            if (!isContainRTL)
                return DirectionType.UNKNOW;    // 这边unknow暂时代表文本一个RTL字符都没有，无需进行RTL转换
            else if (!isContainLTR)
                return DirectionType.RTL;
            return BaseDirection;
        }

        private static bool CheckSeparator(char input)
        {
            if (!IsArabicLetter(input))
            {
                return true;
            }
            else
            {
                return (input == '،') || (input == '?') || (input == '؟');
            }

            //             if ((input != ' ') && (input != '\t') && (input != '!') && (input != '.') && 
            //                 (input != '،') && (input != '?') && (input != '؟') && 
            //                 !_IsBracket(input) &&   // 括号也算 [2018/8/1/ 15:12:20 by aq_1000]
            //                 !_IsNeutrality(input))
            //             {
            //                 return false;
            //             }
            //             return true;
        }

        private static bool CheckSpecific(char input)
        {
            int num = input;
            if ((num != 0x622) && (num != 0x623) && (num != 0x627) && (num != 0x62f) && (num != 0x625) &&
                (num != 0x630) && (num != 0x631) && (num != 0x632) && (num != 0x698) && (num != 0x648) &&
                !_CheckSoundmark(input))
            {
                return false;
            }
            return true;
        }

        private static bool _CheckSoundmark(char ch)
        {
            int un = ch;
            return (un >= 0x610 && un <= 0x61e) || (un >= 0x64b && un <= 0x65f);
        }

        public static string DoMapping(string input)
        {
            if (!isCharsInitialized)
            {
                isCharsInitialized = true;
                InitChars();
            }

            // 伊斯兰教真主安拉在阿拉伯文里写作الله
            // 键盘输入时输入 ل (lam) + ل (lam) + ه (ha) 后会自动转换成带记号的符号。 [2018/3/13 20:03:45 --By aq_1000]
            if (input == "الله")
            {
                input = "ﷲ";
            }

            sbFinal.Length = 0;
            sbFinal.Append(input);
            char perChar = '\0';
            for (int i = 0; i < sbFinal.Length; i++)
            {
                if (!mapping.ContainsKey(sbFinal[i]))
                {
                    perChar = sbFinal[i];
                    continue;
                }

                if ((i + 1) == sbFinal.Length)
                {
                    if (sbFinal.Length == 1)
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.isolated];
                    }
                    else if (CheckSeparator(perChar) || CheckSpecific(perChar))
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.isolated];
                    }
                    else
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.final];
                    }
                }
                else if (i == 0)
                {
                    if (!CheckSeparator(sbFinal[i + 1]))
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.lead];
                    }
                    else
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.isolated];
                    }
                }
                else if (CheckSeparator(sbFinal[i + 1]))
                {
                    if (CheckSeparator(perChar) || CheckSpecific(perChar))
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.isolated];
                    }
                    else
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.final];
                    }
                }
                else if (CheckSeparator(perChar))
                {
                    if (CheckSeparator(sbFinal[i + 1]))
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.isolated];
                    }
                    else
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.lead];
                    }
                }
                else if (CheckSpecific(sbFinal[i + 1]))
                {
                    if (CheckSeparator(perChar) || CheckSpecific(perChar))
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.lead];
                    }
                    else
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.middle];
                    }
                }
                else if (CheckSpecific(perChar))
                {
                    if (CheckSeparator(sbFinal[i + 1]))
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.isolated];
                    }
                    else
                    {
                        perChar = sbFinal[i];
                        sbFinal[i] = mapping[sbFinal[i]][(int)CharState.lead];
                    }
                }
                else
                {
                    perChar = sbFinal[i];
                    sbFinal[i] = mapping[sbFinal[i]][(int)CharState.middle];
                }
            }
            return sbFinal.ToString();
        }

        // 主体语言是LTR
        public static string ConvertLineL(string source)
        {
            listFinal.Clear();
            listRep.Clear();
            sbRep.Length = 0;
            sbN.Length = 0;
            int iReplace = 0;
            DirectionType ePre = DirectionType.LTR;
            char nextChar = '\0';
            for (int j = 0; j < source.Length; j++)
            {
                if (j < source.Length - 1)
                    nextChar = source[j + 1];
                else
                    nextChar = '\0';
                char item = source[j];
                DirectionType eCType = _GetDirection(item, nextChar, ePre, DirectionType.LTR);
                if (eCType == DirectionType.RTL)
                {
                    if (sbRep.Length == 0)
                    {
                        listFinal.Add('\x00bf');
                        iReplace++;
                    }

                    if (sbN.Length > 0)
                        sbRep.Append(sbN.ToString());
                    sbN.Length = 0;
                    sbRep.Append(item);
                }
                else if (eCType == DirectionType.LTR)
                {
                    if (sbRep.Length > 0)
                    {
                        listRep.Add(sbRep.ToString());
                    }
                    sbRep.Length = 0;

                    if (sbN.Length > 0)
                    {
                        for (int n = 0; n < sbN.Length; ++n)
                        {
                            listFinal.Add(sbN[n]);
                        }
                    }
                    sbN.Length = 0;

                    //                    item = _ProcessBracket(item);       // 文本主方向为LTR的话，括号不需要翻转 [2018/12/20/ 17:03:23 by aq_1000]
                    listFinal.Add(item);
                }
                else
                {
                    sbN.Append(item);
                }
                ePre = eCType;
            }
            if (sbRep.Length > 0)
            {
                listRep.Add(sbRep.ToString());
            }

            // 一行中都只有中立字符的情况，就直接返回中立字符 [2018/12/6/ 16:34:38 by aq_1000]
            if (sbN.Length > 0)
            {
                for (int n = 0; n < sbN.Length; ++n)
                {
                    listFinal.Add(sbN[n]);
                }
            }

            sbRep.Length = 0;
            sbN.Length = 0;
            sbFinal.Length = 0;
            sbFinal.Append(listFinal.ToArray());
            for (int m = 0; m < iReplace; m++)
            {
                for (int n = 0; n < sbFinal.Length; n++)
                {
                    if (sbFinal[n] == '\x00bf')
                    {
                        sbRep.Length = 0;
                        sbRep.Append(_Reverse(listRep[0]));
                        listRep.RemoveAt(0);

                        // 字符串反向的时候造成末尾空格跑到词首 [2018/4/11 20:04:35 --By aq_1000]
                        sbN.Length = 0;
                        for (int num4 = 0; num4 < sbRep.Length; num4++)
                        {
                            if (!_IsNeutrality(sbRep[num4]))
                                break;
                            sbN.Append(sbRep[num4]);
                        }
                        if (sbN.Length > 0)    // 词首空格重新放到词尾
                        {
                            sbRep.Remove(0, sbN.Length);
                            for (int iSpace = sbN.Length - 1; iSpace >= 0; --iSpace)   // 空格也要取反
                            {
                                sbRep.Append(sbN[iSpace]);
                            }
                        }

                        sbFinal.Replace(sbFinal[n].ToString(), sbRep.ToString(), n, 1);
                        break;
                    }
                }
            }
            return sbFinal.ToString();
        }

        // 主体语言是RTL，整个文本就从右往左读，LTR语言就作为嵌入语段处理
        public static string ConvertLineR(string source)
        {
            listFinal.Clear();
            listRep.Clear();
            sbRep.Length = 0;
            sbN.Length = 0;
            int iReplace = 0;
            DirectionType ePre = DirectionType.RTL;
            char nextChar = '\0';
            for (int j = 0; j < source.Length; j++)
            {
                if (j < source.Length - 1)
                    nextChar = source[j + 1];
                else
                    nextChar = '\0';
                char item = source[j];
                DirectionType eCType = _GetDirection(item, nextChar, ePre, DirectionType.RTL);
                if (eCType == DirectionType.LTR)
                {
                    if (sbRep.Length == 0)
                    {
                        listFinal.Add('\x00bf');
                        iReplace++;
                    }

                    if (sbN.Length > 0)
                        sbRep.Append(sbN.ToString());
                    sbN.Length = 0;
                    sbRep.Append(item);
                }
                else if (eCType == DirectionType.RTL)
                {
                    if (sbRep.Length > 0)
                    {
                        listRep.Add(sbRep.ToString());
                    }
                    sbRep.Length = 0;

                    if (sbN.Length > 0)
                    {
                        for (int n = 0; n < sbN.Length; ++n)
                        {
                            listFinal.Add(sbN[n]);
                        }
                    }
                    sbN.Length = 0;

                    item = _ProcessBracket(item);
                    listFinal.Add(item);
                }
                else
                {
                    sbN.Append(item);
                }
                ePre = eCType;
            }
            if (sbRep.Length > 0)
            {
                listRep.Add(sbRep.ToString());
            }

            // 一行中都只有中立字符的情况，就直接返回中立字符 [2018/12/6/ 16:34:38 by aq_1000]
            if (sbN.Length > 0)
            {
                for (int n = 0; n < sbN.Length; ++n)
                {
                    listFinal.Add(sbN[n]);
                }
            }

            sbFinal.Length = 0;
            sbFinal.Append(listFinal.ToArray());
            for (int m = 0; m < iReplace; m++)
            {
                for (int n = 0; n < sbFinal.Length; n++)
                {
                    if (sbFinal[n] == '\x00bf')
                    {
                        sbRep.Length = 0;
                        sbRep.Append(_Reverse(listRep[0]));
                        listRep.RemoveAt(0);

                        // 字符串反向的时候造成末尾空格跑到词首 [2018/4/11 20:04:35 --By aq_1000]
                        sbN.Length = 0;
                        for (int num4 = 0; num4 < sbRep.Length; num4++)
                        {
                            if (!_IsNeutrality(sbRep[num4]))
                                break;
                            sbN.Append(sbRep[num4]);
                        }
                        if (sbN.Length > 0)    // 词首空格重新放到词尾
                        {
                            sbRep.Remove(0, sbN.Length);
                            for (int iSpace = sbN.Length - 1; iSpace >= 0; --iSpace)   // 空格也要取反
                            {
                                sbRep.Append(sbN[iSpace]);
                            }
                        }

                        sbFinal.Replace(sbFinal[n].ToString(), sbRep.ToString(), n, 1);
                        break;
                    }
                }
            }
            return sbFinal.ToString();
        }


        private static string _Reverse(string source)
        {
            sbReverse.Length = 0;
            int len = source.Length;
            int i = len - 1;
            while (i >= 0)
            {
                char ch = source[i];
                if (ch == '\r' && i != len - 1 && source[i + 1] == '\n')
                {
                    i--;
                    continue;
                }

                if (char.IsLowSurrogate(ch)) //不要反向高低代理对
                {
                    sbReverse.Append(source[i - 1]);
                    sbReverse.Append(ch);
                    i--;
                }
                else
                    sbReverse.Append(ch);
                i--;
            }

            return sbReverse.ToString();
        }

        private static void InitChars()
        {
            // {isolated,final,lead,middle} [2018/11/27 16:04:18 --By aq_1000]
            mapping.Add(0x621, new char[4] { (char)0xFE80, (char)0xFE8A, (char)0xFE8B, (char)0xFE8C });    // Hamza      
            mapping.Add(0x627, new char[4] { (char)0xFE8D, (char)0xFE8E, (char)0xFE8D, (char)0xFE8E });    // Alef       
            mapping.Add(0x623, new char[4] { (char)0xFE83, (char)0xFE84, (char)0xFE83, (char)0xFE84 });    // AlefHamza
            mapping.Add(0x624, new char[4] { (char)0xFE85, (char)0xFE85, (char)0xFE85, (char)0xFE85 });    // WawHamza
            mapping.Add(0x625, new char[4] { (char)0xFE87, (char)0xFE87, (char)0xFE87, (char)0xFE87 });    // AlefMaksoor
            mapping.Add(0x649, new char[4] { (char)0xFBFC, (char)0xFBFD, (char)0xFBFE, (char)0xFBFF });    // AlefMagsora
            mapping.Add(0x626, new char[4] { (char)0xFE89, (char)0xFE8A, (char)0xFE8B, (char)0xFE8C });    // HamzaNabera
            mapping.Add(0x628, new char[4] { (char)0xFE8F, (char)0xFE90, (char)0xFE91, (char)0xFE92 });    // Ba
            mapping.Add(0x62A, new char[4] { (char)0xFE95, (char)0xFE96, (char)0xFE97, (char)0xFE98 });    // Ta
            mapping.Add(0x62B, new char[4] { (char)0xFE99, (char)0xFE9A, (char)0xFE9B, (char)0xFE9C });    // Tha2
            mapping.Add(0x62C, new char[4] { (char)0xFE9D, (char)0xFE9E, (char)0xFE9F, (char)0xFEA0 });    // Jeem
            mapping.Add(0x62D, new char[4] { (char)0xFEA1, (char)0xFEA2, (char)0xFEA3, (char)0xFEA4 });    // H7aa
            mapping.Add(0x62E, new char[4] { (char)0xFEA5, (char)0xFEA6, (char)0xFEA7, (char)0xFEA8 });    // Khaa2
            mapping.Add(0x62F, new char[4] { (char)0xFEA9, (char)0xFEAA, (char)0xFEA9, (char)0xFEAA });    // Dal
            mapping.Add(0x630, new char[4] { (char)0xFEAB, (char)0xFEAC, (char)0xFEAB, (char)0xFEAC });    // Thal
            mapping.Add(0x631, new char[4] { (char)0xFEAD, (char)0xFEAE, (char)0xFEAD, (char)0xFEAD });    // Ra2
            mapping.Add(0x632, new char[4] { (char)0xFEAF, (char)0xFEB0, (char)0xFEAF, (char)0xFEB0 });    // Zeen
            mapping.Add(0x633, new char[4] { (char)0xFEB1, (char)0xFEB2, (char)0xFEB3, (char)0xFEB4 });    // Seen
            mapping.Add(0x634, new char[4] { (char)0xFEB5, (char)0xFEB6, (char)0xFEB7, (char)0xFEB8 });    // Sheen
            mapping.Add(0x635, new char[4] { (char)0xFEB9, (char)0xFEBA, (char)0xFEBB, (char)0xFEBC });    // S9a
            mapping.Add(0x636, new char[4] { (char)0xFEBD, (char)0xFEBE, (char)0xFEBF, (char)0xFEC0 });    // Dha
            mapping.Add(0x637, new char[4] { (char)0xFEC1, (char)0xFEC2, (char)0xFEC3, (char)0xFEC4 });    // T6a
            mapping.Add(0x638, new char[4] { (char)0xFEC5, (char)0xFEC6, (char)0xFEC7, (char)0xFEC8 });    // T6ha
            mapping.Add(0x639, new char[4] { (char)0xFEC9, (char)0xFECA, (char)0xFECB, (char)0xFECC });    // Ain
            mapping.Add(0x63A, new char[4] { (char)0xFECD, (char)0xFECE, (char)0xFECF, (char)0xFED0 });    // Gain
            mapping.Add(0x641, new char[4] { (char)0xFED1, (char)0xFED2, (char)0xFED3, (char)0xFED4 });    // Fa
            mapping.Add(0x642, new char[4] { (char)0xFED5, (char)0xFED6, (char)0xFED7, (char)0xFED8 });    // Gaf
            mapping.Add(0x643, new char[4] { (char)0xFED9, (char)0xFEDA, (char)0xFEDB, (char)0xFEDC });    // Kaf
            mapping.Add(0x644, new char[4] { (char)0xFEDD, (char)0xFEDE, (char)0xFEDF, (char)0xFEE0 });    // Lam
            mapping.Add(0x645, new char[4] { (char)0xFEE1, (char)0xFEE2, (char)0xFEE3, (char)0xFEE4 });    // Meem
            mapping.Add(0x646, new char[4] { (char)0xFEE5, (char)0xFEE6, (char)0xFEE7, (char)0xFEE8 });    // Noon
            mapping.Add(0x647, new char[4] { (char)0xFEE9, (char)0xFEEA, (char)0xFEEB, (char)0xFEEC });    // Ha
            mapping.Add(0x648, new char[4] { (char)0xFEED, (char)0xFEEE, (char)0xFEED, (char)0xFEEE });    // Waw
            mapping.Add(0x64A, new char[4] { (char)0xFEF1, (char)0xFEF2, (char)0xFEF3, (char)0xFEF4 });    // Ya
            mapping.Add(0x622, new char[4] { (char)0xFE81, (char)0xFE81, (char)0xFE81, (char)0xFE81 });    // AlefMad
            mapping.Add(0x629, new char[4] { (char)0xFE93, (char)0xFE94, (char)0xFE94, (char)0xFE94 });    // TaMarboota // 该字符只会出现在末尾 [2018/4/10 16:04:18 --By aq_1000]
            mapping.Add(0x67E, new char[4] { (char)0xFB56, (char)0xFB57, (char)0xFB58, (char)0xFB59 });    // PersianPe
            mapping.Add(0x686, new char[4] { (char)0xFB7A, (char)0xFB7B, (char)0xFB7C, (char)0xFB7D });    // PersianChe
            mapping.Add(0x698, new char[4] { (char)0xFB8A, (char)0xFB8B, (char)0xFB8A, (char)0xFB8B });    // PersianZe
            mapping.Add(0x6AF, new char[4] { (char)0xFB92, (char)0xFB93, (char)0xFB94, (char)0xFB95 });    // PersianGaf
            mapping.Add(0x6A9, new char[4] { (char)0xFB8E, (char)0xFB8F, (char)0xFB90, (char)0xFB91 });    // PersianGaf2

            mapping.Add(0x6BE, new char[4] { (char)0xFEE9, (char)0xFEEA, (char)0xFEEB, (char)0xFEEC });
            mapping.Add(0x6CC, new char[4] { (char)0xFBFC, (char)0xFBFD, (char)0xFBFE, (char)0xFBFF });
        }

        // 是否中立方向字符
        private static bool _IsNeutrality(char uc)
        {
            return (uc == ':' || uc == '：' || uc == ' ' || /*uc == '%' ||*/ /*uc == '+' ||*/ /*uc == '-' ||*/ uc == '\n' || uc == '\r' || uc == '\t' || uc == '@' ||
                (uc >= 0x2600 && uc <= 0x27BF)); // 表情符号
        }

        // 是否句末标点符号
        private static bool _IsEndPunctuation(char uc, char nextChar)
        {
            if (uc == '.')
                return _IsNeutrality(nextChar);
            return (uc == '!' || uc == '！' || uc == '。' || uc == '،' || uc == '?' || uc == '؟');
        }

        // 判断字符方向
        private static DirectionType _GetDirection(char uc, char nextChar, DirectionType ePre, DirectionType eBase)
        {
            DirectionType eCType = ePre;
            int uni = uc;

            if (_IsBracket(uc) || _IsEndPunctuation(uc, nextChar))
            {
                eCType = eBase;    // 括号和句末标点符号，方向根据上个字符为准 [2018/12/26/ 15:56:24 by aq_1000]
            }
            else if ((uni >= 0x660) && (uni <= 0x66D))
            {
                eCType = DirectionType.LTR;
            }
            else if (IsArabicLetter(uc) || uc == '+' /*|| uc == '-'*/ || uc == '%')
            {
                eCType = DirectionType.RTL;
            }
            else if (uc == '-')
            {
                if (char.IsNumber(nextChar))
                    eCType = DirectionType.LTR;
                else
                    eCType = DirectionType.RTL;
            }
            else if (_IsNeutrality(uc))    // 中立方向字符，方向就和上一个字符一样 [2018/3/24 16:03:27 --By aq_1000]
            {
                if (ePre == DirectionType.UNKNOW || ePre == DirectionType.NEUTRAL)
                {
                    if (char.IsNumber(nextChar))    // 数字都是弱LTR方向符，开头中立字符后面紧跟着数字的话，中立字符方向算文本主方向 [IsDigit()只是0-9] [2018/12/20/ 16:32:32 by aq_1000]
                    {
                        eCType = BaseDirection;
                    }
                    else
                        eCType = DirectionType.NEUTRAL;
                }
                else
                    eCType = ePre;
            }
            else
                eCType = DirectionType.LTR;

            return eCType;
        }

        // 是否括号
        private static bool _IsBracket(char uc)
        {
            return (uc == ')' || uc == '(' || uc == '）' || uc == '（' ||
                    uc == ']' || uc == '[' || uc == '】' || uc == '【' ||
                    uc == '}' || uc == '{' ||
                    //                   uc == '≥' || uc == '≤' || uc == '>' || uc == '<' || 
                    uc == '》' || uc == '《' || uc == '“' || uc == '”' || uc == '"');
        }

        // 这些配对符,在从右至左排列中应该逆序显示
        private static char _ProcessBracket(char uc)
        {
            if (uc == '[') return ']';
            else if (uc == ']') return '[';
            else if (uc == '【') return '】';
            else if (uc == '】') return '【';
            else if (uc == '{') return '}';
            else if (uc == '}') return '{';
            else if (uc == '(') return ')';
            else if (uc == ')') return '(';
            else if (uc == '（') return '）';
            else if (uc == '）') return '（';
            else if (uc == '<') return '>';
            else if (uc == '>') return '<';
            else if (uc == '《') return '》';
            else if (uc == '》') return '《';
            else if (uc == '≤') return '≥';
            else if (uc == '≥') return '≤';
            else if (uc == '”') return '“';
            else if (uc == '”') return '“';
            else return uc;
        }
    }
}

