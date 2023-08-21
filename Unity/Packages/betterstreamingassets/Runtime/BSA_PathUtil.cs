// Better Streaming Assets, Piotr Gwiazdowski <gwiazdorrr+github at gmail.com>, 2017

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Better.StreamingAssets
{
    public static partial class PathUtil
    {
        private enum NormalizeState
        {
            PrevSlash,
            PrevDot,
            PrevDoubleDot,
            NothingSpecial,
        }

        public static bool IsDirectorySeparator(char c)
        {
            return c == '/' || c == '\\';
        }

        public static string FixTrailingDirectorySeparators(string path)
        {
            if ( path.Length >= 2 )
            {
                var lastChar = path[path.Length - 1];
                var prevChar = path[path.Length - 2];
                if ( PathUtil.IsDirectorySeparator(lastChar) && PathUtil.IsDirectorySeparator(prevChar) )
                {
                    return path.TrimEnd('\\', '/') + lastChar;
                }
            }

            return path;
        }
        
        public static string CombineSlash(string a, string b)
        {
            if ( a == null )
                throw new ArgumentNullException("a");
            if ( b == null )
                throw new ArgumentNullException("b");

            if ( string.IsNullOrEmpty(b) )
                return a;
            if ( string.IsNullOrEmpty(a) )
                return b;

            if (b[0] == '/')
                return b;

            if ( IsDirectorySeparator(a[a.Length -1]) )
                return a + b;
            else
                return a + '/' + b;
        }

        public static string NormalizeRelativePath(string relative, bool forceTrailingSlash = false)
        {
            if (string.IsNullOrEmpty(relative))
                throw new System.ArgumentException("Empty or null", "relative");

            StringBuilder output = new StringBuilder(relative.Length);

            NormalizeState state = NormalizeState.PrevSlash;
            output.Append('/');

            int startIndex = 0;
            int lastIndexPlus1 = relative.Length;

            if ( relative[0] == '"' && relative.Length > 2 && relative[relative.Length - 1] == '"')
            {
                startIndex++;
                lastIndexPlus1--;
            }

            for ( int i = startIndex; i <= lastIndexPlus1; ++i )
            {
                if (i == lastIndexPlus1 || relative[i] == '/' || relative[i] == '\\')
                {
                    if ( state == NormalizeState.PrevSlash || state == NormalizeState.PrevDot )
                    {
                        // do nothing
                    }
                    else if ( state == NormalizeState.PrevDoubleDot )
                    {
                        if ( output.Length == 1 )
                            throw new System.IO.IOException("Invalid path: double dot error (before " + i + ")");

                        // on level up!
                        int j;
                        for ( j = output.Length - 2; j >= 0 && output[j] != '/'; --j)
                        {
                        }

                        output.Remove(j + 1, output.Length - j - 1);
                    }
                    else if ( i < lastIndexPlus1 || forceTrailingSlash )
                    {
                        output.Append('/');
                    }

                    state = NormalizeState.PrevSlash;
                }
                else if ( relative[i] == '.' )
                {
                    if ( state == NormalizeState.PrevSlash )
                    {
                        state = NormalizeState.PrevDot;
                    }
                    else if ( state == NormalizeState.PrevDot )
                    {
                        state = NormalizeState.PrevDoubleDot;
                    }
                    else if ( state == NormalizeState.PrevDoubleDot )
                    {
                        state = NormalizeState.NothingSpecial;
                        output.Append("...");
                    }
                    else
                    {
                        output.Append('.');
                    }
                }
                else
                {
                    if ( state == NormalizeState.PrevDot )
                    {
                        output.Append('.');
                    }
                    else if ( state == NormalizeState.PrevDoubleDot )
                    {
                        output.Append("..");
                    }

                    if (!IsValidCharacter(relative[i]))
                        throw new System.IO.IOException("Invalid characters");

                    output.Append(relative[i]);
                    state = NormalizeState.NothingSpecial;
                }
            }

            return output.ToString();
        }

        public static bool IsValidCharacter(char c)
        {
            if (c == '\"' || c == '<' || c == '>' || c == '|' || c < 32 || c == ':' || c == '*' || c == '?')
                return false;
            return true;
        }

        public static Regex WildcardToRegex(string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase);
        }
    }
}
