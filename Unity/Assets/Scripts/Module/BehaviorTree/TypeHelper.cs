using System;
using UnityEngine;

namespace ETModel
{
	public static class TypeHelper
	{
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
			if (fieldType == typeof(UnityEngine.Object[]) || fieldType == typeof(GameObject[]) || fieldType == typeof(Material[]) ||
			    fieldType == typeof(Texture[]) || fieldType == typeof(Texture2D[]) || fieldType == typeof(Texture3D[]) || fieldType == typeof(Shader[]) ||
			    fieldType == typeof(AudioClip[]) || fieldType == typeof(Sprite[]))
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
	}
}
