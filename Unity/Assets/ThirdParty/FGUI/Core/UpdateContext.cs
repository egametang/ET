using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// UpdateContext is for internal use.
	/// </summary>
	public class UpdateContext
	{
		public struct ClipInfo
		{
			public Rect rect;
			public Vector4 clipBox;
			public bool soft;
			public Vector4 softness;//left-top-right-bottom
			public uint clipId;
			public bool stencil;
			public bool reversedMask;
		}

		Stack<ClipInfo> _clipStack;

		public bool clipped;
		public ClipInfo clipInfo;

		public int renderingOrder;
		public int batchingDepth;
		public int rectMaskDepth;
		public int stencilReferenceValue;
		public float alpha;
		public bool grayed;

		public static UpdateContext current;
		public static uint frameId;
		public static bool working;
		public static EventCallback0 OnBegin;
		public static EventCallback0 OnEnd;

		static EventCallback0 _tmpBegin;

		public UpdateContext()
		{
			_clipStack = new Stack<ClipInfo>();
			frameId = 1;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Begin()
		{
			current = this;

			frameId++;
			if (frameId == 0)
				frameId = 1;
			renderingOrder = 0;
			batchingDepth = 0;
			rectMaskDepth = 0;
			stencilReferenceValue = 0;
			alpha = 1;
			grayed = false;

			clipped = false;
			_clipStack.Clear();

			Stats.ObjectCount = 0;
			Stats.GraphicsCount = 0;

			_tmpBegin = OnBegin;
			OnBegin = null;

			//允许OnBegin里再次Add，这里没有做死锁检查
			while (_tmpBegin != null)
			{
				_tmpBegin.Invoke();
				_tmpBegin = OnBegin;
				OnBegin = null;
			}

			working = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void End()
		{
			working = false;

			if (OnEnd != null)
				OnEnd.Invoke();

			OnEnd = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clipId"></param>
		/// <param name="clipRect"></param>
		/// <param name="softness"></param>
		public void EnterClipping(uint clipId, Rect? clipRect, Vector4? softness, bool reversedMask)
		{
			_clipStack.Push(clipInfo);

			if (clipRect == null)
			{
				if (stencilReferenceValue == 0)
					stencilReferenceValue = 1;
				else
					stencilReferenceValue = stencilReferenceValue << 1;
				clipInfo.clipId = clipId;
				clipInfo.stencil = true;
				clipInfo.reversedMask = reversedMask;
				clipped = true;
			}
			else
			{
				Rect rect = (Rect)clipRect;
				if (rectMaskDepth > 0)
					rect = ToolSet.Intersection(ref clipInfo.rect, ref rect);

				rectMaskDepth++;
				clipInfo.stencil = false;
				clipped = true;

				/* clipPos = xy * clipBox.zw + clipBox.xy
					* 利用这个公式，使clipPos变为当前顶点距离剪切区域中心的距离值，剪切区域的大小为2x2
					* 那么abs(clipPos)>1的都是在剪切区域外
					*/

				clipInfo.rect = rect;
				rect.x = rect.x + rect.width / 2f;
				rect.y = rect.y + rect.height / 2f;
				rect.width /= 2f;
				rect.height /= 2f;
				if (rect.width == 0 || rect.height == 0)
					clipInfo.clipBox = new Vector4(-2, -2, 0, 0);
				else
					clipInfo.clipBox = new Vector4(-rect.x / rect.width, -rect.y / rect.height,
						1.0f / rect.width, 1.0f / rect.height);
				clipInfo.clipId = clipId;
				clipInfo.soft = softness != null;
				if (clipInfo.soft)
				{
					clipInfo.softness = (Vector4)softness;
					float vx = clipInfo.rect.width * Screen.height * 0.25f;
					float vy = clipInfo.rect.height * Screen.height * 0.25f;

					if (clipInfo.softness.x > 0)
						clipInfo.softness.x = vx / clipInfo.softness.x;
					else
						clipInfo.softness.x = 10000f;

					if (clipInfo.softness.y > 0)
						clipInfo.softness.y = vy / clipInfo.softness.y;
					else
						clipInfo.softness.y = 10000f;

					if (clipInfo.softness.z > 0)
						clipInfo.softness.z = vx / clipInfo.softness.z;
					else
						clipInfo.softness.z = 10000f;

					if (clipInfo.softness.w > 0)
						clipInfo.softness.w = vy / clipInfo.softness.w;
					else
						clipInfo.softness.w = 10000f;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void LeaveClipping()
		{
			if (clipInfo.stencil)
				stencilReferenceValue = stencilReferenceValue >> 1;
			else
				rectMaskDepth--;

			clipInfo = _clipStack.Pop();
			clipped = _clipStack.Count > 0;
		}
	}
}
