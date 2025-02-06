using System;
using System.Collections.Generic;

namespace ET.PackageManager.Editor
{
    [Serializable]
    public struct StrChunk
    {
        /// <summary>
        /// 字符串，或者key
        /// 如果是字符串，keyIndex = 0;
        /// 如果是key，keyIndex表示索引，从1开始
        /// </summary>
        public string TextOrKey;

        /// <summary>
        /// 如果StrOrKey为Key时的索引值，从1开始
        /// </summary>
        public byte KeyIndex;
    }

    public static partial class StrUtil
    {
        /// <summary>
        /// 高性能处理字符串中{%key%num}形式的占位符
        /// 其中key为连续的小写字母,nul为非0开头的连续数字
        /// 返回一个StrChunk数组，方便使用的地方缓存以加速替换占位符
        /// </summary>
        /// <param name="value"></param>
        /// <param name="throwError">
        /// 是否抛出错误
        /// 如果为false，则会进行容错性解析
        /// </param>
        /// <returns></returns>
        public static StrChunk[] GetStrChunk(string value, bool throwError = false)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<StrChunk>();
            }

            var chunks  = g_cacheStrChunkList;
            var sb      = SbPool.Get();
            var keySb   = SbPool.Get();
            var indexSb = SbPool.Get();

            int            throwErrId = -1;
            char           lastChar   = char.MaxValue;
            ReadChunkState state      = ReadChunkState.ReadText;

            const char beginChar = '{';
            const char endChar   = '}';

            int chrCount = value.Length;
            for (int i = 0; i < chrCount; i++)
            {
                char c = value[i];
                switch (state)
                {
                    case ReadChunkState.ReadText:
                        switch (c)
                        {
                            case beginChar:
                                state = ReadChunkState.ReadyReadKey;
                                break;

                            case endChar:
                                state = ReadChunkState.Escape;
                                break;

                            default:
                                sb.Append(c);
                                break;
                        }

                        break;

                    case ReadChunkState.ReadyReadKey:

                        if (c == beginChar)
                        {
                            //{{，转义为{
                            sb.Append(beginChar);
                            state = ReadChunkState.ReadText;
                            break;
                        }

                        if (c < 'a' || c > 'z')
                        {
                            throwErrId = 1;
                            sb.Append(beginChar).Append(c);
                            state = ReadChunkState.ReadText;
                            break;
                        }

                        keySb.Append(c);
                        state = ReadChunkState.ReadKey;
                        break;

                    case ReadChunkState.ReadKey:

                        if (c >= 'a' && c <= 'z')
                        {
                            keySb.Append(c);
                            break;
                        }

                        if (c >= '1' && c <= '9')
                        {
                            indexSb.Append(c);
                            state = ReadChunkState.ReadIndex;
                            break;
                        }

                        throwErrId = 2;
                        sb.Append(beginChar).Append(keySb.ToString()).Append(c);
                        keySb.Clear();
                        state = ReadChunkState.ReadText;
                        break;

                    case ReadChunkState.ReadIndex:

                        if (c >= '0' && c <= '9')
                        {
                            indexSb.Append(c);
                            break;
                        }

                        if (c == endChar)
                        {
                            chunks.Add(new StrChunk() { TextOrKey = sb.ToString() });
                            chunks.Add(new StrChunk()
                                       {
                                           KeyIndex  = byte.Parse(indexSb.ToString()),
                                           TextOrKey = keySb.ToString()
                                       });
                            sb.Clear();
                        }
                        else
                        {
                            throwErrId = 3;
                            sb.Append(beginChar)
                              .Append(keySb.ToString())
                              .Append(indexSb.ToString())
                              .Append(c);
                        }

                        keySb.Clear();
                        indexSb.Clear();
                        state = ReadChunkState.ReadText;
                        break;

                    case ReadChunkState.Escape:
                        if (c == lastChar)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            throwErrId = 0;
                            sb.Append(lastChar).Append(c);
                        }

                        state = ReadChunkState.ReadText;
                        break;
                }

                //处理错误
                if (throwError && throwErrId > -1)
                {
                    switch (throwErrId)
                    {
                        case 0:
                            throw new Exception(string.Format("text={3}, index={0}, chr='{1}'：此处只能出现'{2}'", i, c,
                                                              lastChar, value));
                        case 1:
                            throw new Exception(string.Format("text={2}, index={0}, chr='{1}', 此处只能出现小写英文字符", i, c,
                                                              value));
                        case 2:
                            throw new Exception(string.Format("text={2}, index={0}, chr='{1}', 此处只能出现小写英文或1-9的数字", i, c,
                                                              value));
                        case 3:
                            throw new Exception(string.Format("text={2}, index={0}, chr='{1}', 0-9的数字或'}}'", i, c,
                                                              value));
                    }
                }

                lastChar = c;
            }

            //结尾处理
            if (state != ReadChunkState.ReadText)
            {
                if (throwError)
                {
                    throw new Exception("没有正确结束, state=" + state.ToString());
                }

                sb.Append(keySb.ToString()).Append(indexSb.ToString());
            }

            if (sb.Length > 0)
            {
                chunks.Add(new StrChunk() { TextOrKey = sb.ToString() });
            }

            SbPool.Put(sb);
            SbPool.Put(keySb);
            SbPool.Put(indexSb);
            var arr = chunks.ToArray();
            chunks.Clear();
            return arr;
        }

        private static readonly List<StrChunk> g_cacheStrChunkList = new List<StrChunk>();

        private enum ReadChunkState
        {
            ReadText,
            ReadyReadKey,
            ReadKey,
            ReadIndex,

            //转义处理
            Escape
        }
    }
}
