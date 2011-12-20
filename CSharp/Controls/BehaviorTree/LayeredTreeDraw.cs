#define FIX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BehaviorTree
{
	public enum VerticalJustification
	{
		TOP,
		CENTER,
		BOTTOM
	}

	public class LayeredTreeDraw
	{
		ITreeNode tnRoot;
		double pxBufferHorizontal;
		double pxBufferHorizontalSubtree;
		double pxBufferVertical;
		List<TreeConnection> lsttcn = new List<TreeConnection>();
		List<double> lstLayerHeight = new List<double>();
		VerticalJustification vj;
		static TreeNodeGroup tngEmpty = new TreeNodeGroup();

		public double PxOverallHeight 
		{ 
			get; 
			private set; 
		}

		public double PxOverallWidth
		{
			get
			{
				return Info(tnRoot).SubTreeWidth;
			}
		}

		public List<TreeConnection> Connections
		{
			get
			{
				return lsttcn;
			}
		}

		public LayeredTreeDraw(
			ITreeNode tnRoot, double pxBufferHorizontal,
			double pxBufferHorizontalSubtree, double pxBufferVertical,
			VerticalJustification vj)
		{
			this.pxBufferHorizontal = pxBufferHorizontal;
			this.pxBufferHorizontalSubtree = pxBufferHorizontalSubtree;
			this.pxBufferVertical = pxBufferVertical;
			PxOverallHeight = 0.0;
			this.tnRoot = tnRoot;
			this.vj = vj;
		}

		private static LayeredTreeInfo Info(ITreeNode ign)
		{
			return (LayeredTreeInfo)ign.PrivateNodeInfo;
		}

		public double X(ITreeNode tn)
		{
			if (Info(tn) == null)
			{
				return 0;
			}
			return Info(tn).pxFromLeft;
		}

		public double Y(ITreeNode tn)
		{
			if (Info(tn) == null)
			{
				return 0;
			}
			return Info(tn).pxFromTop;
		}

		static public IEnumerable<T> VisibleDescendants<T>(ITreeNode tn)
		{
			foreach (ITreeNode tnCur in tn.TreeChildren)
			{
				if (!tnCur.Collapsed)
				{
					foreach (T item in VisibleDescendants<T>(tnCur))
					{
						yield return item;
					}
				}
				yield return (T)tnCur;
			}
		}


		static public IEnumerable<T> Descendants<T>(ITreeNode tn)
		{
			foreach (ITreeNode tnCur in tn.TreeChildren)
			{
				foreach (T item in Descendants<T>(tnCur))
				{
					yield return item;
				}
				yield return (T)tnCur;
			}
		}

		public void LayoutTree()
		{
			LayoutTree(tnRoot, 0);
			DetermineFinalPositions(tnRoot, 0, 0, Info(tnRoot).pxLeftPosRelativeToBoundingBox);
		}

		private void LayoutTree(ITreeNode tnRoot, int iLayer)
		{
			if (GetChildren(tnRoot).Count == 0)
			{
				LayoutLeafNode(tnRoot);
			}
			else
			{
				LayoutInteriorNode(tnRoot, iLayer);
			}
			UpdateLayerHeight(tnRoot, iLayer);
		}

		private static void LayoutLeafNode(ITreeNode tnRoot)
		{
			double width = tnRoot.TreeWidth;
			LayeredTreeInfo lti = new LayeredTreeInfo(width, tnRoot);
			lti.lstPosLeftBoundaryRelativeToRoot.Add(0);
			lti.lstPosRightBoundaryRelativeToRoot.Add(width);
			tnRoot.PrivateNodeInfo = lti;
		}

		private void LayoutInteriorNode(ITreeNode tnRoot, int iLayer)
		{
			ITreeNode tnLast = null;
			TreeNodeGroup tng = GetChildren(tnRoot);
			ITreeNode itn = tng[0];
			LayeredTreeInfo ltiThis;

			LayoutAllOurChildren(iLayer, tnLast, tng);

			// This width doesn't account for the parent node's width...
			ltiThis = new LayeredTreeInfo(CalculateWidthFromInterChildDistances(tnRoot), tnRoot);
			tnRoot.PrivateNodeInfo = ltiThis;

			// ...so that this centering may place the parent node negatively while the "width" is the width of
			// all the child nodes.
			CenterOverChildren(tnRoot, ltiThis);
			DetermineParentRelativePositionsOfChildren(tnRoot);
			CalculateBoundaryLists(tnRoot);
		}

		private void LayoutAllOurChildren(int iLayer, ITreeNode tnLast, TreeNodeGroup tng)
		{
			List<Double> lstLeftToBB = new List<double>();
			List<int> lstResponsible = new List<int>();
			for (int i = 0; i < tng.Count; i++)
			{
				ITreeNode tn = tng[i];
				LayoutTree(tn, iLayer + 1);
				RepositionSubtree(i, tng, lstLeftToBB, lstResponsible);
				tnLast = tn;
			}
		}

		private static void CenterOverChildren(ITreeNode tnRoot, LayeredTreeInfo ltiThis)
		{
			// We should be centered between  the connection points of our children...
			ITreeNode tnLeftMost = tnRoot.TreeChildren.LeftMost();
			double pxLeftChild = Info(tnLeftMost).pxLeftPosRelativeToBoundingBox + tnLeftMost.TreeWidth / 2;
			ITreeNode tnRightMost = tnRoot.TreeChildren.RightMost();
			double pxRightChild = Info(tnRightMost).pxLeftPosRelativeToBoundingBox + tnRightMost.TreeWidth / 2;
			ltiThis.pxLeftPosRelativeToBoundingBox = (pxLeftChild + pxRightChild - tnRoot.TreeWidth) / 2;

			// If the root node was wider than the subtree, then we'll have a negative position for it.  We need
			// to readjust things so that the left of the root node represents the left of the bounding box and
			// the child distances to the Bounding box need to be adjusted accordingly.
			if (ltiThis.pxLeftPosRelativeToBoundingBox < 0)
			{
				foreach (ITreeNode tnChildCur in tnRoot.TreeChildren)
				{
					Info(tnChildCur).pxLeftPosRelativeToBoundingBox -= ltiThis.pxLeftPosRelativeToBoundingBox;
				}
				ltiThis.pxLeftPosRelativeToBoundingBox = 0;
			}
		}

		private void DetermineParentRelativePositionsOfChildren(ITreeNode tnRoot)
		{
			LayeredTreeInfo ltiRoot = Info(tnRoot);
			foreach (ITreeNode tn in GetChildren(tnRoot))
			{
				LayeredTreeInfo ltiCur = Info(tn);
				ltiCur.pxLeftPosRelativeToParent = ltiCur.pxLeftPosRelativeToBoundingBox - ltiRoot.pxLeftPosRelativeToBoundingBox;
			}
		}

		private double CalculateWidthFromInterChildDistances(ITreeNode tnRoot)
		{
			double pxWidthCur;
			LayeredTreeInfo lti;
			double pxWidth = 0.0;

			lti = Info(tnRoot.TreeChildren.LeftMost());
			pxWidthCur = lti.pxLeftPosRelativeToBoundingBox;

			// If a subtree extends deeper than it's left neighbors then at that lower level it could potentially extend beyond those neighbors
			// on the left.  We have to check for this and make adjustements after the loop if it occurred.
			double pxUndercut = 0.0;

			foreach (ITreeNode tn in tnRoot.TreeChildren)
			{
				lti = Info(tn);
				pxWidthCur += lti.pxToLeftSibling;

				if (lti.pxLeftPosRelativeToBoundingBox > pxWidthCur)
				{
					pxUndercut = Math.Max(pxUndercut, lti.pxLeftPosRelativeToBoundingBox - pxWidthCur);
				}

				// pxWidth might already be wider than the current node's subtree if earlier nodes "undercut" on the
				// right hand side so we have to take the Max here...
				pxWidth = Math.Max(pxWidth, pxWidthCur + lti.SubTreeWidth - lti.pxLeftPosRelativeToBoundingBox);

				// After this next statement, the BoundingBox we're relative to is the one of our parent's subtree rather than
				// our own subtree (with the exception of undercut considerations)
				lti.pxLeftPosRelativeToBoundingBox = pxWidthCur;
			}
			if (pxUndercut > 0.0)
			{
				foreach (ITreeNode tn in tnRoot.TreeChildren)
				{
					Info(tn).pxLeftPosRelativeToBoundingBox += pxUndercut;
				}
				pxWidth += pxUndercut;
			}

			// We are never narrower than our root node's width which we haven't taken into account yet so
			// we do that here.
			return Math.Max(tnRoot.TreeWidth, pxWidth);
		}

		private void CalculateBoundaryLists(ITreeNode tnRoot)
		{
			LayeredTreeInfo lti = Info(tnRoot);
			lti.lstPosLeftBoundaryRelativeToRoot.Add(0.0);
			lti.lstPosRightBoundaryRelativeToRoot.Add(tnRoot.TreeWidth);
			DetermineBoundary(tnRoot.TreeChildren, true /* fLeft */, lti.lstPosLeftBoundaryRelativeToRoot);
			DetermineBoundary(tnRoot.TreeChildren.Reverse(), false /* fLeft */, lti.lstPosRightBoundaryRelativeToRoot);

		}

		private void DetermineBoundary(IEnumerable<ITreeNode> entn, bool fLeft, List<double> lstPos)
		{
			int cLayersDeep = 1;
			List<double> lstPosCur;
			foreach (ITreeNode tnChild in entn)
			{
				LayeredTreeInfo ltiChild = Info(tnChild);

				if (fLeft)
				{
					lstPosCur = ltiChild.lstPosLeftBoundaryRelativeToRoot;
				}
				else
				{
					lstPosCur = ltiChild.lstPosRightBoundaryRelativeToRoot;
				}

				if (lstPosCur.Count >= lstPos.Count)
				{
					using (IEnumerator<double> enPosCur = lstPosCur.GetEnumerator())
					{
						for (int i = 0; i < cLayersDeep - 1; i++)
						{
							enPosCur.MoveNext();
						}

						while (enPosCur.MoveNext())
						{
							lstPos.Add(enPosCur.Current + ltiChild.pxLeftPosRelativeToParent);
							cLayersDeep++;
						}
					}
				}
			}
		}

		private void ApportionSlop(int itn, int itnResponsible, TreeNodeGroup tngSiblings)
		{
			LayeredTreeInfo lti = Info(tngSiblings[itn]);
			ITreeNode tnLeft = tngSiblings[itn - 1];

			double pxSlop = lti.pxToLeftSibling - tnLeft.TreeWidth - pxBufferHorizontal;
			if (pxSlop > 0)
			{
				for (int i = itnResponsible + 1; i < itn; i++)
				{
					Info(tngSiblings[i]).pxToLeftSibling += pxSlop * (i - itnResponsible) / (itn - itnResponsible);
				}
				lti.pxToLeftSibling -= (itn - itnResponsible - 1) * pxSlop / (itn - itnResponsible);
			}
		}

		private void RepositionSubtree(
				int itn, TreeNodeGroup tngSiblings,
				List<double> lstLeftToBB, List<int> lsttnResponsible)
		{
			int itnResponsible;
			ITreeNode tn = tngSiblings[itn];
			LayeredTreeInfo lti = Info(tn);

			if (itn == 0)
			{
				// No shifting but we still have to prepare the initial version of the
				// left hand skeleton list
				foreach (double pxRelativeToRoot in lti.lstPosRightBoundaryRelativeToRoot)
				{
					lstLeftToBB.Add(pxRelativeToRoot + lti.pxLeftPosRelativeToBoundingBox);
					lsttnResponsible.Add(0);
				}
				return;
			}

			ITreeNode tnLeft = tngSiblings[itn - 1];
			LayeredTreeInfo ltiLeft = Info(tnLeft);
			int iLayer;
			double pxHorizontalBuffer = pxBufferHorizontal;

			double pxNewPosFromBB = PxCalculateNewPos(lti, lstLeftToBB, lsttnResponsible, out itnResponsible, out iLayer);
			if (iLayer != 0)
			{
				pxHorizontalBuffer = pxBufferHorizontalSubtree;
			}

			lti.pxToLeftSibling = pxNewPosFromBB - lstLeftToBB.First() + tnLeft.TreeWidth + pxHorizontalBuffer;

			int cLevels = Math.Min(lti.lstPosRightBoundaryRelativeToRoot.Count, lstLeftToBB.Count);
			for (int i = 0; i < cLevels; i++)
			{
				lstLeftToBB[i] = lti.lstPosRightBoundaryRelativeToRoot[i] + pxNewPosFromBB + pxHorizontalBuffer;
				lsttnResponsible[i] = itn;
			}
			for (int i = lstLeftToBB.Count; i < lti.lstPosRightBoundaryRelativeToRoot.Count; i++)
			{
				lstLeftToBB.Add(lti.lstPosRightBoundaryRelativeToRoot[i] + pxNewPosFromBB + pxHorizontalBuffer);
				lsttnResponsible.Add(itn);
			}

			ApportionSlop(itn, itnResponsible, tngSiblings);
		}

		private double PxCalculateNewPos(
				LayeredTreeInfo lti, List<double> lstLeftToBB,
				List<int> lstitnResponsible, out int itnResponsible,
				out int iLayerRet)
		{
			double pxOffsetToBB = lstLeftToBB[0];
			int cLayers = Math.Min(lti.lstPosLeftBoundaryRelativeToRoot.Count, lstLeftToBB.Count);
			double pxRootPosRightmost = 0.0;
			iLayerRet = 0;

			using (IEnumerator<double> enRight = lti.lstPosLeftBoundaryRelativeToRoot.GetEnumerator(),
				enLeft = lstLeftToBB.GetEnumerator())
			using (IEnumerator<int> enResponsible = lstitnResponsible.GetEnumerator())
			{
				itnResponsible = -1;

				enRight.MoveNext();
				enLeft.MoveNext();
				enResponsible.MoveNext();
				for (int iLayer = 0; iLayer < cLayers; iLayer++)
				{
					double pxLeftBorderFromBB = enLeft.Current;
					double pxRightBorderFromRoot = enRight.Current;
					double pxRightRootBasedOnThisLevel;
					int itnResponsibleCur = enResponsible.Current;

					enLeft.MoveNext();
					enRight.MoveNext();
					enResponsible.MoveNext();

					pxRightRootBasedOnThisLevel = pxLeftBorderFromBB - pxRightBorderFromRoot;
					if (pxRightRootBasedOnThisLevel > pxRootPosRightmost)
					{
						iLayerRet = iLayer;
						pxRootPosRightmost = pxRightRootBasedOnThisLevel;
						itnResponsible = itnResponsibleCur;
					}
				}
			}
			return pxRootPosRightmost;
		}

		private void UpdateLayerHeight(ITreeNode tnRoot, int iLayer)
		{
			while (lstLayerHeight.Count <= iLayer)
			{
				lstLayerHeight.Add(0.0);
			}
			lstLayerHeight[iLayer] = Math.Max(tnRoot.TreeHeight, lstLayerHeight[iLayer]);
		}

		private System.Double CalcJustify(double height, double pxRowHeight)
		{
			double dRet = 0.0;

			switch (vj)
			{
				case VerticalJustification.TOP:
					break;

				case VerticalJustification.CENTER:
					dRet = (pxRowHeight - height) / 2;
					break;

				case VerticalJustification.BOTTOM:
					dRet = pxRowHeight - height;
					break;
			}
			return dRet;
		}

		private TreeNodeGroup GetChildren(ITreeNode tn)
		{
			if (tn.Collapsed)
			{
				return tngEmpty;
			}
			return tn.TreeChildren;
		}

		private void DetermineFinalPositions(ITreeNode tn, int iLayer, double pxFromTop, double pxParentFromLeft)
		{
			double pxRowHeight = lstLayerHeight[iLayer];
			LayeredTreeInfo lti = Info(tn);
			double pxBottom;
			Point dptOrigin;

			lti.pxFromTop = pxFromTop + CalcJustify(tn.TreeHeight, pxRowHeight);
			pxBottom = lti.pxFromTop + tn.TreeHeight;
			if (pxBottom > PxOverallHeight)
			{
				PxOverallHeight = pxBottom;
			}
			lti.pxFromLeft = lti.pxLeftPosRelativeToParent + pxParentFromLeft;
			dptOrigin = new Point(lti.pxFromLeft + tn.TreeWidth / 2, lti.pxFromTop + tn.TreeHeight);
			iLayer++;
			foreach (ITreeNode tnCur in GetChildren(tn))
			{
				List<Point> lstcpt = new List<Point>();
				LayeredTreeInfo ltiCur = Info(tnCur);
				lstcpt.Add(dptOrigin);
				DetermineFinalPositions(tnCur, iLayer, pxFromTop + pxRowHeight + pxBufferVertical, lti.pxFromLeft);
				lstcpt.Add(new Point(ltiCur.pxFromLeft + tnCur.TreeWidth / 2, ltiCur.pxFromTop));
				lsttcn.Add(new TreeConnection(tn, tnCur, lstcpt));
			}
		}

		private class LayeredTreeInfo
		{
			public double SubTreeWidth { get; set; }
			public double pxLeftPosRelativeToParent { get; set; }
			public double pxLeftPosRelativeToBoundingBox { get; set; }
			public double pxToLeftSibling { get; set; }
			public double pxFromTop { get; set; }
			public double pxFromLeft { get; set; }
			public ITreeNode ign { get; private set; }
			public List<double> lstPosLeftBoundaryRelativeToRoot = new List<double>();
			public List<double> lstPosRightBoundaryRelativeToRoot = new List<double>();

			/// <summary>
			/// Initializes a new instance of the GraphLayoutInfo class.
			/// </summary>
			public LayeredTreeInfo(double subTreeWidth, ITreeNode tn)
			{
				SubTreeWidth = subTreeWidth;
				pxLeftPosRelativeToParent = 0;
				pxFromTop = 0;
				ign = tn;
			}
		}
	}
}
