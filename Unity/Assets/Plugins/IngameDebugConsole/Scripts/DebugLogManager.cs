using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Receives debug entries and custom events (e.g. Clear, Collapse, Filter by Type)
// and notifies the recycled list view of changes to the list of debug entries
// 
// - Vocabulary -
// Debug/Log entry: a Debug.Log/LogError/LogWarning/LogException/LogAssertion request made by
//                   the client and intercepted by this manager object
// Debug/Log item: a visual (uGUI) representation of a debug entry
// 
// There can be a lot of debug entries in the system but there will only be a handful of log items 
// to show their properties on screen (these log items are recycled as the list is scrolled)

// An enum to represent filtered log types
namespace IngameDebugConsole
{
	public enum DebugLogFilter
	{
		None = 0,
		Info = 1,
		Warning = 2,
		Error = 4,
		All = 7
	}

	public class DebugLogManager : MonoBehaviour
	{
		public static DebugLogManager Instance { get; private set; }

#pragma warning disable 0649
		[Header( "Properties" )]
		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, console window will persist between scenes (i.e. not be destroyed when scene changes)" )]
		private bool singleton = true;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "Minimum height of the console window" )]
		private float minimumHeight = 200f;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If disabled, no popup will be shown when the console window is hidden" )]
		private bool enablePopup = true;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, console will be initialized as a popup" )]
		private bool startInPopupMode = true;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, console window will initially be invisible" )]
		private bool startMinimized = false;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, pressing the Toggle Key will show/hide (i.e. toggle) the console window at runtime" )]
		private bool toggleWithKey = false;

		[SerializeField]
		[HideInInspector]
		private KeyCode toggleKey = KeyCode.BackQuote;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, the console window will have a searchbar" )]
		private bool enableSearchbar = true;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "Width of the canvas determines whether the searchbar will be located inside the menu bar or underneath the menu bar. This way, the menu bar doesn't get too crowded on narrow screens. This value determines the minimum width of the canvas for the searchbar to appear inside the menu bar" )]
		private float topSearchbarMinWidth = 360f;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, the command input field at the bottom of the console window will automatically be cleared after entering a command" )]
		private bool clearCommandAfterExecution = true;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "Console keeps track of the previously entered commands. This value determines the capacity of the command history (you can scroll through the history via up and down arrow keys while the command input field is focused)" )]
		private int commandHistorySize = 15;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, while typing a command, all of the matching commands' signatures will be displayed in a popup" )]
		private bool showCommandSuggestions = true;

		[SerializeField]
		[HideInInspector]
		[Tooltip( "If enabled, on Android platform, logcat entries of the application will also be logged to the console with the prefix \"LOGCAT: \". This may come in handy especially if you want to access the native logs of your Android plugins (like Admob)" )]
		private bool receiveLogcatLogsInAndroid = false;

#pragma warning disable 0414
		[SerializeField]
		[HideInInspector]
		[Tooltip( "Native logs will be filtered using these arguments. If left blank, all native logs of the application will be logged to the console. But if you want to e.g. see Admob's logs only, you can enter \"-s Ads\" (without quotes) here" )]
		private string logcatArguments;
#pragma warning restore 0414

		[SerializeField]
		[Tooltip( "If enabled, on Android and iOS devices with notch screens, the console window will be repositioned so that the cutout(s) don't obscure it" )]
		private bool avoidScreenCutout = true;

		[SerializeField]
		[Tooltip( "If a log is longer than this limit, it will be truncated. This helps avoid reaching Unity's 65000 vertex limit for UI canvases" )]
		private int maxLogLength = 10000;

		[SerializeField]
		[Tooltip( "If enabled, on standalone platforms, command input field will automatically be focused (start receiving keyboard input) after opening the console window" )]
		private bool autoFocusOnCommandInputField = true;

		[Header( "Visuals" )]
		[SerializeField]
		private DebugLogItem logItemPrefab;

		[SerializeField]
		private Text commandSuggestionPrefab;

		// Visuals for different log types
		[SerializeField]
		private Sprite infoLog;
		[SerializeField]
		private Sprite warningLog;
		[SerializeField]
		private Sprite errorLog;

		private Dictionary<LogType, Sprite> logSpriteRepresentations;

		[SerializeField]
		private Color collapseButtonNormalColor;
		[SerializeField]
		private Color collapseButtonSelectedColor;

		[SerializeField]
		private Color filterButtonsNormalColor;
		[SerializeField]
		private Color filterButtonsSelectedColor;

		[SerializeField]
		private string commandSuggestionHighlightStart = "<color=orange>";
		[SerializeField]
		private string commandSuggestionHighlightEnd = "</color>";

		[Header( "Internal References" )]
		[SerializeField]
		private RectTransform logWindowTR;

		private RectTransform canvasTR;

		[SerializeField]
		private RectTransform logItemsContainer;

		[SerializeField]
		private RectTransform commandSuggestionsContainer;

		[SerializeField]
		private InputField commandInputField;

		[SerializeField]
		private Button hideButton;

		[SerializeField]
		private Button clearButton;

		[SerializeField]
		private Image collapseButton;

		[SerializeField]
		private Image filterInfoButton;
		[SerializeField]
		private Image filterWarningButton;
		[SerializeField]
		private Image filterErrorButton;

		[SerializeField]
		private Text infoEntryCountText;
		[SerializeField]
		private Text warningEntryCountText;
		[SerializeField]
		private Text errorEntryCountText;

		[SerializeField]
		private RectTransform searchbar;
		[SerializeField]
		private RectTransform searchbarSlotTop;
		[SerializeField]
		private RectTransform searchbarSlotBottom;

		[SerializeField]
		private GameObject snapToBottomButton;

		// Canvas group to modify visibility of the log window
		[SerializeField]
		private CanvasGroup logWindowCanvasGroup;

		[SerializeField]
		private DebugLogPopup popupManager;

		[SerializeField]
		private ScrollRect logItemsScrollRect;
		private RectTransform logItemsScrollRectTR;
		private Vector2 logItemsScrollRectOriginalSize;

		// Recycled list view to handle the log items efficiently
		[SerializeField]
		private DebugLogRecycledListView recycledListView;
#pragma warning restore 0649

		private bool isLogWindowVisible = true;
		public bool IsLogWindowVisible { get { return isLogWindowVisible; } }

		public bool PopupEnabled
		{
			get { return popupManager.gameObject.activeSelf; }
			set { popupManager.gameObject.SetActive( value ); }
		}

		private bool screenDimensionsChanged = true;

		// Number of entries filtered by their types
		private int infoEntryCount = 0, warningEntryCount = 0, errorEntryCount = 0;

		// Filters to apply to the list of debug entries to show
		private bool isCollapseOn = false;
		private DebugLogFilter logFilter = DebugLogFilter.All;

		// Search filter
		private string searchTerm;
		private bool isInSearchMode;

		// If the last log item is completely visible (scrollbar is at the bottom),
		// scrollbar will remain at the bottom when new debug entries are received
		private bool snapToBottom = true;

		// List of unique debug entries (duplicates of entries are not kept)
		private List<DebugLogEntry> collapsedLogEntries;

		// Dictionary to quickly find if a log already exists in collapsedLogEntries
		private Dictionary<DebugLogEntry, int> collapsedLogEntriesMap;

		// The order the collapsedLogEntries are received 
		// (duplicate entries have the same index (value))
		private DebugLogIndexList uncollapsedLogEntriesIndices;

		// Filtered list of debug entries to show
		private DebugLogIndexList indicesOfListEntriesToShow;

		// Logs that should be registered in Update-loop
		private DynamicCircularBuffer<QueuedDebugLogEntry> queuedLogEntries;
		private object logEntriesLock;
		private int pendingLogToAutoExpand;

		// Command suggestions that match the currently entered command
		private List<Text> commandSuggestionInstances;
		private int visibleCommandSuggestionInstances = 0;
		private List<ConsoleMethodInfo> matchingCommandSuggestions;
		private List<int> commandCaretIndexIncrements;
		private StringBuilder commandSuggestionsStringBuilder;
		private string commandInputFieldPrevCommand;
		private string commandInputFieldPrevCommandName;
		private int commandInputFieldPrevParamCount = -1;
		private int commandInputFieldPrevCaretPos = -1;
		private int commandInputFieldPrevCaretArgumentIndex = -1;

		// Pools for memory efficiency
		private List<DebugLogEntry> pooledLogEntries;
		private List<DebugLogItem> pooledLogItems;

		// History of the previously entered commands
		private CircularBuffer<string> commandHistory;
		private int commandHistoryIndex = -1;

		// Required in ValidateScrollPosition() function
		private PointerEventData nullPointerEventData;

#if UNITY_EDITOR
		private bool isQuittingApplication;
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
		private DebugLogLogcatListener logcatListener;
#endif

		#region 显示FPS
		
		private float _fps;
		private Color _fpsColor = Color.white;
		private int _frameNumber;
		private float _lastShowFPSTime;
		
		#endregion
		
		private void Awake()
		{
			// Only one instance of debug console is allowed
			if( !Instance )
			{
				Instance = this;

				// If it is a singleton object, don't destroy it between scene changes
				if( singleton )
					DontDestroyOnLoad( gameObject );
			}
			else if( Instance != this )
			{
				Destroy( gameObject );
				return;
			}

			pooledLogEntries = new List<DebugLogEntry>( 16 );
			pooledLogItems = new List<DebugLogItem>( 16 );
			commandSuggestionInstances = new List<Text>( 8 );
			matchingCommandSuggestions = new List<ConsoleMethodInfo>( 8 );
			commandCaretIndexIncrements = new List<int>( 8 );
			queuedLogEntries = new DynamicCircularBuffer<QueuedDebugLogEntry>( 16 );
			commandHistory = new CircularBuffer<string>( commandHistorySize );

			logEntriesLock = new object();
			commandSuggestionsStringBuilder = new StringBuilder( 128 );

			canvasTR = (RectTransform) transform;
			logItemsScrollRectTR = (RectTransform) logItemsScrollRect.transform;
			logItemsScrollRectOriginalSize = logItemsScrollRectTR.sizeDelta;

			// Associate sprites with log types
			logSpriteRepresentations = new Dictionary<LogType, Sprite>()
			{
				{ LogType.Log, infoLog },
				{ LogType.Warning, warningLog },
				{ LogType.Error, errorLog },
				{ LogType.Exception, errorLog },
				{ LogType.Assert, errorLog }
			};

			// Initially, all log types are visible
			filterInfoButton.color = filterButtonsSelectedColor;
			filterWarningButton.color = filterButtonsSelectedColor;
			filterErrorButton.color = filterButtonsSelectedColor;

			collapsedLogEntries = new List<DebugLogEntry>( 128 );
			collapsedLogEntriesMap = new Dictionary<DebugLogEntry, int>( 128 );
			uncollapsedLogEntriesIndices = new DebugLogIndexList();
			indicesOfListEntriesToShow = new DebugLogIndexList();

			recycledListView.Initialize( this, collapsedLogEntries, indicesOfListEntriesToShow, logItemPrefab.Transform.sizeDelta.y );
			recycledListView.UpdateItemsInTheList( true );

			if( minimumHeight < 200f )
				minimumHeight = 200f;

			if( !enableSearchbar )
			{
				searchbar = null;
				searchbarSlotTop.gameObject.SetActive( false );
				searchbarSlotBottom.gameObject.SetActive( false );
			}

			if( commandSuggestionsContainer.gameObject.activeSelf )
				commandSuggestionsContainer.gameObject.SetActive( false );

			// Register to UI events
			commandInputField.onValidateInput += OnValidateCommand;
			commandInputField.onValueChanged.AddListener( RefreshCommandSuggestions );
			commandInputField.onEndEdit.AddListener( OnEndEditCommand );
			searchbar.GetComponent<InputField>().onValueChanged.AddListener( SearchTermChanged );
			hideButton.onClick.AddListener( HideLogWindow );
			clearButton.onClick.AddListener( ClearLogs );
			collapseButton.GetComponent<Button>().onClick.AddListener( CollapseButtonPressed );
			filterInfoButton.GetComponent<Button>().onClick.AddListener( FilterLogButtonPressed );
			filterWarningButton.GetComponent<Button>().onClick.AddListener( FilterWarningButtonPressed );
			filterErrorButton.GetComponent<Button>().onClick.AddListener( FilterErrorButtonPressed );
			snapToBottomButton.GetComponent<Button>().onClick.AddListener( () => SetSnapToBottom( true ) );

			nullPointerEventData = new PointerEventData( null );
		}

		private void OnEnable()
		{
			if( Instance != this )
				return;

			// Intercept debug entries
			Application.logMessageReceivedThreaded -= ReceivedLog;
			Application.logMessageReceivedThreaded += ReceivedLog;

			if( receiveLogcatLogsInAndroid )
			{
#if !UNITY_EDITOR && UNITY_ANDROID
				if( logcatListener == null )
					logcatListener = new DebugLogLogcatListener();

				logcatListener.Start( logcatArguments );
#endif
			}

			DebugLogConsole.AddCommand( "save_logs", "Saves logs to a file", SaveLogsToFile );

			//Debug.LogAssertion( "assert" );
			//Debug.LogError( "error" );
			//Debug.LogException( new System.IO.EndOfStreamException() );
			//Debug.LogWarning( "warning" );
			//Debug.Log( "log" );
		}

		private void OnDisable()
		{
			if( Instance != this )
				return;

			// Stop receiving debug entries
			Application.logMessageReceivedThreaded -= ReceivedLog;

#if !UNITY_EDITOR && UNITY_ANDROID
			if( logcatListener != null )
				logcatListener.Stop();
#endif

			DebugLogConsole.RemoveCommand( "save_logs" );
		}

		private void Start()
		{
			if( ( enablePopup && startInPopupMode ) || ( !enablePopup && startMinimized ) )
				HideLogWindow();
			else
				ShowLogWindow();

			PopupEnabled = enablePopup;
		}

#if UNITY_EDITOR
		private void OnApplicationQuit()
		{
			isQuittingApplication = true;
		}
#endif

		// Window is resized, update the list
		private void OnRectTransformDimensionsChange()
		{
			screenDimensionsChanged = true;
		}

		private void Update()
		{
			// Toggling the console with toggleKey is handled in Update instead of LateUpdate because
			// when we hide the console, we don't want the commandInputField to capture the toggleKey.
			// InputField captures input in LateUpdate so deactivating it in Update ensures that
			// no further input is captured
			if( toggleWithKey )
			{
				if( Input.GetKeyDown( toggleKey ) )
				{
					if( isLogWindowVisible )
						HideLogWindow();
					else
						ShowLogWindow();
				}
			}
			
			this._frameNumber += 1;

			float time = Time.realtimeSinceStartup - this._lastShowFPSTime;

			if (!(time >= 1)) return;

			this._fps = (this._frameNumber / time);
			this._frameNumber = 0;
			this._lastShowFPSTime = Time.realtimeSinceStartup;
			this.popupManager.NewFPSCountArrived(this._fps);
		}

		private void LateUpdate()
		{
#if UNITY_EDITOR
			if( isQuittingApplication )
				return;
#endif

			int queuedLogCount = queuedLogEntries.Count;
			if( queuedLogCount > 0 )
			{
				for( int i = 0; i < queuedLogCount; i++ )
				{
					QueuedDebugLogEntry logEntry;
					lock( logEntriesLock )
					{
						logEntry = queuedLogEntries.RemoveFirst();
					}

					ProcessLog( logEntry );
				}
			}

			if( showCommandSuggestions && commandInputField.isFocused && commandInputField.caretPosition != commandInputFieldPrevCaretPos )
				RefreshCommandSuggestions( commandInputField.text );

			if( screenDimensionsChanged )
			{
				// Update the recycled list view
				if( isLogWindowVisible )
					recycledListView.OnViewportDimensionsChanged();
				else
					popupManager.OnViewportDimensionsChanged();

#if UNITY_ANDROID || UNITY_IOS
				CheckScreenCutout();
#endif

				if( searchbar )
				{
					float logWindowWidth = logWindowTR.rect.width;
					if( logWindowWidth >= topSearchbarMinWidth )
					{
						if( searchbar.parent == searchbarSlotBottom )
						{
							searchbarSlotTop.gameObject.SetActive( true );
							searchbar.SetParent( searchbarSlotTop, false );
							searchbarSlotBottom.gameObject.SetActive( false );

							logItemsScrollRectTR.anchoredPosition = Vector2.zero;
							logItemsScrollRectTR.sizeDelta = logItemsScrollRectOriginalSize;
						}
					}
					else
					{
						if( searchbar.parent == searchbarSlotTop )
						{
							searchbarSlotBottom.gameObject.SetActive( true );
							searchbar.SetParent( searchbarSlotBottom, false );
							searchbarSlotTop.gameObject.SetActive( false );

							float searchbarHeight = searchbarSlotBottom.sizeDelta.y;
							logItemsScrollRectTR.anchoredPosition = new Vector2( 0f, searchbarHeight * -0.5f );
							logItemsScrollRectTR.sizeDelta = logItemsScrollRectOriginalSize - new Vector2( 0f, searchbarHeight );
						}
					}
				}

				screenDimensionsChanged = false;
			}

			// If snapToBottom is enabled, force the scrollbar to the bottom
			if( snapToBottom )
			{
				logItemsScrollRect.verticalNormalizedPosition = 0f;

				if( snapToBottomButton.activeSelf )
					snapToBottomButton.SetActive( false );
			}
			else
			{
				float scrollPos = logItemsScrollRect.verticalNormalizedPosition;
				if( snapToBottomButton.activeSelf != ( scrollPos > 1E-6f && scrollPos < 0.9999f ) )
					snapToBottomButton.SetActive( !snapToBottomButton.activeSelf );
			}

			if( isLogWindowVisible && commandInputField.isFocused )
			{
				if( Input.GetKeyDown( KeyCode.UpArrow ) )
				{
					if( commandHistoryIndex == -1 )
						commandHistoryIndex = commandHistory.Count - 1;
					else if( --commandHistoryIndex < 0 )
						commandHistoryIndex = 0;

					if( commandHistoryIndex >= 0 && commandHistoryIndex < commandHistory.Count )
					{
						commandInputField.text = commandHistory[commandHistoryIndex];
						commandInputField.caretPosition = commandInputField.text.Length;
					}
				}
				else if( Input.GetKeyDown( KeyCode.DownArrow ) )
				{
					if( commandHistoryIndex == -1 )
						commandHistoryIndex = commandHistory.Count - 1;
					else if( ++commandHistoryIndex >= commandHistory.Count )
						commandHistoryIndex = commandHistory.Count - 1;

					if( commandHistoryIndex >= 0 && commandHistoryIndex < commandHistory.Count )
						commandInputField.text = commandHistory[commandHistoryIndex];
				}
			}

#if !UNITY_EDITOR && UNITY_ANDROID
			if( logcatListener != null )
			{
				string log;
				while( ( log = logcatListener.GetLog() ) != null )
					ReceivedLog( "LOGCAT: " + log, string.Empty, LogType.Log );
			}
#endif
		}

		public void ShowLogWindow()
		{
			// Show the log window
			logWindowCanvasGroup.interactable = true;
			logWindowCanvasGroup.blocksRaycasts = true;
			logWindowCanvasGroup.alpha = 1f;

			popupManager.Hide();

			// Update the recycled list view 
			// (in case new entries were intercepted while log window was hidden)
			recycledListView.OnLogEntriesUpdated( true );

#if UNITY_EDITOR || UNITY_STANDALONE
			// Focus on the command input field on standalone platforms when the console is opened
			if( autoFocusOnCommandInputField )
				StartCoroutine( ActivateCommandInputFieldCoroutine() );
#endif

			isLogWindowVisible = true;
		}

		public void HideLogWindow()
		{
			// Hide the log window
			logWindowCanvasGroup.interactable = false;
			logWindowCanvasGroup.blocksRaycasts = false;
			logWindowCanvasGroup.alpha = 0f;

			if( commandInputField.isFocused )
				commandInputField.DeactivateInputField();

			popupManager.Show();

			commandHistoryIndex = -1;
			isLogWindowVisible = false;
		}

		// Command field input is changed, check if command is submitted
		private char OnValidateCommand( string text, int charIndex, char addedChar )
		{
			if( addedChar == '\t' ) // Autocomplete attempt
			{
				if( !string.IsNullOrEmpty( text ) )
				{
					string autoCompletedCommand = DebugLogConsole.GetAutoCompleteCommand( text );
					if( !string.IsNullOrEmpty( autoCompletedCommand ) )
						commandInputField.text = autoCompletedCommand;
				}

				return '\0';
			}
			else if( addedChar == '\n' ) // Command is submitted
			{
				// Clear the command field
				if( clearCommandAfterExecution )
					commandInputField.text = "";

				if( text.Length > 0 )
				{
					if( commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != text )
						commandHistory.Add( text );

					commandHistoryIndex = -1;

					// Execute the command
					DebugLogConsole.ExecuteCommand( text );

					// Snap to bottom and select the latest entry
					SetSnapToBottom( true );
				}

				return '\0';
			}

			return addedChar;
		}

		// A debug entry is received
		private void ReceivedLog( string logString, string stackTrace, LogType logType )
		{
#if UNITY_EDITOR
			if( isQuittingApplication )
				return;
#endif

			// Truncate the log if it is longer than maxLogLength
			int logLength = logString.Length;
			if( stackTrace == null )
			{
				if( logLength > maxLogLength )
					logString = logString.Substring( 0, maxLogLength - 11 ) + "<truncated>";
			}
			else
			{
				logLength += stackTrace.Length;
				if( logLength > maxLogLength )
				{
					// Decide which log component(s) to truncate
					int halfMaxLogLength = maxLogLength / 2;
					if( logString.Length >= halfMaxLogLength )
					{
						if( stackTrace.Length >= halfMaxLogLength )
						{
							// Truncate both logString and stackTrace
							logString = logString.Substring( 0, halfMaxLogLength - 11 ) + "<truncated>";

							// If stackTrace doesn't end with a blank line, its last line won't be visible in the console for some reason
							stackTrace = stackTrace.Substring( 0, halfMaxLogLength - 12 ) + "<truncated>\n";
						}
						else
						{
							// Truncate logString
							logString = logString.Substring( 0, maxLogLength - stackTrace.Length - 11 ) + "<truncated>";
						}
					}
					else
					{
						// Truncate stackTrace
						stackTrace = stackTrace.Substring( 0, maxLogLength - logString.Length - 12 ) + "<truncated>\n";
					}
				}
			}

			QueuedDebugLogEntry queuedLogEntry = new QueuedDebugLogEntry( logString, stackTrace, logType );

			lock( logEntriesLock )
			{
				queuedLogEntries.Add( queuedLogEntry );
			}
		}

		// Present the log entry in the console
		private void ProcessLog( QueuedDebugLogEntry queuedLogEntry )
		{
			LogType logType = queuedLogEntry.logType;
			DebugLogEntry logEntry;
			if( pooledLogEntries.Count > 0 )
			{
				logEntry = pooledLogEntries[pooledLogEntries.Count - 1];
				pooledLogEntries.RemoveAt( pooledLogEntries.Count - 1 );
			}
			else
				logEntry = new DebugLogEntry();

			logEntry.Initialize( queuedLogEntry.logString, queuedLogEntry.stackTrace );

			// Check if this entry is a duplicate (i.e. has been received before)
			int logEntryIndex;
			bool isEntryInCollapsedEntryList = collapsedLogEntriesMap.TryGetValue( logEntry, out logEntryIndex );
			if( !isEntryInCollapsedEntryList )
			{
				// It is not a duplicate,
				// add it to the list of unique debug entries
				logEntry.logTypeSpriteRepresentation = logSpriteRepresentations[logType];

				logEntryIndex = collapsedLogEntries.Count;
				collapsedLogEntries.Add( logEntry );
				collapsedLogEntriesMap[logEntry] = logEntryIndex;
			}
			else
			{
				// It is a duplicate, pool the duplicate log entry and
				// increment the original debug item's collapsed count
				pooledLogEntries.Add( logEntry );

				logEntry = collapsedLogEntries[logEntryIndex];
				logEntry.count++;
			}

			// Add the index of the unique debug entry to the list
			// that stores the order the debug entries are received
			uncollapsedLogEntriesIndices.Add( logEntryIndex );

			// If this debug entry matches the current filters,
			// add it to the list of debug entries to show
			int logEntryIndexInEntriesToShow = -1;
			Sprite logTypeSpriteRepresentation = logEntry.logTypeSpriteRepresentation;
			if( isCollapseOn && isEntryInCollapsedEntryList )
			{
				if( isLogWindowVisible )
				{
					if( !isInSearchMode && logFilter == DebugLogFilter.All )
						logEntryIndexInEntriesToShow = logEntryIndex;
					else
						logEntryIndexInEntriesToShow = indicesOfListEntriesToShow.IndexOf( logEntryIndex );

					recycledListView.OnCollapsedLogEntryAtIndexUpdated( logEntryIndexInEntriesToShow );
				}
			}
			else if( ( !isInSearchMode || queuedLogEntry.MatchesSearchTerm( searchTerm ) ) && ( logFilter == DebugLogFilter.All ||
			   ( logTypeSpriteRepresentation == infoLog && ( ( logFilter & DebugLogFilter.Info ) == DebugLogFilter.Info ) ) ||
			   ( logTypeSpriteRepresentation == warningLog && ( ( logFilter & DebugLogFilter.Warning ) == DebugLogFilter.Warning ) ) ||
			   ( logTypeSpriteRepresentation == errorLog && ( ( logFilter & DebugLogFilter.Error ) == DebugLogFilter.Error ) ) ) )
			{
				indicesOfListEntriesToShow.Add( logEntryIndex );
				logEntryIndexInEntriesToShow = indicesOfListEntriesToShow.Count - 1;

				if( isLogWindowVisible )
					recycledListView.OnLogEntriesUpdated( false );
			}

			if( logType == LogType.Log )
			{
				infoEntryCount++;
				infoEntryCountText.text = infoEntryCount.ToString();

				// If debug popup is visible, notify it of the new debug entry
				if( !isLogWindowVisible )
					popupManager.NewInfoLogArrived();
			}
			else if( logType == LogType.Warning )
			{
				warningEntryCount++;
				warningEntryCountText.text = warningEntryCount.ToString();

				// If debug popup is visible, notify it of the new debug entry
				if( !isLogWindowVisible )
					popupManager.NewWarningLogArrived();
			}
			else
			{
				errorEntryCount++;
				errorEntryCountText.text = errorEntryCount.ToString();

				// If debug popup is visible, notify it of the new debug entry
				if( !isLogWindowVisible )
					popupManager.NewErrorLogArrived();
			}

			// Automatically expand this log if necessary
			if( pendingLogToAutoExpand > 0 && --pendingLogToAutoExpand <= 0 && isLogWindowVisible && logEntryIndexInEntriesToShow >= 0 )
				recycledListView.SelectAndFocusOnLogItemAtIndex( logEntryIndexInEntriesToShow );
		}

		// Value of snapToBottom is changed (user scrolled the list manually)
		public void SetSnapToBottom( bool snapToBottom )
		{
			this.snapToBottom = snapToBottom;
		}

		// Make sure the scroll bar of the scroll rect is adjusted properly
		internal void ValidateScrollPosition()
		{
			logItemsScrollRect.OnScroll( nullPointerEventData );
		}

		// Automatically expand the latest log in queuedLogEntries
		internal void ExpandLatestPendingLog()
		{
			pendingLogToAutoExpand = queuedLogEntries.Count;
		}

		// Clear all the logs
		public void ClearLogs()
		{
			snapToBottom = true;

			infoEntryCount = 0;
			warningEntryCount = 0;
			errorEntryCount = 0;

			infoEntryCountText.text = "0";
			warningEntryCountText.text = "0";
			errorEntryCountText.text = "0";

			collapsedLogEntries.Clear();
			collapsedLogEntriesMap.Clear();
			uncollapsedLogEntriesIndices.Clear();
			indicesOfListEntriesToShow.Clear();

			recycledListView.DeselectSelectedLogItem();
			recycledListView.OnLogEntriesUpdated( true );
		}

		// Collapse button is clicked
		private void CollapseButtonPressed()
		{
			// Swap the value of collapse mode
			isCollapseOn = !isCollapseOn;

			snapToBottom = true;
			collapseButton.color = isCollapseOn ? collapseButtonSelectedColor : collapseButtonNormalColor;
			recycledListView.SetCollapseMode( isCollapseOn );

			// Determine the new list of debug entries to show
			FilterLogs();
		}

		// Filtering mode of info logs has changed
		private void FilterLogButtonPressed()
		{
			logFilter = logFilter ^ DebugLogFilter.Info;

			if( ( logFilter & DebugLogFilter.Info ) == DebugLogFilter.Info )
				filterInfoButton.color = filterButtonsSelectedColor;
			else
				filterInfoButton.color = filterButtonsNormalColor;

			FilterLogs();
		}

		// Filtering mode of warning logs has changed
		private void FilterWarningButtonPressed()
		{
			logFilter = logFilter ^ DebugLogFilter.Warning;

			if( ( logFilter & DebugLogFilter.Warning ) == DebugLogFilter.Warning )
				filterWarningButton.color = filterButtonsSelectedColor;
			else
				filterWarningButton.color = filterButtonsNormalColor;

			FilterLogs();
		}

		// Filtering mode of error logs has changed
		private void FilterErrorButtonPressed()
		{
			logFilter = logFilter ^ DebugLogFilter.Error;

			if( ( logFilter & DebugLogFilter.Error ) == DebugLogFilter.Error )
				filterErrorButton.color = filterButtonsSelectedColor;
			else
				filterErrorButton.color = filterButtonsNormalColor;

			FilterLogs();
		}

		// Search term has changed
		private void SearchTermChanged( string searchTerm )
		{
			if( searchTerm != null )
				searchTerm = searchTerm.Trim();

			this.searchTerm = searchTerm;
			bool isInSearchMode = !string.IsNullOrEmpty( searchTerm );
			if( isInSearchMode || this.isInSearchMode )
			{
				this.isInSearchMode = isInSearchMode;
				FilterLogs();
			}
		}

		// Show suggestions for the currently entered command
		private void RefreshCommandSuggestions( string command )
		{
			if( !showCommandSuggestions )
				return;

			commandInputFieldPrevCaretPos = commandInputField.caretPosition;

			// Don't recalculate the command suggestions if the input command hasn't changed (i.e. only caret's position has changed)
			bool commandChanged = command != commandInputFieldPrevCommand;
			bool commandNameOrParametersChanged = false;
			if( commandChanged )
			{
				commandInputFieldPrevCommand = command;

				matchingCommandSuggestions.Clear();
				commandCaretIndexIncrements.Clear();

				string prevCommandName = commandInputFieldPrevCommandName;
				int numberOfParameters;
				DebugLogConsole.GetCommandSuggestions( command, matchingCommandSuggestions, commandCaretIndexIncrements, ref commandInputFieldPrevCommandName, out numberOfParameters );
				if( prevCommandName != commandInputFieldPrevCommandName || numberOfParameters != commandInputFieldPrevParamCount )
				{
					commandInputFieldPrevParamCount = numberOfParameters;
					commandNameOrParametersChanged = true;
				}
			}

			int caretArgumentIndex = 0;
			int caretPos = commandInputField.caretPosition;
			for( int i = 0; i < commandCaretIndexIncrements.Count && caretPos > commandCaretIndexIncrements[i]; i++ )
				caretArgumentIndex++;

			if( caretArgumentIndex != commandInputFieldPrevCaretArgumentIndex )
				commandInputFieldPrevCaretArgumentIndex = caretArgumentIndex;
			else if( !commandChanged || !commandNameOrParametersChanged )
			{
				// Command suggestions don't need to be updated if:
				// a) neither the entered command nor the argument that the caret is hovering has changed
				// b) entered command has changed but command's name hasn't changed, parameter count hasn't changed and the argument
				//    that the caret is hovering hasn't changed (i.e. user has continued typing a parameter's value)
				return;
			}

			if( matchingCommandSuggestions.Count == 0 )
				OnEndEditCommand( command );
			else
			{
				if( !commandSuggestionsContainer.gameObject.activeSelf )
					commandSuggestionsContainer.gameObject.SetActive( true );

				int suggestionInstancesCount = commandSuggestionInstances.Count;
				int suggestionsCount = matchingCommandSuggestions.Count;

				for( int i = 0; i < suggestionsCount; i++ )
				{
					if( i >= visibleCommandSuggestionInstances )
					{
						if( i >= suggestionInstancesCount )
							commandSuggestionInstances.Add( (Text) Instantiate( commandSuggestionPrefab, commandSuggestionsContainer, false ) );
						else
							commandSuggestionInstances[i].gameObject.SetActive( true );

						visibleCommandSuggestionInstances++;
					}

					ConsoleMethodInfo suggestedCommand = matchingCommandSuggestions[i];
					commandSuggestionsStringBuilder.Length = 0;
					if( caretArgumentIndex > 0 )
						commandSuggestionsStringBuilder.Append( suggestedCommand.command );
					else
						commandSuggestionsStringBuilder.Append( commandSuggestionHighlightStart ).Append( matchingCommandSuggestions[i].command ).Append( commandSuggestionHighlightEnd );

					if( suggestedCommand.parameters.Length > 0 )
					{
						commandSuggestionsStringBuilder.Append( " " );

						// If the command name wasn't highlighted, a parameter must always be highlighted
						int caretParameterIndex = caretArgumentIndex - 1;
						if( caretParameterIndex >= suggestedCommand.parameters.Length )
							caretParameterIndex = suggestedCommand.parameters.Length - 1;

						for( int j = 0; j < suggestedCommand.parameters.Length; j++ )
						{
							if( caretParameterIndex != j )
								commandSuggestionsStringBuilder.Append( suggestedCommand.parameters[j] );
							else
								commandSuggestionsStringBuilder.Append( commandSuggestionHighlightStart ).Append( suggestedCommand.parameters[j] ).Append( commandSuggestionHighlightEnd );
						}
					}

					commandSuggestionInstances[i].text = commandSuggestionsStringBuilder.ToString();
				}

				for( int i = visibleCommandSuggestionInstances - 1; i >= suggestionsCount; i-- )
					commandSuggestionInstances[i].gameObject.SetActive( false );

				visibleCommandSuggestionInstances = suggestionsCount;
			}
		}

		// Command input field has lost focus
		private void OnEndEditCommand( string command )
		{
			if( commandSuggestionsContainer.gameObject.activeSelf )
				commandSuggestionsContainer.gameObject.SetActive( false );
		}

		// Debug window is being resized,
		// Set the sizeDelta property of the window accordingly while
		// preventing window dimensions from going below the minimum dimensions
		internal void Resize( PointerEventData eventData )
		{
			// Grab the resize button from top; 36f is the height of the resize button
			float newHeight = ( eventData.position.y - logWindowTR.position.y ) / -canvasTR.localScale.y + 36f;
			if( newHeight < minimumHeight )
				newHeight = minimumHeight;

			Vector2 anchorMin = logWindowTR.anchorMin;
			anchorMin.y = Mathf.Max( 0f, 1f - newHeight / canvasTR.sizeDelta.y );
			logWindowTR.anchorMin = anchorMin;

			// Update the recycled list view
			recycledListView.OnViewportDimensionsChanged();
		}

		// Determine the filtered list of debug entries to show on screen
		private void FilterLogs()
		{
			indicesOfListEntriesToShow.Clear();

			if( logFilter != DebugLogFilter.None )
			{
				if( logFilter == DebugLogFilter.All )
				{
					if( isCollapseOn )
					{
						if( !isInSearchMode )
						{
							// All the unique debug entries will be listed just once.
							// So, list of debug entries to show is the same as the
							// order these unique debug entries are added to collapsedLogEntries
							for( int i = 0, count = collapsedLogEntries.Count; i < count; i++ )
								indicesOfListEntriesToShow.Add( i );
						}
						else
						{
							for( int i = 0, count = collapsedLogEntries.Count; i < count; i++ )
							{
								if( collapsedLogEntries[i].MatchesSearchTerm( searchTerm ) )
									indicesOfListEntriesToShow.Add( i );
							}
						}
					}
					else
					{
						if( !isInSearchMode )
						{
							for( int i = 0, count = uncollapsedLogEntriesIndices.Count; i < count; i++ )
								indicesOfListEntriesToShow.Add( uncollapsedLogEntriesIndices[i] );
						}
						else
						{
							for( int i = 0, count = uncollapsedLogEntriesIndices.Count; i < count; i++ )
							{
								if( collapsedLogEntries[uncollapsedLogEntriesIndices[i]].MatchesSearchTerm( searchTerm ) )
									indicesOfListEntriesToShow.Add( uncollapsedLogEntriesIndices[i] );
							}
						}
					}
				}
				else
				{
					// Show only the debug entries that match the current filter
					bool isInfoEnabled = ( logFilter & DebugLogFilter.Info ) == DebugLogFilter.Info;
					bool isWarningEnabled = ( logFilter & DebugLogFilter.Warning ) == DebugLogFilter.Warning;
					bool isErrorEnabled = ( logFilter & DebugLogFilter.Error ) == DebugLogFilter.Error;

					if( isCollapseOn )
					{
						for( int i = 0, count = collapsedLogEntries.Count; i < count; i++ )
						{
							DebugLogEntry logEntry = collapsedLogEntries[i];

							if( isInSearchMode && !logEntry.MatchesSearchTerm( searchTerm ) )
								continue;

							if( logEntry.logTypeSpriteRepresentation == infoLog )
							{
								if( isInfoEnabled )
									indicesOfListEntriesToShow.Add( i );
							}
							else if( logEntry.logTypeSpriteRepresentation == warningLog )
							{
								if( isWarningEnabled )
									indicesOfListEntriesToShow.Add( i );
							}
							else if( isErrorEnabled )
								indicesOfListEntriesToShow.Add( i );
						}
					}
					else
					{
						for( int i = 0, count = uncollapsedLogEntriesIndices.Count; i < count; i++ )
						{
							DebugLogEntry logEntry = collapsedLogEntries[uncollapsedLogEntriesIndices[i]];

							if( isInSearchMode && !logEntry.MatchesSearchTerm( searchTerm ) )
								continue;

							if( logEntry.logTypeSpriteRepresentation == infoLog )
							{
								if( isInfoEnabled )
									indicesOfListEntriesToShow.Add( uncollapsedLogEntriesIndices[i] );
							}
							else if( logEntry.logTypeSpriteRepresentation == warningLog )
							{
								if( isWarningEnabled )
									indicesOfListEntriesToShow.Add( uncollapsedLogEntriesIndices[i] );
							}
							else if( isErrorEnabled )
								indicesOfListEntriesToShow.Add( uncollapsedLogEntriesIndices[i] );
						}
					}
				}
			}

			// Update the recycled list view
			recycledListView.DeselectSelectedLogItem();
			recycledListView.OnLogEntriesUpdated( true );

			ValidateScrollPosition();
		}

		public string GetAllLogs()
		{
			int count = uncollapsedLogEntriesIndices.Count;
			int length = 0;
			int newLineLength = System.Environment.NewLine.Length;
			for( int i = 0; i < count; i++ )
			{
				DebugLogEntry entry = collapsedLogEntries[uncollapsedLogEntriesIndices[i]];
				length += entry.logString.Length + entry.stackTrace.Length + newLineLength * 3;
			}

			length += 100; // Just in case...

			StringBuilder sb = new StringBuilder( length );
			for( int i = 0; i < count; i++ )
			{
				DebugLogEntry entry = collapsedLogEntries[uncollapsedLogEntriesIndices[i]];
				sb.AppendLine( entry.logString ).AppendLine( entry.stackTrace ).AppendLine();
			}

			return sb.ToString();
		}

		private void SaveLogsToFile()
		{
			string path = Path.Combine( Application.persistentDataPath, System.DateTime.Now.ToString( "dd-MM-yyyy--HH-mm-ss" ) + ".txt" );
			File.WriteAllText( path, GetAllLogs() );

			Debug.Log( "Logs saved to: " + path );
		}

		// If a cutout is intersecting with debug window on notch screens, shift the window downwards
		private void CheckScreenCutout()
		{
			if( !avoidScreenCutout )
				return;

#if UNITY_2017_2_OR_NEWER && !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
			// Check if there is a cutout at the top of the screen
			int screenHeight = Screen.height;
			float safeYMax = Screen.safeArea.yMax;
			if( safeYMax < screenHeight - 1 ) // 1: a small threshold
			{
				// There is a cutout, shift the log window downwards
				float cutoutPercentage = ( screenHeight - safeYMax ) / Screen.height;
				float cutoutLocalSize = cutoutPercentage * canvasTR.rect.height;

				logWindowTR.anchoredPosition = new Vector2( 0f, -cutoutLocalSize );
				logWindowTR.sizeDelta = new Vector2( 0f, -cutoutLocalSize );
			}
			else
			{
				logWindowTR.anchoredPosition = Vector2.zero;
				logWindowTR.sizeDelta = Vector2.zero;
			}
#endif
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		private IEnumerator ActivateCommandInputFieldCoroutine()
		{
			// Waiting 1 frame before activating commandInputField ensures that the toggleKey isn't captured by it
			yield return null;
			commandInputField.ActivateInputField();

			yield return null;
			commandInputField.MoveTextEnd( false );
		}
#endif

		// Pool an unused log item
		internal void PoolLogItem( DebugLogItem logItem )
		{
			logItem.gameObject.SetActive( false );
			pooledLogItems.Add( logItem );
		}

		// Fetch a log item from the pool
		internal DebugLogItem PopLogItem()
		{
			DebugLogItem newLogItem;

			// If pool is not empty, fetch a log item from the pool,
			// create a new log item otherwise
			if( pooledLogItems.Count > 0 )
			{
				newLogItem = pooledLogItems[pooledLogItems.Count - 1];
				pooledLogItems.RemoveAt( pooledLogItems.Count - 1 );
				newLogItem.gameObject.SetActive( true );
			}
			else
			{
				newLogItem = (DebugLogItem) Instantiate( logItemPrefab, logItemsContainer, false );
				newLogItem.Initialize( recycledListView );
			}

			return newLogItem;
		}
	}
}