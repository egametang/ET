using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
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

		public static Type GetBTType(string name)
		{
			Type type = Type.GetType($"Base.{name}");
			return null;
		}

		public static Type GetBTType(Type originType)
		{
			Type type = null;
			try
			{
				if (BehaviorTreeArgsDict.IsEnumType(originType))
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