using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class BlurFilter : IFilter
	{
		//ref http://blog.csdn.net/u011047171/article/details/47947397

		/// <summary>
		/// 
		/// </summary>
		public float blurSize;

		DisplayObject _target;
		Material _blitMaterial;

		public BlurFilter()
		{
			blurSize = 1f;
		}

		public DisplayObject target
		{
			get { return _target; }
			set
			{
				_target = value;
				_target.EnterPaintingMode(1, null);
				_target.onPaint += OnRenderImage;

				_blitMaterial = new Material(ShaderConfig.GetShader("FairyGUI/BlurFilter"));
				_blitMaterial.hideFlags = DisplayOptions.hideFlags;
			}
		}

		public void Dispose()
		{
			_target.LeavePaintingMode(1);
			_target.onPaint -= OnRenderImage;
			_target = null;

			if (Application.isPlaying)
				Material.Destroy(_blitMaterial);
			else
				Material.DestroyImmediate(_blitMaterial);
		}

		public void Update()
		{
		}

		void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
		{
			float off = blurSize * iteration + 0.5f;
			Graphics.BlitMultiTap(source, dest, _blitMaterial,
				new Vector2(-off, -off),
				new Vector2(-off, off),
				new Vector2(off, off),
				new Vector2(off, -off)
			);
		}

		void DownSample4x(RenderTexture source, RenderTexture dest)
		{
			float off = 1.0f;
			Graphics.BlitMultiTap(source, dest, _blitMaterial,
				new Vector2(off, off),
				new Vector2(-off, off),
				new Vector2(off, off),
				new Vector2(off, -off)
			);
		}

		void OnRenderImage()
		{
			if (blurSize < 0.01)
				return;

			RenderTexture sourceTexture = (RenderTexture)_target.paintingGraphics.texture.nativeTexture;
			int rtW = sourceTexture.width / 8;
			int rtH = sourceTexture.height / 8;
			RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

			DownSample4x(sourceTexture, buffer);

			for (int i = 0; i < 2; i++)
			{
				RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
				FourTapCone(buffer, buffer2, i);
				RenderTexture.ReleaseTemporary(buffer);
				buffer = buffer2;
			}
			Graphics.Blit(buffer, sourceTexture);

			RenderTexture.ReleaseTemporary(buffer);
		}
	}
}
