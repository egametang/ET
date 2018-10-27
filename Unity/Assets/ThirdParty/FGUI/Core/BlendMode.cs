using UnityEngine;
using NativeBlendMode = UnityEngine.Rendering.BlendMode;

namespace FairyGUI
{
	/*关于BlendMode.Off, 这种模式相当于Blend Off指令的效果。当然，在着色器里使用Blend Off指令可以获得更高的效率，
		但因为Image着色器本身就有多个关键字，复制一个这样的着色器代价太大，所有为了节省Shader数量便增加了这样一种模式，也是可以接受的。
	*/

	/// <summary>
	/// 
	/// </summary>
	public enum BlendMode
	{
		Normal,
		None,
		Add,
		Multiply,
		Screen,
		Erase,
		Mask,
		Below,
		Off,
		Custom1,
		Custom2,
		Custom3
	}

	/// <summary>
	/// 
	/// </summary>
	public class BlendModeUtils
	{
		public class BlendFactor
		{
			public NativeBlendMode srcFactor;
			public NativeBlendMode dstFactor;
			public bool pma;

			public BlendFactor(NativeBlendMode srcFactor, NativeBlendMode dstFactor, bool pma = false)
			{
				this.srcFactor = srcFactor;
				this.dstFactor = dstFactor;
				this.pma = pma;
			}
		}

		//Source指的是被计算的颜色，Destination是已经在屏幕上的颜色。
		//混合结果=Source * factor1 + Destination * factor2
		public static BlendFactor[] Factors = new BlendFactor[] {
			//Normal
			new BlendFactor(NativeBlendMode.SrcAlpha, NativeBlendMode.OneMinusSrcAlpha),
			//None
			new BlendFactor(NativeBlendMode.One, NativeBlendMode.One),
			//Add
			new BlendFactor(NativeBlendMode.SrcAlpha, NativeBlendMode.One),
			//Multiply
			new BlendFactor(NativeBlendMode.DstColor, NativeBlendMode.OneMinusSrcAlpha, true),
			//Screen
			new BlendFactor(NativeBlendMode.One, NativeBlendMode.OneMinusSrcColor, true),
			//Erase
			new BlendFactor(NativeBlendMode.Zero, NativeBlendMode.OneMinusSrcAlpha),
			//Mask
			new BlendFactor(NativeBlendMode.Zero, NativeBlendMode.SrcAlpha),
			//Below
			new BlendFactor(NativeBlendMode.OneMinusDstAlpha, NativeBlendMode.DstAlpha),
			//Off
			new BlendFactor(NativeBlendMode.One, NativeBlendMode.Zero),
			//Custom1
			new BlendFactor(NativeBlendMode.SrcAlpha, NativeBlendMode.OneMinusSrcAlpha),
			//Custom2
			new BlendFactor(NativeBlendMode.SrcAlpha, NativeBlendMode.OneMinusSrcAlpha),
			//Custom3
			new BlendFactor(NativeBlendMode.SrcAlpha, NativeBlendMode.OneMinusSrcAlpha)
		};

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="blendMode"></param>
		public static void Apply(Material mat, BlendMode blendMode)
		{
			BlendFactor bf = Factors[(int)blendMode];
			mat.SetFloat("_BlendSrcFactor", (float)bf.srcFactor);
			mat.SetFloat("_BlendDstFactor", (float)bf.dstFactor);

			if (bf.pma)
				mat.SetFloat("_ColorOption", 1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blendMode"></param>
		/// <param name="srcFactor"></param>
		/// <param name="dstFactor"></param>
		public static void Override(BlendMode blendMode, NativeBlendMode srcFactor, NativeBlendMode dstFactor)
		{
			BlendFactor bf = Factors[(int)blendMode];
			bf.srcFactor = srcFactor;
			bf.dstFactor = dstFactor;
		}
	}
}
