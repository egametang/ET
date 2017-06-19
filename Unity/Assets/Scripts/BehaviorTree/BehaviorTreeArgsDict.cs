using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

namespace Model
{
	[Serializable]
	public class BehaviorTreeArgsDict
	{
		private readonly Dictionary<string, ValueBase> dict = new Dictionary<string, ValueBase>();

		public void Add(string key, ValueBase value)
		{
			this.dict.Add(key, value);
		}

		public void Remove(string key)
		{
			this.dict.Remove(key);
		}

		public bool ContainsKey(string key)
		{
			return this.dict.ContainsKey(key);
		}

		public Dictionary<string, ValueBase> Dict()
		{
			 return this.dict;
		}

		public BehaviorTreeArgsDict Clone()
		{
			BehaviorTreeArgsDict behaviorTreeArgsDict = new BehaviorTreeArgsDict();
			foreach (KeyValuePair<string, ValueBase> keyValuePair in this.dict)
			{
				behaviorTreeArgsDict.Add(keyValuePair.Key, keyValuePair.Value.Clone());
			}
			return behaviorTreeArgsDict;
		}


		public void SetKeyValueComp(Type type, string fieldName, object value)
		{
			try
			{
				ValueBase valueBase;
				if (!this.dict.TryGetValue(fieldName, out valueBase))
				{
					valueBase = new ValueBase();
					this.dict.Add(fieldName, valueBase);
				}
				valueBase.SetValue(value);
			}
			catch (Exception e)
			{
				Log.Error($"SetKeyValueComp error: {fieldName} {e}");
			}

		}

		public object GetTreeDictValue(Type fieldType, string fieldName)
		{
			if (!this.dict.ContainsKey(fieldName))
			{
				Log.Error($"fieldName:{fieldName} 不存在！！！！");
				return null;
			}
			ValueBase obj = this.dict[fieldName];
			return obj.GetValue();
		}

		/// <summary>
		/// judge
		/// </summary>
		/// <param nodeName="type"></param>
		/// <returns></returns>
		public static bool IsStringType(Type type)
		{
			return typeof (string) == type;
		}

		public static bool IsBoolType(Type type)
		{
			return typeof (bool) == type;
		}

		public static bool IsIntType(Type type)
		{
			return typeof (int) == type;
		}

		public static bool IsLongType(Type type)
		{
			return typeof (long) == type;
		}

		public static bool IsFloatType(Type type)
		{
			return typeof (float) == type;
		}

		public static bool IsDoubleType(Type type)
		{
			return typeof (double) == type;
		}

		public static bool IsIntArrType(Type type)
		{
			return typeof (int[]) == type;
		}

		public static bool IsLongArrType(Type type)
		{
			return typeof (long[]) == type;
		}

		public static bool IsFloatArrType(Type type)
		{
			return typeof (float[]) == type;
		}

		public static bool IsDoubleArrType(Type type)
		{
			return typeof (double[]) == type;
		}

		public static bool IsStringArrType(Type type)
		{
			return typeof (string[]) == type;
		}

		public static bool IsObjectType(Type fieldType)
		{
			Type objecType = typeof (UnityEngine.Object);
			if (fieldType == objecType || fieldType.IsSubclassOf(objecType))
			{
				return true;
			}
			return false;
		}

		public static bool IsObjectArrayType(Type fieldType)
		{
			if (fieldType == typeof (UnityEngine.Object[]) || fieldType == typeof (GameObject[]) || fieldType == typeof (Material[]) ||
			    fieldType == typeof (Texture[]) || fieldType == typeof (Texture2D[]) || fieldType == typeof (Texture3D[]) || fieldType == typeof (Shader[]) ||
			    fieldType == typeof (AudioClip[]) || fieldType == typeof (Sprite[]))
			{
				return true;
			}
			return false;
		}

		public static bool IsConvertble(Type type)
		{
			return type == typeof (IConvertible);
		}

		public static bool IsAudioClipType(Type fieldType)
		{
			return fieldType == typeof (AudioClip);
		}

		public static bool IsMaterialType(Type fieldType)
		{
			return fieldType == typeof (Material);
		}

		public static bool IsGameObjectType(Type fieldType)
		{
			return fieldType == typeof (GameObject);
		}

		public static bool IsShaderType(Type fieldType)
		{
			return fieldType == typeof (Shader);
		}

		public static bool IsTextureType(Type fieldType)
		{
			return fieldType == typeof (Texture);
		}

		public static bool IsTexture2DType(Type fieldType)
		{
			return fieldType == typeof (Texture2D);
		}

		public static bool IsTexture3DType(Type fieldType)
		{
			return fieldType == typeof (Texture3D);
		}

		public static bool IsGameObjectArrayType(Type fieldType)
		{
			return fieldType == typeof (GameObject[]);
		}

		public static bool IsMaterialArrayType(Type fieldType)
		{
			return fieldType == typeof (Material[]);
		}

		public static bool IsTextureArrayType(Type fieldType)
		{
			return fieldType == typeof (Texture[]);
		}

		public static bool IsTexture2DArrayType(Type fieldType)
		{
			return fieldType == typeof (Texture2D[]);
		}

		public static bool IsTexture3DArrayType(Type fieldType)
		{
			return fieldType == typeof (Texture3D[]);
		}

		public static bool IsShaderArrayType(Type fieldType)
		{
			return fieldType == typeof (Shader[]);
		}

		public static bool IsAudioClipArrayType(Type fieldType)
		{
			return fieldType == typeof (AudioClip[]);
		}

		public static bool IsUnitConfigArrayType(Type fieldType)
		{
			return false;
		}

		public static bool IsSpriteArrayType(Type fieldType)
		{
			return fieldType == typeof (Sprite[]);
		}

		public static bool IsEnumType(Type fieldType)
		{
			Type enumType = typeof (Enum);
			if (fieldType == enumType || fieldType.IsSubclassOf(enumType))
			{
				return true;
			}
			return false;
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