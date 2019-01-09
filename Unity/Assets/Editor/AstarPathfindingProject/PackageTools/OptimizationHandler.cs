using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Pathfinding {
	/**
	 * Helper for enabling or disabling compiler directives.
	 * Used only in the editor.
	 * \astarpro
	 */
	public static class OptimizationHandler {
		public class DefineDefinition {
			public string name;
			public string description;
			public bool enabled;
			public bool consistent;
		}

		/** Various build targets that Unity have deprecated.
		 * There is apparently no way to figute out which these are without hard coding them.
		 */
		static readonly BuildTargetGroup[] deprecatedBuildTargets = new BuildTargetGroup[] {
			BuildTargetGroup.Unknown,
#if UNITY_5_4_OR_NEWER
			(BuildTargetGroup)16, /* BlackBerry */
#endif
#if UNITY_5_5_OR_NEWER
			(BuildTargetGroup)5, /* PS3 */
			(BuildTargetGroup)6, /* XBox360 */
			(BuildTargetGroup)15, /* WP8 */
#endif
		};

		static string GetPackageRootDirectory () {
			var paths = Directory.GetDirectories($"./Assets/Model/Module/Pathfinding/", "AstarPathfindingProject", SearchOption.AllDirectories);

			if (paths.Length > 0) {
				return paths[0];
			}

			Debug.LogError("Could not find AstarPathfindingProject root folder");
			return Application.dataPath + "/AstarPathfindingProject";
		}

		static Dictionary<BuildTargetGroup, List<string> > GetDefineSymbols () {
			var result = new Dictionary<BuildTargetGroup, List<string> >();
			var buildTypes = System.Enum.GetValues(typeof(BuildTargetGroup)) as int[];

			for (int i = 0; i < buildTypes.Length; i++) {
				if (deprecatedBuildTargets.Contains((BuildTargetGroup)buildTypes[i])) continue;

				string defineString = PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)buildTypes[i]);
				if (defineString == null) continue;

				var defines = defineString.Split(';').Select(s => s.Trim()).ToList();
				result[(BuildTargetGroup)buildTypes[i]] = defines;
			}
			return result;
		}

		static void SetDefineSymbols (Dictionary<BuildTargetGroup, List<string> > symbols) {
			foreach (var pair in symbols) {
				var defineString = string.Join(";", pair.Value.Distinct().ToArray());
				PlayerSettings.SetScriptingDefineSymbolsForGroup(pair.Key, defineString);
			}
		}

		public static void EnableDefine (string name) {
			name = name.Trim();
			var newSymbols = GetDefineSymbols().ToDictionary(pair => pair.Key, pair => {
				pair.Value.Add(name);
				return pair.Value;
			});
			SetDefineSymbols(newSymbols);
		}

		public static void DisableDefine (string name) {
			name = name.Trim();
			var newSymbols = GetDefineSymbols().ToDictionary(pair => pair.Key, pair => {
				pair.Value.Remove(name);
				return pair.Value;
			});
			SetDefineSymbols(newSymbols);
		}

		public static void IsDefineEnabled (string name, out bool enabled, out bool consistent) {
			name = name.Trim();
			int foundEnabled = 0;
			int foundDisabled = 0;

			foreach (var pair in GetDefineSymbols()) {
				if (pair.Value.Contains(name)) {
					foundEnabled++;
				} else {
					foundDisabled++;
				}
			}

			enabled = foundEnabled > foundDisabled;
			consistent = (foundEnabled > 0) != (foundDisabled > 0);
		}

		public static List<DefineDefinition> FindDefines () {
			var path = GetPackageRootDirectory()+"/defines.csv";

			if (File.Exists(path)) {
				// Read a file consisting of lines with the format
				// NAME;Description
				// Ignore empty lines and lines which do not contain exactly 1 ';'
				var definePairs = File.ReadAllLines(path)
								  .Select(line => line.Trim())
								  .Where(line => line.Length > 0)
								  .Select(line => line.Split(';'))
								  .Where(opts => opts.Length == 2);

				return definePairs.Select(opts => {
					var def = new DefineDefinition { name = opts[0].Trim(), description = opts[1].Trim() };
					IsDefineEnabled(def.name, out def.enabled, out def.consistent);
					return def;
				}).ToList();
			}

			Debug.LogError("Could not find file '"+path+"'");
			return new List<DefineDefinition>();
		}

		public static void ApplyDefines (List<DefineDefinition> defines) {
			foreach (var define in defines) {
				if (define.enabled) {
					EnableDefine(define.name);
				} else {
					DisableDefine(define.name);
				}
			}
		}
	}
}
