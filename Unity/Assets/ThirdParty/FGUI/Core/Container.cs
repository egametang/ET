using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Container : DisplayObject
	{
		/// <summary>
		/// 
		/// </summary>
		public RenderMode renderMode;

		/// <summary>
		/// 
		/// </summary>
		public Camera renderCamera;

		/// <summary>
		/// 
		/// </summary>
		public bool opaque;

		/// <summary>
		/// 
		/// </summary>
		public Vector4? clipSoftness;

		/// <summary>
		/// 
		/// </summary>
		public IHitTest hitArea;

		/// <summary>
		/// 
		/// </summary>
		public bool touchChildren;

		/// <summary>
		/// 
		/// </summary>
		public EventCallback0 onUpdate;

		/// <summary>
		/// 
		/// </summary>
		public bool reversedMask;

		List<DisplayObject> _children;
		DisplayObject _mask;
		Rect? _clipRect;

		bool _fBatchingRequested;
		bool _fBatchingRoot;
		bool _fBatching;
		List<DisplayObject> _descendants;

		internal bool _disabled;
		internal int _panelOrder;

		/// <summary>
		/// 
		/// </summary>
		public Container()
			: base()
		{
			CreateGameObject("Container");
			Init();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameObjectName"></param>
		public Container(string gameObjectName)
			: base()
		{
			CreateGameObject(gameObjectName);
			Init();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attachTarget"></param>
		public Container(GameObject attachTarget)
			: base()
		{
			SetGameObject(attachTarget);
			Init();
		}

		void Init()
		{
			_children = new List<DisplayObject>();
			touchChildren = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public int numChildren
		{
			get { return _children.Count; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public DisplayObject AddChild(DisplayObject child)
		{
			AddChildAt(child, _children.Count);
			return child;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public DisplayObject AddChildAt(DisplayObject child, int index)
		{
			int count = _children.Count;
			if (index >= 0 && index <= count)
			{
				if (child.parent == this)
				{
					SetChildIndex(child, index);
				}
				else
				{
					child.RemoveFromParent();
					if (index == count)
						_children.Add(child);
					else
						_children.Insert(index, child);
					child.InternalSetParent(this);

					if (stage != null)
					{
						if (child is Container)
							child.onAddedToStage.BroadcastCall();
						else
							child.onAddedToStage.Call();
					}

					InvalidateBatchingState(true);
				}
				return child;
			}
			else
			{
				throw new Exception("Invalid child index");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public bool Contains(DisplayObject child)
		{
			return _children.Contains(child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public DisplayObject GetChildAt(int index)
		{
			return _children[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public DisplayObject GetChild(string name)
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; ++i)
			{
				if (_children[i].name == name)
					return _children[i];
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public int GetChildIndex(DisplayObject child)
		{
			return _children.IndexOf(child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public DisplayObject RemoveChild(DisplayObject child)
		{
			return RemoveChild(child, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="dispose"></param>
		/// <returns></returns>
		public DisplayObject RemoveChild(DisplayObject child, bool dispose)
		{
			if (child.parent != this)
				throw new Exception("obj is not a child");

			int i = _children.IndexOf(child);
			if (i >= 0)
				return RemoveChildAt(i, dispose);
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public DisplayObject RemoveChildAt(int index)
		{
			return RemoveChildAt(index, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="dispose"></param>
		/// <returns></returns>
		public DisplayObject RemoveChildAt(int index, bool dispose)
		{
			if (index >= 0 && index < _children.Count)
			{
				DisplayObject child = _children[index];

				if (stage != null && !child._disposed)
				{
					if (child is Container)
						child.onRemovedFromStage.BroadcastCall();
					else
						child.onRemovedFromStage.Call();
				}
				_children.Remove(child);
				InvalidateBatchingState(true);
				if (!dispose)
					child.InternalSetParent(null);
				else
					child.Dispose();

				return child;
			}
			else
				throw new Exception("Invalid child index");
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveChildren()
		{
			RemoveChildren(0, int.MaxValue, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="beginIndex"></param>
		/// <param name="endIndex"></param>
		/// <param name="dispose"></param>
		public void RemoveChildren(int beginIndex, int endIndex, bool dispose)
		{
			if (endIndex < 0 || endIndex >= numChildren)
				endIndex = numChildren - 1;

			for (int i = beginIndex; i <= endIndex; ++i)
				RemoveChildAt(beginIndex, dispose);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		public void SetChildIndex(DisplayObject child, int index)
		{
			int oldIndex = _children.IndexOf(child);
			if (oldIndex == index) return;
			if (oldIndex == -1) throw new ArgumentException("Not a child of this container");
			_children.RemoveAt(oldIndex);
			if (index >= _children.Count)
				_children.Add(child);
			else
				_children.Insert(index, child);
			InvalidateBatchingState(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child1"></param>
		/// <param name="child2"></param>
		public void SwapChildren(DisplayObject child1, DisplayObject child2)
		{
			int index1 = _children.IndexOf(child1);
			int index2 = _children.IndexOf(child2);
			if (index1 == -1 || index2 == -1)
				throw new Exception("Not a child of this container");
			SwapChildrenAt(index1, index2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index1"></param>
		/// <param name="index2"></param>
		public void SwapChildrenAt(int index1, int index2)
		{
			DisplayObject obj1 = _children[index1];
			DisplayObject obj2 = _children[index2];
			_children[index1] = obj2;
			_children[index2] = obj1;
			InvalidateBatchingState(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indice"></param>
		/// <param name="objs"></param>
		public void ChangeChildrenOrder(List<int> indice, List<DisplayObject> objs)
		{
			int cnt = indice.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject obj = objs[i];
				if (obj.parent != this)
					throw new Exception("Not a child of this container");

				_children[indice[i]] = obj;
			}
			InvalidateBatchingState(true);
		}

		/// <summary>
		/// 
		/// </summary>
		public Rect? clipRect
		{
			get { return _clipRect; }
			set
			{
				if (_clipRect != value)
				{
					_clipRect = value;
					UpdateBatchingFlags();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject mask
		{
			get { return _mask; }
			set
			{
				if (_mask != value)
				{
					_mask = value;
					UpdateBatchingFlags();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		override public bool touchable
		{
			get { return base.touchable; }
			set
			{
				base.touchable = value;
				if (hitArea != null)
					hitArea.SetEnabled(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Rect contentRect
		{
			get { return _contentRect; }
			set
			{
				_contentRect = value;
				OnSizeChanged(true, true);
			}
		}

		public override Rect GetBounds(DisplayObject targetSpace)
		{
			if (_clipRect != null)
				return TransformRect((Rect)_clipRect, targetSpace);

			int count = _children.Count;

			Rect rect;
			if (count == 0)
			{
				Vector2 v = TransformPoint(Vector2.zero, targetSpace);
				rect = Rect.MinMaxRect(v.x, v.y, 0, 0);
			}
			else if (count == 1)
			{
				rect = _children[0].GetBounds(targetSpace);
			}
			else
			{
				float minX = float.MaxValue, maxX = float.MinValue;
				float minY = float.MaxValue, maxY = float.MinValue;

				for (int i = 0; i < count; ++i)
				{
					rect = _children[i].GetBounds(targetSpace);
					minX = minX < rect.xMin ? minX : rect.xMin;
					maxX = maxX > rect.xMax ? maxX : rect.xMax;
					minY = minY < rect.yMin ? minY : rect.yMin;
					maxY = maxY > rect.yMax ? maxY : rect.yMax;
				}

				rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
			}

			return rect;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Camera GetRenderCamera()
		{
			if (renderMode == RenderMode.ScreenSpaceOverlay)
				return StageCamera.main;
			else
			{
				Camera cam = this.renderCamera;
				if (cam == null)
					cam = HitTestContext.cachedMainCamera;
				if (cam == null)
					cam = StageCamera.main;
				return cam;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stagePoint"></param>
		/// <returns></returns>
		public DisplayObject HitTest(Vector2 stagePoint, bool forTouch)
		{
			if (StageCamera.main == null)
			{
				if (this is Stage)
					return this;
				else
					return null;
			}

			HitTestContext.screenPoint = new Vector2(stagePoint.x, Screen.height - stagePoint.y);
			HitTestContext.worldPoint = StageCamera.main.ScreenToWorldPoint(HitTestContext.screenPoint);
			HitTestContext.direction = Vector3.back;
			HitTestContext.forTouch = forTouch;

			DisplayObject ret = HitTest();
			if (ret != null)
				return ret;
			else if (this is Stage)
				return this;
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Vector2 GetHitTestLocalPoint()
		{
			if (this.renderMode == RenderMode.WorldSpace)
			{
				Camera camera = GetRenderCamera();

				Vector3 screenPoint = camera.WorldToScreenPoint(this.cachedTransform.position); //only for query z value
				screenPoint.x = HitTestContext.screenPoint.x;
				screenPoint.y = HitTestContext.screenPoint.y;

				//获得本地z轴在世界坐标的方向
				HitTestContext.worldPoint = camera.ScreenToWorldPoint(screenPoint);
				Ray ray = camera.ScreenPointToRay(screenPoint);
				HitTestContext.direction = Vector3.zero - ray.direction;
			}

			return WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
		}

		override protected DisplayObject HitTest()
		{
			if (_disabled)
				return null;

			if (this.cachedTransform.localScale.x == 0 || this.cachedTransform.localScale.y == 0)
				return null;

			Vector2 localPoint = new Vector2();
			Vector3 savedWorldPoint = HitTestContext.worldPoint;
			Vector3 savedDirection = HitTestContext.direction;

			if (hitArea != null)
			{
				if (!hitArea.HitTest(this, ref localPoint))
				{
					HitTestContext.worldPoint = savedWorldPoint;
					HitTestContext.direction = savedDirection;
					return null;
				}
			}
			else
			{
				localPoint = GetHitTestLocalPoint();
				if (_clipRect != null && !((Rect)_clipRect).Contains(localPoint))
				{
					HitTestContext.worldPoint = savedWorldPoint;
					HitTestContext.direction = savedDirection;
					return null;
				}
			}

			if (_mask != null)
			{
				DisplayObject tmp = _mask.InternalHitTestMask();
				if (!reversedMask && tmp == null || reversedMask && tmp != null)
					return null;
			}

			DisplayObject target = null;
			if (touchChildren)
			{
				int count = _children.Count;
				for (int i = count - 1; i >= 0; --i) // front to back!
				{
					DisplayObject child = _children[i];
					if (child == _mask)
						continue;

					target = child.InternalHitTest();
					if (target != null)
						break;
				}
			}

			if (target == null && opaque && (hitArea != null || _contentRect.Contains(localPoint)))
				target = this;

			HitTestContext.worldPoint = savedWorldPoint;
			HitTestContext.direction = savedDirection;

			return target;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsAncestorOf(DisplayObject obj)
		{
			if (obj == null)
				return false;

			Container p = obj.parent;
			while (p != null)
			{
				if (p == this)
					return true;

				p = p.parent;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool fairyBatching
		{
			get { return _fBatching; }
			set
			{
				if (_fBatching != value)
				{
					_fBatching = value;
					UpdateBatchingFlags();
				}
			}
		}

		internal void UpdateBatchingFlags()
		{
			bool oldValue = _fBatchingRoot;
			_fBatchingRoot = _fBatching || _clipRect != null || _mask != null || _paintingMode > 0;
			if (oldValue != _fBatchingRoot)
			{
				if (_fBatchingRoot)
					_fBatchingRequested = true;
				else if (_descendants != null)
					_descendants.Clear();

				InvalidateBatchingState();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="childrenChanged"></param>
		public void InvalidateBatchingState(bool childrenChanged)
		{
			if (childrenChanged && _fBatchingRoot)
				_fBatchingRequested = true;
			else
			{
				Container p = this.parent;
				while (p != null)
				{
					if (p._fBatchingRoot)
					{
						p._fBatchingRequested = true;
						break;
					}

					p = p.parent;
				}
			}
		}

		/// <summary>
		/// s
		/// </summary>
		/// <param name="value"></param>
		public void SetChildrenLayer(int value)
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _children[i];
				child.layer = value;
				if ((child is Container) && !child.paintingMode)
					((Container)child).SetChildrenLayer(value);
			}
		}

		override public void Update(UpdateContext context)
		{
			if (_disabled)
				return;

			base.Update(context);

			if (_cacheAsBitmap && _paintingMode != 0 && _paintingFlag == 2)
			{
				if (onUpdate != null)
					onUpdate();
				return;
			}

			if (_mask != null)
				context.EnterClipping(this.id, null, null, reversedMask);
			else if (_clipRect != null)
				context.EnterClipping(this.id, this.TransformRect((Rect)_clipRect, null), clipSoftness, false);

			float savedAlpha = context.alpha;
			context.alpha *= this.alpha;
			bool savedGrayed = context.grayed;
			context.grayed = context.grayed || this.grayed;

			if (_fBatching)
				context.batchingDepth++;

			if (context.batchingDepth > 0)
			{
				if (_mask != null)
					_mask.graphics.maskFrameId = UpdateContext.frameId;

				int cnt = _children.Count;
				for (int i = 0; i < cnt; i++)
				{
					DisplayObject child = _children[i];
					if (child.visible)
						child.Update(context);
				}
			}
			else
			{
				if (_mask != null)
				{
					_mask.graphics.maskFrameId = UpdateContext.frameId;
					_mask.renderingOrder = context.renderingOrder++;
				}

				int cnt = _children.Count;
				for (int i = 0; i < cnt; i++)
				{
					DisplayObject child = _children[i];
					if (child.visible)
					{
						if (child != _mask)
							child.renderingOrder = context.renderingOrder++;

						child.Update(context);
					}
				}

				if (_mask != null)
					_mask.graphics.SetStencilEraserOrder(context.renderingOrder++);
			}

			if (_fBatching)
			{
				if (context.batchingDepth == 1)
					SetRenderingOrder(context);
				context.batchingDepth--;
			}

			context.alpha = savedAlpha;
			context.grayed = savedGrayed;

			if (_clipRect != null || _mask != null)
				context.LeaveClipping();

			if (_paintingMode > 0 && paintingGraphics.texture != null)
				UpdateContext.OnEnd += _captureDelegate;

			if (onUpdate != null)
				onUpdate();
		}

		private void SetRenderingOrder(UpdateContext context)
		{
			if (_fBatchingRequested)
				DoFairyBatching();

			if (_mask != null)
				_mask.renderingOrder = context.renderingOrder++;

			int cnt = _descendants.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _descendants[i];
				if (child != _mask)
					child.renderingOrder = context.renderingOrder++;

				if ((child is Container) && ((Container)child)._fBatchingRoot)
					((Container)child).SetRenderingOrder(context);
			}

			if (_mask != null && _mask.graphics != null)
				_mask.graphics.SetStencilEraserOrder(context.renderingOrder++);
		}

		private void DoFairyBatching()
		{
			_fBatchingRequested = false;

			if (_descendants == null)
				_descendants = new List<DisplayObject>();
			else
				_descendants.Clear();
			CollectChildren(this, false);

			int cnt = _descendants.Count;

			int i, j, k, m;
			object curMat, testMat, lastMat;
			DisplayObject current, test;
			float[] bound;
			for (i = 0; i < cnt; i++)
			{
				current = _descendants[i];
				bound = current._internal_bounds;
				curMat = current.material;
				if (curMat == null || current._skipInFairyBatching)
					continue;

				k = -1;
				lastMat = null;
				m = i;
				for (j = i - 1; j >= 0; j--)
				{
					test = _descendants[j];
					if (test._skipInFairyBatching)
						break;

					testMat = test.material;
					if (testMat != null)
					{
						if (lastMat != testMat)
						{
							lastMat = testMat;
							m = j + 1;
						}

						if (curMat == testMat)
							k = m;
					}

					if ((bound[0] > test._internal_bounds[0] ? bound[0] : test._internal_bounds[0])
						<= (bound[2] < test._internal_bounds[2] ? bound[2] : test._internal_bounds[2])
						&& (bound[1] > test._internal_bounds[1] ? bound[1] : test._internal_bounds[1])
						<= (bound[3] < test._internal_bounds[3] ? bound[3] : test._internal_bounds[3]))
					{
						if (k == -1)
							k = m;
						break;
					}
				}
				if (k != -1 && i != k)
				{
					_descendants.RemoveAt(i);
					_descendants.Insert(k, current);
				}
			}

			//Debug.Log("DoFairyBatching " + cnt + "," + this.cachedTransform.GetInstanceID());
		}

		private void CollectChildren(Container initiator, bool outlineChanged)
		{
			int count = _children.Count;
			for (int i = 0; i < count; i++)
			{
				DisplayObject child = _children[i];
				if (!child.visible)
					continue;

				if (child is Container)
				{
					Container container = (Container)child;
					if (container._fBatchingRoot)
					{
						initiator._descendants.Add(container);
						if (outlineChanged || container._outlineChanged)
						{
							Rect rect = container.GetBounds(initiator);
							container._internal_bounds[0] = rect.xMin;
							container._internal_bounds[1] = rect.yMin;
							container._internal_bounds[2] = rect.xMax;
							container._internal_bounds[3] = rect.yMax;
						}
						if (container._fBatchingRequested)
							container.DoFairyBatching();
					}
					else
						container.CollectChildren(initiator, outlineChanged || container._outlineChanged);
				}
				else if (child != initiator._mask)
				{
					if (outlineChanged || child._outlineChanged)
					{
						Rect rect = child.GetBounds(initiator);
						child._internal_bounds[0] = rect.xMin;
						child._internal_bounds[1] = rect.yMin;
						child._internal_bounds[2] = rect.xMax;
						child._internal_bounds[3] = rect.yMax;
					}
					initiator._descendants.Add(child);
				}

				child._outlineChanged = false;
			}
		}

		public override void Dispose()
		{
			if (_disposed)
				return;

			base.Dispose(); //Destroy GameObject tree first, avoid destroying each seperately;

			int numChildren = _children.Count;
			for (int i = numChildren - 1; i >= 0; --i)
			{
				DisplayObject obj = _children[i];
				obj.InternalSetParent(null); //Avoid RemoveParent call
				obj.Dispose();
			}
		}
	}
}
