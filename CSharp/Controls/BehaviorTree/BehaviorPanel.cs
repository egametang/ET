using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BehaviorTree
{
	public class BehaviorPanel : Panel
	{
		private LayeredTreeDraw ltd;
		private int iNextNameSuffix = 0;
		private Path pthConnections = null;

		public static readonly DependencyProperty VerticalBufferProperty =
			DependencyProperty.Register(
				"VerticalBuffer",
				typeof(double),
				typeof(BehaviorPanel),
				null
			);

		public static readonly DependencyProperty VerticalJustificationProperty =
			DependencyProperty.Register(
				"VerticalJustification",
				typeof(VerticalJustification),
				typeof(BehaviorPanel),
				null
			);

		public readonly static DependencyProperty HorizontalBufferSubtreeProperty =
			DependencyProperty.Register(
				"HorizontalBufferSubtree",
				typeof(double),
				typeof(BehaviorPanel),
				null
			);

		public readonly static DependencyProperty HorizontalBufferProperty =
			DependencyProperty.Register(
				"HorizontalBuffer",
				typeof(double),
				typeof(BehaviorPanel),
				null
			);

		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register(
				"Root",
				typeof(String),
				typeof(BehaviorPanel),
				null
			);

		private static Point PtFromPoint(Point dpt)
		{
			return new Point(dpt.X, dpt.Y);
		}

		public string Root
		{
			get
			{
				return (string)GetValue(RootProperty);
			}
			set
			{
				SetValue(RootProperty, value);
			}
		}

		public VerticalJustification VerticalJustification
		{
			get
			{
				return (VerticalJustification)GetValue(VerticalJustificationProperty);
			}
			set
			{
				SetValue(VerticalJustificationProperty, value);
			}
		}

		public double VerticalBuffer
		{
			get { return (double)GetValue(VerticalBufferProperty); }
			set { SetValue(VerticalBufferProperty, value); }
		}

		public double HorizontalBufferSubtree
		{
			get { return (double)GetValue(HorizontalBufferSubtreeProperty); }
			set { SetValue(HorizontalBufferSubtreeProperty, value); }
		}

		public double HorizontalBuffer
		{
			get { return (double)GetValue(HorizontalBufferProperty); }
			set { SetValue(HorizontalBufferProperty, value); }
		}

		private void SetParents(TreeNode tnRoot)
		{
			// First pass to clear all parents
			foreach (UIElement uiel in Children)
			{
				TreeNode tn = uiel as TreeNode;
				if (tn != null)
				{
					tn.ClearParent();
				}
			}

			// Second pass to properly set them from their children...
			foreach (UIElement uiel in Children)
			{
				TreeNode tn = uiel as TreeNode;
				if (tn != null && tn != tnRoot)
				{
					tn.SetParent();
				}
			}
		}

		public void Clear()
		{
			Children.Clear();
			pthConnections = null;
		}

		private void SetName(TreeNode tn, string strName)
		{
			tn.Name = strName;
			tn.SetValue(Panel.NameProperty, strName);
		}

		public TreeNode AddRoot(Object objContent, string strName)
		{
			TreeNode tnNew = new TreeNode();
			SetName(tnNew, strName);
			tnNew.Content = objContent;
			Children.Add(tnNew);
			Root = strName;
			return tnNew;
		}

		public TreeNode AddRoot(Object objContent)
		{
			return AddRoot(objContent, StrNextName());
		}

		public TreeNode AddNode(Object objContent, string strName, string strParent)
		{
			TreeNode tnNew = new TreeNode();
			SetName(tnNew, strName);
			tnNew.Content = objContent;
			tnNew.TreeParent = strParent;
			Children.Add(tnNew);
			return tnNew;
		}

		private string StrNextName()
		{
			return "__TreeNode" + iNextNameSuffix++;
		}

		public TreeNode AddNode(Object objContent, string strName, TreeNode tnParent)
		{
			return AddNode(objContent, strName, tnParent.Name);
		}

		public TreeNode AddNode(Object objContent, TreeNode tnParent)
		{
			return AddNode(objContent, StrNextName(), tnParent.Name);
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (Children.Count == 0)
			{
				return new Size(100, 20);
			}

			if (pthConnections != null && Children.Contains(pthConnections))
			{
				Children.Remove(pthConnections);
				pthConnections = null;
			}

			Size szFinal = new Size(0, 0);
			string strRoot = Root;
			TreeNode tnRoot = this.FindName(strRoot) as TreeNode;

			foreach (UIElement uiel in Children)
			{
				uiel.Measure(availableSize);
				Size szThis = uiel.DesiredSize;

				if (szThis.Width > szFinal.Width || szThis.Height > szFinal.Height)
				{
					szFinal = new Size(
						Math.Max(szThis.Width, szFinal.Width),
						Math.Max(szThis.Height, szFinal.Height));
				}
			}

			if (tnRoot != null)
			{
				SetParents(tnRoot);
				ltd = new LayeredTreeDraw(tnRoot, HorizontalBuffer, HorizontalBufferSubtree, VerticalBuffer, VerticalJustification.TOP);
				ltd.LayoutTree();
				szFinal = new Size(ltd.PxOverallWidth, ltd.PxOverallHeight);
			}

			// Put in the connections too...
			if (ltd.Connections != null)
			{
				pthConnections = new Path();
				PathGeometry pg = new PathGeometry();
				pthConnections.Stroke = new SolidColorBrush(Colors.Black);
				pthConnections.StrokeThickness = 1.0;
				pg.Figures = new PathFigureCollection();

				foreach (TreeConnection tcn in ltd.Connections)
				{
					PathFigure pf = new PathFigure();

					pf.StartPoint = PtFromPoint(tcn.LstPt[0]);
					pf.IsClosed = false;
					pf.Segments = new PathSegmentCollection();
					for (int iPt = 1; iPt < tcn.LstPt.Count; iPt++)
					{
						LineSegment ls = new LineSegment();
						ls.Point = PtFromPoint(tcn.LstPt[iPt]);
						pf.Segments.Add(ls);
					}
					pg.Figures.Add(pf);
				}
				pthConnections.Data = pg;
				Children.Add(pthConnections);
				pthConnections.Measure(availableSize);
			}
			return szFinal;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (UIElement uiel in Children)
			{
				TreeNode tn = uiel as TreeNode;
				Point ptLocation = new Point(0, 0);
				if (tn != null)
				{
					ptLocation = new Point(ltd.X(tn), ltd.Y(tn));
				}
				uiel.Arrange(new Rect(ptLocation, uiel.DesiredSize));
			}
			return finalSize;
		}
	}
}
