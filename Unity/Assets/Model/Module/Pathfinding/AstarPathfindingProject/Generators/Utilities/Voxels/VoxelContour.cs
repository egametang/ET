using UnityEngine;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;

namespace Pathfinding.Voxels {
	public partial class Voxelize {
		public void BuildContours (float maxError, int maxEdgeLength, VoxelContourSet cset, int buildFlags) {
			AstarProfiler.StartProfile("Build Contours");

			AstarProfiler.StartProfile("- Init");
			int w = voxelArea.width;
			int d = voxelArea.depth;

			int wd = w*d;

			//cset.bounds = voxelArea.bounds;

			int maxContours = Mathf.Max(8 /*Max Regions*/, 8);


			//cset.conts = new VoxelContour[maxContours];
			List<VoxelContour> contours = new List<VoxelContour>(maxContours);

			AstarProfiler.EndProfile("- Init");
			AstarProfiler.StartProfile("- Mark Boundaries");

			//cset.nconts = 0;

			//NOTE: This array may contain any data, but since we explicitly set all data in it before we use it, it's OK.
			ushort[] flags = voxelArea.tmpUShortArr;
			if (flags.Length < voxelArea.compactSpanCount) {
				flags = voxelArea.tmpUShortArr = new ushort[voxelArea.compactSpanCount];
			}

			// Mark boundaries. (@?)
			for (int z = 0; z < wd; z += voxelArea.width) {
				for (int x = 0; x < voxelArea.width; x++) {
					CompactVoxelCell c = voxelArea.compactCells[x+z];

					for (int i = (int)c.index, ci = (int)(c.index+c.count); i < ci; i++) {
						ushort res = 0;
						CompactVoxelSpan s = voxelArea.compactSpans[i];

						if (s.reg == 0 || (s.reg & BorderReg) == BorderReg) {
							flags[i] = 0;
							continue;
						}

						for (int dir = 0; dir < 4; dir++) {
							int r = 0;

							if (s.GetConnection(dir) != NotConnected) {
								int nx = x + voxelArea.DirectionX[dir];
								int nz = z + voxelArea.DirectionZ[dir];

								int ni = (int)voxelArea.compactCells[nx+nz].index + s.GetConnection(dir);
								r = voxelArea.compactSpans[ni].reg;
							}

							//@TODO - Why isn't this inside the previous IF
							if (r == s.reg) {
								res |= (ushort)(1 << dir);
							}
						}

						//Inverse, mark non connected edges.
						flags[i] = (ushort)(res ^ 0xf);
					}
				}
			}

			AstarProfiler.EndProfile("- Mark Boundaries");

			AstarProfiler.StartProfile("- Simplify Contours");
			List<int> verts = ListPool<int>.Claim(256);//new List<int> (256);
			List<int> simplified = ListPool<int>.Claim(64);//new List<int> (64);

			for (int z = 0; z < wd; z += voxelArea.width) {
				for (int x = 0; x < voxelArea.width; x++) {
					CompactVoxelCell c = voxelArea.compactCells[x+z];

					for (int i = (int)c.index, ci = (int)(c.index+c.count); i < ci; i++) {
						//CompactVoxelSpan s = voxelArea.compactSpans[i];

						if (flags[i] == 0 || flags[i] == 0xf) {
							flags[i] = 0;
							continue;
						}

						int reg = voxelArea.compactSpans[i].reg;

						if (reg == 0 || (reg & BorderReg) == BorderReg) {
							continue;
						}

						int area = voxelArea.areaTypes[i];

						verts.Clear();
						simplified.Clear();

						WalkContour(x, z, i, flags, verts);

						SimplifyContour(verts, simplified, maxError, maxEdgeLength, buildFlags);
						RemoveDegenerateSegments(simplified);

						VoxelContour contour = new VoxelContour();
						contour.verts = ArrayPool<int>.Claim(simplified.Count);//simplified.ToArray ();
						for (int j = 0; j < simplified.Count; j++) contour.verts[j] = simplified[j];
#if ASTAR_RECAST_INCLUDE_RAW_VERTEX_CONTOUR
						//Not used at the moment, just debug stuff
						contour.rverts = ClaimIntArr(verts.Count);
						for (int j = 0; j < verts.Count; j++) contour.rverts[j] = verts[j];
#endif
						contour.nverts = simplified.Count/4;
						contour.reg = reg;
						contour.area = area;

						contours.Add(contour);

						#if ASTARDEBUG
						for (int q = 0, j = (simplified.Count/4)-1; q < (simplified.Count/4); j = q, q++) {
							int i4 = q*4;
							int j4 = j*4;

							Vector3 p1 = Vector3.Scale(
								new Vector3(
									simplified[i4+0],
									simplified[i4+1],
									(simplified[i4+2]/(float)voxelArea.width)
									),
								cellScale)
										 +voxelOffset;

							Vector3 p2 = Vector3.Scale(
								new Vector3(
									simplified[j4+0],
									simplified[j4+1],
									(simplified[j4+2]/(float)voxelArea.width)
									)
								, cellScale)
										 +voxelOffset;


							if (CalcAreaOfPolygon2D(contour.verts, contour.nverts) > 0) {
								Debug.DrawLine(p1, p2, AstarMath.IntToColor(reg, 0.5F));
							} else {
								Debug.DrawLine(p1, p2, Color.red);
							}
						}
						#endif
					}
				}
			}

			ListPool<int>.Release(ref verts);
			ListPool<int>.Release(ref simplified);

			AstarProfiler.EndProfile("- Simplify Contours");

			AstarProfiler.StartProfile("- Fix Contours");

			// Check and merge droppings.
			// Sometimes the previous algorithms can fail and create several contours
			// per area. This pass will try to merge the holes into the main region.
			for (int i = 0; i < contours.Count; i++) {
				VoxelContour cont = contours[i];
				// Check if the contour is would backwards.
				if (CalcAreaOfPolygon2D(cont.verts, cont.nverts) < 0) {
					// Find another contour which has the same region ID.
					int mergeIdx = -1;
					for (int j = 0; j < contours.Count; j++) {
						if (i == j) continue;
						if (contours[j].nverts > 0 && contours[j].reg == cont.reg) {
							// Make sure the polygon is correctly oriented.
							if (CalcAreaOfPolygon2D(contours[j].verts, contours[j].nverts) > 0) {
								mergeIdx = j;
								break;
							}
						}
					}
					if (mergeIdx == -1) {
						Debug.LogError("rcBuildContours: Could not find merge target for bad contour "+i+".");
					} else {
						// Debugging
						//Debug.LogWarning ("Fixing contour");

						VoxelContour mcont = contours[mergeIdx];
						// Merge by closest points.
						int ia = 0, ib = 0;
						GetClosestIndices(mcont.verts, mcont.nverts, cont.verts, cont.nverts, ref ia, ref ib);

						if (ia == -1 || ib == -1) {
							Debug.LogWarning("rcBuildContours: Failed to find merge points for "+i+" and "+mergeIdx+".");
							continue;
						}

#if ASTARDEBUG
						int p4 = ia*4;
						int p42 = ib*4;

						Vector3 p12 = Vector3.Scale(
							new Vector3(
								mcont.verts[p4+0],
								mcont.verts[p4+1],
								(mcont.verts[p4+2]/(float)voxelArea.width)
								),
							cellScale)
									  +voxelOffset;

						Vector3 p22 = Vector3.Scale(
							new Vector3(
								cont.verts[p42+0],
								cont.verts[p42+1],
								(cont.verts[p42+2]/(float)voxelArea.width)
								)
							, cellScale)
									  +voxelOffset;

						Debug.DrawLine(p12, p22, Color.green);
#endif

						if (!MergeContours(ref mcont, ref cont, ia, ib)) {
							Debug.LogWarning("rcBuildContours: Failed to merge contours "+i+" and "+mergeIdx+".");
							continue;
						}

						contours[mergeIdx] = mcont;
						contours[i] = cont;

						#if ASTARDEBUG
						Debug.Log(mcont.nverts);

						for (int q = 0, j = (mcont.nverts)-1; q < (mcont.nverts); j = q, q++) {
							int i4 = q*4;
							int j4 = j*4;

							Vector3 p1 = Vector3.Scale(
								new Vector3(
									mcont.verts[i4+0],
									mcont.verts[i4+1],
									(mcont.verts[i4+2]/(float)voxelArea.width)
									),
								cellScale)
										 +voxelOffset;

							Vector3 p2 = Vector3.Scale(
								new Vector3(
									mcont.verts[j4+0],
									mcont.verts[j4+1],
									(mcont.verts[j4+2]/(float)voxelArea.width)
									)
								, cellScale)
										 +voxelOffset;

							Debug.DrawLine(p1, p2, Color.red);
							//}
						}
						#endif
					}
				}
			}

			cset.conts = contours;

			AstarProfiler.EndProfile("- Fix Contours");

			AstarProfiler.EndProfile("Build Contours");
		}

		void GetClosestIndices (int[] vertsa, int nvertsa,
								int[] vertsb, int nvertsb,
								ref int ia, ref int ib) {
			int closestDist = 0xfffffff;

			ia = -1;
			ib = -1;
			for (int i = 0; i < nvertsa; i++) {
				//in is a keyword in C#, so I can't use that as a variable name
				int in2 = (i+1) % nvertsa;
				int ip = (i+nvertsa-1) % nvertsa;
				int va = i*4;
				int van = in2*4;
				int vap = ip*4;

				for (int j = 0; j < nvertsb; ++j) {
					int vb = j*4;
					// vb must be "infront" of va.
					if (Ileft(vap, va, vb, vertsa, vertsa, vertsb) && Ileft(va, van, vb, vertsa, vertsa, vertsb)) {
						int dx = vertsb[vb+0] - vertsa[va+0];
						int dz = (vertsb[vb+2]/voxelArea.width) - (vertsa[va+2]/voxelArea.width);
						int d = dx*dx + dz*dz;
						if (d < closestDist) {
							ia = i;
							ib = j;
							closestDist = d;
						}
					}
				}
			}
		}

		/** Releases contents of a contour set to caches */
		static void ReleaseContours (VoxelContourSet cset) {
			for (int i = 0; i < cset.conts.Count; i++) {
				VoxelContour cont = cset.conts[i];
				ArrayPool<int>.Release(ref cont.verts);
				ArrayPool<int>.Release(ref cont.rverts);
			}
			cset.conts = null;
		}

		public static bool MergeContours (ref VoxelContour ca, ref VoxelContour cb, int ia, int ib) {
			int maxVerts = ca.nverts + cb.nverts + 2;

			int[] verts = ArrayPool<int>.Claim(maxVerts*4);

			//if (!verts)
			//	return false;

			int nv = 0;

			// Copy contour A.
			for (int i = 0; i <= ca.nverts; i++) {
				int dst = nv*4;
				int src = ((ia+i) % ca.nverts)*4;
				verts[dst+0] = ca.verts[src+0];
				verts[dst+1] = ca.verts[src+1];
				verts[dst+2] = ca.verts[src+2];
				verts[dst+3] = ca.verts[src+3];
				nv++;
			}

			// Copy contour B
			for (int i = 0; i <= cb.nverts; i++) {
				int dst = nv*4;
				int src = ((ib+i) % cb.nverts)*4;
				verts[dst+0] = cb.verts[src+0];
				verts[dst+1] = cb.verts[src+1];
				verts[dst+2] = cb.verts[src+2];
				verts[dst+3] = cb.verts[src+3];
				nv++;
			}

			ArrayPool<int>.Release(ref ca.verts);
			ArrayPool<int>.Release(ref cb.verts);

			ca.verts = verts;
			ca.nverts = nv;

			cb.verts = ArrayPool<int>.Claim(0);
			cb.nverts = 0;

			return true;
		}

		public void SimplifyContour (List<int> verts, List<int> simplified, float maxError, int maxEdgeLenght, int buildFlags) {
			// Add initial points.
			bool hasConnections = false;

			for (int i = 0; i < verts.Count; i += 4) {
				if ((verts[i+3] & ContourRegMask) != 0) {
					hasConnections = true;
					break;
				}
			}

			if (hasConnections) {
				// The contour has some portals to other regions.
				// Add a new point to every location where the region changes.
				for (int i = 0, ni = verts.Count/4; i < ni; i++) {
					int ii = (i+1) % ni;
					bool differentRegs = (verts[i*4+3] & ContourRegMask) != (verts[ii*4+3] & ContourRegMask);
					bool areaBorders = (verts[i*4+3] & RC_AREA_BORDER) != (verts[ii*4+3] & RC_AREA_BORDER);

					if (differentRegs || areaBorders) {
						simplified.Add(verts[i*4+0]);
						simplified.Add(verts[i*4+1]);
						simplified.Add(verts[i*4+2]);
						simplified.Add(i);
					}
				}
			}


			if (simplified.Count == 0) {
				// If there is no connections at all,
				// create some initial points for the simplification process.
				// Find lower-left and upper-right vertices of the contour.
				int llx = verts[0];
				int lly = verts[1];
				int llz = verts[2];
				int lli = 0;
				int urx = verts[0];
				int ury = verts[1];
				int urz = verts[2];
				int uri = 0;

				for (int i = 0; i < verts.Count; i += 4) {
					int x = verts[i+0];
					int y = verts[i+1];
					int z = verts[i+2];
					if (x < llx || (x == llx && z < llz)) {
						llx = x;
						lly = y;
						llz = z;
						lli = i/4;
					}
					if (x > urx || (x == urx && z > urz)) {
						urx = x;
						ury = y;
						urz = z;
						uri = i/4;
					}
				}

				simplified.Add(llx);
				simplified.Add(lly);
				simplified.Add(llz);
				simplified.Add(lli);

				simplified.Add(urx);
				simplified.Add(ury);
				simplified.Add(urz);
				simplified.Add(uri);
			}

			// Add points until all raw points are within
			// error tolerance to the simplified shape.
			int pn = verts.Count/4;

			//Use the max squared error instead
			maxError *= maxError;

			for (int i = 0; i < simplified.Count/4; ) {
				int ii = (i+1) % (simplified.Count/4);

				int ax = simplified[i*4+0];
				int az = simplified[i*4+2];
				int ai = simplified[i*4+3];

				int bx = simplified[ii*4+0];
				int bz = simplified[ii*4+2];
				int bi = simplified[ii*4+3];

				// Find maximum deviation from the segment.
				float maxd = 0;
				int maxi = -1;
				int ci, cinc, endi;

				// Traverse the segment in lexilogical order so that the
				// max deviation is calculated similarly when traversing
				// opposite segments.
				if (bx > ax || (bx == ax && bz > az)) {
					cinc = 1;
					ci = (ai+cinc) % pn;
					endi = bi;
				} else {
					cinc = pn-1;
					ci = (bi+cinc) % pn;
					endi = ai;
					Memory.Swap(ref ax, ref bx);
					Memory.Swap(ref az, ref bz);
				}

				// Tessellate only outer edges or edges between areas.
				if ((verts[ci*4+3] & ContourRegMask) == 0 ||
					(verts[ci*4+3] & RC_AREA_BORDER) == RC_AREA_BORDER) {
					while (ci != endi) {
						float d2 = VectorMath.SqrDistancePointSegmentApproximate(verts[ci*4+0], verts[ci*4+2]/voxelArea.width, ax, az/voxelArea.width, bx, bz/voxelArea.width);

						if (d2 > maxd) {
							maxd = d2;
							maxi = ci;
						}
						ci = (ci+cinc) % pn;
					}
				}

				// If the max deviation is larger than accepted error,
				// add new point, else continue to next segment.
				if (maxi != -1 && maxd > maxError) {
					// Add space for the new point.
					//simplified.resize(simplified.size()+4);
					simplified.Add(0);
					simplified.Add(0);
					simplified.Add(0);
					simplified.Add(0);

					int n = simplified.Count/4;

					for (int j = n-1; j > i; --j) {
						simplified[j*4+0] = simplified[(j-1)*4+0];
						simplified[j*4+1] = simplified[(j-1)*4+1];
						simplified[j*4+2] = simplified[(j-1)*4+2];
						simplified[j*4+3] = simplified[(j-1)*4+3];
					}
					// Add the point.
					simplified[(i+1)*4+0] = verts[maxi*4+0];
					simplified[(i+1)*4+1] = verts[maxi*4+1];
					simplified[(i+1)*4+2] = verts[maxi*4+2];
					simplified[(i+1)*4+3] = maxi;
				} else {
					i++;
				}
			}



			//Split too long edges

			float maxEdgeLen = maxEdgeLength / cellSize;

			if (maxEdgeLen > 0 && (buildFlags & (RC_CONTOUR_TESS_WALL_EDGES|RC_CONTOUR_TESS_AREA_EDGES|RC_CONTOUR_TESS_TILE_EDGES)) != 0) {
				for (int i = 0; i < simplified.Count/4; ) {
					if (simplified.Count/4 > 200) {
						break;
					}

					int ii = (i+1) % (simplified.Count/4);

					int ax = simplified[i*4+0];
					int az = simplified[i*4+2];
					int ai = simplified[i*4+3];

					int bx = simplified[ii*4+0];
					int bz = simplified[ii*4+2];
					int bi = simplified[ii*4+3];

					// Find maximum deviation from the segment.
					int maxi = -1;
					int ci = (ai+1) % pn;

					// Tessellate only outer edges or edges between areas.
					bool tess = false;

					// Wall edges.
					if ((buildFlags & RC_CONTOUR_TESS_WALL_EDGES) != 0 && (verts[ci*4+3] & ContourRegMask) == 0)
						tess = true;

					// Edges between areas.
					if ((buildFlags & RC_CONTOUR_TESS_AREA_EDGES) != 0 && (verts[ci*4+3] & RC_AREA_BORDER) == RC_AREA_BORDER)
						tess = true;

					// Border of tile
					if ((buildFlags & RC_CONTOUR_TESS_TILE_EDGES) != 0 && (verts[ci*4+3] & BorderReg) == BorderReg)
						tess = true;

					if (tess) {
						int dx = bx - ax;
						int dz = (bz/voxelArea.width) - (az/voxelArea.width);
						if (dx*dx + dz*dz > maxEdgeLen*maxEdgeLen) {
							// Round based on the segments in lexilogical order so that the
							// max tesselation is consistent regardles in which direction
							// segments are traversed.
							int n = bi < ai ? (bi+pn - ai) : (bi - ai);
							if (n > 1) {
								if (bx > ax || (bx == ax && bz > az)) {
									maxi = (ai + n/2) % pn;
								} else {
									maxi = (ai + (n+1)/2) % pn;
								}
							}
						}
					}

					// If the max deviation is larger than accepted error,
					// add new point, else continue to next segment.
					if (maxi != -1) {
						// Add space for the new point.
						//simplified.resize(simplified.size()+4);
						simplified.AddRange(new int[4]);

						int n = simplified.Count/4;
						for (int j = n-1; j > i; --j) {
							simplified[j*4+0] = simplified[(j-1)*4+0];
							simplified[j*4+1] = simplified[(j-1)*4+1];
							simplified[j*4+2] = simplified[(j-1)*4+2];
							simplified[j*4+3] = simplified[(j-1)*4+3];
						}
						// Add the point.
						simplified[(i+1)*4+0] = verts[maxi*4+0];
						simplified[(i+1)*4+1] = verts[maxi*4+1];
						simplified[(i+1)*4+2] = verts[maxi*4+2];
						simplified[(i+1)*4+3] = maxi;
					} else {
						++i;
					}
				}
			}

			for (int i = 0; i < simplified.Count/4; i++) {
				// The edge vertex flag is take from the current raw point,
				// and the neighbour region is take from the next raw point.
				int ai = (simplified[i*4+3]+1) % pn;
				int bi = simplified[i*4+3];
				simplified[i*4+3] = (verts[ai*4+3] & ContourRegMask) | (verts[bi*4+3] & RC_BORDER_VERTEX);
			}
		}

		public void WalkContour (int x, int z, int i, ushort[] flags, List<int> verts) {
			// Choose the first non-connected edge
			int dir = 0;

			while ((flags[i] & (ushort)(1 << dir)) == 0) {
				dir++;
			}

			int startDir = dir;
			int startI = i;

			int area = voxelArea.areaTypes[i];

			int iter = 0;

			#if ASTARDEBUG
			Vector3 previousPos;
			Vector3 currentPos;

			previousPos = ConvertPos(
				x,
				0,
				z
				);

			Vector3 previousPos2 = ConvertPos(
				x,
				0,
				z
				);
			#endif

			while (iter++ < 40000) {
				//Are we facing a region edge
				if ((flags[i] & (ushort)(1 << dir)) != 0) {
					#if ASTARDEBUG
					Vector3 pos = ConvertPos(x, 0, z)+new Vector3((voxelArea.DirectionX[dir] != 0) ? Mathf.Sign(voxelArea.DirectionX[dir]) : 0, 0, (voxelArea.DirectionZ[dir]) != 0 ? Mathf.Sign(voxelArea.DirectionZ[dir]) : 0)*0.6F;
					//int dir2 = (dir+1) & 0x3;
					//pos += new Vector3 ((voxelArea.DirectionX[dir2] != 0) ? Mathf.Sign(voxelArea.DirectionX[dir2]) : 0,0,(voxelArea.DirectionZ[dir2]) != 0 ? Mathf.Sign(voxelArea.DirectionZ[dir2]) : 0)*1.2F;

					//Debug.DrawLine (ConvertPos (x,0,z),pos,Color.cyan);
					Debug.DrawLine(previousPos2, pos, Color.blue);
					previousPos2 = pos;
					#endif

					//Choose the edge corner
					bool isBorderVertex = false;
					bool isAreaBorder = false;

					int px = x;
					int py = GetCornerHeight(x, z, i, dir, ref isBorderVertex);
					int pz = z;

					switch (dir) {
					case 0: pz += voxelArea.width;; break;
					case 1: px++; pz += voxelArea.width; break;
					case 2: px++; break;
					}

					/*case 1: px++; break;
					 *  case 2: px++; pz += voxelArea.width; break;
					 *  case 3: pz += voxelArea.width; break;
					 */

					int r = 0;
					CompactVoxelSpan s = voxelArea.compactSpans[i];

					if (s.GetConnection(dir) != NotConnected) {
						int nx = x + voxelArea.DirectionX[dir];
						int nz = z + voxelArea.DirectionZ[dir];
						int ni = (int)voxelArea.compactCells[nx+nz].index + s.GetConnection(dir);
						r = (int)voxelArea.compactSpans[ni].reg;

						if (area != voxelArea.areaTypes[ni]) {
							isAreaBorder = true;
						}
					}

					if (isBorderVertex) {
						r |= RC_BORDER_VERTEX;
					}
					if (isAreaBorder) {
						r |= RC_AREA_BORDER;
					}

					verts.Add(px);
					verts.Add(py);
					verts.Add(pz);
					verts.Add(r);

					//Debug.DrawRay (previousPos,new Vector3 ((dir == 1 || dir == 2) ? 1 : 0, 0, (dir == 0 || dir == 1) ? 1 : 0),Color.cyan);

					flags[i] = (ushort)(flags[i] & ~(1 << dir)); // Remove visited edges

					dir = (dir+1) & 0x3;  // Rotate CW
				} else {
					int ni = -1;
					int nx = x + voxelArea.DirectionX[dir];
					int nz = z + voxelArea.DirectionZ[dir];

					CompactVoxelSpan s = voxelArea.compactSpans[i];

					if (s.GetConnection(dir) != NotConnected) {
						CompactVoxelCell nc = voxelArea.compactCells[nx+nz];
						ni = (int)nc.index + s.GetConnection(dir);
					}

					if (ni == -1) {
						Debug.LogWarning("Degenerate triangles might have been generated.\n" +
							"Usually this is not a problem, but if you have a static level, try to modify the graph settings slightly to avoid this edge case.");
						return;
					}
					x = nx;
					z = nz;
					i = ni;

					// & 0x3 is the same as % 4 (modulo 4)
					dir = (dir+3) & 0x3;    // Rotate CCW

					#if ASTARDEBUG
					currentPos = ConvertPos(
						x,
						0,
						z
						);

					Debug.DrawLine(previousPos+Vector3.up*0, currentPos, Color.blue);
					previousPos = currentPos;
					#endif
				}

				if (startI == i && startDir == dir) {
					break;
				}
			}

			#if ASTARDEBUG
			Color col = new Color(Random.value, Random.value, Random.value);

			for (int q = 0, j = (verts.Count/4)-1; q < (verts.Count/4); j = q, q++) {
				int i4 = q*4;
				int j4 = j*4;

				Vector3 p1 = ConvertPosWithoutOffset(
					verts[i4+0],
					verts[i4+1],
					verts[i4+2]
					);

				Vector3 p2 = ConvertPosWithoutOffset(
					verts[j4+0],
					verts[j4+1],
					verts[j4+2]
					);

				Debug.DrawLine(p1, p2, col);
			}
			#endif
		}

		public int GetCornerHeight (int x, int z, int i, int dir, ref bool isBorderVertex) {
			CompactVoxelSpan s = voxelArea.compactSpans[i];

			int ch = (int)s.y;

			//dir + clockwise direction
			int dirp = (dir+1) & 0x3;

			//int dirp = (dir+3) & 0x3;

			uint[] regs = new uint[4];

			regs[0] = (uint)voxelArea.compactSpans[i].reg | ((uint)voxelArea.areaTypes[i] << 16);

			if (s.GetConnection(dir) != NotConnected) {
				int nx = x + voxelArea.DirectionX[dir];
				int nz = z + voxelArea.DirectionZ[dir];
				int ni = (int)voxelArea.compactCells[nx+nz].index + s.GetConnection(dir);

				CompactVoxelSpan ns = voxelArea.compactSpans[ni];

				ch = System.Math.Max(ch, (int)ns.y);
				regs[1] = (uint)ns.reg | ((uint)voxelArea.areaTypes[ni] << 16);

				if (ns.GetConnection(dirp) != NotConnected) {
					int nx2 = nx + voxelArea.DirectionX[dirp];
					int nz2 = nz + voxelArea.DirectionZ[dirp];
					int ni2 = (int)voxelArea.compactCells[nx2+nz2].index + ns.GetConnection(dirp);

					CompactVoxelSpan ns2 = voxelArea.compactSpans[ni2];

					ch = System.Math.Max(ch, (int)ns2.y);
					regs[2] = (uint)ns2.reg | ((uint)voxelArea.areaTypes[ni2] << 16);
				}
			}

			if (s.GetConnection(dirp) != NotConnected) {
				int nx = x + voxelArea.DirectionX[dirp];
				int nz = z + voxelArea.DirectionZ[dirp];
				int ni = (int)voxelArea.compactCells[nx+nz].index + s.GetConnection(dirp);

				CompactVoxelSpan ns = voxelArea.compactSpans[ni];

				ch = System.Math.Max(ch, (int)ns.y);
				regs[3] = (uint)ns.reg | ((uint)voxelArea.areaTypes[ni] << 16);

				if (ns.GetConnection(dir) != NotConnected) {
					int nx2 = nx + voxelArea.DirectionX[dir];
					int nz2 = nz + voxelArea.DirectionZ[dir];
					int ni2 = (int)voxelArea.compactCells[nx2+nz2].index + ns.GetConnection(dir);

					CompactVoxelSpan ns2 = voxelArea.compactSpans[ni2];

					ch = System.Math.Max(ch, (int)ns2.y);
					regs[2] = (uint)ns2.reg | ((uint)voxelArea.areaTypes[ni2] << 16);
				}
			}

			// Check if the vertex is special edge vertex, these vertices will be removed later.
			for (int j = 0; j < 4; ++j) {
				int a = j;
				int b = (j+1) & 0x3;
				int c = (j+2) & 0x3;
				int d = (j+3) & 0x3;

				// The vertex is a border vertex there are two same exterior cells in a row,
				// followed by two interior cells and none of the regions are out of bounds.
				bool twoSameExts = (regs[a] & regs[b] & BorderReg) != 0 && regs[a] == regs[b];
				bool twoInts = ((regs[c] | regs[d]) & BorderReg) == 0;
				bool intsSameArea = (regs[c]>>16) == (regs[d]>>16);
				bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
				if (twoSameExts && twoInts && intsSameArea && noZeros) {
					isBorderVertex = true;
					break;
				}
			}

			return ch;
		}

		public void RemoveDegenerateSegments (List<int> simplified) {
			// Remove adjacent vertices which are equal on xz-plane,
			// or else the triangulator will get confused
			for (int i = 0; i < simplified.Count/4; i++) {
				int ni = i+1;
				if (ni >= (simplified.Count/4))
					ni = 0;

				if (simplified[i*4+0] == simplified[ni*4+0] &&
					simplified[i*4+2] == simplified[ni*4+2]) {
					// Degenerate segment, remove.
					simplified.RemoveRange(i, 4);
				}
			}
		}

		public int CalcAreaOfPolygon2D (int[] verts, int nverts) {
			int area = 0;

			for (int i = 0, j = nverts-1; i < nverts; j = i++) {
				int vi = i*4;
				int vj = j*4;
				area += verts[vi+0] * (verts[vj+2]/voxelArea.width) - verts[vj+0] * (verts[vi+2]/voxelArea.width);
			}

			return (area+1) / 2;
		}

		public static bool Ileft (int a, int b, int c, int[] va, int[] vb, int[] vc) {
			return (vb[b+0] - va[a+0]) * (vc[c+2] - va[a+2]) - (vc[c+0] - va[a+0]) * (vb[b+2] - va[a+2]) <= 0;
		}
	}
}
