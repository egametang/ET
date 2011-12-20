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
using System.ComponentModel.Composition;
using Infrastructure;
using BehaviorTree;

namespace TreeCanvas
{
	/// <summary>
	/// TreeCanvasView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "TreeCanvasRegion")]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class TreeCanvasView : UserControl
	{
		public TreeCanvasView()
		{
			InitializeComponent();
		}

		[Import]
		TreeCanvasViewModel ViewModel
		{
			get
			{
				return this.DataContext as TreeCanvasViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}

		private void NewNode_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void btnDynamic_Click(object sender, RoutedEventArgs e)
		{
			DrawDynamicTree();
		}

		private void DrawDynamicTree()
		{
			tc.Clear();
			Button btnA = new Button();
			Button btnB = new Button();
			Button btnC = new Button();
			Button btnD = new Button();
			Button btnE = new Button();
			Button btnF = new Button();
			Button btnG = new Button();
			Button btnH = new Button();
			Button btnI = new Button();
			Button btnJ = new Button();
			Button btnK = new Button();
			Button btnL = new Button();
			Button btnM = new Button();
			Button btnN = new Button();
			Button btnO = new Button();
			btnA.Content = "A";
			btnB.Content = "B";
			btnC.Content = "C";
			btnD.Content = "D";
			btnE.Content = "E";
			btnF.Content = "F";
			btnG.Content = "G";
			btnH.Content = "H";
			btnI.Content = "I";
			btnJ.Content = "J";
			btnK.Content = "K";
			btnL.Content = "L";
			btnM.Content = "M";
			btnN.Content = "N";
			btnO.Content = "O";
			btnA.Click += new RoutedEventHandler(btn_Click);
			btnB.Click += new RoutedEventHandler(btn_Click);
			btnC.Click += new RoutedEventHandler(btn_Click);
			btnD.Click += new RoutedEventHandler(btn_Click);
			btnE.Click += new RoutedEventHandler(btn_Click);
			btnF.Click += new RoutedEventHandler(btn_Click);
			btnG.Click += new RoutedEventHandler(btn_Click);
			btnH.Click += new RoutedEventHandler(btn_Click);
			btnI.Click += new RoutedEventHandler(btn_Click);
			btnJ.Click += new RoutedEventHandler(btn_Click);
			btnK.Click += new RoutedEventHandler(btn_Click);
			btnL.Click += new RoutedEventHandler(btn_Click);
			btnM.Click += new RoutedEventHandler(btn_Click);
			btnN.Click += new RoutedEventHandler(btn_Click);
			btnO.Click += new RoutedEventHandler(btn_Click);

			TreeNode tnSubtreeRootO = tc.AddRoot(btnO);
			TreeNode tnSubtreeRootE = tc.AddNode(btnE, tnSubtreeRootO);
			tc.AddNode(btnF, tnSubtreeRootO);
			TreeNode tnSubtreeRootN = tc.AddNode(btnN, tnSubtreeRootO);
			TreeNode tnSubtreeRootD = tc.AddNode(btnD, tnSubtreeRootE);
			tc.AddNode(btnA, tnSubtreeRootE);
			tc.AddNode(btnB, tnSubtreeRootD);
			tc.AddNode(btnC, tnSubtreeRootD);
			tc.AddNode(btnG, tnSubtreeRootN);
			TreeNode tnSubtreeRootM = tc.AddNode(btnM, tnSubtreeRootN);
			tc.AddNode(btnH, tnSubtreeRootM);
			tc.AddNode(btnI, tnSubtreeRootM);
			tc.AddNode(btnJ, tnSubtreeRootM);
			tc.AddNode(btnK, tnSubtreeRootM);
			tc.AddNode(btnL, tnSubtreeRootM);
		}

		private void btn_Click(object sender, RoutedEventArgs e)
		{
			Button btn = e.OriginalSource as Button;
			if (btn != null)
			{
				TreeNode tn = (TreeNode)(btn.Parent);
				tn.Collapsed = !tn.Collapsed;
			}
		}
	}
}
