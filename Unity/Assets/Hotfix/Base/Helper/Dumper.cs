using System;
using System.Collections;
using System.Reflection;
using System.Text;
using ETModel;
using Google.Protobuf;
using UnityEngine;

namespace ETHotfix
{
    public static class Dumper
    {
        private static readonly StringBuilder _text = new StringBuilder("", 1024);

        private static void AppendIndent(int num)
        {
            _text.Append(' ', num);
        }

        private static void DoDump(object obj)
        {
            if (obj == null)
            {
                _text.Append("null");
                _text.Append(",");
                return;
            }

            Type t = obj.GetType();

            //repeat field
            if (obj is IList)
            {
                /*
                _text.Append(t.FullName);
                _text.Append(",");
                AppendIndent(1);
                */

                _text.Append("[");
                IList list = obj as IList;
                foreach (object v in list)
                {
                    DoDump(v);
                }

                _text.Append("]");
            }
            else if (t.IsValueType)
            {
                _text.Append(obj);
                _text.Append(",");
                AppendIndent(1);
            }
            else if (obj is string)
            {
                _text.Append("\"");
                _text.Append(obj);
                _text.Append("\"");
                _text.Append(",");
                AppendIndent(1);
            }
            else if (obj is ByteString)
            {
                _text.Append("\"");
                _text.Append(((ByteString) obj).bytes.Utf8ToStr());
                _text.Append("\"");
                _text.Append(",");
                AppendIndent(1);
            }
            else if (t.IsArray)
            {
                Array a = (Array) obj;
                _text.Append("[");
                for (int i = 0; i < a.Length; i++)
                {
                    _text.Append(i);
                    _text.Append(":");
                    DoDump(a.GetValue(i));
                }

                _text.Append("]");
            }
            else if (t.IsClass)
            {
                _text.Append($"<{t.Name}>");
                _text.Append("{");
                var fields = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (fields.Length > 0)
                {
                    foreach (PropertyInfo info in fields)
                    {
                        _text.Append(info.Name);
                        _text.Append(":");
                        object value = info.GetGetMethod().Invoke(obj, null);
                        DoDump(value);
                    }
                }

                _text.Append("}");
            }
            else
            {
                Debug.LogWarning("unsupport type: " + t.FullName);
                _text.Append(obj);
                _text.Append(",");
                AppendIndent(1);
            }
        }

        public static string DumpAsString(object obj, string hint = "")
        {
            _text.Clear();
            _text.Append(hint);
            DoDump(obj);
            return _text.ToString();
        }
    }
}