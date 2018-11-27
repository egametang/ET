/** This is a simple utility class for importing obj files into a Unity mesh at runtime.
 * This version of ObjImporter first reads through the entire file, getting a count of how large
 * the final arrays will be, and then uses standard arrays for everything (as opposed to ArrayLists
 * or any other fancy things).
 * \author el anónimo at the UnifyCommunity wiki (at least he seems to have created the page)
 */

using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if NETFX_CORE && !UNITY_EDITOR
//using MarkerMetro.Unity.WinLegacy.IO;
#endif

namespace Pathfinding {
	public class ObjImporter {
		private struct meshStruct {
			public Vector3[] vertices;
			public Vector3[] normals;
			public Vector2[] uv;
			public Vector2[] uv1;
			public Vector2[] uv2;
			public int[] triangles;
			public int[] faceVerts;
			public int[] faceUVs;
			public Vector3[] faceData;
			public string name;
			public string fileName;
		}

		// Use this for initialization
		public static Mesh ImportFile (string filePath) {
#if NETFX_CORE
			throw new System.NotSupportedException("Method not available on this platform");
#else
			if (!File.Exists(filePath)) {
				Debug.LogError("No file was found at '"+filePath+"'");
				return null;
			}

			meshStruct newMesh = createMeshStruct(filePath);
			populateMeshStruct(ref newMesh);

			Vector3[] newVerts = new Vector3[newMesh.faceData.Length];
			Vector2[] newUVs = new Vector2[newMesh.faceData.Length];
			Vector3[] newNormals = new Vector3[newMesh.faceData.Length];
			int i = 0;
			/* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
			 * for the appropriate Unity mesh array.
			 */
			foreach (Vector3 v in newMesh.faceData) {
				newVerts[i] = newMesh.vertices[(int)v.x - 1];
				if (v.y >= 1)
					newUVs[i] = newMesh.uv[(int)v.y - 1];

				if (v.z >= 1)
					newNormals[i] = newMesh.normals[(int)v.z - 1];
				i++;
			}

			Mesh mesh = new Mesh();

			mesh.vertices = newVerts;
			mesh.uv = newUVs;
			mesh.normals = newNormals;
			mesh.triangles = newMesh.triangles;

			mesh.RecalculateBounds();
			//mesh.Optimize();

			return mesh;
#endif
		}

		private static meshStruct createMeshStruct (string filename) {
#if NETFX_CORE
			throw new System.NotSupportedException("Method not available on this platform");
#else
			int triangles = 0;
			int vertices = 0;
			int vt = 0;
			int vn = 0;
			int face = 0;
			meshStruct mesh = new meshStruct();
			mesh.fileName = filename;
			StreamReader stream = File.OpenText(filename);
			string entireText = stream.ReadToEnd();
			stream.Dispose();
			using (StringReader reader = new StringReader(entireText))
			{
				string currentText = reader.ReadLine();
				char[] splitIdentifier = { ' ' };
				string[] brokenString;
				while (currentText != null) {
					if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
						&& !currentText.StartsWith("vn ")) {
						currentText = reader.ReadLine();
						if (currentText != null) {
							currentText = currentText.Replace("  ", " ");
						}
					} else {
						currentText = currentText.Trim();                           //Trim the current line
						brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
						switch (brokenString[0]) {
						case "v":
							vertices++;
							break;
						case "vt":
							vt++;
							break;
						case "vn":
							vn++;
							break;
						case "f":
							face = face + brokenString.Length - 1;
							triangles = triangles + 3 * (brokenString.Length - 2);     /*brokenString.Length is 3 or greater since a face must have at least
							                                                            * 3 vertices.  For each additional vertice, there is an additional
							                                                            * triangle in the mesh (hence this formula).*/
							break;
						}
						currentText = reader.ReadLine();
						if (currentText != null) {
							currentText = currentText.Replace("  ", " ");
						}
					}
				}
			}
			mesh.triangles = new int[triangles];
			mesh.vertices = new Vector3[vertices];
			mesh.uv = new Vector2[vt];
			mesh.normals = new Vector3[vn];
			mesh.faceData = new Vector3[face];
			return mesh;
#endif
		}

		private static void populateMeshStruct (ref meshStruct mesh) {
#if NETFX_CORE
			throw new System.NotSupportedException("Method not available on this platform");
#else
			StreamReader stream = File.OpenText(mesh.fileName);
			string entireText = stream.ReadToEnd();
			stream.Close();
			using (StringReader reader = new StringReader(entireText))
			{
				string currentText = reader.ReadLine();

				char[] splitIdentifier = { ' ' };
				char[] splitIdentifier2 = { '/' };
				string[] brokenString;
				string[] brokenBrokenString;
				int f = 0;
				int f2 = 0;
				int v = 0;
				int vn = 0;
				int vt = 0;
				int vt1 = 0;
				int vt2 = 0;
				while (currentText != null) {
					if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
						!currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
						!currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
						!currentText.StartsWith("vc ") && !currentText.StartsWith("usemap ")) {
						currentText = reader.ReadLine();
						if (currentText != null) {
							currentText = currentText.Replace("  ", " ");
						}
					} else {
						currentText = currentText.Trim();
						brokenString = currentText.Split(splitIdentifier, 50);
						switch (brokenString[0]) {
						case "g":
							break;
						case "usemtl":
							break;
						case "usemap":
							break;
						case "mtllib":
							break;
						case "v":
							mesh.vertices[v] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
								System.Convert.ToSingle(brokenString[3]));
							v++;
							break;
						case "vt":
							mesh.uv[vt] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
							vt++;
							break;
						case "vt1":
							mesh.uv[vt1] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
							vt1++;
							break;
						case "vt2":
							mesh.uv[vt2] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
							vt2++;
							break;
						case "vn":
							mesh.normals[vn] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
								System.Convert.ToSingle(brokenString[3]));
							vn++;
							break;
						case "vc":
							break;
						case "f":

							int j = 1;
							List<int> intArray = new List<int>();
							while (j < brokenString.Length && ("" + brokenString[j]).Length > 0) {
								Vector3 temp = new Vector3();
								brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);        //Separate the face into individual components (vert, uv, normal)
								temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
								if (brokenBrokenString.Length > 1) {                                    //Some .obj files skip UV and normal
									if (brokenBrokenString[1] != "") {                                      //Some .obj files skip the uv and not the normal
										temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
									}
									temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
								}
								j++;

								mesh.faceData[f2] = temp;
								intArray.Add(f2);
								f2++;
							}
							j = 1;
							while (j + 2 < brokenString.Length) {       //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
								mesh.triangles[f] = intArray[0];
								f++;
								mesh.triangles[f] = intArray[j];
								f++;
								mesh.triangles[f] = intArray[j+1];
								f++;

								j++;
							}
							break;
						}
						currentText = reader.ReadLine();
						if (currentText != null) {
							currentText = currentText.Replace("  ", " ");       //Some .obj files insert double spaces, this removes them.
						}
					}
				}
			}
#endif
		}
	}
}
