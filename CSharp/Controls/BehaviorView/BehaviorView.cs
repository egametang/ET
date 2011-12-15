using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Controls.BehaviorView
{
	/// <summary>
	/// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
	///
	/// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中:
	///
	///     xmlns:MyNamespace="clr-namespace:BehaviorView"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中:
	///
	///     xmlns:MyNamespace="clr-namespace:BehaviorView;assembly=BehaviorView"
	///
	/// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
	/// 并重新生成以避免编译错误:
	///
	///     在解决方案资源管理器中右击目标项目，然后依次单击
	///     “添加引用”->“项目”->[选择此项目]
	///
	///
	/// 步骤 2)
	/// 继续操作并在 XAML 文件中使用控件。
	///
	///     <MyNamespace:CustomControl1/>
	///
	/// </summary>
	/// <summary>
	/// A Canvas which manages dragging of the UIElements it contains.  
	/// </summary>
	public class BehaviorView : Canvas
	{
		// Stores a reference to the UIElement currently being dragged by the user.
		private UIElement elementBeingDragged;

		// Keeps track of where the mouse cursor was when a drag operation began.		
		private Point origCursorLocation;

		// The offsets from the DragCanvas' edges when the drag operation began.
		private double origHorizOffset, origVertOffset;

		// Keeps track of which horizontal and vertical offset should be modified for the drag element.
		private bool modifyLeftOffset, modifyTopOffset;

		// True if a drag operation is underway, else false.
		private bool isDragInProgress;

		public static readonly DependencyProperty CanBeDraggedProperty;

		public static bool GetCanBeDragged(UIElement uiElement)
		{
			if (uiElement == null)
				return false;

			return (bool)uiElement.GetValue(CanBeDraggedProperty);
		}

		public static void SetCanBeDragged(UIElement uiElement, bool value)
		{
			if (uiElement != null)
				uiElement.SetValue(CanBeDraggedProperty, value);
		}

		public static readonly DependencyProperty AllowDraggingProperty;
		public static readonly DependencyProperty AllowDragOutOfViewProperty;

		static BehaviorView()
		{
			AllowDraggingProperty = DependencyProperty.Register(
				"AllowDragging",
				typeof(bool),
				typeof(BehaviorView),
				new PropertyMetadata(true));

			AllowDragOutOfViewProperty = DependencyProperty.Register(
				"AllowDragOutOfView",
				typeof(bool),
				typeof(BehaviorView),
				new UIPropertyMetadata(false));

			CanBeDraggedProperty = DependencyProperty.RegisterAttached(
				"CanBeDragged",
				typeof(bool),
				typeof(BehaviorView),
				new UIPropertyMetadata(true));
		}

		/// <summary>
		/// Initializes a new instance of DragCanvas.  UIElements in
		/// the DragCanvas will immediately be draggable by the user.
		/// </summary>
		public BehaviorView()
		{
		}

		/// <summary>
		/// Gets/sets whether elements in the DragCanvas should be draggable by the user.
		/// The default value is true.  This is a dependency property.
		/// </summary>
		public bool AllowDragging
		{
			get { return (bool)base.GetValue(AllowDraggingProperty); }
			set { base.SetValue(AllowDraggingProperty, value); }
		}

		/// <summary>
		/// Gets/sets whether the user should be able to drag elements in the DragCanvas out of
		/// the viewable area.  The default value is false.  This is a dependency property.
		/// </summary>
		public bool AllowDragOutOfView
		{
			get { return (bool)GetValue(AllowDragOutOfViewProperty); }
			set { SetValue(AllowDragOutOfViewProperty, value); }
		}

		/// <summary>
		/// Assigns the element a z-index which will ensure that 
		/// it is in front of every other element in the Canvas.
		/// The z-index of every element whose z-index is between 
		/// the element's old and new z-index will have its z-index 
		/// decremented by one.
		/// </summary>
		/// <param name="targetElement">
		/// The element to be sent to the front of the z-order.
		/// </param>
		public void BringToFront(UIElement element)
		{
			this.UpdateZOrder(element, true);
		}

		/// <summary>
		/// Assigns the element a z-index which will ensure that 
		/// it is behind every other element in the Canvas.
		/// The z-index of every element whose z-index is between 
		/// the element's old and new z-index will have its z-index 
		/// incremented by one.
		/// </summary>
		/// <param name="targetElement">
		/// The element to be sent to the back of the z-order.
		/// </param>
		public void SendToBack(UIElement element)
		{
			this.UpdateZOrder(element, false);
		}

		/// <summary>
		/// Returns the UIElement currently being dragged, or null.
		/// </summary>
		/// <remarks>
		/// Note to inheritors: This property exposes a protected 
		/// setter which should be used to modify the drag element.
		/// </remarks>
		public UIElement ElementBeingDragged
		{
			get
			{
				if (!this.AllowDragging)
					return null;
				else
					return this.elementBeingDragged;
			}
			protected set
			{
				if (this.elementBeingDragged != null)
					this.elementBeingDragged.ReleaseMouseCapture();

				if (!this.AllowDragging)
					this.elementBeingDragged = null;
				else
				{
					if (BehaviorView.GetCanBeDragged(value))
					{
						this.elementBeingDragged = value;
						this.elementBeingDragged.CaptureMouse();
					}
					else
						this.elementBeingDragged = null;
				}
			}
		}

		/// <summary>
		/// Walks up the visual tree starting with the specified DependencyObject, 
		/// looking for a UIElement which is a child of the Canvas.  If a suitable 
		/// element is not found, null is returned.  If the 'depObj' object is a 
		/// UIElement in the Canvas's Children collection, it will be returned.
		/// </summary>
		/// <param name="depObj">
		/// A DependencyObject from which the search begins.
		/// </param>
		public UIElement FindCanvasChild(DependencyObject depObj)
		{
			while (depObj != null)
			{
				// If the current object is a UIElement which is a child of the
				// Canvas, exit the loop and return it.
				UIElement elem = depObj as UIElement;
				if (elem != null && base.Children.Contains(elem))
					break;

				// VisualTreeHelper works with objects of type Visual or Visual3D.
				// If the current object is not derived from Visual or Visual3D,
				// then use the LogicalTreeHelper to find the parent element.
				if (depObj is Visual || depObj is Visual3D)
					depObj = VisualTreeHelper.GetParent(depObj);
				else
					depObj = LogicalTreeHelper.GetParent(depObj);
			}
			return depObj as UIElement;
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			this.isDragInProgress = false;

			// Cache the mouse cursor location.
			this.origCursorLocation = e.GetPosition(this);

			// Walk up the visual tree from the element that was clicked, 
			// looking for an element that is a direct child of the Canvas.
			this.ElementBeingDragged = this.FindCanvasChild(e.Source as DependencyObject);
			if (this.ElementBeingDragged == null)
				return;

			// Get the element's offsets from the four sides of the Canvas.
			double left = Canvas.GetLeft(this.ElementBeingDragged);
			double right = Canvas.GetRight(this.ElementBeingDragged);
			double top = Canvas.GetTop(this.ElementBeingDragged);
			double bottom = Canvas.GetBottom(this.ElementBeingDragged);

			// Calculate the offset deltas and determine for which sides
			// of the Canvas to adjust the offsets.
			this.origHorizOffset = ResolveOffset(left, right, out this.modifyLeftOffset);
			this.origVertOffset = ResolveOffset(top, bottom, out this.modifyTopOffset);

			// Set the Handled flag so that a control being dragged 
			// does not react to the mouse input.
			e.Handled = true;

			this.isDragInProgress = true;
		}

		protected override void OnPreviewMouseMove(MouseEventArgs e)
		{
			base.OnPreviewMouseMove(e);

			// If no element is being dragged, there is nothing to do.
			if (this.ElementBeingDragged == null || !this.isDragInProgress)
				return;

			// Get the position of the mouse cursor, relative to the Canvas.
			Point cursorLocation = e.GetPosition(this);

			// These values will store the new offsets of the drag element.
			double newHorizontalOffset, newVerticalOffset;

			// Determine the horizontal offset.
			if (this.modifyLeftOffset)
				newHorizontalOffset = this.origHorizOffset + (cursorLocation.X - this.origCursorLocation.X);
			else
				newHorizontalOffset = this.origHorizOffset - (cursorLocation.X - this.origCursorLocation.X);

			// Determine the vertical offset.
			if (this.modifyTopOffset)
				newVerticalOffset = this.origVertOffset + (cursorLocation.Y - this.origCursorLocation.Y);
			else
				newVerticalOffset = this.origVertOffset - (cursorLocation.Y - this.origCursorLocation.Y);

			if (!this.AllowDragOutOfView)
			{
				// Get the bounding rect of the drag element.
				Rect elemRect = this.CalculateDragElementRect(newHorizontalOffset, newVerticalOffset);

				//
				// If the element is being dragged out of the viewable area, 
				// determine the ideal rect location, so that the element is 
				// within the edge(s) of the canvas.
				//
				bool leftAlign = elemRect.Left < 0;
				bool rightAlign = elemRect.Right > this.ActualWidth;

				if (leftAlign)
					newHorizontalOffset = modifyLeftOffset ? 0 : this.ActualWidth - elemRect.Width;
				else if (rightAlign)
					newHorizontalOffset = modifyLeftOffset ? this.ActualWidth - elemRect.Width : 0;

				bool topAlign = elemRect.Top < 0;
				bool bottomAlign = elemRect.Bottom > this.ActualHeight;

				if (topAlign)
					newVerticalOffset = modifyTopOffset ? 0 : this.ActualHeight - elemRect.Height;
				else if (bottomAlign)
					newVerticalOffset = modifyTopOffset ? this.ActualHeight - elemRect.Height : 0;
			}

			if (this.modifyLeftOffset)
				Canvas.SetLeft(this.ElementBeingDragged, newHorizontalOffset);
			else
				Canvas.SetRight(this.ElementBeingDragged, newHorizontalOffset);

			if (this.modifyTopOffset)
				Canvas.SetTop(this.ElementBeingDragged, newVerticalOffset);
			else
				Canvas.SetBottom(this.ElementBeingDragged, newVerticalOffset);
		}

		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseUp(e);

			// Reset the field whether the left or right mouse button was 
			// released, in case a context menu was opened on the drag element.
			this.ElementBeingDragged = null;
		}

		/// <summary>
		/// Returns a Rect which describes the bounds of the element being dragged.
		/// </summary>
		private Rect CalculateDragElementRect(double newHorizOffset, double newVertOffset)
		{
			if (this.ElementBeingDragged == null)
				throw new InvalidOperationException("ElementBeingDragged is null.");

			Size elemSize = this.ElementBeingDragged.RenderSize;

			double x, y;

			if (this.modifyLeftOffset)
				x = newHorizOffset;
			else
				x = this.ActualWidth - newHorizOffset - elemSize.Width;

			if (this.modifyTopOffset)
				y = newVertOffset;
			else
				y = this.ActualHeight - newVertOffset - elemSize.Height;

			Point elemLoc = new Point(x, y);

			return new Rect(elemLoc, elemSize);
		}

		/// <summary>
		/// Determines one component of a UIElement's location 
		/// within a Canvas (either the horizontal or vertical offset).
		/// </summary>
		/// <param name="side1">
		/// The value of an offset relative to a default side of the 
		/// Canvas (i.e. top or left).
		/// </param>
		/// <param name="side2">
		/// The value of the offset relative to the other side of the 
		/// Canvas (i.e. bottom or right).
		/// </param>
		/// <param name="useSide1">
		/// Will be set to true if the returned value should be used 
		/// for the offset from the side represented by the 'side1' 
		/// parameter.  Otherwise, it will be set to false.
		/// </param>
		private static double ResolveOffset(double side1, double side2, out bool useSide1)
		{
			// If the Canvas.Left and Canvas.Right attached properties 
			// are specified for an element, the 'Left' value is honored.
			// The 'Top' value is honored if both Canvas.Top and 
			// Canvas.Bottom are set on the same element.  If one 
			// of those attached properties is not set on an element, 
			// the default value is Double.NaN.
			useSide1 = true;
			double result;
			if (Double.IsNaN(side1))
			{
				if (Double.IsNaN(side2))
				{
					// Both sides have no value, so set the
					// first side to a value of zero.
					result = 0;
				}
				else
				{
					result = side2;
					useSide1 = false;
				}
			}
			else
			{
				result = side1;
			}
			return result;
		}

		/// <summary>
		/// Helper method used by the BringToFront and SendToBack methods.
		/// </summary>
		/// <param name="element">
		/// The element to bring to the front or send to the back.
		/// </param>
		/// <param name="bringToFront">
		/// Pass true if calling from BringToFront, else false.
		/// </param>
		private void UpdateZOrder(UIElement element, bool bringToFront)
		{

			if (element == null)
				throw new ArgumentNullException("element");

			if (!base.Children.Contains(element))
				throw new ArgumentException("Must be a child element of the Canvas.", "element");

			// Determine the Z-Index for the target UIElement.
			int elementNewZIndex = -1;
			if (bringToFront)
			{
				foreach (UIElement elem in base.Children)
					if (elem.Visibility != Visibility.Collapsed)
						++elementNewZIndex;
			}
			else
			{
				elementNewZIndex = 0;
			}

			// Determine if the other UIElements' Z-Index 
			// should be raised or lowered by one. 
			int offset = (elementNewZIndex == 0) ? +1 : -1;

			int elementCurrentZIndex = Canvas.GetZIndex(element);


			// Update the Z-Index of every UIElement in the Canvas.
			foreach (UIElement childElement in base.Children)
			{
				if (childElement == element)
					Canvas.SetZIndex(element, elementNewZIndex);
				else
				{
					int zIndex = Canvas.GetZIndex(childElement);

					// Only modify the z-index of an element if it is  
					// in between the target element's old and new z-index.
					if (bringToFront && elementCurrentZIndex < zIndex ||
						!bringToFront && zIndex < elementCurrentZIndex)
					{
						Canvas.SetZIndex(childElement, zIndex + offset);
					}
				}
			}
		}
	}
}
