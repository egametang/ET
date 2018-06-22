using System;
using System.Collections;
using System.Collections.Generic;

namespace ETHotfix
{
    public static class IllegalWordDetectionHelper
    {
        // <summary>
        // 存了所有的长度大于1的敏感词汇
        // </summary>
        static HashSet<string> wordsSet = new HashSet<string>();
        // <summary>
        // 存了某一个词在所有敏感词中的位置，（超出8个的截断为第8个位置）
        // </summary>
        static byte[] fastCheck = new byte[char.MaxValue];
        // <summary>
        // 存了所有敏感词的长度信息，“Key”值为所有敏感词的第一个词，敏感词的长度会截断为8
        // </summary>
        static byte[] fastLength = new byte[char.MaxValue];
        // <summary>
        // 保有所有敏感词汇的第一个词的记录，可用来判断是否一个词是一个或者多个敏感词汇的“第一个词”，且可判断以某一个词作为第一个词的一系列的敏感词的最大的长度
        // </summary>
        static byte[] startCache = new byte[char.MaxValue];
        static char[] dectectedBuffer = null;
        static string SkipList = " \t\r\n~!@#$%^&*()_+-=【】、{}|;':\"，。、《》？αβγδεζηθικλμνξοπρστυφχψωΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ。，、；：？！…—·ˉ¨‘’“”々～‖∶＂＇｀｜〃〔〕〈〉《》「」『』．〖〗【】（）［］｛｝ⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩⅪⅫ⒈⒉⒊⒋⒌⒍⒎⒏⒐⒑⒒⒓⒔⒕⒖⒗⒘⒙⒚⒛㈠㈡㈢㈣㈤㈥㈦㈧㈨㈩①②③④⑤⑥⑦⑧⑨⑩⑴⑵⑶⑷⑸⑹⑺⑻⑼⑽⑾⑿⒀⒁⒂⒃⒄⒅⒆⒇≈≡≠＝≤≥＜＞≮≯∷±＋－×÷／∫∮∝∞∧∨∑∏∪∩∈∵∴⊥∥∠⌒⊙≌∽√§№☆★○●◎◇◆□℃‰€■△▲※→←↑↓〓¤°＃＆＠＼︿＿￣―♂♀┌┍┎┐┑┒┓─┄┈├┝┞┟┠┡┢┣│┆┊┬┭┮┯┰┱┲┳┼┽┾┿╀╁╂╃└┕┖┗┘┙┚┛━┅┉┤┥┦┧┨┩┪┫┃┇┋┴┵┶┷┸┹┺┻╋╊╉╈╇╆╅╄";
        static BitArray SkipBitArray = new BitArray(char.MaxValue);
        // <summary>
        // 保有所有敏感词汇的最后一个词的记录，仅用来判断是否一个词是一个或者多个敏感词汇的“最后一个词”
        // </summary>
        static BitArray endCache = new BitArray(char.MaxValue);

        unsafe public static void Init(string minganstr)
        {
            string[] badwords = minganstr.Split('|');

            if (badwords == null || badwords.Length == 0) return;

            int wordLength = 0;
            int maxWordLength = int.MinValue;

            for (int stringIndex = 0, len = badwords.Length; stringIndex < len; ++stringIndex)
            {
                if (string.IsNullOrEmpty(badwords[stringIndex]))
                    continue;

                string strBadWord = OriginalToLower(badwords[stringIndex]);
                //求得单个的敏感词汇的长度
                wordLength = strBadWord.Length;
                maxWordLength = Math.Max(wordLength, maxWordLength);

                fixed (char* pWordStart = strBadWord)
                {
                    for (int i = 0; i < wordLength; ++i)
                    {
                        //准确记录8位以内的敏感词汇的某个词在词汇中的“位置”
                        if (i < 7)
                            fastCheck[*(pWordStart + i)] |= (byte)(1 << i);
                        else//8位以外的敏感词汇的词直接限定在第8位
                            fastCheck[*(pWordStart + i)] |= 0x80;//0x80在内存中即为1000 0000，因为一个byte顶多标示8位，故超出8位的都位或上0x80，截断成第8位
                    }

                    //缓存敏感词汇的长度
                    int cachedWordslength = Math.Min(8, wordLength);
                    char firstWord = *pWordStart;
                    //记录敏感词汇的“大致长度（超出8个字的敏感词汇会被截取成8的长度）”，“key”值为敏感词汇的第一个词
                    fastLength[firstWord] |= (byte)(1 << (cachedWordslength - 1));
                    //缓存出当前以badWord第一个字开头的一系列的敏感词汇的最长的长度
                    if (startCache[firstWord] < cachedWordslength)
                        startCache[firstWord] = (byte)(cachedWordslength);

                    //存好敏感词汇的最后一个词汇的“出现情况”
                    endCache[*(pWordStart + wordLength - 1)] = true;
                    //将长度大于1的敏感词汇都压入到字典中
                    if (!wordsSet.Contains(strBadWord))
                        wordsSet.Add(strBadWord);
                }
            }

            // 初始化好一个用来存检测到的字符串的buffer
            dectectedBuffer = new char[maxWordLength];
            // 记录应该跳过的不予检测的词
            fixed (char* start = SkipList)
            {
                char* itor = start;
                char* end = start + SkipList.Length;
                while (itor < end) SkipBitArray[*itor++] = true;
            }
        }

        unsafe static string OriginalToLower(string text)
        {
            fixed (char* newText = text)
            {
                char* itor = newText;
                char* end = newText + text.Length;
                char c;

                while (itor < end)
                {
                    c = *itor;

                    if ('A' <= c && c <= 'Z')
                    {
                        *itor = (char)(c | 0x20);
                    }

                    ++itor;
                }
            }

            return text;
        }

        unsafe static bool EnsuranceLower(string text)
        {
            fixed (char* newText = text)
            {
                char* itor = newText;
                char* end = newText + text.Length;
                char c;

                while (itor < end)
                {
                    c = *itor;

                    if ('A' <= c && c <= 'Z')
                    {
                        return true;
                    }

                    ++itor;
                }
            }

            return false;
        }

        // <summary>
        // 过滤字符串,默认遇到敏感词汇就以'*'代替
        // </summary>
        // <param name="text"></param>
        // <param name="mask"></param>
        // <returns></returns>
        unsafe public static string Filter(string text, string mask = "*")
        {
            Dictionary<int, int> dic = DetectIllegalWords(text);
            //如果没有敏感词汇，则直接返回出去
            if (dic.Count == 0)
                return text;

            fixed (char* newText = text, cMask = mask)
            {
                var itor = newText;
                Dictionary<int, int>.Enumerator enumerator = dic.GetEnumerator();
                //开始替换敏感词汇
                while (enumerator.MoveNext())
                {
                    //偏移到敏感词出现的位置
                    itor = newText + enumerator.Current.Key;
                    for (int index = 0; index < enumerator.Current.Value; index++)
                    {
                        //屏蔽掉敏感词汇
                        *itor++ = *cMask;
                    }
                }
                enumerator.Dispose();
            }

            return text;
        }

        // <summary>
        // 判断text是否有敏感词汇,如果有返回敏感的词汇的位置,利用指针操作来加快运算速度
        // </summary>
        // <param name="text"></param>
        // <returns></returns>
        unsafe public static Dictionary<int, int> DetectIllegalWords(string text)
        {
            var findResult = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(text)) return findResult;
            if (EnsuranceLower(text)) text = text.ToLower();
            var bufferLength = dectectedBuffer.Length;
            if (text.Length > bufferLength) dectectedBuffer = new char[bufferLength << 1];

            fixed (char* ptext = text, detectedStrStart = dectectedBuffer)
            {
                //缓存字符串的初始位置
                char* itor = (fastCheck[*ptext] & 0x01) == 0 ? ptext + 1 : ptext;
                //缓存字符串的末尾位置
                char* end = ptext + text.Length;

                while (itor < end)
                {
                    //如果text的第一个词不是敏感词汇或者当前遍历到了text第一个词的后面的词，则循环检测到text词汇的倒数第二个词，看看这一段子字符串中有没有敏感词汇
                    if ((fastCheck[*itor] & 0x01) == 0)
                    {
                        while (itor < end - 1 && (fastCheck[*(++itor)] & 0x01) == 0) ;
                    }

                    //如果有只有一个词的敏感词，且当前的字符串的“非第一个词”满足这个敏感词，则先加入已检测到的敏感词列表
                    if (startCache[*itor] != 0 && (fastLength[*itor] & 0x01) > 0)
                    {
                        //返回敏感词在text中的位置，以及敏感词的长度，供过滤功能用
                        findResult.Add((int)(itor - ptext), 1);
                    }

                    char* strItor = detectedStrStart;
                    *strItor++ = *itor;
                    int remainLength = (int)(end - itor - 1);
                    int skipCount = 0;
                    //此时已经检测到一个敏感词的“首词”了,记录下第一个检测到的敏感词的位置
                    //从当前的位置检测到字符串末尾
                    for (int i = 1; i <= remainLength; ++i)
                    {
                        char* subItor = itor + i;
                        // 跳过一些过滤的字符,比如空格特殊符号之类的
                        if (SkipBitArray[*subItor])
                        {
                            ++skipCount;
                            continue;
                        }
                        //如果检测到当前的词在所有敏感词中的位置信息中没有处在第i位的，则马上跳出遍历
                        if ((fastCheck[*subItor] >> Math.Min(i - skipCount, 7)) == 0)
                        {
                            break;
                        }

                        *strItor++ = *subItor;
                        //如果有检测到敏感词的最后一个词，并且此时的“检测到的敏感词汇”的长度也符合要求，则才进一步查看检测到的敏感词汇是否是真的敏感
                        if ((fastLength[*itor] >> Math.Min(i - 1 - skipCount, 7)) > 0 && endCache[*subItor])
                        {
                            //如果此子字符串在敏感词字典中存在，则记录。做此判断是避免敏感词中夹杂了其他敏感词的单词，而上面的算法无法剔除，故先用hash数组来剔除
                            //上述算法是用于减少大部分的比较消耗
                            if (wordsSet.Contains(new string(dectectedBuffer, 0, (int)(strItor - detectedStrStart))))
                            {
                                int curDectectedStartIndex = (int)(itor - ptext);
                                findResult[curDectectedStartIndex] = i + 1;
                                itor = subItor;
                                break;
                            }
                        }
                        else if (i - skipCount > startCache[*itor] && startCache[*itor] < 0x80)//如果超过了以该词为首的一系列的敏感词汇的最大的长度，则不继续判断(前提是该词对应的所有敏感词汇没有超过8个词的)
                        {
                            break;
                        }
                    }
                    ++itor;
                }
            }

            return findResult;
        }
    }
}
