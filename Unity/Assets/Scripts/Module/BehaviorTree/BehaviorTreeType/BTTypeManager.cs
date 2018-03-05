using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	public class BTTypeManager
	{
		public static Dictionary<Type, Type> BTTypeDict { get; } = new Dictionary<Type, Type>
		{
			{ typeof (int), typeof (BTIntComponent) },
			{ typeof (long), typeof (BTInt64Component) },
			{ typeof (string), typeof (BTStringComponent) },
			{ typeof (Enum), typeof (BTEnumComponent) },
			{ typeof (bool), typeof (BTBoolComponent) },
			{ typeof (float), typeof (BTFloatComponent) },
			{ typeof (double), typeof (BTDoubleComponent) },
			{ typeof (int[]), typeof (BTArrayIntComponent) },
			{ typeof (long[]), typeof (BTArrayInt64Component) },
			{ typeof (string[]), typeof (BTArrayStringComponent) },
			{ typeof (float[]), typeof (BTArrayFloatComponent) },
			{ typeof (double[]), typeof (BTArrayDoubleComponent) },
			{ typeof (GameObject), typeof (BTGameObjectComponent) },
			{ typeof (AudioClip), typeof (BTAudioClipComponent) },
			{ typeof (Material), typeof (BTMaterialComponent) },
			{ typeof (Shader), typeof (BTShaderComponent) },
			{ typeof (Texture), typeof (BTTextureComponent) },
			{ typeof (Texture2D), typeof (BTTexture2DComponent) },
			{ typeof (Texture3D), typeof (BTTexture3DComponent) },
			{ typeof (Sprite), typeof (BTSpriteComponent) },
			{ typeof (GameObject[]), typeof (BTArrayGameObjectComponent) },
			{ typeof (AudioClip[]), typeof (BTArrayAudioClipComponent) },
			{ typeof (Material[]), typeof (BTArrayMaterialComponent) },
			{ typeof (Shader[]), typeof (BTArrayShaderComponent) },
			{ typeof (Texture[]), typeof (BTArrayTextureComponent) },
			{ typeof (Texture2D[]), typeof (BTArrayTexture2DComponent) },
			{ typeof (Texture3D[]), typeof (BTArrayTexture3DComponent) },
			{ typeof (Sprite[]), typeof (BTArraySpriteComponent) }
		};

		public static Dictionary<Type, object> BTTypeDefaultValue { get; } = new Dictionary<Type, object>
		{
			{ typeof (int), 0 },
			{ typeof (long), 0L },
			{ typeof (string), "" },
			{ typeof (Enum), "" },
			{ typeof (bool), false },
			{ typeof (float), 0f },
			{ typeof (double), default(double) },
			{ typeof (int[]), new int[] {}},
			{ typeof (long[]), new long[] {} },
			{ typeof (string[]), new string[] {} },
			{ typeof (float[]), new float[] {} },
			{ typeof (double[]), new double[] {} },
			{ typeof (GameObject), null },
			{ typeof (AudioClip), null },
			{ typeof (Material), null },
			{ typeof (Shader), null },
			{ typeof (Texture), null },
			{ typeof (Texture2D), null },
			{ typeof (Texture3D), null },
			{ typeof (Sprite), null },
			{ typeof (GameObject[]), new GameObject[] {}},
			{ typeof (AudioClip[]), new AudioClip[] {} },
			{ typeof (Material[]), new Material[] {} },
			{ typeof (Shader[]), new Shader[] {} },
			{ typeof (Texture[]), new Texture[] {} },
			{ typeof (Texture2D[]), new Texture2D[] {} },
			{ typeof (Texture3D[]), new Texture3D[] {} },
			{ typeof (Sprite[]), new Sprite[] {} }
		};

		public static object GetDefaultValue(Type originType)
		{
			object value = null;
			BTTypeDefaultValue.TryGetValue(originType, out value);
			return value;
		}

		public static Type GetBTType(Type originType)
		{
			Type type = null;
			try
			{
				if (TypeHelper.IsEnumType(originType))
				{
					type = BTTypeDict[typeof (Enum)];
				}
				else
				{
					type = BTTypeDict[originType];
				}
			}
			catch (Exception e)
			{
				throw new Exception($"{originType} not exist!", e);
			}
			return type;
		}
	}
}