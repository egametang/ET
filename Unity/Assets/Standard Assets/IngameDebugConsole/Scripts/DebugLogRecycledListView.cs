using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles the log items in an optimized way such that existing log items are
// recycled within the list instead of creating a new log item at each chance
namespace IngameDebugConsole
{
	public class DebugLogRecycledListView : MonoBehaviour
	{
#pragma warning disable 0649
		// Cached components
		[SerializeField]
		private RectTransform transformComponent;
		[SerializeField]
		private RectTransform viewportTransform;

		[SerializeField]
		private DebugLogManager debugManager;

		[SerializeField]
		private Color logItemNormalColor1;
		[SerializeField]
		private Color logItemNormalColor2;
		[SerializeField]
		private Color logItemSelectedColor;
#pragma warning restore 0649

		private DebugLogManager manager;
		private ScrollRect scrollView;

		private float logItemHeight, _1OverLogItemHeight;
		private float viewportHeight;

		// Unique debug entries
		private List<DebugLogEntry> collapsedLogEntries = null;

		// Indices of debug entries to show in collapsedLogEntries
		private DebugLogIndexList indicesOfEntriesToShow = null;

		private int indexOfSelectedLogEntry = int.MaxValue;
		private float positionOfSelectedLogEntry = float.MaxValue;
		private float heightOfSelectedLogEntry;
		private float deltaHeightOfSelectedLogEntry;

		// Log items used to visualize the debug entries at specified indices
		private Dictionary<int, DebugLogItem> logItemsAtIndices = new Dictionary<int, DebugLogItem>();

		private bool isCollapseOn = false;

		// Current indices of debug entries shown on screen
		private int currentTopIndex = -1, currentBottomIndex = -1;

		public float ItemHeight { get { return logItemHeight; } }
		public float SelectedItemHeight { get { return heightOfSelectedLogEntry; } }

		private void Awake()
		{
			scrollView = viewportTransform.GetComponentInParent<ScrollRect>();
			scrollView.onValueChanged.AddListener( ( pos ) => UpdateItemsInTheList( false ) );

			viewportHeight = viewportTransform.rect.height;
		}

		public void Initialize( DebugLogManager manager, List<DebugLogEntry> collapsedLogEntries, DebugLogIndexList indicesOfEntriesToShow, float logItemHeight )
		{
			this.manager = manager;
			this.collapsedLogEntries = collapsedLogEntries;
			this.indicesOfEntriesToShow = indicesOfEntriesToShow;
			this.logItemHeight = logItemHeight;
			_1OverLogItemHeight = 1f / logItemHeight;
		}

		public void SetCollapseMode( bool collapse )
		{
			isCollapseOn = collapse;
		}

		// A log item is clicked, highlight it
		public void OnLogItemClicked( DebugLogItem item )
		{
			OnLogItemClickedInternal( item.Index, item );
		}

		// Force expand the log item at specified index
		public void SelectAndFocusOnLogItemAtIndex( int itemIndex )
		{
			if( indexOfSelectedLogEntry != itemIndex ) // Make sure that we aren't deselecting the target log item
				OnLogItemClickedInternal( itemIndex );

			float transformComponentCenterYAtTop = viewportHeight * 0.5f;
			float transformComponentCenterYAtBottom = transformComponent.sizeDelta.y - viewportHeight * 0.5f;
			float transformComponentTargetCenterY = itemIndex * logItemHeight + viewportHeight * 0.5f;
			if( transformComponentCenterYAtTop == transformComponentCenterYAtBottom )
				scrollView.verticalNormalizedPosition = 0.5f;
			else
				scrollView.verticalNormalizedPosition = Mathf.Clamp01( Mathf.InverseLerp( transformComponentCenterYAtBottom, transformComponentCenterYAtTop, transformComponentTargetCenterY ) );

			manager.SetSnapToBottom( false );
		}

		private void OnLogItemClickedInternal( int itemIndex, DebugLogItem referenceItem = null )
		{
			if( indexOfSelectedLogEntry != itemIndex )
			{
				DeselectSelectedLogItem();

				if( !referenceItem )
				{
					if( currentTopIndex == -1 )
						UpdateItemsInTheList( false ); // Try to generate some DebugLogItems, we need one DebugLogItem to calculate the text height

					referenceItem = logItemsAtIndices[currentTopIndex];
				}

				indexOfSelectedLogEntry = itemIndex;
				positionOfSelectedLogEntry = itemIndex * logItemHeight;
				heightOfSelectedLogEntry = referenceItem.CalculateExpandedHeight( collapsedLogEntries[indicesOfEntriesToShow[itemIndex]].ToString() );
				deltaHeightOfSelectedLogEntry = heightOfSelectedLogEntry - logItemHeight;

				manager.SetSnapToBottom( false );
			}
			else
				DeselectSelectedLogItem();

			if( indexOfSelectedLogEntry >= currentTopIndex && indexOfSelectedLogEntry <= currentBottomIndex )
				ColorLogItem( logItemsAtIndices[indexOfSelectedLogEntry], indexOfSelectedLogEntry );

			CalculateContentHeight();

			HardResetItems();
			UpdateItemsInTheList( true );

			manager.ValidateScrollPosition();
		}

		// Deselect the currently selected log item
		public void DeselectSelectedLogItem()
		{
			int indexOfPreviouslySelectedLogEntry = indexOfSelectedLogEntry;
			indexOfSelectedLogEntry = int.MaxValue;

			positionOfSelectedLogEntry = float.MaxValue;
			heightOfSelectedLogEntry = deltaHeightOfSelectedLogEntry = 0f;

			if( indexOfPreviouslySelectedLogEntry >= currentTopIndex && indexOfPreviouslySelectedLogEntry <= currentBottomIndex )
				ColorLogItem( logItemsAtIndices[indexOfPreviouslySelectedLogEntry], indexOfPreviouslySelectedLogEntry );
		}

		// Number of debug entries may be changed, update the list
		public void OnLogEntriesUpdated( bool updateAllVisibleItemContents )
		{
			CalculateContentHeight();
			viewportHeight = viewportTransform.rect.height;

			if( updateAllVisibleItemContents )
				HardResetItems();

			UpdateItemsInTheList( updateAllVisibleItemContents );
		}

		// A single collapsed log entry at specified index is updated, refresh its item if visible
		public void OnCollapsedLogEntryAtIndexUpdated( int index )
		{
			DebugLogItem logItem;
			if( logItemsAtIndices.TryGetValue( index, out logItem ) )
				logItem.ShowCount();
		}

		// Log window is resized, update the list
		public void OnViewportDimensionsChanged()
		{
			viewportHeight = viewportTransform.rect.height;
			UpdateItemsInTheList( false );
		}

		private void HardResetItems()
		{
			if( currentTopIndex != -1 )
			{
				DestroyLogItemsBetweenIndices( currentTopIndex, currentBottomIndex );
				currentTopIndex = -1;
			}
		}

		private void CalculateContentHeight()
		{
			float newHeight = Mathf.Max( 1f, indicesOfEntriesToShow.Count * logItemHeight + deltaHeightOfSelectedLogEntry );
			transformComponent.sizeDelta = new Vector2( 0f, newHeight );
		}

		// Calculate the indices of log entries to show
		// and handle log items accordingly
		public void UpdateItemsInTheList( bool updateAllVisibleItemContents )
		{
			// If there is at least one log entry to show
			if( indicesOfEntriesToShow.Count > 0 )
			{
				float contentPosTop = transformComponent.anchoredPosition.y - 1f;
				float contentPosBottom = contentPosTop + viewportHeight + 2f;

				if( positionOfSelectedLogEntry <= contentPosBottom )
				{
					if( positionOfSelectedLogEntry <= contentPosTop )
					{
						contentPosTop -= deltaHeightOfSelectedLogEntry;
						contentPosBottom -= deltaHeightOfSelectedLogEntry;

						if( contentPosTop < positionOfSelectedLogEntry - 1f )
							contentPosTop = positionOfSelectedLogEntry - 1f;

						if( contentPosBottom < contentPosTop + 2f )
							contentPosBottom = contentPosTop + 2f;
					}
					else
					{
						contentPosBottom -= deltaHeightOfSelectedLogEntry;
						if( contentPosBottom < positionOfSelectedLogEntry + 1f )
							contentPosBottom = positionOfSelectedLogEntry + 1f;
					}
				}

				int newTopIndex = (int) ( contentPosTop * _1OverLogItemHeight );
				int newBottomIndex = (int) ( contentPosBottom * _1OverLogItemHeight );

				if( newTopIndex < 0 )
					newTopIndex = 0;

				if( newBottomIndex > indicesOfEntriesToShow.Count - 1 )
					newBottomIndex = indicesOfEntriesToShow.Count - 1;

				if( currentTopIndex == -1 )
				{
					// There are no log items visible on screen,
					// just create the new log items
					updateAllVisibleItemContents = true;

					currentTopIndex = newTopIndex;
					currentBottomIndex = newBottomIndex;

					CreateLogItemsBetweenIndices( newTopIndex, newBottomIndex );
				}
				else
				{
					// There are some log items visible on screen

					if( newBottomIndex < currentTopIndex || newTopIndex > currentBottomIndex )
					{
						// If user scrolled a lot such that, none of the log items are now within
						// the bounds of the scroll view, pool all the previous log items and create
						// new log items for the new list of visible debug entries
						updateAllVisibleItemContents = true;

						DestroyLogItemsBetweenIndices( currentTopIndex, currentBottomIndex );
						CreateLogItemsBetweenIndices( newTopIndex, newBottomIndex );
					}
					else
					{
						// User did not scroll a lot such that, there are still some log items within
						// the bounds of the scroll view. Don't destroy them but update their content,
						// if necessary
						if( newTopIndex > currentTopIndex )
							DestroyLogItemsBetweenIndices( currentTopIndex, newTopIndex - 1 );

						if( newBottomIndex < currentBottomIndex )
							DestroyLogItemsBetweenIndices( newBottomIndex + 1, currentBottomIndex );

						if( newTopIndex < currentTopIndex )
						{
							CreateLogItemsBetweenIndices( newTopIndex, currentTopIndex - 1 );

							// If it is not necessary to update all the log items,
							// then just update the newly created log items. Otherwise,
							// wait for the major update
							if( !updateAllVisibleItemContents )
								UpdateLogItemContentsBetweenIndices( newTopIndex, currentTopIndex - 1 );
						}

						if( newBottomIndex > currentBottomIndex )
						{
							CreateLogItemsBetweenIndices( currentBottomIndex + 1, newBottomIndex );

							// If it is not necessary to update all the log items,
							// then just update the newly created log items. Otherwise,
							// wait for the major update
							if( !updateAllVisibleItemContents )
								UpdateLogItemContentsBetweenIndices( currentBottomIndex + 1, newBottomIndex );
						}
					}

					currentTopIndex = newTopIndex;
					currentBottomIndex = newBottomIndex;
				}

				if( updateAllVisibleItemContents )
				{
					// Update all the log items
					UpdateLogItemContentsBetweenIndices( currentTopIndex, currentBottomIndex );
				}
			}
			else
				HardResetItems();
		}

		private void CreateLogItemsBetweenIndices( int topIndex, int bottomIndex )
		{
			for( int i = topIndex; i <= bottomIndex; i++ )
				CreateLogItemAtIndex( i );
		}

		// Create (or unpool) a log item
		private void CreateLogItemAtIndex( int index )
		{
			DebugLogItem logItem = debugManager.PopLogItem();

			// Reposition the log item
			Vector2 anchoredPosition = new Vector2( 1f, -index * logItemHeight );
			if( index > indexOfSelectedLogEntry )
				anchoredPosition.y -= deltaHeightOfSelectedLogEntry;

			logItem.Transform.anchoredPosition = anchoredPosition;

			// Color the log item
			ColorLogItem( logItem, index );

			// To access this log item easily in the future, add it to the dictionary
			logItemsAtIndices[index] = logItem;
		}

		private void DestroyLogItemsBetweenIndices( int topIndex, int bottomIndex )
		{
			for( int i = topIndex; i <= bottomIndex; i++ )
				debugManager.PoolLogItem( logItemsAtIndices[i] );
		}

		private void UpdateLogItemContentsBetweenIndices( int topIndex, int bottomIndex )
		{
			DebugLogItem logItem;
			for( int i = topIndex; i <= bottomIndex; i++ )
			{
				logItem = logItemsAtIndices[i];
				logItem.SetContent( collapsedLogEntries[indicesOfEntriesToShow[i]], i, i == indexOfSelectedLogEntry );

				if( isCollapseOn )
					logItem.ShowCount();
				else
					logItem.HideCount();
			}
		}

		// Color a log item using its index
		private void ColorLogItem( DebugLogItem logItem, int index )
		{
			if( index == indexOfSelectedLogEntry )
				logItem.Image.color = logItemSelectedColor;
			else if( index % 2 == 0 )
				logItem.Image.color = logItemNormalColor1;
			else
				logItem.Image.color = logItemNormalColor2;
		}
	}
}