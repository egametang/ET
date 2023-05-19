using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Listens to scroll events on the scroll rect that debug items are stored
// and decides whether snap to bottom should be true or not
// 
// Procedure: if, after a user input (drag or scroll), scrollbar is at the bottom, then 
// snap to bottom shall be true, otherwise it shall be false
namespace IngameDebugConsole
{
	public class DebugsOnScrollListener : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
	{
		public ScrollRect debugsScrollRect;
		public DebugLogManager debugLogManager;

		public void OnScroll( PointerEventData data )
		{
			if( IsScrollbarAtBottom() )
				debugLogManager.SetSnapToBottom( true );
			else
				debugLogManager.SetSnapToBottom( false );
		}

		public void OnBeginDrag( PointerEventData data )
		{
			debugLogManager.SetSnapToBottom( false );
		}

		public void OnEndDrag( PointerEventData data )
		{
			if( IsScrollbarAtBottom() )
				debugLogManager.SetSnapToBottom( true );
			else
				debugLogManager.SetSnapToBottom( false );
		}

		public void OnScrollbarDragStart( BaseEventData data )
		{
			debugLogManager.SetSnapToBottom( false );
		}

		public void OnScrollbarDragEnd( BaseEventData data )
		{
			if( IsScrollbarAtBottom() )
				debugLogManager.SetSnapToBottom( true );
			else
				debugLogManager.SetSnapToBottom( false );
		}

		private bool IsScrollbarAtBottom()
		{
			float scrollbarYPos = debugsScrollRect.verticalNormalizedPosition;
			if( scrollbarYPos <= 1E-6f )
				return true;

			return false;
		}
	}
}