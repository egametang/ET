using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using UnityEngine;

namespace Model
{
    [Serializable]
    public class BehaviorTreeArgsDict :  Dictionary<string, ValueBase>
    {
        public void SetKeyValueComp(Type type, string fieldName, object value)
        {
            if (IsStringType(type))
            {
                SetStrValue(fieldName, (string)value);
            }
            else if (IsObjectType(type))
            {
                SetObjectValue(fieldName, type, (UnityEngine.Object)value);
            }
            else if (IsIntType(type))
            {
                int intValue = 0;
                int.TryParse(value.ToString(), out intValue);
                SetIntValue(fieldName, intValue);
            }
            else if (IsLongType(type))
            {
                long longValue = 0;
                long.TryParse(value.ToString(), out longValue);
                SetLongValue(fieldName, longValue);
            }
            else if (IsFloatType(type))
            {
                float floatValue = 0;
                float.TryParse(value.ToString(), out floatValue);
                SetFloatValue(fieldName, floatValue);
            }
            else if (IsDoubleType(type))
            {
                double doubleValue = 0;
                double.TryParse(value.ToString(), out doubleValue);
                SetDoubleValue(fieldName, doubleValue);
            }
            else if (IsBoolType(type))
            {
                bool boolValue = false;
                bool.TryParse(value.ToString(), out boolValue);
                SetBoolValue(fieldName, boolValue);
            }
            else if (IsStringArrType(type))
            {
                SetStrArrValue(fieldName, (string[])value);
            }
            else if (IsIntArrType(type))
            {
                SetIntArrValue(fieldName, (int[])value);
            }
            else if (IsLongArrType(type))
            {
                SetLongArrValue(fieldName, (long[])value);
            }
            else if (IsFloatArrType(type))
            {
                SetFloatArrValue(fieldName, (float[])value);
            }
            else if (IsDoubleArrType(type))
            {
                SetDoubleArrValue(fieldName, (double[])value);
            }
            else if (IsEnumType(type))
            {
                SetEnumValue(fieldName, value.ToString());
            }
            else if (IsObjectArrayType(type))
            {
                SetObjectArrayValue(fieldName, type, (UnityEngine.Object[])value);
            }
            else
            {
                Log.Error($"行为树节点暂时未支持此类型:{type}！");
            }
        }

        private void SetConvertebleValue(string fieldName, float value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].Convertivle = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.Convertivle = value;
                this.Add(fieldName, valueBase);
            }
        }

        public object GetTreeDictValue(Type fieldType, string fieldName)
        {
            if (!this.ContainsKey(fieldName))
            {
                Log.Error($"fieldName:{fieldName} 不存在！！！！");
                return null;
            }
            ValueBase obj = this[fieldName];
            return obj.GetValueByType(fieldType);
        }

        /// <summary>
        /// judge
        /// </summary>
        /// <param nodeName="type"></param>
        /// <returns></returns>
        public static bool IsStringType(Type type)
        {
            return typeof(string) == type;
        }

        public static bool IsBoolType(Type type)
        {
            return typeof(bool) == type;
        }

        public static bool IsIntType(Type type)
        {
            return typeof(int) == type;
        }

        public static bool IsLongType(Type type)
        {
            return typeof(long) == type;
        }

        public static bool IsFloatType(Type type)
        {
            return typeof(float) == type;
        }

        public static bool IsDoubleType(Type type)
        {
            return typeof(double) == type;
        }

        public static bool IsIntArrType(Type type)
        {
            return typeof(int[]) == type;
        }

        public static bool IsLongArrType(Type type)
        {
            return typeof(long[]) == type;
        }

        public static bool IsFloatArrType(Type type)
        {
            return typeof(float[]) == type;
        }

        public static bool IsDoubleArrType(Type type)
        {
            return typeof(double[]) == type;
        }

        public static bool IsStringArrType(Type type)
        {
            return typeof(string[]) == type;
        }

        public static bool IsObjectType(Type fieldType)
        {
            Type objecType = typeof(UnityEngine.Object);
            if (fieldType == objecType || fieldType.IsSubclassOf(objecType))
            {
                return true;
            }
            return false;
        }
        public static bool IsObjectArrayType(Type fieldType)
        {

            if ( fieldType == typeof(UnityEngine.Object[]) || fieldType == typeof(GameObject[]) || fieldType == typeof(Material[]) ||
                 fieldType == typeof(Texture[]) || fieldType == typeof(Texture2D[]) || fieldType == typeof(Texture3D[]) || 
                 fieldType == typeof(Shader[]) || fieldType == typeof(AudioClip []) || fieldType == typeof(Sprite []))
            {
                return true;
            }
            return false;
        }

        public static bool IsConvertble(Type type)
        {
            return type == typeof(IConvertible);
        }

        public static bool IsAudioClipType(Type fieldType)
        {
            return fieldType == typeof(AudioClip);
        }
        public static bool IsMaterialType(Type fieldType)
        {

            return fieldType == typeof(Material);
        }
        public static bool IsGameObjectType(Type fieldType)
        {

            return fieldType == typeof(GameObject);
        }
        public static bool IsShaderType(Type fieldType)
        {

            return fieldType == typeof(Shader);
        }
        public static bool IsTextureType(Type fieldType)
        {

            return fieldType == typeof(Texture);
        }
        public static bool IsTexture2DType(Type fieldType)
        {

            return fieldType == typeof(Texture2D);
        }
        public static bool IsTexture3DType(Type fieldType)
        {

            return fieldType == typeof(Texture3D);
        }
        public static bool IsGameObjectArrayType(Type fieldType)
        {
            return fieldType == typeof(GameObject[]);
        }
        public static bool IsMaterialArrayType(Type fieldType)
        {
            return fieldType == typeof(Material[]);
        }
        public static bool IsTextureArrayType(Type fieldType)
        {
            return fieldType == typeof(Texture[]);
        }
        public static bool IsTexture2DArrayType(Type fieldType)
        {
            return fieldType == typeof(Texture2D[]);
        }
        public static bool IsTexture3DArrayType(Type fieldType)
        {
            return fieldType == typeof(Texture3D[]);
        }
        public static bool IsShaderArrayType(Type fieldType)
        {
            return fieldType == typeof(Shader[]);
        }
        public static bool IsAudioClipArrayType(Type fieldType)
        {
            return fieldType == typeof(AudioClip[]);
        }
        public static bool IsUnitConfigArrayType(Type fieldType)
        {
            return false;
        }
        public static bool IsSpriteArrayType(Type fieldType)
        {
            return fieldType == typeof(Sprite[]);
        }
        public static bool IsEnumType(Type fieldType)
        {
            Type enumType = typeof(Enum);
            if (fieldType == enumType || fieldType.IsSubclassOf(enumType))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param nodeName="fieldName"></param>
        /// <param nodeName="value"></param>
        public void SetStrValue(string fieldName, string value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].StringValue = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.StringValue = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetIntValue(string fieldName, int value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].Int32Value = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.Int32Value = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetLongValue(string fieldName, long value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].Int64Value = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.Int64Value = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetFloatValue(string fieldName, float value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].SingleValue = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.SingleValue = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetDoubleValue(string fieldName, double value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].DoubleValue = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.DoubleValue = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetBoolValue(string fieldName, bool value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].BooleanValue = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.BooleanValue = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetStrArrValue(string fieldName, string[] value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].StringArray = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.StringArray = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetIntArrValue(string fieldName, int[] value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].Int32Array = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.Int32Array = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetLongArrValue(string fieldName, long[] value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].Int64Array = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.Int64Array = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetFloatArrValue(string fieldName, float[] value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].SingleArray = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.SingleArray = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetDoubleArrValue(string fieldName, double[] value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].DoubleArray = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.DoubleArray = value;
                this.Add(fieldName, valueBase);
            }
        }

        public void SetObjectValue(string fieldName,Type type, UnityEngine.Object value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName]?.SetValueByType(type, value);
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                if (value != null)
                {
                    valueBase.SetValueByType(type, value);
                }
                this.Add(fieldName, valueBase);
            }
        }

        public void SetEnumValue(string fieldName, string value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName].enumValue = value;
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                valueBase.enumValue = value;
                this.Add(fieldName, valueBase);
            }
        }
        public void SetObjectArrayValue(string fieldName,Type type, UnityEngine.Object[] value)
        {
            if (this.ContainsKey(fieldName))
            {
                this[fieldName]?.SetValueByType(type, value);
            }
            else
            {
                ValueBase valueBase = new ValueBase();
                if (value != null)
                {
                    valueBase.SetValueByType(type, value);
                }
                this.Add(fieldName, valueBase);
            }
        }

        public static GameObject[] ConvertToGameObjectArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            GameObject[] newObjectArray = new GameObject[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] =  (GameObject)objectArray[i];
            }
            return newObjectArray;
        }
        public static Material[] ConvertToMaterialArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            Material[] newObjectArray = new Material[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (Material)objectArray[i];
            }
            return newObjectArray;
        }
        public static Texture[] ConvertToTextureArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            Texture[] newObjectArray = new Texture[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (Texture)objectArray[i];
            }
            return newObjectArray;
        }
        public static Texture2D[] ConvertToTexture2DArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            Texture2D[] newObjectArray = new Texture2D[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (Texture2D)objectArray[i];
            }
            return newObjectArray;
        }
        public static Texture3D[] ConvertToTexture3DArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            Texture3D[] newObjectArray = new Texture3D[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (Texture3D)objectArray[i];
            }
            return newObjectArray;
        }
        public static Shader[] ConvertToShaderArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            Shader[] newObjectArray = new Shader[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (Shader)objectArray[i];
            }
            return newObjectArray;
        }
        public static AudioClip[] ConvertToAudioClipArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            AudioClip[] newObjectArray = new AudioClip[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (AudioClip)objectArray[i];
            }
            return newObjectArray;
        }
        public static Sprite [] ConvertToSpriteArray(UnityEngine.Object[] objectArray)
        {
            if (objectArray == null)
            {
                return null;
            }
            Sprite[] newObjectArray = new Sprite[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                newObjectArray[i] = (Sprite)objectArray[i];
            }
            return newObjectArray;
        }

        public static bool SatisfyCondition(GameObject go, Type[] constraintTypes)
        {
            if (go == null || constraintTypes == null || constraintTypes.Length <= 0)
            {
                return true;
            }
            foreach (var constraint in constraintTypes)
            {
                if (go.GetComponent(constraint) == null)
                {
                    Log.Error($"此GameObject必须包含:{constraint}");
                    return false;
                }
            }
            return true;
        }
      
    }
}