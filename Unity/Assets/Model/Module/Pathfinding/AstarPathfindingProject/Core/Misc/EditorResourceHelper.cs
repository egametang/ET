namespace Pathfinding {
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;

	/** Internal utility class for looking up editor resources */
	public static class EditorResourceHelper {
		/** Path to the editor assets folder for the A* Pathfinding Project. If this path turns out to be incorrect, the script will try to find the correct path
		 * \see LoadStyles */
		public static string editorAssets;

		static EditorResourceHelper () {
			// Look up editor assets directory when first accessed
			LocateEditorAssets();
		}

		static Material surfaceMat, lineMat;
		public static Material GizmoSurfaceMaterial {
			get {
				if (!surfaceMat) surfaceMat = UnityEditor.AssetDatabase.LoadAssetAtPath(EditorResourceHelper.editorAssets + "/Materials/Navmesh.mat", typeof(Material)) as Material;
				return surfaceMat;
			}
		}

		public static Material GizmoLineMaterial {
			get {
				if (!lineMat) lineMat = UnityEditor.AssetDatabase.LoadAssetAtPath(EditorResourceHelper.editorAssets + "/Materials/NavmeshOutline.mat", typeof(Material)) as Material;
				return lineMat;
			}
		}

		/** Locates the editor assets folder in case the user has moved it */
		public static bool LocateEditorAssets () {
			string projectPath = Application.dataPath;

			if (projectPath.EndsWith("/Assets")) {
				projectPath = projectPath.Remove(projectPath.Length-("Assets".Length));
			}

			editorAssets = "Assets/AstarPathfindingProject/Editor/EditorAssets";
			if (!System.IO.File.Exists(projectPath + editorAssets + "/AstarEditorSkinLight.guiskin") && !System.IO.File.Exists(projectPath + editorAssets + "/AstarEditorSkin.guiskin")) {
				//Initiate search

				var sdir = new System.IO.DirectoryInfo(Application.dataPath);

				var dirQueue = new Queue<System.IO.DirectoryInfo>();
				dirQueue.Enqueue(sdir);

				bool found = false;
				while (dirQueue.Count > 0) {
					System.IO.DirectoryInfo dir = dirQueue.Dequeue();
					if (System.IO.File.Exists(dir.FullName + "/AstarEditorSkinLight.guiskin") || System.IO.File.Exists(dir.FullName + "/AstarEditorSkin.guiskin")) {
						// Handle windows file paths
						string path = dir.FullName.Replace('\\', '/');
						found = true;
						// Remove data path from string to make it relative
						path = path.Replace(projectPath, "");

						if (path.StartsWith("/")) {
							path = path.Remove(0, 1);
						}

						editorAssets = path;
						return true;
					}
					var dirs = dir.GetDirectories();
					for (int i = 0; i < dirs.Length; i++) {
						dirQueue.Enqueue(dirs[i]);
					}
				}

				if (!found) {
					Debug.LogWarning("Could not locate editor assets folder. Make sure you have imported the package correctly.\nA* Pathfinding Project");
					return false;
				}
			}
			return true;
		}
	}
#endif
}
