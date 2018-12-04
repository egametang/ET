using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding {
	/** Handles update checking for the A* Pathfinding Project */
	[InitializeOnLoad]
	public static class AstarUpdateChecker {
		/** Used for downloading new version information */
		static WWW updateCheckDownload;

		static System.DateTime _lastUpdateCheck;
		static bool _lastUpdateCheckRead;

		static System.Version _latestVersion;

		static System.Version _latestBetaVersion;

		/** Description of the latest update of the A* Pathfinding Project */
		static string _latestVersionDescription;

		static bool hasParsedServerMessage;

		/** Number of days between update checks */
		const double updateCheckRate = 1F;

		/** URL to the version file containing the latest version number. */
		const string updateURL = "http://www.arongranberg.com/astar/version.php";

		/** Last time an update check was made */
		public static System.DateTime lastUpdateCheck {
			get {
				try {
					// Reading from EditorPrefs is relatively slow, avoid it
					if (_lastUpdateCheckRead) return _lastUpdateCheck;

					_lastUpdateCheck = System.DateTime.Parse(EditorPrefs.GetString("AstarLastUpdateCheck", "1/1/1971 00:00:01"), System.Globalization.CultureInfo.InvariantCulture);
					_lastUpdateCheckRead = true;
				}
				catch (System.FormatException) {
					lastUpdateCheck = System.DateTime.UtcNow;
					Debug.LogWarning("Invalid DateTime string encountered when loading from preferences");
				}
				return _lastUpdateCheck;
			}
			private set {
				_lastUpdateCheck = value;
				EditorPrefs.SetString("AstarLastUpdateCheck", _lastUpdateCheck.ToString(System.Globalization.CultureInfo.InvariantCulture));
			}
		}

		/** Latest version of the A* Pathfinding Project */
		public static System.Version latestVersion {
			get {
				RefreshServerMessage();
				return _latestVersion ?? AstarPath.Version;
			}
			private set {
				_latestVersion = value;
			}
		}

		/** Latest beta version of the A* Pathfinding Project */
		public static System.Version latestBetaVersion {
			get {
				RefreshServerMessage();
				return _latestBetaVersion ?? AstarPath.Version;
			}
			private set {
				_latestBetaVersion = value;
			}
		}

		/** Summary of the latest update */
		public static string latestVersionDescription {
			get {
				RefreshServerMessage();
				return _latestVersionDescription ?? "";
			}
			private set {
				_latestVersionDescription = value;
			}
		}

		/** Holds various URLs and text for the editor.
		 * This info can be updated when a check for new versions is done to ensure that there are no invalid links.
		 */
		static Dictionary<string, string> astarServerData = new Dictionary<string, string> {
			{ "URL:modifiers", "http://www.arongranberg.com/astar/docs/modifiers.php" },
			{ "URL:astarpro", "http://arongranberg.com/unity/a-pathfinding/astarpro/" },
			{ "URL:documentation", "http://arongranberg.com/astar/docs/" },
			{ "URL:findoutmore", "http://arongranberg.com/astar" },
			{ "URL:download", "http://arongranberg.com/unity/a-pathfinding/download" },
			{ "URL:changelog", "http://arongranberg.com/astar/docs/changelog.php" },
			{ "URL:tags", "http://arongranberg.com/astar/docs/tags.php" },
			{ "URL:homepage", "http://arongranberg.com/astar/" }
		};

		static AstarUpdateChecker() {
			// Add a callback so that we can parse the message when it has been downloaded
			EditorApplication.update += UpdateCheckLoop;
		}

		static void RefreshServerMessage () {
			if (!hasParsedServerMessage) {
				var serverMessage = EditorPrefs.GetString("AstarServerMessage");

				if (!string.IsNullOrEmpty(serverMessage)) {
					ParseServerMessage(serverMessage);
					ShowUpdateWindowIfRelevant();
				}
			}
		}

		public static string GetURL (string tag) {
			RefreshServerMessage();
			string url;
			astarServerData.TryGetValue("URL:"+tag, out url);
			return url ?? "";
		}

		/** Initiate a check for updates now, regardless of when the last check was done */
		public static void CheckForUpdatesNow () {
			lastUpdateCheck = System.DateTime.UtcNow.AddDays(-5);

			// Remove the callback if it already exists
			EditorApplication.update -= UpdateCheckLoop;

			// Add a callback so that we can parse the message when it has been downloaded
			EditorApplication.update += UpdateCheckLoop;
		}

		/**
		 * Checking for updates...
		 * Should be called from EditorApplication.update
		 */
		static void UpdateCheckLoop () {
			// Go on until the update check has been completed
			if (!CheckForUpdates()) {
				EditorApplication.update -= UpdateCheckLoop;
			}
		}

		/** Checks for updates if there was some time since last check.
		 * It must be called repeatedly to ensure that the result is processed.
		 * \returns True if an update check is progressing (WWW request)
		 */
		static bool CheckForUpdates () {
			if (updateCheckDownload != null && updateCheckDownload.isDone) {
				if (!string.IsNullOrEmpty(updateCheckDownload.error)) {
					Debug.LogWarning("There was an error checking for updates to the A* Pathfinding Project\n" +
						"The error might disappear if you switch build target from Webplayer to Standalone because of the webplayer security emulation\nError: " +
						updateCheckDownload.error);
					updateCheckDownload = null;
					return false;
				}
				UpdateCheckCompleted(updateCheckDownload.text);
				updateCheckDownload = null;
			}

			// Check if it is time to check for updates
			// Check for updates a bit earlier if we are in play mode or have the AstarPath object in the scene
			// as then the collected statistics will be a bit more accurate
			var offsetMinutes = (Application.isPlaying && Time.time > 60) || AstarPath.active != null ? -20 : 20;
			var minutesUntilUpdate = lastUpdateCheck.AddDays(updateCheckRate).AddMinutes(offsetMinutes).Subtract(System.DateTime.UtcNow).TotalMinutes;
			if (minutesUntilUpdate < 0) {
				DownloadVersionInfo();
			}

			return updateCheckDownload != null || minutesUntilUpdate < 10;
		}

		static void DownloadVersionInfo () {
			var script = AstarPath.active != null ? AstarPath.active : GameObject.FindObjectOfType(typeof(AstarPath)) as AstarPath;

			if (script != null) {
				script.ConfigureReferencesInternal();
				if ((!Application.isPlaying && (script.data.graphs == null || script.data.graphs.Length == 0)) || script.data.graphs == null) {
					script.data.DeserializeGraphs();
				}
			}

			bool mecanim = GameObject.FindObjectOfType(typeof(Animator)) != null;
			string query = updateURL+
						   "?v="+AstarPath.Version+
						   "&pro=1"+
						   "&check="+updateCheckRate+"&distr="+AstarPath.Distribution+
						   "&unitypro="+(Application.HasProLicense() ? "1" : "0")+
						   "&inscene="+(script != null ? "1" : "0")+
						   "&targetplatform="+EditorUserBuildSettings.activeBuildTarget+
						   "&devplatform="+Application.platform+
						   "&mecanim="+(mecanim ? "1" : "0")+
						   "&hasNavmesh=" + (script != null && script.data.graphs.Any(g => g.GetType().Name == "NavMeshGraph") ? 1 : 0) +
						   "&hasPoint=" + (script != null && script.data.graphs.Any(g => g.GetType().Name == "PointGraph") ? 1 : 0) +
						   "&hasGrid=" + (script != null && script.data.graphs.Any(g => g.GetType().Name == "GridGraph") ? 1 : 0) +
						   "&hasLayered=" + (script != null && script.data.graphs.Any(g => g.GetType().Name == "LayerGridGraph") ? 1 : 0) +
						   "&hasRecast=" + (script != null && script.data.graphs.Any(g => g.GetType().Name == "RecastGraph") ? 1 : 0) +
						   "&hasGrid=" + (script != null && script.data.graphs.Any(g => g.GetType().Name == "GridGraph") ? 1 : 0) +
						   "&hasCustom=" + (script != null && script.data.graphs.Any(g => g != null && !g.GetType().FullName.Contains("Pathfinding.")) ? 1 : 0) +
						   "&graphCount=" + (script != null ? script.data.graphs.Count(g => g != null) : 0) +
						   "&unityversion="+Application.unityVersion +
						   "&branch="+AstarPath.Branch;

			updateCheckDownload = new WWW(query);
			lastUpdateCheck = System.DateTime.UtcNow;
		}

		/** Handles the data from the update page */
		static void UpdateCheckCompleted (string result) {
			EditorPrefs.SetString("AstarServerMessage", result);
			ParseServerMessage(result);
			ShowUpdateWindowIfRelevant();
		}

		static void ParseServerMessage (string result) {
			if (string.IsNullOrEmpty(result)) {
				return;
			}

			hasParsedServerMessage = true;

			#if ASTARDEBUG
			Debug.Log("Result from update check:\n"+result);
			#endif

			string[] splits = result.Split('|');
			latestVersionDescription = splits.Length > 1 ? splits[1] : "";

			if (splits.Length > 4) {
				// First 4 are just compatibility fields
				var fields = splits.Skip(4).ToArray();

				// Take all pairs of fields
				for (int i = 0; i < (fields.Length/2)*2; i += 2) {
					string key = fields[i];
					string val = fields[i+1];
					astarServerData[key] = val;
				}
			}

			try {
				latestVersion = new System.Version(astarServerData["VERSION:branch"]);
			} catch (System.Exception ex) {
				Debug.LogWarning("Could not parse version\n"+ex);
			}

			try {
				latestBetaVersion = new System.Version(astarServerData["VERSION:beta"]);
			} catch (System.Exception ex) {
				Debug.LogWarning("Could not parse version\n"+ex);
			}
		}

		static void ShowUpdateWindowIfRelevant () {
			try {
				System.DateTime remindDate;
				var remindVersion = new System.Version(EditorPrefs.GetString("AstarRemindUpdateVersion", "0.0.0.0"));
				if (latestVersion == remindVersion && System.DateTime.TryParse(EditorPrefs.GetString("AstarRemindUpdateDate", "1/1/1971 00:00:01"), out remindDate)) {
					if (System.DateTime.UtcNow < remindDate) {
						// Don't remind yet
						return;
					}
				} else {
					EditorPrefs.DeleteKey("AstarRemindUpdateDate");
					EditorPrefs.DeleteKey("AstarRemindUpdateVersion");
				}
			} catch {
				Debug.LogError("Invalid AstarRemindUpdateVersion or AstarRemindUpdateDate");
			}

			var skipVersion = new System.Version(EditorPrefs.GetString("AstarSkipUpToVersion", AstarPath.Version.ToString()));

			if (AstarPathEditor.FullyDefinedVersion(latestVersion) != AstarPathEditor.FullyDefinedVersion(skipVersion) && AstarPathEditor.FullyDefinedVersion(latestVersion) > AstarPathEditor.FullyDefinedVersion(AstarPath.Version)) {
				EditorPrefs.DeleteKey("AstarSkipUpToVersion");
				EditorPrefs.DeleteKey("AstarRemindUpdateDate");
				EditorPrefs.DeleteKey("AstarRemindUpdateVersion");

				AstarUpdateWindow.Init(latestVersion, latestVersionDescription);
			}
		}
	}
}
