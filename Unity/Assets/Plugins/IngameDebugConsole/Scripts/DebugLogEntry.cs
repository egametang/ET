using UnityEngine;

// Container for a simple debug entry
namespace IngameDebugConsole
{
	public class DebugLogEntry : System.IEquatable<DebugLogEntry>
	{
		private const int HASH_NOT_CALCULATED = -623218;

		public string logString;
		public string stackTrace;

		private string completeLog;

		// Sprite to show with this entry
		public Sprite logTypeSpriteRepresentation;

		// Collapsed count
		public int count;

		private int hashValue;

		public void Initialize( string logString, string stackTrace )
		{
			this.logString = logString;
			this.stackTrace = stackTrace;

			completeLog = null;
			count = 1;
			hashValue = HASH_NOT_CALCULATED;
		}

		// Check if two entries have the same origin
		public bool Equals( DebugLogEntry other )
		{
			return this.logString == other.logString && this.stackTrace == other.stackTrace;
		}

		// Checks if logString or stackTrace contains the search term
		public bool MatchesSearchTerm( string searchTerm )
		{
			return ( logString != null && logString.IndexOf( searchTerm, System.StringComparison.OrdinalIgnoreCase ) >= 0 ) ||
				( stackTrace != null && stackTrace.IndexOf( searchTerm, System.StringComparison.OrdinalIgnoreCase ) >= 0 );
		}

		// Return a string containing complete information about this debug entry
		public override string ToString()
		{
			if( completeLog == null )
				completeLog = string.Concat( logString, "\n", stackTrace );

			return completeLog;
		}

		// Credit: https://stackoverflow.com/a/19250516/2373034
		public override int GetHashCode()
		{
			if( hashValue == HASH_NOT_CALCULATED )
			{
				unchecked
				{
					hashValue = 17;
					hashValue = hashValue * 23 + logString == null ? 0 : logString.GetHashCode();
					hashValue = hashValue * 23 + stackTrace == null ? 0 : stackTrace.GetHashCode();
				}
			}

			return hashValue;
		}
	}

	public struct QueuedDebugLogEntry
	{
		public readonly string logString;
		public readonly string stackTrace;
		public readonly LogType logType;

		public QueuedDebugLogEntry( string logString, string stackTrace, LogType logType )
		{
			this.logString = logString;
			this.stackTrace = stackTrace;
			this.logType = logType;
		}

		// Checks if logString or stackTrace contains the search term
		public bool MatchesSearchTerm( string searchTerm )
		{
			return ( logString != null && logString.IndexOf( searchTerm, System.StringComparison.OrdinalIgnoreCase ) >= 0 ) ||
				( stackTrace != null && stackTrace.IndexOf( searchTerm, System.StringComparison.OrdinalIgnoreCase ) >= 0 );
		}
	}
}