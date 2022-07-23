using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
    public class TranslationHelper
    {
        public static Dictionary<string, Dictionary<string, string>> strings;

        public static void LoadFromXML(XML source)
        {
            strings = new Dictionary<string, Dictionary<string, string>>();
            XMLList.Enumerator et = source.GetEnumerator("string");
            while (et.MoveNext())
            {
                XML cxml = et.Current;
                string key = cxml.GetAttribute("name");
                string text = cxml.text;
                int i = key.IndexOf("-");
                if (i == -1)
                    continue;

                string key2 = key.Substring(0, i);
                string key3 = key.Substring(i + 1);
                Dictionary<string, string> col;
                if (!strings.TryGetValue(key2, out col))
                {
                    col = new Dictionary<string, string>();
                    strings[key2] = col;
                }
                col[key3] = text;
            }
        }

        public static void TranslateComponent(PackageItem item)
        {
            if (TranslationHelper.strings == null)
                return;

            Dictionary<string, string> strings;
            if (!TranslationHelper.strings.TryGetValue(item.owner.id + item.id, out strings))
                return;

            string elementId, value;
            ByteBuffer buffer = item.rawData;

            buffer.Seek(0, 2);

            int childCount = buffer.ReadShort();
            for (int i = 0; i < childCount; i++)
            {
                int dataLen = buffer.ReadShort();
                int curPos = buffer.position;

                buffer.Seek(curPos, 0);

                ObjectType baseType = (ObjectType)buffer.ReadByte();
                ObjectType type = baseType;
                buffer.Skip(4);
                elementId = buffer.ReadS();

                if (type == ObjectType.Component)
                {
                    if (buffer.Seek(curPos, 6))
                        type = (ObjectType)buffer.ReadByte();
                }

                buffer.Seek(curPos, 1);

                if (strings.TryGetValue(elementId + "-tips", out value))
                    buffer.WriteS(value);

                buffer.Seek(curPos, 2);

                int gearCnt = buffer.ReadShort();
                for (int j = 0; j < gearCnt; j++)
                {
                    int nextPos = buffer.ReadUshort();
                    nextPos += buffer.position;

                    if (buffer.ReadByte() == 6) //gearText
                    {
                        buffer.Skip(2);//controller
                        int valueCnt = buffer.ReadShort();
                        for (int k = 0; k < valueCnt; k++)
                        {
                            string page = buffer.ReadS();
                            if (page != null)
                            {
                                if (strings.TryGetValue(elementId + "-texts_" + k, out value))
                                    buffer.WriteS(value);
                                else
                                    buffer.Skip(2);
                            }
                        }

                        if (buffer.ReadBool() && strings.TryGetValue(elementId + "-texts_def", out value))
                            buffer.WriteS(value);
                    }

                    buffer.position = nextPos;
                }

                if (baseType == ObjectType.Component && buffer.version >= 2)
                {
                    buffer.Seek(curPos, 4);

                    buffer.Skip(2); //pageController

                    buffer.Skip(4 * buffer.ReadShort());

                    int cpCount = buffer.ReadShort();
                    for (int k = 0; k < cpCount; k++)
                    {
                        string target = buffer.ReadS();
                        int propertyId = buffer.ReadShort();
                        if (propertyId == 0 && strings.TryGetValue(elementId + "-cp-" + target, out value))
                            buffer.WriteS(value);
                        else
                            buffer.Skip(2);
                    }
                }

                switch (type)
                {
                    case ObjectType.Text:
                    case ObjectType.RichText:
                    case ObjectType.InputText:
                        {
                            if (strings.TryGetValue(elementId, out value))
                            {
                                buffer.Seek(curPos, 6);
                                buffer.WriteS(value);
                            }
                            if (strings.TryGetValue(elementId + "-prompt", out value))
                            {
                                buffer.Seek(curPos, 4);
                                buffer.WriteS(value);
                            }
                            break;
                        }

                    case ObjectType.List:
                    case ObjectType.Tree:
                        {
                            buffer.Seek(curPos, 8);
                            buffer.Skip(2);
                            int itemCount = buffer.ReadShort();
                            for (int j = 0; j < itemCount; j++)
                            {
                                int nextPos = buffer.ReadUshort();
                                nextPos += buffer.position;

                                buffer.Skip(2); //url
                                if (type == ObjectType.Tree)
                                    buffer.Skip(2);

                                //title
                                if (strings.TryGetValue(elementId + "-" + j, out value))
                                    buffer.WriteS(value);
                                else
                                    buffer.Skip(2);

                                //selected title
                                if (strings.TryGetValue(elementId + "-" + j + "-0", out value))
                                    buffer.WriteS(value);
                                else
                                    buffer.Skip(2);

                                if (buffer.version >= 2)
                                {
                                    buffer.Skip(6);
                                    buffer.Skip(buffer.ReadShort() * 4);//controllers

                                    int cpCount = buffer.ReadShort();
                                    for (int k = 0; k < cpCount; k++)
                                    {
                                        string target = buffer.ReadS();
                                        int propertyId = buffer.ReadShort();
                                        if (propertyId == 0 && strings.TryGetValue(elementId + "-" + j + "-" + target, out value))
                                            buffer.WriteS(value);
                                        else
                                            buffer.Skip(2);
                                    }
                                }

                                buffer.position = nextPos;
                            }
                            break;
                        }

                    case ObjectType.Label:
                        {
                            if (buffer.Seek(curPos, 6) && (ObjectType)buffer.ReadByte() == type)
                            {
                                if (strings.TryGetValue(elementId, out value))
                                    buffer.WriteS(value);
                                else
                                    buffer.Skip(2);

                                buffer.Skip(2);
                                if (buffer.ReadBool())
                                    buffer.Skip(4);
                                buffer.Skip(4);
                                if (buffer.ReadBool() && strings.TryGetValue(elementId + "-prompt", out value))
                                    buffer.WriteS(value);
                            }
                            break;
                        }

                    case ObjectType.Button:
                        {
                            if (buffer.Seek(curPos, 6) && (ObjectType)buffer.ReadByte() == type)
                            {
                                if (strings.TryGetValue(elementId, out value))
                                    buffer.WriteS(value);
                                else
                                    buffer.Skip(2);
                                if (strings.TryGetValue(elementId + "-0", out value))
                                    buffer.WriteS(value);
                            }
                            break;
                        }

                    case ObjectType.ComboBox:
                        {
                            if (buffer.Seek(curPos, 6) && (ObjectType)buffer.ReadByte() == type)
                            {
                                int itemCount = buffer.ReadShort();
                                for (int j = 0; j < itemCount; j++)
                                {
                                    int nextPos = buffer.ReadUshort();
                                    nextPos += buffer.position;

                                    if (strings.TryGetValue(elementId + "-" + j, out value))
                                        buffer.WriteS(value);

                                    buffer.position = nextPos;
                                }

                                if (strings.TryGetValue(elementId, out value))
                                    buffer.WriteS(value);
                            }

                            break;
                        }
                }

                buffer.position = curPos + dataLen;
            }
        }
    }
}
