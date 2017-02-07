using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Model
{
    [Serializable]
    public class ValueBase
    {
        public IConvertible Convertivle;
        public string enumValue;
        public string StringValue;
        public bool BooleanValue;
        public int Int32Value;
        public long Int64Value;
        public float SingleValue;
        public double DoubleValue;
        public int[] Int32Array;
        public long[] Int64Array;
        public string[] StringArray;
        public float[] SingleArray;
        public double[] DoubleArray;

        public GameObject GameObjectValue;
        public AudioClip AudioClipValue;
        public Material MaterialValue;
        public Shader ShaderValue;
        public Texture TextureValue;
        public Texture2D Texture2DValue;
        public Texture3D Texture3DValue;
        public Sprite SpriteValue;

        public GameObject[] GameObjectArray;
        public AudioClip[] AudioClipArray;
        public Material[] MaterialArray;
        public Shader[] ShaderArray;
        public Texture [] TextureArray;
        public Texture2D [] Texture2DArray;
        public Texture3D [] Texture3DArray;
        public Sprite [] SpriteArray;
        public ValueBase()
        {
        }

        public static ValueBase Clone(ValueBase source)
        {
            ValueBase v = new ValueBase();
            FieldInfo[] infos = source.GetType().GetFields();
            foreach (FieldInfo info in infos)
            {
                object value;
                if (info.FieldType.IsSubclassOf(typeof(Array)))
                {
                    Array sourceArray = (Array)info.GetValue(source);
                    if (sourceArray == null)
                    {
                        continue;
                    }
                    Array dest = Array.CreateInstance(info.FieldType.GetElementType(), sourceArray.Length);
                    Array.Copy(sourceArray, dest, dest.Length);
                    value = dest;
                }
                else
                {
                    value = info.GetValue(source);
                }
                info.SetValue(v, value);
            }
            return v;
        }

        public object GetValueByType(Type type)
        {
            try
            {
                FieldInfo fieldInfo = GetFieldInfo(type);
                if (BehaviorTreeArgsDict.IsEnumType(type))
                {
                    Enum value;
                    if (string.IsNullOrEmpty(enumValue))
                    {
                        value = GetDefaultEnumValue(type);
                    }
                    else
                    {
                        value = (Enum)Enum.Parse(type, enumValue);
                    }
                    return value;
                }
                else if (BehaviorTreeArgsDict.IsStringType(type))
                {
                    if (string.IsNullOrEmpty(StringValue))
                    {
                        StringValue = enumValue;
                        return StringValue;
                    }
                }
                return fieldInfo.GetValue(this);
            }
            catch (Exception err)
            {
                throw new GameException($"行为树报错:{err}");
            }
        }
        private Enum GetDefaultEnumValue(Type type)
        {
            Array array = Enum.GetValues(type);
            Enum value = (Enum)array.GetValue(0);
            return value;
        }
        public void SetValueByType(Type type,object value)
        {
            if (type == null)
            {
                return;
            }
            FieldInfo field = GetFieldInfo(type);
            if (BehaviorTreeArgsDict.IsMaterialArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToMaterialArray((UnityEngine.Object [])value));
            }
            else if (BehaviorTreeArgsDict.IsGameObjectArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToGameObjectArray((UnityEngine.Object[])value));
            }
            else if (BehaviorTreeArgsDict.IsTextureArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToTextureArray((UnityEngine.Object[])value));
            }
            else if (BehaviorTreeArgsDict.IsTexture2DArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToTexture2DArray((UnityEngine.Object[])value));
            }
            else if (BehaviorTreeArgsDict.IsTexture3DArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToTexture3DArray((UnityEngine.Object[])value));
            }
            else if (BehaviorTreeArgsDict.IsShaderArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToShaderArray((UnityEngine.Object[])value));
            }
            else if (BehaviorTreeArgsDict.IsAudioClipArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToAudioClipArray((UnityEngine.Object[])value));
            }
            else if (BehaviorTreeArgsDict.IsSpriteArrayType(field.FieldType))
            {
                field.SetValue(this, BehaviorTreeArgsDict.ConvertToSpriteArray((UnityEngine.Object[])value));
            }
            else
            {
                field.SetValue(this, value);
            }
        }

        private FieldInfo GetFieldInfo(Type type)
        {
            string fieldName;
            if (BehaviorTreeArgsDict.IsEnumType(type))
            {
                fieldName = "enumValue";
            }
            else if (type.IsArray)
            {
                fieldName = type.GetElementType() + "Array";
            }
            else
            {
                fieldName = type.Name + "Value";
            }
            fieldName = RemovePrefix(fieldName);
            FieldInfo fieldInfo = GetType().GetField(fieldName);
            return fieldInfo;
        }
        private string RemovePrefix(string fieldName)
        {
            string enginePrefix = "UnityEngine.";
            int engineIndex = fieldName.IndexOf(enginePrefix);
            if (engineIndex != -1)
            {
                fieldName = fieldName.Remove(engineIndex, enginePrefix.Length);
            }
            string systemPrefix = "System.";
            int systemIndex = fieldName.IndexOf(systemPrefix);
            if (systemIndex != -1)
            {
                fieldName = fieldName.Remove(systemIndex, systemPrefix.Length);
            }
            return fieldName;
        }
    }
}
