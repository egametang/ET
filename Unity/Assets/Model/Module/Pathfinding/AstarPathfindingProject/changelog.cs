/** \page changelog Changelog
\order{-10}

- 4.1.16 (2018-04-26)
	- Fixed PointNode.ContainsConnection could throw an exception if the node didn't have any connections.
	- Fixed AILerp's started out with a destination set to (0,0,0) instead of not having a destination set.
		So if you did not set a destination for it, it would try to move to the world origin.

- 4.1.15 (2018-04-06)
	- Fixed RichAI.desiredVelocity always being zero. Thanks sukrit1234 for finding the bug.
	- Added some video examples to \link Pathfinding.AIPath.pickNextWaypointDist AIPath.pickNextWaypointDist\endlink.
	- Fixed a bug introduced in 4.1.14 which caused scanning recast graphs in the Unity editor to fail with an error sometimes.
	- Fixed the position returned from querying the closest point on the graph to a point (AstarPath.GetNearest) on layered grid graphs would always be the node center, not the closest point on the node's surface. Thanks Kevin_Jenkins for reporting this.
		This caused among other things the ClosestOnNode option for the Seeker's StartEndModifier to be identical to the SnapToNode option.
	- Fixed RVOController.velocity being zero when the game was paused (Time.timeScale = 0).

- 4.1.14 (2018-03-06)
	- Fixed Pathfinding.GridNode.ClosestPointOnNode being completely broken. Thanks Ivan for reporting this.
		This was used internally in some cases when pathfinding on grid graphs. So this fixes a few cases of strange pathfinding results too.
	- It is now possible to use pathfinding from editor scripts. See \ref editor-mode.

- 4.1.13 (2018-03-06)
	- Fixed LayerGridGraph.GetNode not performing out of bounds checks.
	- Exposed a public method \link Pathfinding.PointGraph.ConnectNodes PointGraph.ConnectNodes\endlink which can be useful if you are creating a graph from scratch using e.g PointGraph.AddNode.
	- Improved the \ref multiple-agent-types tutorial.
	- Improved the \ref custom_movement_script tutorial, among other things it can now also be followed if you are creating a 2D game.
		The movement script that you write has also been improved.
	- Improved how the RichAI movement script keeps track of the node it is on. It should now be more stable in some cases, especially when the ground's y-coordinate lines up badly with the y-coordinate of the navmesh.
	- Added an \link Pathfinding.AIPath.constrainInsideGraph option\endlink to AIPath for constraining the agent to be inside the traversable surface of the graph at all times.
		I think it should work everywhere without any issues, but please post in the forum if anything seems to break.
	- Fixed the proper fonts were not imported in the documentation html, so for many browsers it fell back to some other less pretty font.

- 4.1.12 (2018-02-27)
	- Fixed right clicking on array elements in the Unity inspector would bring up the 'Show in online documentation' context menu instead of the Unity built-in context menu (which is very useful).
	- Navmesh assets used in the navmesh graph no longer have to be at the root of the Resources folder, they can be in any subfolder to the Resources folder.

- 4.1.11 (2018-02-22)
	- You can now set which graphs an agent should use directly on the Seeker component instead of having to do it through code.
		\shadowimage{multiple_agents/seeker.png}
	- Added tutorial for how to deal with agents of different sizes: \ref multiple-agent-types.
	- Fixed scanning recast graphs could in rare cases throw an exception due to a multithreading race condition. Thanks emrys90 for reporting the bug.
	- Fixed a regression in 4.0.6 which caused position based penalty to stop working for layered grid graphs. Thanks DougW for reporting the bug.
	- Rotation speed and acceleration are now decoupled for AIPath and RichAI. Previously the acceleration limited how quickly the agents could rotate due to how the math for <a href="https://en.wikipedia.org/wiki/Centripetal_force">centripetal acceleration</a> works out.
	- Acceleration can now be set to a custom value on the AIPath class. It defaults to a 'Default' mode which calculates an acceleration such that the agent reaches its top speed in about 0.4 seconds. This is the same behaviour that was hardcoded in earlier versions.
	- Fixed a bug in \link Pathfinding.GraphUtilities.GetContours GraphUtilities.GetContours\endlink for grid graphs when the nodes parameter was explicitly passed as non null that could cause some contours not to be generated. Thanks andrewBeers for reporting the bug.
	- Improved documentation for \link Pathfinding.StartEndModifier.Exactness StartEndModifier.Exactness\endlink.

- 4.1.10 (2018-01-21)
	- 4.1.0 through 4.1.9 were beta versions, all their changelogs have been merged into this one.
	- Upgrade notes
		- Fixed the AIPath script with rotationIn2D would rotate so that the Z axis pointed in the -Z direction instead of as is common for Unity 2D objects: to point in the +Z direction.
		- ALL classes are now inside the Pathfinding namespace to reduce potential naming collisions with other packages.
			Make sure you have "using Pathfinding;" at the top of your scripts.
			Previously most scripts have been inside the Pathfinding namespace, but not all of them.
			The exception is the AstarPath script to avoid breaking too much existing code (and it has a very distinctive name so name collisions are not likely).
		- Since the API for several movement scripts have been unified (see below), many members of the movement scripts have been deprecated.
			Your code should continue to work exactly as before (except bugs of course, but if some other behaviour is broken, please start a thread in the forum) but you may get deprecation warnings.
			In most cases the changes should be very easy to make as the visible changes mostly consist of renames.
		- A method called \link Pathfinding.IAstarAI.SetPath SetPath\endlink has been added to all movement scripts. This replaces some hacks you could achieve by calling the OnPathComplete method on the movement scripts
			from other scripts. If you have been doing that you should now call SetPath instead.
		- Paths calculated with a heuristic scale greater than 1 (the default is 1) might be slightly less optimal compared to before.
			See below for more information.
		- The StartEndModifier's raycasting options are now only used if the 'Original' snapping option is used as that's the only one it makes sense for.
		- The RaycastModifier has changed a bit, so your paths might look slightly different, however in all but very rare cases it should be at least as good as in previous versions.
		- Linecast methods will now assign the #Pathfinding.GraphHitInfo.node field with the last node that was traversed in case no obstacle was hit, previously it was always null.
		- Multithreading is now enabled by default (1 thread). This may affect you if you have been adding the AstarPath component during runtime using a script, though the change is most likely positive.
		- The DynamicGridObstacle component will now properly update the graph when the object is deactivated since the object just disappeared and shouldn't block the graph anymore.
			Previously it only did this if the object was destroyed, not if it was deactivated.
		- If you have written a custom graph type you may have to change the access modifier on some methods.
			For example the ScanInternal method has been changed from being public to being protected.
		- Some internal methods on graphs have been hidden. They should never have been used by user code
			but in case you have done that anyway you will have to access them using the IGraphInternals or IUpdatableGraph interface now.
		- Removed some compatibility code for Seekers for when upgrading from version 3.6.7 and earlier (released about 2 years ago).
			If you are upgrading from a version that old then the 'Valid Tags' field on the Seeker component may get reset to the default value.
			If you did not use that field then you will not have to do anything.
		- AIPath now rotates towards actual movement direction when RVO is used.
	- Improvements
		- Improved pathfinding performance by around 8% for grid graphs, possibly more for other graph types.
			This involved removing a special case for when the pathfinding heuristic is not <a href="https://en.wikipedia.org/wiki/Admissible_heuristic">admissable</a> (in short, when A* Inspector -> Settings -> Heuristic Scale was greater than 1).
			Now paths calculated with the heuristic scale greater than 1 might be slightly less optimal compared to before.
			If this is important I suggest you reduce the heuristic scale to compensate.
			Note that as before: a heuristic scale of 1 is the default and if it is greater than 1 then the calculated paths may no longer be the shortest possible ones.
		- Improved overall pathfinding performance by an additional 10-12% by heavily optimizing some core algorithms.
		- Improved performance of querying for the closest node to a point when using the PointGraph and \link Pathfinding.PointGraph.optimizeForSparseGraph optimizeForSparseGraph\endlink.
			The improvements are around 7%.
		- Unified the API for the included movement scripts (AIPath, RichAI, AILerp) and added a large number of nice properties and functionality.
			- The \link Pathfinding.IAstarAI IAstarAI\endlink interface can now be used with all movement scripts.
			- To make it easier to migrate from Unity's navmesh system, this interface has been designed to be similar to Unity's NavmeshAgent API.
			- The interface has several nice properties like:
				\link Pathfinding.IAstarAI.remainingDistance remainingDistance\endlink,
				\link Pathfinding.IAstarAI.reachedEndOfPath reachedEndOfPath\endlink,
				\link Pathfinding.IAstarAI.pathPending pathPending\endlink,
				\link Pathfinding.IAstarAI.steeringTarget steeringTarget\endlink,
				\link Pathfinding.IAstarAI.isStopped isStopped\endlink,
				\link Pathfinding.IAstarAI.destination destination\endlink, and many more.
			- You no longer need to set the destination of an agent using a Transform object, instead you can simply set the \link Pathfinding.IAstarAI.destination destination\endlink property.
				Note that when you upgrade, a new AIDestinationSetter component will be automatically created which has a 'target' field. So your existing code will continue to work.
		- Improved behavior when AIPath/RichAI characters move down slopes.
			Previously the way gravity was handled could sometimes lead to a 'bouncing' behavior unless the gravity was very high. Old behavior on the left, new behavior on the right.
			\htmlonly <video class="tinyshadow" controls loop><source src="images/changelog/ai_slope.mp4" type="video/mp4"></video> \endhtmlonly
		- Improved the grid graph inspector by adding preconfigured modes for different node shapes: square grids, isometric grids and hexagons.
			This also reduces clutter in the inspector since irrelevant options can be hidden.
			\shadowimage{changelog/grid_shape.png}
		- For 2D grid graphs the inspector will now show a single rotation value instead of a full 3D rotation which makes it a lot easier to configure.
		- Improved the performance of the \link Pathfinding.RaycastModifier RaycastModifier\endlink significantly. Common speedups on grid graphs range from 2x to 10x.
		- The RaycastModifier now has a \link Pathfinding.RaycastModifier.Quality quality enum\endlink. The higher quality options use a new algorithm that is about the same performance (or slightly slower) compared to the RaycastModifier in previous versions
			however it often manages to simplify the path a lot more.
			The quality of the previous RaycastModifier with default settings corresponds to somewhere between the Low and Medium qualities.
		- Improved support for HiDPI (retina) screens as well as improved visual coherency for some icons.
			\shadowimage{changelog/retina_icons.png}
		- Improved the 'eye' icon for when a graph's gizmos are disabled to make it easier to spot.
		- Added \link Pathfinding.GridGraph.CalculateConnectionsForCellAndNeighbours GridGraph.CalculateConnectionsForCellAndNeighbours\endlink.
		- AIPath now works with point graphs in 2D as well (assuming the 'rotate in 2D' checkbox is enabled).
		- Improved the performance of the RVONavmesh component when used together with navmesh cutting, especially when many navmesh cuts are moving at the same time.
		- A warning is now displayed in the editor if one tries to use both the AIDestinationSetter and Patrol components on an agent at the same time.
		- Improved linecasts on recast/navmesh graphs. They are now more accurate (there were some edge cases that previously could cause it to fail) and faster.
			Performance has been improved by by around 3x for longer linecasts and 1.4x for shorter ones.
		- Linecast methods will now assign the #Pathfinding.GraphHitInfo.node field with the last node that was traversed in case no obstacle was hit.
		- Linecast on graphs now set the hit point to the endpoint of the line if no obstacle was hit. Previously the endpoint would be set to Vector3.zero. Thanks borluse for suggesting this.
		- Multithreading is now enabled by default (1 thread).
		- The DynamicGridObstacle component now works with 2D colliders.
		- Clicking on the graph name in the inspector will no longer focus the name text field.
			To edit the graph name you will now have to click the Edit/Pen button to the right of the graph name.
			Previously it was easy to focus the text field by mistake when you actually wanted to show the graph settings.
			\shadowimage{changelog/edit_icon.png}
		- Reduced memory usage of the PointGraph when using \link Pathfinding.PointGraph.optimizeForSparseGraph optimizeForSparseGraph\endlink.
		- Improved the StartEndModifier inspector slightly.
		- The Seeker inspector now has support for multi-editing.
		- The AIPath and RichAI scripts now rotate to face the direction they are actually moving with when using local avoidance (RVO)
			instead of always facing the direction they want to move with. At very low speeds they fall back to looking the direction they want to move with to avoid jitter.
		- Improved the Seeker inspector. Unified the UI for setting tag penalties and determining if a tag should be traversable.
			\shadowimage{changelog/seeker_tags.png}
		- Reduced string allocations for error messages when paths fail.
		- Added support for 2D physics to the #Pathfinding.RaycastModifier component.
		- Improved performance of GraphUpdateObjects with updatePhysics=false on rotated navmesh/recast graphs.
		- Improved the inspector for AILerp.
		- RVO obstacles can now be visualized by enabling the 'Draw Obstacles' checkbox on the RVOSimulator component.
			\shadowimage{changelog/rvo_navmesh_obstacle.png}
		- Reduced allocations in the funnel modifier.
		- Added a 'filter' parameter to \link Pathfinding.PathUtilities.BFS PathUtilities.BFS\endlink and \link Pathfinding.PathUtilities.GetReachableNodes PathUtilities.GetReachableNodes\endlink.
		- Added a method called \link Pathfinding.IAstarAI.SetPath SetPath\endlink to all movement scripts.
		- Added \link Pathfinding.GraphNode.Graph GraphNode.Graph\endlink.
		- Added #Pathfinding.MeshNode.ContainsPoint(Vector3) in addition to the already existing MeshNode.ContainsPoint(Int3).
		- Added #Pathfinding.MeshNode.ContainsPointInGraphSpace.
		- Added #Pathfinding.TriangleMeshNode.GetVerticesInGraphSpace.
		- Added Pathfinding.AstarData.FindGraph(predicate).
		- Added Pathfinding.AstarData.FindGraphWhichInheritsFrom(type).
		- Added a new class \link Pathfinding.GraphUtilities GraphUtilities\endlink which has some utilities for extracting contours of graphs.
		- Added a new method \link Pathfinding.GridGraph.Linecast(GridNodeBase,GridNodeBase) Linecast(GridNodeBase,GridNodeBase)\endlink to the GridGraph class which is much faster than the normal Linecast methods.
		- Added \link Pathfinding.GridGraph.GetNode(int,int) GridGraph.GetNode(int,int)\endlink.
		- Added MeshNode.AddConnection(node,cost,edge) in addition to the already existing AddConnection(node,cost) method.
		- Added a \link Pathfinding.NavMeshGraph.recalculateNormals\endlink setting to the navmesh graph for using the original mesh normals. This is useful for spherical/curved worlds.
	- Documentation
		- Added a documentation page on error messages: \ref error-messages.
		- Added a tutorial on how to create a wandering AI: \ref wander.
		- Added tutorial on bitmasks: \ref bitmasks.
		- You can now right-click on most fields in the Unity Inspector to bring up a link to the online documentation.
			\shadowimage{inspector_doc_links.png}
		- Various other documentation improvements and fixes.
	- Changes
		- Height or collision testing for grid graphs now never hits triggers, regardless of the Unity Physics setting 'Queries Hit Triggers'
		which has previously controlled this.
		- Seeker.StartPath will no longer overwrite the path's graphMask unless it was explicitly passed as a parameter to the StartPath method.
		- The built in movement scripts no longer uses a coroutine for scheduling path recalculations.
			This shouldn't have any impact for you unless you have been modifying those scripts.
		- Replaced the MineBotAI script that has been used in the tutorials with MineBotAnimation.
			The new script does not inherit from AIPath so in the example scenes there is now one AIPath component and one MineBotAnimation script on each unit.
		- Removed prompt to make the package support UnityScript which would show up the first time you used the package in a new project.
			Few people use UnityScript nowadays so that prompt was mostly annoying. UnityScript support can still be enabled, see \ref javscript.
		- If deserialization fails, the graph data will no longer be stored in a backup byte array to be able to be recovered later.
			This was not very useful, but more importantly if the graph data was very large (several megabytes) then Unity's Undo system would choke on it
			and essentially freeze the Unity editor.
		- The StartEndModifier's raycasting options are now only used if the 'Original' snapping option is used as that's the only one it makes sense for.
		- The RaycastModifier.subdivideEveryIter field has been removed, this is now always enabled except for the lowest quality setting.
		- The RaycastModifier.iterations field has been removed. The number of iterations is now controlled by the quality field.
			Unfortunately this setting cannot be directly mapped to a quality value, so if you are upgrading all RaycastModifier components will use the quality Medium after the upgrade.
		- The default value for \link Pathfinding.RVOController.lockWhenNotMoving RVOController.lockWhenNotMoving\endlink is now false.
		- Tiles are now enabled by default on recast graphs.
		- Modifiers now register/unregister themselves with the Seeker component during OnEnable/OnDisable instead of Awake/OnDestroy.
			If you have written any custom modifiers which defines those methods you may have to add the 'override' modifier to those methods and call base.OnEnable/OnDisable.
		- When paths fail this is now always logged as a warning in the Unity console instead of a normal log message.
		- Node connections now store which edge of the node shape that is used for that node. This is used for navmesh/recast graphs.
		- The \link Pathfinding.RVOController.velocity velocity\endlink property on the RVOController can now be assigned to and that has the same effect as calling ForceSetVelocity.
		- Deprecated the RVOController.ForceSetVelocity method. You should use the velocity property instead.
		- All graphs now explicitly implement the IUpdatableGraph interface.
			This is done to hide those methods (which should not be used directly) and thereby reduce confusion about which methods should be used to update graphs.
		- Hid several internal methods behind the IGraphInternals interface to reduce clutter in the documentation and IntelliSense suggestions.
		- Removed NavGraph.UnloadGizmoMeshes because it was not used for anything.
		- Since 4.0 individual graphs can be scanned using AstarPath.Scan. The older NavGraph.Scan method now redirects to that method
			which is more robust. This may cause slight changes in behavior, however the recommendation in the documentation has always been to use AstarPath.Scan anyway
			so I do not expect many to have used the NavGraph.Scan method.
		- Deprecated the NavGraph.ScanGraph method since it just does the same thing as NavGraph.Scan.
		- Deprecated the internal methods Path.LogError and Path.Log.
		- Added the new internal method Path.FailWithError which replaces LogError and Log.
		- Made the AIPath.TrySearchPath method private, it should never have been public to begin with.
	- Fixes
		- Fixed AIPath/RichAI throwing exceptions in the Unity Editor when drawing gizmos if the game starts while they are enabled in a disabled gameObject.
		- Fixed some typos in the documentation for PathUtilities.BFS and PathUtilities.GetReachableNodes.
		- For some point graph settings, path requests to points that could not be reached would fail completely instead of going to the closest node that it could reach. Thanks BYELIK for reporting this bug.
			If you for some reason have been relying on the old buggy behavior you can emulate it by setting A* Inspector -> Settings -> Max Nearest Node Distance to a very low value.
		- Fixed connection costs were assumed to be equal in both directions for bidirectional connections.
		- Fixed a compiler error when building for UWP/HoloLens.
		- Fixed some cases where circles used for debugging could have a much lower resolution than intended (#Pathfinding.Util.Draw.Debug.CircleXZ).
		- Fixed RVO agents which were locked but some script sent it movement commands would cause the RVO system to think it was moving
			even though it was actually stationary, causing some odd behavior. Now locked agents are always treated as stationary.
		- Fixed RVO obstacles generated from graph borders (using the RVONavmesh component) could be incorrect if a tiled recast graph and navmesh cutting was used.
			The bug resulted in an RVO obstacle around the tile that was most recently updated by a navmesh cut even where there should be no obstacle.
		- Fixed the RVONavmesh component could throw an exception in some cases when using tiled recast graphs.
		- Fixed a regression in some 4.0.x version where setting \link Pathfinding.RVOController.velocity RVOController.velocity\endlink to make the agent's movement externally controlled
			would not work properly (the system would always think the agent had a velocity of zero).
		- Fixed the RichAI movement script could sometimes get stuck on the border between two tiles.
			(due to a possibility of division by zero that would cause its velocity to become NaN).
		- Fixed AIPath/RichAI movement not working properly with rigidbodies in Unity 2017.3+ when the new physics setting "Auto Sync Transforms" was disabled. Thanks DougW for reporting this and coming up with a fix.
		- Fixed a few cases where \link Pathfinding.RichAI RichAI\endlink would automatically recalculate its path even though \link Pathfinding.RichAI.canSearch canSearch\endlink was disabled.
		- Fixed some compiler warnings when using Unity 2017.3 or later.
		- Fixed graphical artifacts in the graph visualization line drawing code which could show up at very large coordinate values or steep viewing angles.
			Differential calculus can be really useful sometimes.
		- Fixed the \ref MultiTargetPathExample.cs.
		- Fixed the width/depth fields in the recast graph inspector causing warnings to be logged (introduced in 4.1.7). Thanks NoxMortem for reporting this.
		- Fixed the Pathfinding.GraphHitInfo.tangentOrigin field was offset by half a node when using linecasting on grid graphs.
		- Fixed the AIPath script with rotationIn2D would rotate so that the Z axis pointed in the -Z direction instead of as is common for Unity 2D objects: to point in the +Z direction.
		- Fixed the AILerp script with rotationIn2D would rotate incorrectly if it started out with the Z axis pointed in the -Z direction.
		- Clamp recast graph bounding box size to be non-zero on all axes.
		- The DynamicGridObstacle component will now properly update the graph when the object is deactivated since the object just disappeared and shouldn't block the graph anymore.
			Previously it only did this if the object was destroyed, not if it was deactivated.
		- Fixed \link Pathfinding.AILerp AILerp\endlink ceasing to work properly if one of the paths it tries to calculate fails.
		- Fixed the \link Pathfinding.FunnelModifier FunnelModifier\endlink could yield a zero length path in some rare circumstances when using custom node links.
			This could lead to an exception in some of the movement scripts. Thanks DougW for reporting the bug.
		- Fixed calling Seeker.CancelCurrentPathRequest could in some cases cause an exception to be thrown due to multithreading race conditions.
		- Fixed a multithreading race condition which could cause a path canceled by Seeker.CancelCurrentPathRequest to not actually be canceled.
		- Fixed a rare ArrayOutOfBoundsException when using the FunnelModifier with the 'unwrap' option enabled.
		- Fixed Seeker -> Start End Modifier could not be expanded in the Unity inspector. Thanks Dee_Lucky for reporting this.
		- Fixed a few compatiblity bugs relating to AIPath/RichAI that were introduced in 4.1.0.
		- Fixed funnel modifier could sometimes fail if the agent started exactly on the border between two nodes.
		- Fixed another bug which could cause the funnel modifier to produce incorrect results (it was checking for colinearity of points in 2D instead of in 3D).
		- Fixed the funnel modifier would sometimes clip a corner near the end of the path.
		- Fixed ProceduralGridMover would not detect user defined graphs that were subclasses of the GridGraph class. Thanks viveleroi for reporting this.
		- Fixed enabling and disabling a AIPath or RichAI component a very large number of times could potentially have a negative performance impact.
		- Fixed AIPath/RichAI would continue searching for paths even when the component had been disabled.
		- MeshNode.ContainsPoint now supports rotated graphs properly. MeshNode is used in navmesh and recast graphs.
		- Fixed Linecast for navmesh and recast graphs not working for rotated graphs.
		- Fixed RVONavmesh component not working properly with grid graphs that had height differences.
		- Fixed 2D RVO agents sometimes ignoring obstacles.
		- Fixed RVONavmesh not removing the obstacles it had created when the component was disabled.
		- Fixed RaycastModifier could miss obstacles when thick raycasting was used due to Unity's Physics.SphereCast method not
			reporting hits very close to the start of the raycast.
		- In the free version the inspector for RaycastModifier now displays a warning if graph raycasting is enabled since
			for all built-in graphs raycasts are only supported in the pro version.
		- Fixed some cases where the funnel modifier would produce incorrect results.
		- Fixed typo in a private method in the AstarPath class. Renamed the UpdateGraphsInteral method to UpdateGraphsInternal.
		- Fixed AIPath.remainingDistance and AIPath.targetReached could be incorrect for 1 frame when a new path had just been calculated (introduced in a previous beta release).
	
- 4.0.11 (2017-09-09)
	- Fixed paths would ignore the ITraversalProvider (used for the turn based utilities) on the first node of the path, resulting in successful paths where they should have failed.
	- Fixed BlockManager.BlockMode.AllExceptSelector could often produce incorrect results. Thanks Cquels for spotting the bug.
	- Fixed various bugs related to destroying/adding graphs that could cause exceptions. Thanks DougW for reporting this.
	- Fixed destroying a grid graph would not correctly clear all custom connections. Thanks DougW for reporting this.
	- Fixed the MultiTargetPath did not reset all fields to their default values when using path pooling.
	- Added some additional error validation in the MultiTargetPath class.
	- Fixed scanning a recast graph that was not using tiles using Unity 2017.1 or later on Windows could block indefinitely. Thanks David Drummond and ceebeee for reporting this.
	- Improved compatibility with Nintendo Switch. Thanks Noogy for the help.
	- Fixed GraphUpdateScene would not handle the GameObject's scale properly which could cause it to not update some nodes.
	- Fixed a regression in 4.0 which could cause the error to be omitted from log messages when paths failed.
	- Fixed several bugs relating to #Pathfinding.NNConstraint.distanceXZ and #Pathfinding.NavmeshBase.nearestSearchOnlyXZ. Thanks koirat for reporting this.
	- Fixed scanning a graph that threw an error would prevent any future scans. Thanks Baste for reporting this.
	- Added a new get started video tutorial. See \ref getstarted.
	- The PointGraph.nodeCount property is now protected instead of private, which fixes some compatibility issues.
	- Improved compatibility with Unity 2017.1, esp. when using the experimental .Net 4.6 target. Thanks Scott_Richmond for reporting the issues.
	- Fixed DynamicGridObstacle trying to update the graphs even when outside of play mode.
	- Fixed runtime error when targeting the Windows Store. Thanks cedtat for reporting the bug.
	- Fixed compilation error when targeting the Windows Store. Introduced in 4.0.3. Thanks cedtat for reporting the bug.

- 4.0.10 (2017-05-01)
	- Fixed compiler errors in the free version because the ManualRVOAgent.cs script being included by mistake. Thanks hummerbummer for reporting the issue.
	- Fixed Unity's scene view picking being blocked by graph gizmos. Thanks Scott_Richmond for reporting the bug.

- 4.0.9 (2017-04-28)
	- Significantly improved performance and reduced allocations when recalculating indivudal recast tiles during runtime and there are terrains in the scene.
	- Fixed the GraphUpdateScene inspector showing a warning for one frame after the 'convex' field has been changed.
	- Fixed a few compiler warnings in Unity 5.6. Thanks TotalXep for reporting the issue.
	- Fixed graph drawing could generate large amounts of garbage due to a missing GetHashCode override which causes Mono to have to allocate some dummy objects.
	- Fixed graph gizmo lines could be rendered incorrectly on Unity 5.6 on mac and possibly on Windows too.

- 4.0.8 (2017-04-28)
	- Added \link Pathfinding.AIBase.rotationIn2D rotationIn2D\endlink to the AIPath script. It makes it possible to use the Y axis as the forward axis of the character which is useful for 2D games.
	- Exposed the GridGraph.LayerCount property which works for both grid graphs and layered grid graphs (for grid graphs it always returns 1).
	- Made the LayerGridGraph.layerCount field internal to discourage its use outside the LayerGridGraph class.
	- Fixed exception when destroying some graph types (introduced in 4.0.6). Thanks unfalco for reporting the bug.
	- Fixed exception in GridGraph.GetNodesInRegion when being called with an invalid rectangle or a rectangle or bounds object that was completely outside the graph. Thanks WillG for finding the bug.
	- Fixed AIPath/RichAI not rotating to the correct direction if they started in a rotation such that the forward axis was perpendicular to the movement plane.

- 4.0.7 (2017-04-27)
	- Fixed 2D example scenes had their grids rotated by (90,0,0) instead of (-90,0,0).
		It doesn't matter for those scenes, but the (-90,0,0) leads to more intuitive axis rotations for most use cases. Thanks GeloMan for noticing this.
	- Renamed AISimpleLerp to AILerp in the component menu as the documentation only refers to it by the name 'AILerp'.
	- Added a new documentation page and video tutorial (\ref pathfinding-2d) showing how to configure pathfinding in 2D games.

- 4.0.6 (2017-04-21)
	- Fixed creating a RichAI and in the same frame setting the target and calling UpdatePath would always result in that path being canceled.
	- Fixed a race condition which meant that if you called RichAI.UpdatePath, AILerp.SearchPath or AIPath.SearchPath during the same frame that the agent was created
		then the callback for that path would sometimes be missed and the AI would wait indefinitely for it. This could cause the agents to sometimes never start moving.
	- Fixed adding a new graph while graph updates were running at the same time could potentially cause errors.
	- Added NavGraph.exists which will become false when a graph has been destroyed.
	- Fixed TileHandlerHelper could throw exceptions if the graph it was tracking was destroyed.
	- Fixed TileHandlerHelper not detecting new NavmeshCut or NavmeshAdd components that were created before the
		TileHandlerHelper component was created or when it was disabled.
	- TileHandlerHelper no longer logs an error if it is created before a recast/navmesh graph exists in the scene
		and when one is created the TileHandlerHelper will automatically detect it and start to update it.
	- Fixed TileHandlerHelper could throw exceptions if the graph it was tracking changed dimensions.
	- Fixed recast graphs would always rasterize capsule colliders as if they had their 'direction' setting set to 'Y-axis'. Thanks emrys90 for reporting the bug.
	- The package now contains a 'documentation.html' file which contains an offline version of the 'Get Started' tutorial.

- 4.0.5 (2017-04-18)
	- Improved compatibility with Opsive's Behavior Designer - Movement Pack (https://www.assetstore.unity3d.com/en/#!/content/16853).
		- The 4.0 update combined with the Movement Pack caused some compiler errors previously.

- 4.0.4 (2017-04-17)
	- Fixed the funnel modifier not working if 'Add Points' on the Seeker's Start End Modifier was enabled. Thanks Blaze_Barclay for reporting it.
	- Fixed code typo in the \ref write-modifiers tutorial as well as made a few smaller improvements to it.
	- Fixed some cases where the LegacyRVOController would not behave like the RVOController before version 4.0.
	- Fixed LegacyAIPath not using the same custom inspector as the AIPath component.

- 4.0.3 (2017-04-16)
	- Improved code style and improved documentation for some classes.
	- Reduced memory allocations a bit when using the NavmeshAdd component.
	- Fixed graph types not necessarily being initialized when scanning the graph outside of play mode.
	- Fixed LayerGridGraph not reporting scanning progress properly.
		This caused it to not work well with ScanAsync and when scanning the graph in the editor the progress bar would only update once the whole graph had been scanned.
	- Removed the DebugUtility class which was only used for development when debugging the recast graph.

- 4.0.2 (2017-04-16)
	- Fixed a minor bug in the update checker.
	- Deduplicated code for drawing circles and other shapes using Debug.Draw* or Gizmos.Draw* and moved this code to a new class Pathfinding.Util.Draw.

- 4.0.1 (2017-04-15)
	- Improved how AIPath and RichAI work with rigidbodies.
	- Added option for gravity to AIPath.
	- Removed the RichAI.raycastingForGroundPlacement field as it is automatically enabled now if any gravity is used.
	- AIPath and RichAI now inherit from the same base class Pathfinding.AIBase.

- 4.0 (2017-04-10)
	- Upgrade Notes
		- This release contains some significant changes. <b>It is strongly recommended that you back up your
			project before upgrading</b>.
		- If you get errors immediately after upgrading, try to delete the AstarPathfindingProject folder
			and import the package again. Sometimes UnityPackages will leave old files which can cause issues.
		- Moved some things to inside the Pathfinding namespace to avoid naming collisions with other packages.
			Make sure you have the line 'using Pathfinding;' at the top of your scripts.
			Some example scripts have been moved to the Pathfinding.Examples namespace.
		- The RVOController component no longer handles movement as it turned out that was a bad idea.
			Having multiple components that handled movement (e.g RichAI and RVOController) didn't turn out well
			and it was very hard to configure the settings so that it worked well.
			The RVOController now exposes the CalculateMovementDelta method which allows other scripts to
			ask it how the local avoidance system thinks the character should move during this frame.
			If you use the RichAI or AIPath components for movement, everything should work straight away.
			If you use a custom movement script you may need to change your code to use the CalculateMovementDelta
			method for movement. Some settings may need to be tweaked, but hopefully it should not be too hard.
		- Node connections are now represented using an array of structs (of type \link Pathfinding.Connection Connection\endlink) instead of
			one array for target nodes and one array for costs.
		- When upgrading an existing project legacy versions of the RVOController, RichAI, AIPath and GraphUpdateScene components
			will be used for compatibility reasons. You will have to click a button in the inspector to upgrade them to the latest versions.
			I have tried to make sure that the movement scripts behave the same as they did before version 4.0, but it is possible that there are some minor differences.
			If you have used a custom movement script which inherits from AIPath or RichAI then the legacy components cannot be used automatically, instead the new versions will be used from the start.
	- New Features And Improvements
		- Local Avoidance
			- The RVO system has been cleaned up a lot.
				- Agents will now always avoid walls and obstacles even if that would put them on a collision course with another agent.
					This helps with a previous problem of agents being able to be pushed into walls and obstacles (note that RVONavmesh or RVOSquareObstacle still need to be used).
				- The RVOSimulator can now be configured for XZ space or XY space (2D).
				- The RVOController no longer handles movement itself as this turned out to be a really bad idea (see upgrade notes section).
				- The RVOController can now be used to stop at a target much more precisely than before using the SetTarget method.
				- Agents are now \link Pathfinding.RVO.RVOSimulator.symmetryBreakingBias biased slightly\endlink towards passing other agents on the right side, this helps resolve some situations
					with a lot of symmetry much faster.
				- All fuzzy and hard to adjust parameters from the \link Pathfinding.RVO.RVOSimulator RVOSimulator\endlink component have been removed.
					It should now be much easier to configure.
				- The RichAI movement script now works a lot better with the RVOController.
					Previously the movement could be drastically different when the RVOController was used
					and local avoidance didn't work well when the agent was at the edge of the navmesh.
				- Improved gizmos for the RVOController.
				- Added \link Pathfinding.RVO.RVOController.ForceSetVelocity RVOController.ForceSetVelocity\endlink to use when you want agents to avoid a player (or otherwise externally controlled) character.
				- RVO agents can now have different priorities, lower priority agents will avoid higher priority agents more.
				- The neighbour distance field is now automatically calculated. This makes it easier to configure the agents and it will
					also improve performance slightly when the agents are moving slowly (for example in very crowded scenarios).
				- Added support for grid graphs to \link Pathfinding.RVO.RVONavmesh RVONavmesh\endlink.
				- Added a new example scene for RVO in 2D
					\htmlonly <video class="tinyshadow" controls loop><source src="images/3vs4/rvo2d.mp4" type="video/mp4"></video> \endhtmlonly
		- General
			- Huge increase in the performance of graph gizmos.
				This was accomplished by bypassing the Unity Gizmos and creating a custom gizmo rendererer that is able to retain
				the gizmo meshes instead of recreating them every frame (as well as using a lot fewer draw calls than Unity Gizmos).
				Therefore the graphs usually only need to check if the nodes have changed, and only if they have changed they will
				rebuild the gizmo meshes. <b>This may cause graph updates to seem like they introduce more lag than they actually do</b>
				since a graph update will also trigger a gizmo rebuild. So make sure to always profile with gizmos disabled.
				For a 1000*1000 graph, which previously almost froze the editor, the time per frame went from over 4200 ms to
				around 90 ms when no nodes had changed.
				\htmlonly <video class="tinyshadow" controls loop><source src="images/3vs4/gizmo_performance.mp4" type="video/mp4"></video> \endhtmlonly
			- Improved the style of graph gizmos. A solid surface is now rendered instead of only the connections between the nodes.
				The previous mode of rendering only connections is of course still available.
				\shadowimage{3vs4/gizmos.png}
			- Added a new example scene showing how to configure hexagon graphs.
			- Added gizmos for hexagon graphs (grid graphs with certain settings).
				\shadowimage{3vs4/hexagon_thin.png}
			- Implemented async scanning. \link AstarPath.ScanAsync AstarPath.active.ScanAsync \endlink is an IEnumerable that can be iterated over several frames
				so that e.g a progress bar can be shown while calculating the graphs. Note that this does not guarantee
				a good framerate, but at least you can show a progress bar.
			- Improved behaviour of the AIPath movement script.
				- AIPath now works in the XY plane as well. In fact it works with any graph rotation.
					The Z axis is always the forward axis for the agent, so for 2D games with sprites you may have to attach the sprite
					to a child object which is rotated for it to show up correctly.
				- Previously the slowdownDistance had to be smaller than the forwardLook field otherwise the character
					could slow down even when it had not reached the end of the path.
				- The agent should stop much more precisely at the end of the path now.
				- The agent now rotates with a fixed angular speed instead of a varying one as this is often more realistic.
				- Reduced the likelihood of the agent spinning around when it reaches the end of the path.
				- It no longer uses the forwardLook variable.
					It was very tricky to set correctly, now the pickNextWaypointDist variable is used for everything instead
					and generally this should give you smoother movement.
			- Improved behaviour of the \link Pathfinding.RichAI RichAI \endlink movement script.
				- The agent should stop much more precisely at the end of the path now.
				- Reduced the likelihood of the agent spinning around when it reaches the end of the path.
			- Scanning the graph using AstarPath.Scan will now profile the various parts of the graph scanning
				process using the Unity profiler (Profiler.BeginSample and Profiler.EndSample).
			- \link Pathfinding.DynamicGridObstacle DynamicGridObstacle \endlink will now update the graph immediately if an object with that component is created during runtime
				instead of waiting until it was moved for the first time.
			- \link Pathfinding.GraphUpdateScene GraphUpdateScene \endlink and \link Pathfinding.GraphUpdateShape GraphUpdateShape \endlink can now handle rotated graphs a lot better.
				The rotation of the object the GraphUpdateScene component is attached to determines the 'up' direction for the shape
				and thus which points will be considered to be inside the shape.
				The world space option had to be removed from GraphUpdateScene because it didn't really work with rotated graphs.
				The lockToY option for GraphUpdateScene has also been removed because it wasn't very useful and after this change it would only have had an impact
				in rare cases.
			- Improved \link Pathfinding.GraphUpdateScene GraphUpdateScene \endlink editor. When editing the points in the scene view it now shows helper lines
				to indicate where a new point is going to be added and which other points it will connect to
				as well as several other minor improvements.
				\htmlonly <video class="tinyshadow" controls loop><source src="images/3vs4/graph_update_scene_sd.mp4" type="video/mp4"></video> \endhtmlonly
			- \link Pathfinding.GraphUpdateScene GraphUpdateScene \endlink now supports using the bounds from 2D colliders and the shape from PolygonCollider2D.
			- Added opaqueness slider for the gizmos under Inspector -> Settings -> Colors.
			- Added \link Pathfinding.Path.BlockUntilCalculated Path.BlockUntilCalculated \endlink which is identical to AstarPath.BlockUntilCalculated.
			- Added Seeker.CancelCurrentPathRequest.
			- Added \link Pathfinding.NavGraph.GetNodes NavGraph.GetNodes(System.Action<GraphNode>) \endlink which calls a delegate with each node in the graph.
				Previously NavGraph.GetNodes(GraphNodeDelegateCancelable) existed which did the same thing but required the delegate
				to return true if it wanted the graph to continue calling it with more nodes. It turns out this functionality was very rarely needed.
			- Individual graphs can now be scanned using #AstarPath.Scan(NavGraph) and other related overloads.
			- Improved \link Pathfinding.BinaryHeap priority queue \endlink performance. On average results in about a 2% overall pathfinding performance increase.
			- ObjectPool<T> now requires a ref parameter when calling Release with an object to help prevent silly bugs.
			- 'Min Area Size' has been removed. The edge cases are now handled automatically.
			- Added ObjectPoolSimple<T> as a generic object pool (ObjectPool<T> also exists, but for that T must implement IAstarPooledObject).
			- \link Pathfinding.RaycastModifier RaycastModifier \endlink now supports multi editing.
			- Added \link Pathfinding.GraphNode.RandomPointOnSurface GraphNode.RandomPointOnSurface \endlink.
			- Added \link Pathfinding.GraphNode.SurfaceArea GraphNode.SurfaceArea \endlink.
			- \link Pathfinding.Int2 Int2 \endlink and \link Pathfinding.Int3 Int3 \endlink now implement IEquatable for slightly better performance and fewer allocations in some places.
			- \link Pathfinding.Examples.LocalSpaceRichAI LocalSpaceRichAI \endlink can now be used with any rotation (even things like moving on an object that is upside down).
			- The \link Pathfinding.FunnelModifier funnel modifier \endlink can now handle arbitrary graphs (even graphs in the 2D plane) if the new \link Pathfinding.FunnelModifier.unwrap unwrap \endlink option is enabled.
			- The \link Pathfinding.FunnelModifier funnel modifier \endlink can split the resulting path at each portal if the new \link Pathfinding.FunnelModifier.splitAtEveryPortal splitAtEveryPortal \endlink option is enabled.
		- Recast/Navmesh Graphs
			- Recast graph scanning is now multithreaded which can improve scan times significantly.
			- Recast graph scanning now handles large worlds with lots of objects better. This can improve scan times significantly.
			\htmlonly <video class="tinyshadow" controls loop><source src="images/3vs4/recast_scanning_performance.mp4" type="video/mp4"></video> \endhtmlonly
			- Improved performance of nearest node queries for Recast/navmesh graphs.
			- Editing navmesh cut properties in the inspector now forces updates to happen immediately which makes editing easier.
			- Long edges in recast graphs are now split at tile borders as well as at obstacle borders.
				This can in particular help on terrain maps where the tile borders do not follow the elevation that well
				so the max edge length can be reduced to allow the border to follow the elevation of the terrain better.
			- Recast graphs can now be rotated arbitrarily.
				- Navmesh cutting still works!
				- The RichAI script currently does not support movement on rotated graphs, but the AIPath script does.
			- Improved performance of navmesh cutting for large worlds with many tiles and NavmeshAdd components.
			- Navmesh graphs and recast graphs now share the same base code which means that navmesh graphs
				now support everything that previously only recast graphs could be used for, for example
				navmesh cutting.
			- The NavmeshCut inspector now shows a warning if no TileHandlerHelper component is present in the scene.
				A TileHandlerHelper component is necessary for the NavmeshCuts to update the graphs.
			- Recast graphs now use less memory due to the BBTree class now using around 70% less memory per node.
			- Recast graphs now allocate slightly less memory when recalculating tiles or scanning the graph.
			- Cell height on Recast graphs is now automatically set to a good value.
			- Navmesh cutting is now a bit better at using object pooling to avoid allocations.
			- TileHandlerHelper now updates the tiles properly when one or multiple tiles on the recast graph are recalculated
				due to a graph update or because it was rescanned.
			- Navmesh cutting now uses more pooling to reduce allocations slightly.
			- Improved performance of loading and updating (using navmesh cutting) recast tiles with a large number of nodes.
		- Grid Graphs
			- Added LevelGridNode.XCoordinateInGrid, LevelGridNode.ZCoordinateInGrid, LevelGridNode.LayerCoordinateInGrid.
			- Added GridGraph.GetNodesInRegion(IntRect).
				Also works for layered grid graphs.
			- Layered grid graphs now have support for 'Erosion Uses Tags'.
			- Added GridGraph.CalculateConnections(GridNodeBase) which can be used for both grid graphs and layered grid graphs.
			- Grid graphs can now draw the surface and outline of the graph instead of just the connections between the nodes.
				The inspector now contains several toggles that can be used to switch between the different rendering modes.
			- The ProceduralGridMover component now works with LayerGridGraph as well.
			- Added GridGraph.RecalculateCell(x,y) which works both for grid graphs and layered grid graphs.
				This replaces the UpdateNodePositionCollision method and that method is now deprecated.
			- Improved GridGraph.RelocateNodes which is now a lot more resilient against floating point errors.
			- Added dimetric (60°) to the list of default values for the isometric angle field on grid graphs.
			- Changing the width/depth of a grid graph will now keep the current pivot point at the same position instead of always keeping the bottom left corner fixed.
				(the pivot point can be changed to the center/bottom left/top left/top right/bottom right right next to the position field in the grid graph inspector)
			- Improved fluidity and stability when resizing a grid graph in the scene view.
				It now snaps to full node increments in size.
			- Grid graphs now display a faint grid pattern in the scene view even when the graph is not scanned
				to make it easier to position and resize the graph.
			- Improved styling of some help boxes in the grid graph inspector when using the dark UI skin.
			- The size of the unwalkable node gizmo (red cube) on grid graphs is now based on the node size to avoid the gizmos being much larger or much smaller than the nodes.
			- Implemented \link Pathfinding.ABPath.EndPointGridGraphSpecialCase special case for paths on grid graphs \endlink so that if you request a path to an unwalkable node with several
				walkable nodes around it, it will now not pick the closest walkable node to the requested target point and find a path to that
				but it will find the shortest path which goes to any of the walkable nodes around the unwalkable node.
				\htmlonly <a href="images/abpath_grid_not_special.gif">Before</a>, <a href="images/abpath_grid_special.gif">After</a> \endhtmlonly.
				This is a special case of the MultiTargetPath, for more complicated configurations of targets the multi target path needs to be used to be able to handle it correctly.
	- Changes
		- Node connections are now represented using an array of structs (of type Connection) instead of
			one array for target nodes and one array for costs.
		- When scanning a graph in the editor, the progress bar is not displayed until at least 200 ms has passed.
			Since displaying the progress bar is pretty slow, this makes scanning small graphs feel more snappy.
		- GridGraph and LayerGridGraph classes now have a 'transform' field instead of a matrix and inverseMatrix fields.
			The GraphTransform class also has various other nice utilities.
		- Moved mesh collecting code for Recast graphs to a separate class to improve readability.
		- Refactored out large parts of the AstarPath class to separate smaller classes to improve readability and increase encapsulation.
		- AstarPath.RegisterSafeUpdate is now implemented using WorkItems. This yields a slightly different behavior (previously callbacks added using RegisterSafeUpdate would
			always be executed before work items), but that should rarely be something that you would depend on.
		- Replaced AstarPath.BlockUntilPathQueueBlocked with the more robust AstarPath.PausePathfinding method.
		- The default radius, height and center for RVOControllers is now 0.5, 2 and 1 respectively.
		- To reduce confusion. The second area color is now a greenish color instead of a red one.
			The red color would often be mistaken as indicating unwalkable nodes instead of simply a different connected component.
			Hopefully green will be a more neutral color.
		- Renamed AstarPath.astarData to AstarPath.data.
		- Renamed NavmeshCut.useRotation and NavmeshAdd.useRotation to useRotationAndScale (since they have always affected scale too).
		- Renamed GridGraph.GenerateMatrix to GridGraph.UpdateTransform to be consistent with recast/navmesh graphs.
			The GenerateMatrix method is now deprecated.
		- Renamed AstarPath.WaitForPath to AstarPath.BlockUntilCalculated.
		- Renamed GridNode.GetConnectionInternal to HasConnectionInDirection.
		- Renamed NNInfo.clampedPosition to NNInfo.position.
		- Renamed GridGraph.GetNodesInArea to GetNodesInRegion to avoid confusing the word 'area' for what is used to indicate different connected components in graphs.
		- Renamed AIPath.turningSpeed to \link Pathfinding.AIPath.rotationSpeed rotationSpeed\endlink.
		- Deprecated Seeker.GetNewPath.
		- Deprecated NavGraph.matrix, NavGraph.inverseMatrix, NavGraph.SetMatrix and NavGraph.RelocateNodes(Matrix4x4,Matrix4x4).
			They have been replaced with a single transform field only available on some graph types as well as a few other overloads of teh RelocateNodes method.
		- Changed the signature of NavGraph.GetNodes(GraphNodeDelegateCancelable) to the equivalent NavGraph.GetNodes(System.Func<GraphNode,bool>).
		- Replaced all instances of GraphNodeDelegate with the equivalent type System.Action<GraphNode>.
		- Made a large number of previously public methods internal to reduce confusion about which methods one should use in a class and make the documentation easier to read.
			In particular the Path class has had its set of public methods reduced a lot.
		- Made AstarData.AddGraph(NavGraph) private. Scripts should use AstarData.AddGraph(System.Type) instead.
		- Moved internal fields of NNInfo into a new NNInfoInternal struct to make the API easier to use. Previously NNInfo contained some internal fields, but now they are only in NNInfoInternal.
		- Moved GetNeighbourAlongDirection to GridNodeBase and made it public.
		- An overload of the GridGraph.CalculateConnections method has been made non-static.
		- LayerGridGraph.LinkedLevelNode and LayerGridGraph.LinkedLevelCell are now private classes since they are only used by the LayerGridGraph.
		- MonoModifier.OnDestroy is now a virtual function.
		- AstarPath.IsUsingMultithreading and NumbParallelThreads have been made non-static.
		- AstarPath.inGameDebugPath is now private.
		- AstarPath.lastScanTime is now read only.
		- Removed the 'climb axis' field from grid graphs. The axis is now automatically set to the graph's UP direction (which is
			the only direction that makes sense and all other directions can be transformed to this one anyway).
		- Removed the 'worldSpace' parameter from RecastGraph.ReplaceTile, it is no longer possible to supply world space vertices to
			that method since graph space vertices are required for some things.
		- Removed BBTree.QueryCircle and BBTree.Query since they were not used anywhere.
		- Removed the Path.searchIterations field because it wasn't very useful even as debug information.
		- Removed the Path.maxFrameTime field because it was not used.
		- Removed the Path.callTime property because it was not used.
		- Removed the ABPath.startHint, ABPath.endHint fields because they were not used.
		- Removed the ABPath.recalcStartEndCosts field because it was not used.
		- Removed the RecursiveBinary and RecursiveTrinary modes for RichAI.funnelSimplification because the Iterative mode
			was usually the best and fastest anyway (also the other two modes had a rare bug where they could get cought in infinite loops).
		- Removed the Polygon.Subdivide method because it was not used anywhere.
		- Removed the NavGraph.Awake method because it was not used for anything.
		- Removed ASTAR_OPTIMIZE_POOLING from Optimization tab. It is now always enabled in standalone builds and always disabled in the Unity editor.
		- Removed various unused Recast code.
		- Removed support for forcing the inspector skin to be dark or light. The value provided by EditorGUIUtility.isProSkin is always used now.
		- Removed multiplication operator for Int3 with a Vector3 because it is a nonstandard operation on vectors (and it is not that useful).
		- Removed the since long deprecated example script AIFollow.
		- Removed the AdaptiveSampling algorithm for local avoidance. Only GradientDescent is used now.
		- Removed empty PostProcess method in NavMeshGraph.
	- Fixes
		- Fixed RichAI and AIPath trying to use CharacterControllers even if the CharacterController component was disabled.
		- Fixed rotated recast/navmesh graphs would ensure each node's vertices were laid out clockwise in XZ space instead of in graph space which could cause parts of the graph to become disconnected from the rest.
		- Fixed a bug where graphs could fail to be deserialized correctly if the graph list contained a null element
		- Fixed a bug where the json serializer could emit True/False instead of true/false which is the proper json formatting.
		- Fixed LayerGridGraphs' "max climb" setting not working properly with rotated graphs.
		- Fixed LayerGridGraphs' "character height" setting not working properly with rotated graphs.
		- Fixed LayerGridGraphs assuming there were no obstacles nearby if no ground was found.
		- Fixed DynamicGridObstacle getting caught in an infinite loop if there was no AstarPath component in the scene when it was created. Thanks MeiChen for finding the bug.
		- Fixed NodeLink2 deserialization causing exceptions if the node hadn't linked to anything when it was serialized. Thanks Skalev for finding the bug.
		- Fixed the AlternativePath modifier could crash the pathfinding threads if it logged a warning since it used the Debug.Log(message,object) overload which
			can only be used from the Unity thread.
		- Fixed an issue where layer mask fields in graph editors would show 'Nothing' if they only included layers which had no name set.
		- Fixed potential memory leak.
			Paths in the path pool would still store the callback which is called when the path has been calculated
			which that means it would implicitly hold a reference to the object which had the method that would be called.
			Thanks sehee for pointing this out.
		- Fixed GridNode.ClosestPointOnNode could sometimes return the wrong y coordinate relative to the graph (in particular when the graph was rotated) and the y coordinate would not snap to the node's surface.
		- Fixed AstarData.AddGraph would fill *all* empty slots in the graph array with the graph instead of just the first. Thanks bitwise for finding the bug.
		- Improved compatibility with Unity 5.5 which was need due to the newly introduced UnityEngine.Profiling namespace.
		- Fixed graph updates on LayeredGridGraphs not respecting GraphUpdateObject.resetPenaltyOnPhysics.
		- Fixed potential memory leak when calling RecalculateCell on a layered grid graph.
		- LevelGridNode.ContainsConnection now reports correct values (previously it would only check
			non-grid connections).
		- Fixed not being able to deserialize settings saved with some old versions of the A* Pathfinding Project.
		- Tweaked ListPool to avoid returning lists with a very large capacity when a small one was requested
			as this could cause performance problems since Clear is O(n) where n is the capacity (not the size of the list).
		- Fixed GraphUpdateScene causing 'The Grid Graph is not scanned, cannot update area' to be logged when exiting play mode.
		- Fixed scanning a recast graph could in very rare circumstances throw a 'You are trying to pool a list twice' exception due to a multithreading
			race condition.
		- Fixed recast/navmesh graphs could return the wrong node as the closest one in rare cases, especially near tile borders.
		- Fixed another case of recast/navmesh graphs in rare cases returning the wrong node as the closest one.
		- Fixed gizmo drawing with 'Show Search Tree' enabled sometimes right after graph updates drawing nodes outside the
			search tree as if they were included in it due to leftover data from graph updates.
		- Fixed navmesh and recast graphs would unnecessarily be serialized by Unity which would slow down the inspector slightly.
		- Fixed AstarEnumFlagDrawer not working with private fields that used the [SerializeField] attribute.
			This does not impact anything that the A* Pathfinding Project used, but some users are using the AstarEnumFlagDrawer for
			other fields in their projects. Thanks Skalev for the patch.
		- Clicking 'Apply' in the Optimizations tab will now always refresh the UI instead of assuming that
			a recompilation will happen (it will not happen if only defines for other platforms than the current one were modified).
		- Fixed not being able to multi-edit RVOSquareObstacle components.
		- Fixed GridNode.ClearConnections(true) not removing all reversed connections and could sometimes remove the wrong ones.
		- Fixed TileHandlerHelper regularly checking for if an update needs to be done even if TileHandlerHelper.updateInterval was negative
			even though the documentation specifies that it should not do that (it only disabled updates when updateInterval = -1).
		- Fixed PathUtilities.GetPointsAroundPointWorld and PathUtilities.GetPointsAroundPoint returning incorrect results sometimes.
		- Fixed Path.immediateCallback not being reset to null when using path pooling.
		- TileHandlerHelper will now work even if Scan On Awake in A* Inspector -> Settings is false and you are scanning the graph later.
		- Fixed AstarWorkItem.init could be called multiple times.
		- Fixed some documentation typos.
		- Fixed colliders being included twice in the recast rasterization if the GameObject had a RecastMeshObj attached to it which effectively made RecastMeshObj not work well at all with colliders.
		- Fixed inspector for RecastMeshObj not updating if changes were done to the fields by a script or when an undo or redo was done.
		- Fixed SimpleSmoothModifier custom editor would sometimes set all instances of a field to the same value
			when editing multiple objects at the same time.
		- Fixed division by zero when the TimeScale was zero in the AstarDebugger class. Thanks Booil Jung for reporting the issue.
		- Various other small fixes in the AstarDebugger class.
		- Fixed division by zero when generating a recast graph and the cell size was much larger than the bounds of the graph.
		- Fixed the recast graph data structures could be invalid while a graph update was running in a separate thread.
			This could cause API calls like AstarPath.GetNearest to throw exceptions. Now the affected tiles are recalculated
			in a separate thread and then the updates are applied to the existing graph in the Unity thread.
		- Fixed some cases where the AlternativePath modifier would apply penalties incorrectly and possibly crash the pathfinding thread.
		- Fixed IAgent.NeighbourCount would sometimes not be reset to 0 when the agent was locked and thus takes into account no other agents.
		- Fixed RVO threads would sometimes not be terminated which could lead to memory leaks if switching scenes a lot.
		- Fixed GridGraph.GetNearest and NavGraph.GetNearest not handling constraint=null.
	- Internal changes
		- These are changes to the internals of the system and will most likely not have any significant externally visible effects.
		- Removed some wrapper methods for the heap in the PathHandler class since they were just unnecessary. Exposed the heap field as readonly instead.
		- Renamed BinaryHeapM to BinaryHeap.
		- Renamed ExtraMesh to RasterizationMesh.
		- Refactored TileHandler.CutPoly to reduce code messiness and also fixed some edge case bugs.
	- Documentation
		- Among other things: improved the \ref writing-graph-generators guide (among other things it no longer uses hard to understand calculations to get the index of each node).

- 3.8.8.1 (2017-01-12)
	- Fixes
		- Fixed the 'Optimization' tab sometimes logging errors when clicking Apply on Unity 5.4 and higher.
		- More UWP fixes (pro version only).

- 3.8.8 (2017-01-11)
	- Fixes
		- Fixed errors when deploying for the Universal Windows Platform (UWP).
			This includes the Hololens platform.
		- It is no longer necessary to use the compiler directive ASTAR_NO_ZIP when deploying for UWP.
			zipping will be handled by the System.IO.Compression.ZipArchive class on those platforms (ZipArchive is not available on other platforms).
			If you have previously enabled ASTAR_NO_ZIP it will stay enabled to ensure compatibility.
		- Changed some comments from the '/**<' format to '/**' since Monodevelop shows the wrong docs when using the '/**<' format.

- 3.8.7 (2016-11-26)
	- Fixes
		- Improved compatibility with Unity 5.5 which was needed due to the newly introduced UnityEngine.Profiling namespace.

- 3.8.6 (2016-10-31)
	- Upgrade Notes
		- Note that a few features and some fixes that have been available in the beta releases are not
			included in this version because they were either not ready to be released or depended on other
			changes that were not ready.
		- Dropped support for Unity 5.1.
		- Moved some things to inside the Pathfinding namespace to avoid naming collisions with other packages.
			Make sure you have the line 'using Pathfinding;' at the top of your scripts.
		- Seeker.StartMultiTargetPath will now also set the enabledTags and tagPenalties fields on the path.
			Similar to what StartPath has done. This has been the intended behaviour from the start, but bugs happen.
			See http://forum.arongranberg.com/t/multitargetpath-doesnt-support-tag-constraints/2561/3
		- The JsonFx library is no longer used, so the Pathfinding.JsonFx.dll file in the plugins folder
			may be removed to reduce the build size a bit. UnityPackages cannot delete files, so you have to delete it manually.
		- RecastGraph.UpdateArea (along with a few other functions) is now explicitly implemented for the IUpdatableGraph interface
			as it is usually a bad idea to try to call those methods directly (use AstarPath.UpdateGraphs instead).
		- AstarPath.FlushWorkItems previously had pretty bad default values for the optional parameters.
			By default it would not necessarily complete all work items, it would just complete those that
			took a single frame. This is pretty much never what you actually want so to avoid
			confusion the default value has been changed.
	- New Features and Improvements
		- The JsonFx library is no longer used. Instead a very tiny json serializer and deserializer has been written.
			In addition to reducing code size and being slightly faster, it also means that users using Windows Phone
			no longer have to use the ASTAR_NO_JSON compiler directive. I do not have access to a windows phone
			however, so I have not tested to build it for that platform. If any issues arise I would appreciate if
			you post them in the forum.
		- Improved inspector for NavmeshCut.
		- NodeLink2 can now be used even when using cached startup or when loading serialized data in other ways just as long as the NodeLink2 components are still in the scene.
		- LevelGridNode now has support for custom non-grid connections (just like GridNode has).
		- Added GridNode.XCoordinateInGrid and GridNode.ZCoordinateInGrid.
		- Improved documentation for GraphUpdateShape a bit.
	- Changes
		- Removed EditorUtilities.GetMd5Hash since it was not used anywhere.
		- Deprecated TileHandler.GetTileType and TileHandler.GetTileTypeCount.
		- Seeker.StartPath now properly handles MultiTargetPath objects as well.
		- Seeker.StartMultiTargetPath is now deprecated. Note that it will now also set the
			enabledTags and tagPenalties fields on the path. Similar to what StartPath has done.
		- Removed GridGraph.bounds since it was not used or set anywhere.
		- GraphNode.AddConnection will now throw an ArgumentNullException if you try to call it with a null target node.
		- Made PointGraph.AddChildren and PointGraph.CountChildren protected since it makes no sense for them to be called by other scripts.
		- Changed how the 'Save & Load' tab looks to make it easier to use.
		- Renamed 'Path Debug Mode' to 'Graph Coloring' and 'Path Log Mode' to 'Path Logging' in the inspector.
		- RecastGraph.UpdateArea (along with a few other functions) is now explicitly implemented for the IUpdatableGraph interface
			as it is usually a bad idea to try to call those methods directly (use AstarPath.UpdateGraphs instead).
		- Removed ConnectionType enum since it was not used anywhere.
		- Removed NodeDelegate and GetNextTargetDelegate since they were not used anywhere.
	- Fixes
		- Fixed TinyJson not using culture invariant float parsing and printing.
			This could cause deserialization errors on systems that formatted floats differently.
		- Fixed the EndingCondition example script.
		- Fixed speed being multiplied by Time.deltaTime in the AI script in the get started tutorial when it shouldn't have been.
		- Fixed FunnelModifier could for some very short paths return a straight line even though a corner should have been inserted.
		- Fixed typo. 'Descent' (as in 'Gradient Descent') was spelled as 'Decent' in some cases. Thanks Brad Grimm for finding the typo.
		- Fixed some documentation typos.
		- Fixed some edge cases in RandomPath and FleePath where a node outside the valid range of G scores could be picked in some cases (when it was not necessary to do so).
		- Fixed editor scripts in some cases changing the editor gui styles instead of copying them which could result in headers in unrelated places in the Unity UI had the wrong sizes. Thanks HP for reporting the bug.
		- Fixed NavmeshCut causing errors when cutting the navmesh if it was rotated upside down or scaled with a negative scale.
		- Fixed TriangleMeshNode.ClosestPointOnNodeXZ could sometimes return the wrong point (still on the node surface however).
			This could lead to characters (esp. when using the RichAI component) teleporting in rare cases. Thanks LordCecil for reporting the bug.
		- Fixed GridNodes not serializing custom connections.
		- Fixed nodes could potentially get incorrect graph indices assigned when additive loading was used.
		- Added proper error message when trying to call RecastGraph.ReplaceTile with a vertex count higher than the upper limit.
	- Known Bugs
		- Calling GetNearest when a recast graph is currently being updated on another thread may in some cases result in a null reference exception
			being thrown. This does not impact navmesh cutting. This bug has been present (but not discovered) in previous releases as well.
		- Calling GetNearest on point graphs with 'optimizeForSparseGraph' enabled may in some edge cases return the wrong node as being the closest one.
			It will not be widely off target though and the issue is pretty rare, so for real world use cases it should be fine.
			This bug has been present (but not discovered) in previous releases as well.

- 3.8.3 through 3.8.5 were beta versions

- 3.8.2 (2016-02-29)
	- Improvements
		- DynamicGridObstacle now handles rotation and scaling better.
		- Reduced allocations due to coroutines in DynamicGridObstacle.
	- Fixes
		- Fixed AstarPath.limitGraphUpdates not working properly most of the time.
			In order to keep the most common behaviour after the upgrade, the value of this field will be reset to false when upgrading.
		- Fixed DynamicGridObstacle not setting the correct bounds at start, so the first move of an object with the DynamicGridObstacle
			component could leave some nodes unwalkable even though they should not be. Thanks Dima for reporting the bug.
		- Fixed DynamicGridObstacle stopping to work after the GameObject it is attached to is deactivated and then activated again.
		- Fixed RVOController not working after reloading the scene due to the C# '??' operator not being equivalent to checking
			for '== null' (it doesn't use Unity's special comparison check). Thanks Khan-amil for reporting the bug.
		- Fixed typo in documentation for ProceduralGridMover.floodFill.
	- Changes
		- Renamed 'Max Update Frequency' to 'Max Update Interval' in the editor since it has the unit [second], not [1/second].
		- Renamed AstarPath.limitGraphUpdates to AstarPath.batchGraphUpdates and AstarPath.maxGraphUpdateFreq to AstarPath.graphUpdateBatchingInterval.
			Hopefully these new names are more descriptive. The documentation for the fields has also been improved slightly.

- 3.8.1 (2016-02-17)
	- Improvements
		- The tag visualization mode for graphs can now use the custom list of colors
			that can be configured in the inspector.
			Thanks Arakade for the patch.
	- Fixes
		- Recast graphs now handle meshes and colliders with negative scales correctly.
			Thanks bvance and peted for reporting it.
		- Fixed GridGraphEditor throwing exceptions when a user had created a custom grid graph class
			which inherits from GridGraph.
		- Fixed Seeker.postProcessPath not being called properly.
			Instead it would throw an exception if the postProcessPath delegate was set to a non-null value.
			Thanks CodeSpeaker for finding the bug.

- 3.8 (2016-02-16)
	- The last version released on the Unity Asset Store was 3.7, so if you are upgrading
		from that version check out the release notes for 3.7.1 through 3.7.5 as well.
	- Breaking Changes
		- For the few users that have written their own Path Modifiers. The 'source' parameter to the Apply method has been removed from the IPathModifier interface.
			You will need to remove that parameter from your modifiers as well.
		- Modifier priorities have been removed and the priorities are now set to sensible hard coded values since at least for the
			included modifiers there really is only one ordering that makes sense (hopefully there is no use case I have forgotten).
			This may affect your paths if you have used some other modifier order.
			Hopefully this change will reduce confusion for new users.
	- New Features and Improvements
		- Added NodeConnection mode to the StartEndModifier on the Seeker component.
			This mode will snap the start/end point to a point on the connections of the start/end node.
			Similar to the Interpolate mode, but more often does what you actually want.
		- SimpleSmoothModifier now has support for multi editing.
		- Added a new movement script called AILerp which uses linear interpolation to follow the path.
			This is good for games which want the agent to follow the path exactly and not use any
			physics like behaviour. This movement script works in both 2D and 3D.
		- Added a new 2D example scene which uses the new AILerp movement script.
		- All scripts now have a <a href="http://docs.unity3d.com/ScriptReference/HelpURLAttribute.html">HelpURLAttribute</a>
			so the documentation button at the top left corner of every script inspector now links directly to the documentation.
		- Recast graphs can now draw the surface of a navmesh in the scene view instead of only
			the node outlines. Enable it by checking the 'Show mesh surface' toggle in the inspector.
			Drawing the surface instead of the node outlines is usually faster since it does not use
			Unity Gizmos which have to rebuild the mesh every frame.
		- Improved GUI for the tag mask field on the Seeker.
		- All code is now consistently formatted, utilising the excellent Uncrustify tool.
		- Added animated gifs to the \link Pathfinding.RecastGraph.cellSize Recast graph \endlink documentation showing how some parameters change the resulting navmesh.
			If users like this, I will probably follow up and add similar gifs for variables in other classes.
			\shadowimage{recast/character_radius.gif}
	- Fixes
		- Fixed objects in recast graphs being rasterized with an 0.5 voxel offset.
			Note that this will change how your navmesh is rasterized (but usually for the better), so you may want to make sure it still looks good.
		- Fixed graph updates to navmesh and recast graphs not checking against the y coordinate of the bounding box properly (introduced in 3.7.5).
		- Fixed potential bug when loading graphs from a file and one or more of the graphs were null.
		- Fixed invalid data being saved when calling AstarSerializer.SerializeGraphs with an array that was not equal to the AstarData.graphs array.
			The AstarSerializer is mostly used internally (and internally it is always called with the AstarData.graphs array). Thanks munkman for reporting this.
		- Fixed incorrect documentation for GridNode.NodeInGridIndex. Thanks mfjk for reporting it!
		- Fixed typo in a recast graph log message (where -> were). Thanks bigdaddio for reporting it!
		- Fixed not making sure the file is writable before writing graph cache files (Perforce could sometimes make it read-only). Thanks Jørgen Tjernø for the patch.
		- Fixed RVOController always using FindObjectOfType during Awake, causing performance issues in large scenes. Thanks Jørgen Tjernø for the patch.
		- Removed QuadtreeGraph, AstarParallel, NavMeshRenderer and NavmeshController from the released version.
			These were internal dev files but due to typos they had been included in the release.
			It will also automatically refresh itself if the graph has been rescanned with a different number of tiles.
		- Fixed SimpleSmoothModifier not always including the exact start point of the path.
		- Fixed ASTAR_GRID_NO_CUSTOM_CONNECTIONS being stripped out of the final build, so that entry in the Optimizations tab didn't actually do anything.
		- Fixed performance issue with path pooling. If many paths were being calculated and pooled, the performance could be
			severely reduced unless ASTAR_OPTIMIZE_POOLING was enabled (which it was not by default).
		- Fixed 3 compiler warnings about using some deprecated Unity methods.
	- Changes
		- Recast graphs' 'Snap To Scene' button now snaps to the whole scene instead of the objects that intersect the bounds that are already set.
			This has been a widely requested change. Thanks Jørgen Tjernø for the patch.
		- Moved various AstarMath functions to the new class VectorMath and renamed some of them to reduce confusion.
		- Removed various AstarMath functions because they were either not used or they already exist in e.g Mathf or System.Math.
			DistancePointSegment2, ComputeVertexHash, Hermite, MapToRange, FormatBytes,
			MagnitudeXZ, Repeat, Abs, Min, Max, Sign, Clamp, Clamp01, Lerp, RoundToInt.
		- PathEndingCondition (used with XPath) is now abstract since it doesn't really make any sense to use the default implementation (always returns true).
		- A 'Recyle' method is no longer required on path classes (reduced boilerplate).
		- Removed old IFunnelGraph interface since it was not used by anything.
		- Removed old ConvexMeshNode class since it was not used by anything.
		- Removed old script NavmeshController since it has been disabled since a few versions.
		- Removed Int3.DivBy2, Int3.unsafeSqrMagnitude and Int3.NormalizeTo since they were not used anywere.
		- Removed Int2.sqrMagnitude, Int2.Dot since they were not used anywhere and are prone to overflow (use sqrMagnitudeLong/DotLong instead)
		- Deprecated Int2.Rotate since it was not used anywhere.
		- Deprecated Int3.worldMagnitude since it was not used anywhere.

- 3.7.5 (2015-10-05)
	- Breaking changes
		- Graph updates to navmesh and recast graphs now also check that the nodes are contained in the supplied bounding box on the Y axis.
			If the bounds you have been using were very short along the Y axis, you may have to change them so that they cover the nodes they should update.
	- Improvements
		- Added GridNode.ClosestPointOnNode.
		- Optimized GridGraph.CalculateConnections by approximately 20%.
			This means slightly faster scans and graph updates.
	- Changes
		- Graph updates to navmesh and recast graphs now also check that the nodes are contained in the supplied bounding box on the Y axis.
			If the bounds you have been using were very short along the Y axis, you may have to change them so that they cover the nodes they should update.
	- Fixes
		- Fixed stack overflow exception when a pivot root with no children was assigned in the heuristic optimization settings.
		- Fixed scanning in the editor could sometimes throw exceptions on new versions of Unity.
			Exceptions contained the message "Trying to initialize a node when it is not safe to initialize any node".
			This happened because Unity changed the EditorGUIUtility.DisplayProgressBar function to also call
			OnSceneGUI and OnDrawGizmos and that interfered with the scanning.
		- Fixed paths could be returned with invalid nodes if the path was calculated right
			before a call to AstarPath.Scan() was done. This could result in
			the funnel modifier becoming really confused and returning a straight line to the
			target instead of avoiding obstacles.
		- Fixed sometimes not being able to use the Optimizations tab on newer versions of Unity.

- 3.7.4 (2015-09-13)
	- Changes
		- AIPath now uses the cached transform field in all cases for slightly better performance.
	- Fixes
		- Fixed recast/navmesh graphs could in rare cases think that a point on the navmesh was
		   in fact not on the navmesh which could cause odd paths and agents teleporting short distances.
	- Documentation Fixes
		- Fixed the Seeker class not appearing in the documentation due to a bug in Doxygen (documentation generator).

- 3.7.3 (2015-08-18)
	- Fixed GridGraph->Unwalkable When No Ground used the negated value (true meant false and false meant true).
		This bug was introduced in 3.7 when some code was refactored. Thanks DrowningMonkeys for reporting it.

- 3.7.2 (2015-08-06)
	- Fixed penalties not working on navmesh based graphs (navmesh graphs and recast graphs) due to incorrectly configured compiler directives.
	- Removed undocumented compiler directive ASTAR_CONSTANT_PENALTY and replaced with ASTAR_NO_TRAVERSAL_COST which
		can strip out code handling penalties to get slightly better pathfinding performance (still not documented though as it is not really a big performance boost).

- 3.7.1 (2015-08-01)
	- Removed a few cases where exceptions where needed to better support WebGL when exception handling is disabled.
	- Fixed MultiTargetPath could return the wrong path if the target of the path was the same as the start point.
	- Fixed MultiTargetPath could sometimes throw exceptions when using more than one pathfinding thread.
	- MultiTargetPath will now set path and vectorPath to the shortest path even if pathsForAll is true.
	- The log output for MultiTargetPath now contains the length (in nodes) of the shortest path.
	- Fixed RecastGraph throwing exceptions when trying to rasterize trees with missing (null) prefabs. Now they will simply be ignored.
	- Removed RecastGraph.bbTree since it was not used for anything (bbTrees are stored inside each tile since a few versions)
	- Improved performance of loading and updating large recast graph tiles (improved performance of internal AABB tree).
	- Removed support for the compiler directive ASTAR_OLD_BBTREE.

- 3.7 (2015-07-22)
	- The last version that was released on the Unity Asset Store
	  was version 3.6 so if you are upgrading from that version also check out the release
	  notes for 3.6.1 through 3.6.7.
	- Upgrade notes
		- ProceduralGridMover.updateDistance is now in nodes instead of world units since this value
		   is a lot less world scale dependant. So the defaults should fit more cases.
		   You may have to adjust it slightly.
		- Some old parts of the API that has been marked as deprecated long ago have been removed (see below).
		   Some other unused parts of the API that mostly lead to confusion have been removed as well.
	- Improvements
		- Rewrote several documentation pages to try to explain concepts better and fixed some old code.
			- \ref accessing-data
			- \ref graph-updates
			- \ref writing-graph-generators
			- Pathfinding.NavmeshCut
			- And some other smaller changes.
		- Added an overload of Pathfinding.PathUtilities.IsPathPossible which takes a tag mask.
		- \link Pathfinding.XPath XPath \endlink now works again.
		- The ProceduralGridMover component now supports rotated graphs (and all other ways you can transform it, e.g isometric angle and aspect ratio).
		- Rewrote GridGraph.Linecast to be more accurate and more performant.
			Previously it used a sampling approach which could cut corners of obstacles slightly and was pretty inefficient.
		- Linted lots of files to remove trailing whitespace, fix imports, use 'var' when relevant and various other small tweaks.
		- Added AstarData.layerGridGraph shortcut.
	- Fixes
		- Fixed compilation errors for Windows Store.
			The errors mentioned ThreadPriority and VolatileRead.
		- Fixed LayerGridGraph.GetNearest sometimes returning the wrong node inside a cell (e.g sometimes it would always return the node with the highest y coordinate).\n
			This did not happen when the node size was close to 1 and the grid was positioned close to the origin.
			Which it of course was in all my tests (tests are improved now).
		- Fixed GridGraph.Linecast always returning false (no obstacles) when the start point and end point was the same.
			Now it returns true (obstacle) if the start point was inside an obstacle which makes more sense.
		- Linecasts on layered grid graphs now use the same implementation as the normal grid graph.\n
			This fixed a TON of bugs. If you relied on the old (buggy) behaviour you might have to change your algorithms a bit.
			It will now report more accurate hit information as well.
		- Fixed documentation on LayerGridGraph.Linecast saying that it would return false if there was an obstacle in the way
			when in fact exactly the opposite was true.
		- Fixed inspector GUI throwing exceptions when two or more grid graphs or layered grid graphs were visible and thickRaycast was enabled on only one of them.
		- Fixed a few options only relevant for grid graphs were visible in the layered grid graph inspector as well.
		- Fixed GridGraph.CheckConnection returned the wrong result when neighbours was Four and dir was less than 4.
		- All compiler directives in the Optimizations tab are now tested during the package build phase. So hopefully none of them should give compiler errors now.
		- Improved accuracy of intellisense by changing the start of some documentation comments to /** instead of /**< as the later type is handled well by doxygen
			but apparently not so well by MonoDevelop and VS.
		- Fixed the editor sometimes incorrectly comparing versions which could cause the 'New Update' window to appear even though no new version was available.
	- Changes
		- Removed code only necessary for compatibility with Unity 4.5 and lower.
		- Removed a lot of internal unused old code.
		- Renamed GridGraph.GetNodePosition to GridGraph.GraphPointToWorld to avoid confusion.
		- Renamed 3rd party plugin license files to prevent the Unity Asset Store
			from detecting those as the license for the whole package.
		- Changed Seeker.traversableTags to be a simple int instead of a class.
		- GridNode and LevelGridNode now inherit from a shared base class called GridNodeBase.
		- Removed support for the compiler directive ConfigureTagsAsMultiple since it was not supported by the whole codebase
			and it was pretty old.
		- Marked a few methods in AstarData as deprecated since they used strings instead of types.
			If string to type conversion is needed it should be done elsewhere.
		- Removed some methods which have been marked as obsolete for a very long time.
			- AstarData.GetNode
			- PathModifier and MonoModifier.ApplyOriginal
			- Some old variants of PathModifier.Apply
			- GridGeneratorEditor.ResourcesField
			- Int3.safeMagnitude and safeSqrMagnitude
			- GraphUpdateUtilities.IsPathPossible (this has been since long been moved to the PathUtilities class)
			- All constructors on path classes. The static Construct method should be used instead since that can handle path pooling.
			- GraphNode.Position, walkable, tags, graphIndex. These had small changes made to their names (if they use upper- or lowercase letters) a long time ago.
				(for better or for worse, but I want to avoid changing the names now again to avoid breaking peoples' code)
			- GridNode.GetIndex.
		- Removed the Node class which has been marked as obsolete a very long time. This class has been renamed to GraphNode to avoid name conflicts.
		- Removed LocalAvoidanceMover which has been marked as obsolete a very long time. The RVO system has replaced it.
		- Removed Seeker.ModifierPass.PostProcessOriginal since it was not used. This also caused Seeker.postProcessOriginalPath to be removed.
		- Removed support for ASTAR_MORE_PATH_IDS because it wasn't really useful, it only increased the memory usage.
		- Removed Path.height, radius, turnRadius, walkabilityMask and speed since they were dummy variables that have not been used and are
			better implemented using inheritance anyway. This is also done to reduce confusion for users.
		- Removed the old local avoidance system which has long since been marked as obsolete and replaced by the RVO based system.

- 3.6.7 (2015-06-08)
	- Fixes
		- Fixed a race condition when OnPathPreSearch and OnPathPostSearch were called.
			When the AlternativePath modifier was used, this could cause the pathfinding threads to crash with a null reference exception.

- 3.6.6 (2015-05-27)
	- Improvements
		- Point Graphs are now supported when using ASTAR_NO_JSON.
		- The Optimizations tab now modifies the player settings instead of changing the source files.
			This is more stable and your settings are now preserved even when you upgrade the system.
		- The Optimizations tab now works regardless of the directory you have installed the package in.
			Hopefully the whole project is now directory agnostic, but you never know.
	- Changes
		- Switched out OnVoidDelegate for System.Action.
			You might get a compiler error because of this (for the few that use it)
			but then just rename your delegate to System.Action.
	- Fixes
		- Fixed recast graphs not saving all fields when using ASTAR_NO_JSON.

- 3.6.5 (2015-05-19)
	- Fixes
		- Fixed recast graphs generating odd navmeshes on non-square terrains.
		- Fixed serialization sometimes failing with the error 'Argument cannot be null' when ASTAR_NO_JSON was enabled.
		- The 'Walkable Climb' setting on recast graphs is now clamped to be at most equal to 'Walkable Height' because
			otherwise the navmesh generation can fail in some rare cases.
	- Changes
		- Recast graphs now show unwalkable nodes with a red outline instead of their normal colors.

- 3.6.4 (2015-04-19)
	- Fixes
		- Improved compatibility with WIIU and other big-endian platforms.

- 3.6.3 (2015-04-19)
	- Fixes
		- Fixed RVONavmesh not adding obstacles correctly (they were added added, but all agents ignored them).

- 3.6.2 (2015-04-14)
	- Fixes
		- Fixed null reference exception in the PointGraph OnDrawGizmos method.
		- Fixed a few example scene errors in Unity 5.

- 3.6.1 (2015-04-06)
	- Upgrade notes:
		- The behaviour of NavGraph.RelocateNodes has changed.
			The oldMatrix was previously treated as the newMatrix and vice versa so you might
			need to switch the order of your parameters if you are calling it.
	- Highlights:
		- Works in WebGL/IL2CPP (Unity 5.0.0p3).
			At least according to my limited tests.
		- Implemented RelocateNodes for recast graphs (however it cannot be used on tiled recast graphs).
		- Added support for hexagon graphs.
			Enable it by changing the 'Connections' field on a grid graph to 'Six'.
		- Fixed AstarData.DeserializeGraphsAdditive (thanks tmcsweeney).
		- Fixed pathfinding threads sometimes not terminating correctly.
			This would show up as a 'Could not terminate pathfinding thread...' error message.
		- Added a version of GridGraph.RelocateNodes which takes grid settings instead of a matrix for ease of use.
	- Changes:
		- Removed NavGraph.SafeOnDestroy
		- Removed GridGraph.scans because it is a pretty useless variable.
		- Removed NavGraph.CreateNodes (and overriden methods) since they were not used.
		- Made GridGraph.RemoveGridGraphFromStatic private.
		- Removed NavMeshGraph.DeserializeMeshNodes since it was not used.
		- Made Seeker.lastCompletedVectorPath, lastCompletedNodePath, OnPathComplete, OnMultiPathComplete, OnPartialPathComplete
			private since they really shouldn't be used by other scripts.
		- Removed Seeker.saveGetNearestHints, Seeker.startHint, Seeker.endHint, Seeker.DelayPathStart since they were not used.
		- Removed unused methods of little use: AstarData.GuidToIndex and AstarData.GuidToGraph.
		- Removed RecastGraph.vertices and RecastGraph.vectorVertices since they were obsolete and not used.
		- Removed some old Unity 4.3 and Unity 3 compatibility code.
		- Recast graphs' 'Snap to scene' button now takes into account the layer mask and the tag mask when snapping, it now also checks terrains and colliders instead of just meshes (thanks Kieran).
	- Fixes:
		- Fixed RecastGraph bounds gizmos could sometimes be drawn with the wrong color.
		- Fixed a rare data race which would cause an exception with the message
			'Trying to initialize a node when it is not safe to initialize any nodes' to be thrown
		- Tweaked Undo behaviour, should be more stable now.
		- Fixed grid graph editor changing the center field very little every frame (floating point errors)
			causing an excessive amount of undo items to be created.
		- Reduced unecessary dirtying of the scene (thanks Ben Hymers).
		- Fixed RVOCoreSimulator.WallThickness (thanks tmcsweeney).
		- Fixed recast graph not properly checking for the case where an object had a MeshFilter but no Renderer (thanks 3rinJax).
		- Fixed disabling ASTAR_RECAST_ARRAY_BASED_LINKED_LIST (now ASTAR_RECAST_CLASS_BASED_LINKED_LIST) would cause compiler errors.
		- Fixed recast graphs could sometimes voxelize the world incorrectly and the resulting navmesh would have artifacts.
		- Fixed graphMask code having been removed from the free version in some cases
			due to old code which treated it as a pro only feature.
		- Improved compatibility with Xbox One.
		- Fixed RVOController layer field not working when multiple agents were selected.
		- Fixed grid nodes not being able to have custom connections in the free version.
		- Fixed runtime error on PS4.

- 3.6 (2015-02-02)
	- Upgrade notes:
		- Cache data for faster startup is now stored in a separate file.\n
			This reduces the huge lag some users have been experiencing since Unity changed their Undo system.\n
			You will need to open the AstarPath components which used cached startup, go to the save and load tab
			and press a button labeled "Transfer cache data to a separate file".
	- Highlights:
		- Added support for the Jump Point Search algorithm on grid graphs (pro only).\n
			The JPS algorithm can be used to speed up pathfinding on grid graphs *without any penalties or tag weights applied* (it only works on uniformly weighted graphs).
			It can be several times faster than normal A*.
			It works best on open areas.
		- Added support for heuristic optimizations (pro only).\n
			This can be applied on any static graph, i.e any graph which does not change.
			It requires a rather slow preprocessing step so graph updates will be really slow when using this.
			However when the preprocessing is done, it can speed up pathfinding with an order of magnitude.
			It works especially well in mazes with lots of options and dead ends.\n
			Combined with JPS (mentioned above) I have seen it perform up to 20x better than regular A* with no heuristic optimizations.
		- Added PointNode.gameObject which will contain the GameObject each node was created from.
		- Added support for RVO obstacles.\n
			It is by no means perfect at this point, but at least it works.
		- Undo works reasonably well again.\n
			It took a lot of time working around weird Unity behaviours.
			For example Unity seems to send undo events when dragging items to object fields (why? no idea).
		- Dragging meshes to the NavmeshGraph.SourceMesh field works again.\n
			See fix about undo above.
		- Extended the max number of possible areas (connected components) to 2^17 = 131072 up from 2^10 = 1024.\n
			No memory usage increase, just shuffling bits around.\n
			Deprecated compiler directive ASTAR_MORE_AREAS
		- Extended the max number of graphs in the inspector to 256 up from 4 or 32 depending on settings.\n
			No memory usage increase, just shuffling bits around.
			I still don't recommend that you actually use this many graphs.
		- Added RecastTileUpdate and RecastTileUpdateHandler scripts for easier recast tile updating with good performance.
		- When using A* Inspector -> Settings -> Debug -> Path Debug Mode = {G,F,H,Penalties}
			you previously had to set the limits for what should be displayed as "red" in the scene view yourself, this is now
			optionally automatically calculated. The UI for it has also been improved.
	- Improvements:
		- Added penaltyAnglePower to Grid Graph -> Extra -> Penalty from Angle.\n
			This can be used to increase the penalty even more for large angles than for small angles (more than it already does, that is).
		- ASTAR_NO_JSON now works for recast graphs as well.
		- Added custom inspector for RecastMeshObj, hopefully it will not be as confusing anymore.
	- Changes:
		- FleePath now has a default flee strength of 1 to avoid confusion when the FleePath doesn't seem to flee from anything.
		- Removed some irrelevant defines from the Optimizations tab.
		- IAgent.Position cannot be changed anymore, instead use the Teleport and SetYPosition methods.
		- Exposed GraphUpdateObject.changedNodes.
		- Deprecated the threadSafe paremeter on RegisterSafeUpdate, it is always treated as true now.
		- The default value for AstarPath.minAreaSize is now 0 since the number of areas (connected component) indices has been greatly increased (see highlights).
		- Tweaked ProceduralWorld script (used for the "Procedural" example scene) to reduce FPS drops.
	- Fixes:
		- AstarPath.FlushGraphUpdates will now complete all graph updates instead of just making sure they have started.\n
			In addition to avoiding confusion, this fixes a rare null reference exception which could happen when using
			the GraphUpdateUtilities.UpdateGraphsNoBlock method.
		- Fixed some cases where updating recast graphs could throw exceptions. (message begun with "No Voxelizer object. UpdateAreaInit...")
		- Fixed typo in RVOSimulator. desiredSimulatonFPS -> desiredSimulationFPS.
		- RVO agents move smoother now (previously their velocity could change widely depending on the fps, the average velocity was correct however)
		- Fixed an exception which could, with some graph settings, be thrown when deserializing on iPhone when bytecode stripping was enabled.
		- Fixed a NullReferenceException in MultiTargetPath which was thrown if the path debug mode was set to "Heavy".
		- Fixed PathUtilies.BFS always returning zero nodes (thanks Ajveach).
		- Made reverting GraphUpdateObjects work. The GraphUpdateUtilities.UpdateGraphsNoBlock was also fixed by this change.
		- Fixed compile error with monodevelop.
		- Fixed a bug which caused scanning to fail if more than one NavmeshGraph existed.
		- Fixed the lightweight local avoidance example scene which didn't work previously.
		- Fixed SimpleSmoothModifier not exposing Roundness Factor in the editor for the Curved Nonuniform mode.
		- Fixed an exception when updating RecastGraphs and using RelevantGraphSurfaces and multithreading.
		- Fixed exceptions caused by starting paths from other threads than the Unity thread.
		- Fixed an infinite loop/out of memory exception that could occur sometimes when graph updates were being done at the start of the game (I hate multithreading race conditions).
		- Fixed the Optimizations tab not working when JS Support was enabled.
		- Fixed graph updating not working on navmesh graphs (it was broken before due to a missing line of code).
		- Fixed some misspelled words in the documentation.
		- Removed some unused and/or redundant variables.
		- Fixed a case where graphs added using code might not always be configured correctly (and would throw exceptions when scanning).
		- Improved Windows Store compatibility.
		- Fixed a typo in the GridGraph which could cause compilation to fail when building for Windows Phone or Windows Store (thanks MariuszP)
		- Lots of code cleanups and comments added to various scripts.
		- Fixed some cases where MonoDevelop would pick up the wrong documention for fields since it doesn't support all features that Doxygen supports.
		- Fixed a bug which caused the points field on GraphUpdateScene to sometimes not be editable.
		- Fixed a bug which could cause RVO agents not to move if the fps was low and Interpolation and Double Buffering was used.
		- Set the execution order for RVOController and RVOSimulator to make sure that other scripts will
			get the latest position in their Update method.
		- Fixed a bug which could cause some nearest point on line methods in AstarMath to return NaN.
			This could happen when Seeker->Start End Modifier->StartPoint and EndPoint was set to Interpolate.
		- Fixed a runtime error on PS Vita.
		- Fixed an index out of range exception which could occur when scanning LayeredGridGraphs.
		- Fixed an index out of range exception which could occur when drawing gizmos for a LayeredGridGraph.
		- Fixed a bug which could cause ProduralGridMover to update the graph every frame regardless
		  of if the target moved or not (thanks Makak for finding the bug).
		- Fixed a number of warnings in Unity 5.

- 3.5.9.7 (3.6 beta 6, 2015-01-28)
- 3.5.9.6 (3.6 beta 5, 2015-01-28)
- 3.5.9.5 (3.6 beta 4, 2015-01-27)
- 3.5.9.1 (3.6 beta 3, 2014-10-14)
- 3.5.9   (3.6 beta 2, 2014-10-13)
- 3.5.8   (3.6 beta 1)
	 - See release notes for 3.6

- 3.5.2 (2013-09-01) (tiny bugfix and small feature release)
	- Added isometric angle option for grid graphs to help with isometric 2D games.
	- Fixed a bug with the RVOAgent class which caused the LightweightRVO example scene to not work as intended (no agents were avoiding each other).
	- Fixed some documentation typos.
	- Fixed some compilations errors some people were having with other compilers than Unity's.

- 3.5.1 (2014-06-15)
	- Added avoidance masks to local avoidance.
		Each agent now has a layer and each agent can specify which layers it will avoid.

- 3.5 (2014-06-12)
	- Added back local avoidance!!
		The new system uses a sampling based algorithm instead of a geometric one.
		The API is almost exactly the same so if you used the previous system this will be a drop in replacement.
		As for performance, it is roughly the same, maybe slightly worse in high density situations and slightly better
		in less dense situations. It can handle several thousand agents on an i7 processor.
		Obstacles are not yet supported, but they will be added in a future update.

	- Binary heap switched out for a 4-ary heap.
		This improves pathfinding performances by about 5%.
	- Optimized scanning of navmesh graphs (not the recast graphs)
		Large meshes should be much faster to scan now.
	- Optimized BBTree (nearest node lookup for navmesh/recast graphs, pro version only)
		Nearest node queries on navmesh/recast graphs should be slightly faster now.
	- Minor updates to the documentation, esp. to the GraphNode class.

- 3.4.0.7
	- Vuforia test build

- 3.4.0.6
	- Fixed an issue where serialization could on some machines sometimes cause an exception to get thrown.
	- Fixed an issue where the recast graph would not rasterize terrains properly near the edges of it.
	- Added PathUtilities.BFS.
	- Added PathUtilities.GetPointsAroundPointWorld.

- 3.4.0.5
	- Added offline documentation (Documentation.zip)
	- Misc fixes for namespace conflicts people have been having. This should improve compatibility with other packages.
		You might need to delete the AstarPathfindingProject folder and reimport the package for everything to work.

- 3.4.0.4
	- Removed RVOSimulatorEditor from the free version, it was causing compiler errors.
	- Made PointGraph.nodes public.

- 3.4.0.3
	- Removed Local Avoidance due to licensing issues.
		Agents will fall back to not avoiding each other.
		I am working to get the local avoidance back as soon as possible.

- 3.4.0.2
	- Unity Asset Store forced me to increase version number.

- 3.4.0.1
	- Fixed an ArrayIndexOutOfBounds exception which could be thrown by the ProceduralGridMover script in the Procedural example scene if the target was moved too quickly.
	- The project no longer references assets from the Standard Assets folder (the package on the Unity Asset Store did so by mistake before).

- 3.4
	- Fixed a null reference exception when scanning recast graphs and rasterizing colliders.
	- Removed duplicate clipper_library.dll which was causing compiler errors.
	- Support for 2D Physics collision testing when using Grid Graphs.
	- Better warnings when using odd settings for Grid Graphs.
	- Minor cleanups.
	- Queued graph updates are no longer being performed when the AstarPath object is destroyed, this just took time.
	- Fixed a bug introduced in 3.3.11 which forced grid graphs to be square in Unity versions earlier than 4.3.
	- Fixed a null reference in BBTree ( used by RecastGraph).
	- Fixed NavmeshGraph not rebuilding BBTree on cached start (causing performance issues on larger graphs).

	- Includes all changes from the beta releases below

- Beta 3.3.14 ( available for everyone! )
	- All dlls are now in namespaces (e.g Pathfinding.Ionic.Zip instead of just Ionic.Zip ) to avoid conflicts with other packages.
	- Most scripts are now in namespaces to avoid conflicts with other packages.
	- GridNodes now support custom connections.
	- Cleanups, preparing for release.
	- Reverted to using an Int3 for GraphNode.position instead of an abstract Position property, the tiny memory gains were not worth it.

- Beta 3.3.13 ( 4.3 compatible only )
	- Fixed an issue where deleting a NavmeshCut component would not update the underlaying graph.
	- Better update checking.

- Beta 3.3.12 ( 4.3 compatible only )
	- Fixed an infinite loop which could happen when scanning graphs during runtime ( not the first scan ).
	- NodeLink component is now working correctly.
	- Added options for optimizations to the PointGraph.
	- Improved TileHandler and navmesh cutting.
	- Fixed rare bug which could mess up navmeshes when using navmesh cutting.

- Beta 3.3.11 ( 4.3 compatible only )
	- Fixed update checking. A bug has caused update checking not to run unless you had been running a previous version in which the bug did not exist.
		I am not sure how long this bug has been here, but potentially for a very long time.
	- Added an update notification window which pops up when there is a new version of the A* Pathfinding Project.
	- Lots of UI fixes for Unity 4.3
	- Lots of other UI fixes and imprements.
	- Fixed gravity for RichAI.
	- Fixed Undo for Unity 4.3
	- Added a new example scene showing a procedural environment.

- Beta 3.3.10
	- Removed RecastGraph.includeOutOfBounds.
	- Fixed a few bugs when updating Layered Grid Graphs causing incorrect connections to be created, and valid ones to be left out.
	- Fixed a null reference bug when removing RVO agents.
	- Fixed memory leaks when deserializing graphs or reloading scenes.

- Beta 3.3.9
	- Added new tutorial page about recast graphs.
	- Recast Graph: Fixed a bug which could cause vertical surfaces to be ignored.
	- Removed support for C++ Recast.
	- Fixed rare bug which could mess up navmeshes when using navmesh cutting.
	- Improved TileHandler and navmesh cutting.
	- GraphModifiers now take O(n) (linear) time to destroy at end of game instead of O(n^2) (quadratic).
	- RecastGraph now has a toggle for using tiles or not.
	- Added RelevantGraphSurface which can be used with RecastGraphs to prune away non-relevant surfaces.
	- Removed RecastGraph.accurateNearestNode since it was not used anymore.
	- Added RecastGraph.nearestSearchOnlyXZ.
	- RecastGraph now has support for removing small areas.
	- Added toggle to show or hide connections between nodes on a recast graph.
	- PointNode has some graph searching methods overloaded specially. This increases performance and reduces alloacations when searching
		point graphs.
	- Reduced allocations when searching on RecastGraph.
	- Reduced allocations in RichAI and RichPath. Everything is pooled now, so for most requests no allocations will be done.
	- Reduced allocations in general by using "yield return null" instead of "yield return 0"
	- Fixed teleport for local avoidance agents. Previously moving an agent from one position to another
		could cause it to interpolate between those two positions for a brief amount of time instead of staying at the second position.

- Beta 3.3.8
	- Nicer RichAI gizmo colors.
	- Fixed RichAI not using raycast when no path has been calculated.

- Beta 3.3.7
	- Fixed stack overflow exception in RichPath
	- Fixed RichPath could sometimes generate invalid paths
	- Added gizmos to RichAI

- Beta 3.3.6
	- Fixed node positions being off by half a node size. GetNearest node queries on grid graphs would be slightly inexact.
	- Fixed grid graph updating could get messed up when using erosion.
	- ... among other things, see below

- Beta 3.3.5 and 3.3.6
	- Highlights
		- Rewritten graph nodes. Nodes can now be created more easily (less overhead when creating nodes).
		- Graphs may use their custom optimized memory structure for storing nodes.
		- Performance improvements for scanning recast graphs.
		- Added a whole new AI script. RichAI (and the class RichPath for some things):
			This script is intended for navmesh based graphs and has features such as:
			- Guarantees that the character stays on the navmesh
			- Minor deviations from the path can be fixed without a path recalculation.
			- Very exact stop at endpoint (seriously, precision with something like 7 decimals).
				No more circling around the target point as with AIPath.
			- Does not use path modifiers at all (for good reasons). It has an internal funnel modifier however.
			- Simple wall avoidance to avoid too much wall hugging.
			- Basic support for off-mesh links (see example scene).
		- Improved randomness for RandomPath and FleePath, all nodes considered now have an equal chance of being selected.
		- Recast now has support for tiles. This enabled much larger worlds to be rasterized (without OutOfMemory errors) and allows for dynamic graph updates. Still slow, but much faster than
			a complete recalculation of the graph.
		- Navmesh Cutting can now be done on recast graphs. This is a kind of (relatively) cheap graph updating which punches a hole in the navmesh to make place for obstacles.
			So it only supports removing geometry, not adding it (like bridges). This update is comparitively fast, and it makes real time navmesh updating possible.
			See video: http://youtu.be/qXi5qhhGNIw.
		- Added RecastMeshObj which can be attached to any GameObject to include that object in recast rasterization. It exposes more options and is also
			faster for graph updates with logarithmic lookup complexity instead of linear (good for larger worlds when doing graph updating).
		- Reintroducing special connection costs for start and end nodes.
			Before multithreading was introduced, pathfinding on navmesh graphs could recalculate
			the connection costs for the start and end nodes to take into account that the start point is not actually exactly at the start node's position
			(triangles are usually quite a larger than the player/npc/whatever).
			This didn't work with multithreading however and could mess up pathfinding, so it was removed.
			Now it has been reintroduced, working with multithreading! This means more accurate paths
			on navmeshes.
		- Added several methods to pick random points (e.g for group movement) to Pathfinding.PathUtlitilies.
		- Added RadiusModifier. A new modifier which can offset the path based on the character radius. Intended for navmesh graphs
			which are not shrinked by the character radius at start but can be used for other purposes as well.
		- Improved GraphUpdateScene gizmos. Convex gizmos are now correctly placed. It also shows a bounding box when selected (not showing this has confused a lot of people).
		- AIPath has gotten some cleanups. Among other things it now behaves correctly when disabled and then enabled again
			making it easy to pool and reuse (should that need arise).
		- Funnel modifier on grid graphs will create wider funnels for diagonals which results in nicer paths.
		- If an exception is thrown during pathfinding, the program does no longer hang at quit.
		- Split Automatic thread count into Automatic High Load and Automatic Low Load. The former one using a higher number of thread.
		- Thread count used is now shown in the editor.
		- GridGraph now supports ClosestOnNode (StartEndModifier) properly. SnapToNode gives the previous behaviour on GridGraphs (they were identical before).
		- New example scene Door2 which uses the NavmeshCut component.
	- Fixes
		- Fixed spelling error in GridGraph.uniformWidthDepthGrid.
		- Erosion radius (character radius, recast graphs) could become half of what it really should be in many cases.
		- RecastGraph will not rasterize triggers.
		- Fixed recast not being able to handle multiple terrains.
		- Fixed recast generating an incorrect mesh for terrains in some cases (not the whole terrain was included).
		- Linecast on many graph types had incorrect descriptions saying that the function returns true when the line does not intersect any obstacles,
			it is actually the other way around. Descriptions corrected.
		- The list of nodes returned by a ConstantPath is now guaranteed to have no duplicates.
		- Many recast constants are now proper constants instead of static variables.
		- Fixed bug in GridNode.RemoveGridGraph which caused graphs not being cleaned up correctly. Could cause problems later on.
		- Fixed an ArgumentOutOfRange exception in ListPool class.
		- RelocateNodes on NavMeshGraph now correctly recalculates connection costs and rebuilds the internal query tree (thanks peted on the forums).
		- Much better member documentation for RVOController.
		- Exposed MaxNeighbours from IAgent to RVOController.
		- Fixed AstarData.UpdateShortcuts not being called when caching was enabled. This caused graph shortcuts such as AstarPath.astarData.gridGraph not being set
			when loaded from a cache.
		- RVOCoreSimulator/RVOSimulator now cleans up the worker threads correctly.
		- Tiled recast graphs can now be serialized.
	- Changes
		- Renamed Modifier class to PathModifier to avoid naming conflicts with user scripts and other packages.
		- Cleaned up recast, put inside namespace and split into multiple files.
		- ListPool and friends are now threadsafe.
		- Removed Polygon.Dot since the Vector3 class already contains such a method.
		- The Scan functions now use callbacks for progress info instead of IEnumerators. Graphs can now output progress info as well.
		- Added Pathfinding.NavGraph.CountNodes function.
		- Removed GraphHitInfo.success field since it was not used.
		- GraphUpdateScene will now fall back to collider.bounds or renderer.bounds (depending on what is available) if no points are
			defined for the shape.
		- AstarPath.StartPath now has an option to put the path in the front of the queue to prioritize its calculation over other paths.
		- Time.fixedDeltaTime by Time.deltaTime in AIPath.RotateTowards() to work with both FixedUpdate and Update. (Thanks Pat_AfterMoon)
			You might have to configure the turn speed variable after updating since the actual rotation speed might have changed a bit depending on your settings.
		- Fixed maxNeighbourDistance not being used correctly by the RVOController script. It would stay at the default value. If you
			have had trouble getting local avoidance working on world with a large scale, this could have been the problem. (Thanks to Edgar Sun for providing a reproducible example case)
		- Graphs loaded using DeserializeGraphsAdditive will get their graphIndex variables on the nodes set to the correct values. (thanks peted for noticing the bug).
		- Fixed a null reference exception in MultiTargetPath (thanks Dave for informing me about the bug).
		- GraphUpdateScene.useWorldSpace is now false per default.
		- If no log output is disabled and we are not running in the editor, log output will be discarded as early as possible for performance.
			Even though in theory log output could be enabled between writing to internal log strings and deciding if log output should be written.
		- NavGraph.inverseMatrix is now a field, not a property (for performance). All writes to matrix should be through the SetMatrix method.
		- StartEndModifier now uses ClosestOnNode for both startPoint and endPoint by default.
	- Known bugs
		- Linecasting on graphs is broken at the moment. (working for recast/navmesh graph atm. Except in very special cases)
		- RVONavmesh does not work with tiled recast graphs.



- 3.2.5.1
	- Fixes
		- Pooling of paths had been accidentally disabled in AIPath.

- 3.2.5
	- Changes
		- Added support for serializing dictionaries with integer keys via a Json Converter.
		- If drawGizmos is disabled on the seeker, paths will be recycled instantly.
			This will show up so that if you had a seeker with drawGizmos=false, and then enable
			drawGizmos, it will not draw gizmos until the next path request is issued.
	- Fixes
		- Fixed UNITY_4_0 preprocesor directives which were indented for UNITY 4 and not only 4.0.
			Now they will be enabled for all 4.x versions of unity instead of only 4.0.
		- Fixed a path pool leak in the Seeker which could cause paths not to be released if a seeker
			was destroyed.
		- When using a non-positive maxDistance for point graphs less processing power will be used.
		- Removed unused 'recyclePaths' variable in the AIPath class.
		- NullReferenceException could occur if the Pathfinding.Node.connections array was null.
		- Fixed NullReferenceException which could occur sometimes when using a MultiTargetPath (Issue #16)
		- Changed Ctrl to Alt when recalcing path continously in the Path Types example scene to avoid
			clearing the points for the MultiTargetPath at the same time (it was also using Ctrl).
		- Fixed strange looking movement artifacts during the first few frames when using RVO and interpolation was enabled.
		- AlternativePath modifier will no longer cause underflows if penalties have been reset during the time it was active. It will now
			only log a warning message and zero the penalty.
		- Added Pathfinding.GraphUpdateObject.resetPenaltyOnPhysics (and similar in GraphUpdateScene) to force grid graphs not to reset penalties when
			updating graphs.
		- Fixed a bug which could cause pathfinding to crash if using the preprocessor directive ASTAR_NoTagPenalty.
		- Fixed a case where StartEndModifier.exactEndPoint would incorrectly be used instead of exactStartPoint.
		- AlternativePath modifier now correctly resets penalties if it is destroyed.

- 3.2.4.1
	- Unity Asset Store guys complained about the wrong key image.
		I had to update the version number to submit again.

- 3.2.4
	- Highlights
		- RecastGraph can now rasterize colliders as well!
		- RecastGraph can rasterize colliders added to trees on unity terrains!
		- RecastGraph will use Graphics.DrawMeshNow functions in Unity 4 instead of creating a dummy GameObject.
			This will remove the annoying "cleaning up leaked mesh object" debug message which unity would log sometimes.
			The debug mesh is now also only visible in the Scene View when the A* object is selected as that seemed
			most logical to me (don't like this? post something in the forum saying you want a toggle for it and I will implement
			one).
		- GraphUpdateObject now has a \link Pathfinding.GraphUpdateObject.updateErosion toggle \endlink specifying if erosion (on grid graphs) should be recalculated after applying the guo.
			This enables one to add walkable nodes which should have been made unwalkable by erosion.
		- Made it a bit easier (and added more correct documentation) to add custom graph types when building for iPhone with Fast But No Exceptions (see iPhone page).
	- Changes
		- RecastGraph now only rasterizes enabled MeshRenderers. Previously even disabled ones would be included.
		- Renamed RecastGraph.includeTerrain to RecastGraph.rasterizeTerrain to better match other variable naming.
	- Fixes
		- AIPath now resumes path calculation when the component or GameObject has been disabled and then reenabled.

- 3.2.3 (free version mostly)
	- Fixes
		- A UNITY_IPHONE directive was not included in the free version. This caused compilation errors when building for iPhone.
	- Changes
		- Some documentation updates

- 3.2.2
	- Changes
		- Max Slope in grid graphs is now relative to the graph's up direction instead of world up (makes more sense I hope)
	- Note
		- Update really too small to be an update by itself, but I was updating the build scripts I use for the project and had to upload a new version because of technical reasons.

- 3.2.1
	- Fixes
		- Fixed bug which caused compiler errors on build (player, not in editor).
		- Version number was by mistake set to 3.1 instead of 3.2 in the previous version.

- 3.2
	- Highlights
		- A complete Local Avoidance system is now included in the pro version!
		- Almost every allocation can now be pooled. Which means a drastically lower allocation rate (GC get's called less often).
		- Initial node penalty per graph can now be set.
			Custom graph types implementing CreateNodes must update their implementations to properly assign this value.
		- GraphUpdateScene has now many more tools and options which can be used.
		- Added Pathfinding.PathUtilities which contains some usefull functions for working with paths and nodes.
		- Added Pathfinding.Node.GetConnections to enable easy getting of all connections of a node.
			The Node.connections array does not include custom connections which for example grid graphs use.
		- Seeker.PostProcess function was added for easy postprocessing of paths calculated without a seeker.
		- AstarPath.WaitForPath. Wait (block) until a specific path has been calculated.
		- Path.WaitForPath. Wait using a coroutine until a specific path has been calculated.
		- LayeredGridGraph now has support for up to 65535 layers (theoretically, but don't try it as you would probably run out of memory)
		- Recast graph generation is now up to twice as fast!
		- Fixed some UI glitches in Unity 4.
		- Debugger component has more features and a slightly better layout.
	- Fixes
		- Fixed a bug which caused the SimpleSmoothModifier with uniformSegmentLength enabled to skip points sometimes.
		- Fixed a bug where importing graphs additively which had the same GUID as a graph already loaded could cause bugs in the inspector.
		- Fixed a bug where updating a GridGraph loaded from file would throw a NullReferenceException.
		- Fixed a bug which could cause error messages for paths not to be logged
		- Fixed a number of small bugs related to updating grid graphs (especially when using erosion as well).
		- Overflows could occur in some navmesh/polygon math related functions when working with Int3s. This was because the precision of them had recently been increased.
			Further down the line this could cause incorrect answers to GetNearest queries.
			Fixed by casting to long when necessary.
		- Navmesh2.shader defined "Cull Off" twice.
		- Pathfinding threads are now background threads. This will prevent them from blocking the process to terminate if they of some reason are still alive (hopefully at least).
 		- When really high penalties are applied (which could be underflowed negative penalties) a warning message is logged.
 			Really high penalties (close to max uint value) can otherwise cause overflows and in some cases infinity loops because of that.
 		- ClosestPointOnTriangle is now spelled correctly.
 		- MineBotAI now uses Update instead of FixedUpdate.
 		- Use Dark Skin option is now exposed again since it could be incorrectly set sometimes. Now you can force it to light or dark, or set it to auto.
 		- Fixed recast graph bug when using multiple terrains. Previously only one terrain would be used.
 		- Fixed some UI glitches in Unity 4.
	- Changes
		- Removed Pathfinding.NNInfo.priority.
		- Removed Pathfinding.NearestNodePriority.
		- Conversions between NNInfo and Node are now explicit to comply with the rule of "if information might be lost: use explicit casts".
		- NNInfo is now a struct.
		- GraphHitInfo is now a struct.
		- Path.vectorPath and Path.path are now List<Vector3> and List<Node> respectively. This is done to enable pooling of resources more efficiently.
		- Added Pathfinding.Node.RecalculateConnectionCosts.
		- Moved IsPathPossible from GraphUpdateUtilities to PathUtilities.
		- Pathfinding.Path.processed was replaced with Pathfinding.Path.state. The new variable will have much more information about where
			the path is in the pathfinding pipeline.
		- <b>Paths should not be created with constructors anymore, instead use the PathPool class and then call some Setup() method</b>
		- When the AstarPath object is destroyed, calculated paths in the return queue are not returned with errors anymore, but just returned.
		- Removed depracated methods AstarPath.AddToPathPool, RecyclePath, GetFromPathPool.
	- Bugs
		- C++ Version of Recast does not work on Windows.
		- GraphUpdateScene does in some cases not draw correctly positioned gizmos.
		- Starting two webplayers and closing down the first might cause the other one's pathfinding threads to crash (unity bug?) (confirmed on osx)

- 3.1.4 (iOS fixes)
	- Fixes
		- More fixes for the iOS platform.
		- The "JsonFx.Json.dll" file is now correctly named.
	- Changes
		- Removed unused code from DotNetZip which reduced the size of it with about 20 KB.

- 3.1.3 (free version only)
	- Fixes
		- Some of the fixes which were said to have been made in 3.1.2 were actually not included in the free version of the project. Sorry about that.
		- Also includes a new JsonFx and Ionic.Zip dll. This should make it possible to build with the .Net 2.0 Subset again see:
			http://www.arongranberg.com/forums/topic/ios-problem/page/1/

- 3.1.2 (small bugfix release)
	- Fixes
		- Fixed a bug which caused builds for iPhone to fail.
		- Fixed a bug which caused runtime errors on the iPhone platform.
		- Fixed a bug which caused huge lag in the editor for some users when using grid graphs.
		- ListGraphs are now correctly loaded as PointGraphs when loading data from older versions of the system.
	- Changes
		- Moved JsonFx into the namespace Pathfinding.Serialization.JsonFx to avoid conflicts with users own JsonFx libraries (if they used JsonFx).

	- Known bugs
		- Recast graph does not work when using static batching on any objects included.

- 3.1.1 (small bugfix release)
	- Fixes
		- Fixed a bug which would cause Pathfinding.GraphUpdateUtilities.UpdateGraphsNoBlock to throw an exception when using multithreading
		- Fixed a bug which caused an error to be logged and no pathfinding working when not using multithreading in the free version of the project
		- Fixed some example scene bugs due to downgrading the project from Unity 3.5 to Unity 3.4

- 3.1
	- Fixed bug which caused LayerMask fields (GridGraph inspector for example) to behave weirdly for custom layers on Unity 3.5 and up.
	- The color setting "Node Connection" now actually sets the colors of the node connections when no other information should be shown using the connection colors or when no data is available.
	- Put the Int3 class in a separate file.
	- Casting between Int3 and Vector3 is no longer implicit. This follows the rule of "if information might be lost: use explicit casts".
	- Renamed ListGraph to PointGraph. "ListGraph" has previously been used for historical reasons. PointGraph is a more suitable name.
	- Graph can now have names in the editor (just click the name in the graph list)
	- Graph Gizmos can now be selectively shown or hidden per graph (small "eye" icon to the right of the graph's name)
	- Added GraphUpdateUtilities with many useful functions for updating graphs.
	- Erosion for grid graphs can now use tags instead of walkability
	- Fixed a bug where using One Way links could in some cases result in a NullReferenceException being thrown.
	- Vector3 fields in the graph editors now look a bit better in Unity 3.5+. EditorGUILayout.Vector3Field didn't show the XYZ labels in a good way (no idea why)
	- GridGraph.useRaycastNormal is now enabled only if the Max Slope is less than 90 degrees. Previously it was a manual setting.
	- The keyboard shortcut to scan all graphs does now also work even when the graphs are not deserialized yet (which happens a lot in the editor)
	- Added NodeLink script, which can be attached to GameObjects to add manual links. This system will eventually replace the links system in the A* editor.
	- Added keyboard shortcuts for adding and removing links. See Menubar -> Edit -> Pathfinding
	\note Some features are restricted to Unity 3.5 and newer because of technical limitations in earlier versions (especially multi-object editing related features).


- 3.1 beta (version number 3.0.9.9 in Unity due to technical limitations of the System.Versions class)
	- Multithreading is now enabled in the free version of the A* Pathfinding Project!
	- Better support for graph updates called during e.g OnPostScan.
	- PathID is now used as a short everywhere in the project
	- G,H and penalty is now used as unsigned integers everywhere in the project instead of signed integers.
	- There is now only one tag per node (if not the \#define ConfigureTagsAsMultiple is set).
	- Fixed a bug which could make connections between graphs invalid when loading from file (would also log annoying error messages).
	- Erosion (GridGraph) can now be used even when updating the graph during runtime.
	- Fixed a bug where the GridGraph could return null from it's GetNearestForce calls which ended up later throwing a NullReferenceException.
	- FunnelModifier no longer warns if any graph in the path does not implement the IFunnelGraph interface (i.e have no support for the funnel algorithm)
	and instead falls back to add node positions to the path.
	- Added a new graph type : LayerGridGraph which works like a GridGraph, but has support for multiple layers of nodes (e.g multiple floors in a building).
	- ScanOnStartup is now exposed in the editor.
	- Separated temporary path data and connectivity data.
	- Rewritten multithreading. You can now run any number of threads in parallel.
	- To avoid possible infinite loops, paths are no longer returned with just an error when requested at times they should not (e.g right when destroying the pathfinding object)
	- Cleaned up code in AstarPath.cs, members are now structured and many obsolete members have been removed.
	- Rewritten serialization. Now uses Json for settings along with a small part hardcoded binary data (for performance and memory).
		This is a lot more stable and will be more forwards and backwards compatible.
		Data is now saved as zip files(in memory, but can be saved to file) which means you can actually edit them by hand if you want!
	- Added dependency JsonFx (modified for smaller code size and better compatibility).
	- Added dependency DotNetZip (reduced version and a bit modified) for zip compression.
	- Graph types wanting to serialize members must add the JsonOptIn attribute to the class and JsonMember to any members to serialize (in the JsonFx.Json namespace)
	- Graph types wanting to serialize a bit more data (custom), will have to override some new functions from the NavGraph class to do that instead of the old serialization functions.
	- Changed from using System.Guid to a custom written Guid implementation placed in Pathfinding.Util.Guid. This was done to improve compabitility with iOS and other platforms.
	Previously it could crash when trying to create one because System.Guid was not included in the runtime.
	- Renamed callback AstarPath.OnSafeNodeUpdate to AstarPath.OnSafeCallback (also added AstarPath.OnThreadSafeCallback)
	- MultiTargetPath would throw NullReferenceException if no valid start node was found, fixed now.
	- Binary heaps are now automatically expanded if needed, no annoying warning messages.
	- Fixed a bug where grid graphs would not update the correct area (using GraphUpdateObject) if it was rotated.
	- Node position precision increased from 100 steps per world unit to 1000 steps per world unit (if 1 world unit = 1m, that is mm precision).
		This also means that all costs and penalties in graphs will need to be multiplied by 10 to match the new scale.
		It also means the max range of node positions is reduced a bit... but it is still quite large (about 2 150 000 world units in either direction, that should be enough).
	- If Unity 3.5 is used, the EditorGUIUtility.isProSkin field is used to toggle between light and dark skin.
	- Added LayeredGridGraph which works almost the same as grid graphs, but support multiple layers of nodes.
	- \note Dropped Unity 3.3 support.

	 <b>Known Bugs:</b> The C++ version of Recast does not work on Windows

- Documentation Update
	- Changed from FixedUpdate to Update in the Get Started Guide. CharacterController.SimpleMove should not be called more than once per frame,
			so this might have lowered performance when using many agents, sorry about this typo.
- 3.0.9
	- The List Graph's "raycast" variable is now serialized correctly, so it will be saved.
	- List graphs do not generate connections from nodes to themselves anymore (yielding slightly faster searches)
	- List graphs previously calculated cost values for connections which were very low (they should have been 100 times larger),
		this can have caused searches which were not very accurate on small scales since the values were rounded to the nearest integer.
	- Added Pathfinding.Path.recalcStartEndCosts to specify if the start and end nodes connection costs should be recalculated when searching to reflect
		small differences between the node's position and the actual used start point. It is on by default but if you change node connection costs you might want to switch it off to get more accurate paths.
	- Fixed a compile time warning in the free version from referecing obsolete variables in the project.
	- Added AstarPath.threadTimeoutFrames which specifies how long the pathfinding thread will wait for new work to turn up before aborting (due to request). This variable is not exposed in the inspector yet.
	- Fixed typo, either there are eight (8) or four (4) max connections per node in a GridGraph, never six (6).
	- AlternativePath will no longer cause errors when using multithreading!
	- Added Pathfinding.ConstantPath, a path type which finds all nodes in a specific distance (cost) from a start node.
	- Added Pathfinding.FloodPath and Pathfinding.FloodPathTracer as an extreamly fast way to generate paths to a single point in for example TD games.
	- Fixed a bug in MultiTargetPath which could make it extreamly slow to process. It would not use much CPU power, but it could take half a second for it to complete due to excessive yielding
	- Fixed a bug in FleePath, it now returns the correct path. It had previously sometimes returned the last node searched, but which was not necessarily the best end node (though it was often close)
	- Using \#defines, the pathfinder can now be better profiled (see Optimizations tab -> Profile Astar)
	- Added example scene Path Types (mainly useful for A* Pro users, so I have only included it for them)
	- Added many more tooltips in the editor
	- Fixed a bug which would double the Y coordinate of nodes in grid graphs when loading from saved data (or caching startup)
	- Graph saving to file will now work better for users of the Free version, I had forgot to include a segment of code for Grid Graphs (sorry about that)
	- Some other bugfixes
- 3.0.8.2
	- Fixed a critical bug which could render the A* inspector unusable on Windows due to problems with backslashes and forward slashes in paths.
- 3.0.8.1
	- Fixed critical crash bug. When building, a preprocessor-directive had messed up serialization so the game would probably crash from an OutOfMemoryException.
- 3.0.8
	- Graph saving to file is now exposed for users of the Free version
	- Fixed a bug where penalties added using a GraphUpdateObject would be overriden if updatePhysics was turned on in the GraphUpdateObject
	- Fixed a bug where list graphs could ignore some children nodes, especially common if the hierarchy was deep
	- Fixed the case where empty messages would spam the log (instead of spamming somewhat meaningful messages) when path logging was set to Only Errors
	- Changed the NNConstraint used as default when calling NavGraph.GetNearest from NNConstraint.Default to NNConstraint.None, this is now the same as the default for AstarPath.GetNearest.
	- You can now set the size of the red cubes shown in place of unwalkable nodes (Settings-->Show Unwalkable Nodes-->Size)
	- Dynamic search of where the EditorAssets folder is, so now you can place it anywhere in the project.
	- Minor A* inspector enhancements.
	- Fixed a very rare bug which could, when using multithreading cause the pathfinding thread not to start after it has been terminated due to a long delay
	- Modifiers can now be enabled or disabled in the editor
	- Added custom inspector for the Simple Smooth Modifier. Hopefully it will now be easier to use (or at least get the hang on which fields you should change).
	- Added AIFollow.canSearch to disable or enable searching for paths due to popular request.
	- Added AIFollow.canMove to disable or enable moving due to popular request.
	- Changed behaviour of AIFollow.Stop, it will now set AIFollow.ccanSearch and AIFollow.ccanMove to false thus making it completely stop and stop searching for paths.
	- Removed Path.customData since it is a much better solution to create a new path class which inherits from Path.
	- Seeker.StartPath is now implemented with overloads instead of optional parameters to simplify usage for Javascript users
	- Added Curved Nonuniform spline as a smoothing option for the Simple Smooth modifier.
	- Added Pathfinding.WillBlockPath as function for checking if a GraphUpdateObject would block pathfinding between two nodes (useful in TD games).
	- Unity References (GameObject's, Transforms and similar) are now serialized in another way, hopefully this will make it more stable as people have been having problems with the previous one, especially on the iPhone.
	- Added shortcuts to specific types of graphs, AstarData.navmesh, AstarData.gridGraph, AstarData.listGraph
	- <b>Known Bugs:</b> The C++ version of Recast does not work on Windows
- 3.0.7
	- Grid Graphs can now be scaled to allow non-square nodes, good for isometric games.
	- Added more options for custom links. For example individual nodes or connections can be either enabled or disabled. And penalty can be added to individual nodes
	- Placed the Scan keyboard shortcut code in a different place, hopefully it will work more often now
	- Disabled GUILayout in the AstarPath script for a possible small speed boost
	- Some debug variables (such as AstarPath.PathsCompleted) are now only updated if the ProfileAstar define is enabled
	- DynamicGridObstacle will now update nodes correctly when the object is destroyed
	- Unwalkable nodes no longer shows when Show Graphs is not toggled
	- Removed Path.multithreaded since it was not used
	- Removed Path.preCallback since it was obsolate
	- Added Pathfinding.XPath as a more customizable path
	- Added example of how to use MultiTargetPaths to the documentation as it was seriously lacking info on that area
	- The viewing mesh scaling for recast graphs is now correct also for the C# version
	- The StartEndModifier now changes the path length to 2 for correct applying if a path length of 1 was passed.
	- The progressbar is now removed even if an exception was thrown during scanning
	- Two new example scenes have been added, one for list graphs which includes sample links, and another one for recast graphs
	- Reverted back to manually setting the dark skin option, since it didn't work in all cases, however if a dark skin is detected, the user will be asked if he/she wants to enable the dark skin
	- Added gizmos for the AIFollow script which shows the current waypoint and a circle around it illustrating the distance required for it to be considered "reached".
	- The C# version of Recast does now use Character Radius instead of Erosion Radius (world units instead of voxels)
	- Fixed an IndexOutOfRange exception which could occur when saving a graph with no nodes to file
	- <b>Known Bugs:</b> The C++ version of Recast does not work on Windows
- 3.0.6
	- Added support for a C++ version of Recast which means faster scanning times and more features (though almost no are available at the moment since I haven't added support for them yet).
	- Removed the overload AstarData.AddGraph (string type, NavGraph graph) since it was obsolete. AstarData.AddGraph (Pathfinding.NavGraph) should be used now.
	- Fixed a few bugs in the FunnelModifier which could cause it to return invalid paths
	- A reference image can now be generated for the Use Texture option for Grid Graphs
	- Fixed an editor bug with graphs which had no editors
	- Graphs with no editors now show up in the Add New Graph list to show that they have been found, but they cannot be used
	- Deleted the \a graphIndex parameter in the Pathfinding.NavGraph.Scan function. If you need to use it in your graph's Scan function, get it using Pathfinding.AstarData.GetGraphIndex
	- Javascript support! At last you can use Js code with the A* Pathfinding Project! Go to A* Inspector-->Settings-->Editor-->Enable Js Support to enable it
	- The Dark Skin is now automatically used if the rest of Unity uses the dark skin(hopefully)
	- Fixed a bug which could cause Unity to crash when using multithreading and creating a new AstarPath object during runtime
- 3.0.5
	- \link Pathfinding.PointGraph List Graphs \endlink now support UpdateGraphs. This means that they for example can be used with the DynamicObstacle script.
	- List Graphs can now gather nodes based on GameObject tags instead of all nodes as childs of a specific GameObject.
	- List Graphs can now search recursively for childs to the 'root' GameObject instead of just searching through the top-level children.
	- Added custom area colors which can be edited in the inspector (A* inspector --> Settings --> Color Settings --> Custom Area Colors)
	- Fixed a NullReference bug which could occur when loading a Unity Reference with the AstarSerializer.
	- Fixed some bugs with the FleePath and RandomPath which could cause the StartEndModifier to assign the wrong endpoint to the path.
	- Documentation is now more clear on what is A* Pathfinding Project Pro only features.
	- Pathfinding.NNConstraint now has a variable to constrain which graphs to search (A* Pro only).\n
	  This is also available for Pathfinding.GraphUpdateObject which now have a field for an NNConstraint where it can constrain which graphs to update.
	- StartPath calls on the Seeker can now take a parameter specifying which graphs to search for close nodes on (A* Pro only)
	- Added the delegate AstarPath.OnAwakeSettings which is called as the first thing in the Awake function, can be used to set up settings.
	- Pathfinding.UserConnection.doOverrideCost is now serialized correctly. This represents the toggle to the right of the "Cost" field when editing a link.
	- Fixed some bugs with the RecastGraph when spans were partially out-of-bounds, this could generate seemingly random holes in the mesh
- 3.0.4 (only pro version affected)
	- Added a Dark Skin for Unity Pro users (though it is available to Unity Free users too, even though it doesn't look very good).
	  It can be enabled through A* Inspector --> Settings --> Editor Settings --> Use Dark Skin
	- Added option to include or not include out of bounds voxels (Y axis below the graph only) for Recast graphs.
- 3.0.3 (only pro version affected)
	- Fixed a NullReferenceException caused by Voxelize.cs which could surface if there were MeshFilters with no Renderers on GameObjects (Only Pro version affected)
- 3.0.2
	- Textures can now be used to add penalty, height or change walkability of a Grid Graph (A* Pro only)
	- Slope can now be used to add penalty to nodes
	- Height (Y position) can now be usd to add penalty to nodes
	- Prioritized graphs can be used to enable prioritizing some graphs before others when they are overlapping
	- Several bug fixes
	- Included a new DynamicGridObstacle.cs script which can be attached to any obstacle with a collider and it will update grids around it to account for changed position
- 3.0.1
	- Fixed Unity 3.3 compability
- 3.0
	- Rewrote the system from scratch
	- Funnel modifier
	- Easier to extend the system


- x. releases are major rewrites or updates to the system.
- .x releases are quite big feature updates
- ..x releases are the most common updates, fix bugs, add some features etc.
- ...x releases are quickfixes, most common when there was a really bad bug which needed fixing ASAP.

 */
