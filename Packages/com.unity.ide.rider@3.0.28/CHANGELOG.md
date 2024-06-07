# Code Editor Package for Rider


## [3.0.28] - 2024-02-20

- fix RIDER-103933 "PlayerSettings.suppressCommonWarnings" is not supported in Unity 2019.4.40f
- fix https://github.com/JetBrains/resharper-unity/issues/2431 and [RIDER-104221](https://youtrack.jetbrains.com/issue/RIDER-104221)


## [3.0.27] - 2023-11-30

- Restore the ability to select Rider installation from the custom location
- Fix possible extra project regeneration on moving focus from Rider to Unity
- Improve performance of code generation for very large projects


## [3.0.26] - 2023-10-04

 - https://github.com/JetBrains/resharper-unity/issues/2421
- https://github.com/JetBrains/resharper-unity/issues/2422


## [3.0.25] - 2023-08-18

- unification of functionality to  search JetBrains installations and open solution and file in Rider


## [3.0.22] - 2023-05-2

- RIDER-82999 Unity's plugin SyncAll does not regenerate project files, and instead does basically nothing.
- #2401 Compilation issue with Unity 2021.3.0f1


## [3.0.21] - 2023-04-18

[RIDER-92424](https://youtrack.jetbrains.com/issue/RIDER-92424) JetBrains Rider Editor 3.0.20 package Update for Unity, Cause's Rider to Slows to a Crawl after updating
[RIDER-92419](https://youtrack.jetbrains.com/issue/RIDER-92419) JetBrains Rider Editor 3.0.20 for Unity has duplicate assemblies loaded into runtime


## [3.0.20] - 2023-04-05

- fix loading Rider integration EditorPlugin on first switch of External Editor to Rider, see [RIDER-91185](https://youtrack.jetbrains.com/issue/RIDER-91185)
- Keep the the PackageManager in sync with the Rider changes made to the manifest.json, it should help with [RIDER-77343](https://youtrack.jetbrains.com/issue/RIDER-77343)
- Support CompilerOptions.RoslynAdditionalFilePaths and CompilerOptions.AnalyzerConfigPath


## [3.0.18] - 2023-01-09

- [RIDER-74818](https://youtrack.jetbrains.com/issue/RIDER-74818) Unity doesn't get to play mode if Editor is not running and user starts debug or profiling
- Improve performance of project generation - avoid using Directory.Exists
- avoid doing ProjectGeneration twice on the first start-up


## [3.0.17] - 2022-12-01

 - Avoid adding asset project parts to both editor and player projects, fixes the following issues:
 - [RIDER-75500](https://youtrack.jetbrains.com/issue/RIDER-75500) Local package references completions shows duplicate entries if player projects are generated
 - [RIDER-73795](https://youtrack.jetbrains.com/issue/RIDER-73795) Conversion to guid is not offered for assemblies with generated player projects
 - [RIDER-71238](https://youtrack.jetbrains.com/issue/RIDER-71238) No usages can be found for the assembly if player projects are generated


## [3.0.16] - 2022-09-09

- Update the changelog
- Add folders to the generated csproj files
- Avoid extra RequestScriptReload call on the first start
- Fix shader support for folders in packages, but outside asmdef


## [3.0.15] - 2022-05-24

- Cleanup cache after project generation to reduce memory consumption
- Performance optimization
- RIDER-76126 Rider package should generate an empty csproj for empty Unity project
- RIDER-77206 Unity 2020.1.3 'PlayerSettings' does not contain a definition for 'suppressCommonWarnings


## [3.0.14] - 2022-04-21

- Move Rider package persisted state to Library, to avoid vcs collisions or adding it specifically to gitignore


## [3.0.13] - 2022-03-24

- fix RIDER-69927 "Test not run" status is shown for the test suite when running unit tests for Unity project
- fix RIDER-74676 Unity plugin "JetBrainseRider Editor" completely breaks <= 2019.1.9
- fix RIDER-71503 Unity Hang on "Domain Unload", caused by dispose of FileSystemWatcher


## [3.0.12] - 2022-01-28

- Fix bug, which was introduced in 3.0.10: New script was not added to the csproj, because cached list of assemblies was used.


## [3.0.10] - 2021-12-09

- Fix presentation of the TargetFramework in the csproj
- Fix: Auto-generated solution doesn't compile when code overrides virtual functions in other assemblies
- Fix RIDER-72234 Avoid full project generation, when only content of assembly was changed
- Fix RIDER-71985 Building large Unity projects randomly fails
- Fix RIDER-72174 Looking for Rider installed by dotUltimate installer


## [3.0.9] - 2021-11-09

- Fix path for Roslyn analyser supplied with a package
- Minimal requirement for roslyn analyzer scope is Unity 2020.3.6f1 and above 


## [3.0.8] - 2021-11-08

- Technical release


## [3.0.7] - 2021-05-07

- RIDER-60815 Simplify extensions lists for Rider package
- Fix csc.rsp `-nullable+` / `-nullable-` parsing https://github.com/van800/com.unity.ide.rider/issues/7
- Support `-warnaserror`/`-warnaserror-:`/`-warnaserror+:` in csc.rsp


## [3.0.6] - 2021-04-06

- Fix bug: For Unity 2021.1+ Switching external editor from VS => Rider won't create the connection between Unity and Rider.
- When PlayerSettings.suppressCommonWarnings is true, it is reflected in the generated csproj with NoWarn "0169", "0649"
- By default include T4 templates in the generated solution (RIDER-37159)
- RIDER-60554 Unity crash in case of project without Unity Test Framework Package.
- RIDER-60445 Fix presentation of Rider external editor, when it is installed in a custom location.
- Improve project files generation performance
- RIDER-60508 Project Generation for projects without any cs files - add reference to UnityEditor/UnityEngine, so that Rider would detect Unity path and version and provide rich features for shader file.


## [3.0.5] - 2021-02-25

- More stable in case of possible Rider product code change, improve test. Allows using "Rider for Unreal" with Unity projects (https://youtrack.jetbrains.com/issue/RIDER-51203)
- Remove implicit dependency to Test-Framework package
- Fix "Unreachable code detected" warning (https://youtrack.jetbrains.com/issue/RIDER-57930)


## [3.0.4] - 2021-01-26

- Use LangVersion provided by Unity for generated csproj
- Improve documentation
- Support nullable provided in csc,rsp
- Avoid doing work in Unity secondary processes in UNITY_2021_1_OR_NEWER with UnityEditor.MPE.ProcessLevel.Secondary


## [3.0.3] - 2020-11-18

- Update License
- Avoid connecting Rider from secondary UnityEditor instances
- Fix RIDER-53082 - Generate csproj without cs files, when there are any assets inside


## [3.0.2] - 2020-10-27
- Speedup ProjectGeneration
- Fix RIDER-51958. Callbacks OnGeneratedCSProjectFiles would not work, but show a Warning instead.
- Remove release configuration
- Call RequestScriptReload, when External Editor is changed in Unity.


## [3.0.1] - 2020-10-02

- RIDER-46658 Rider does not run PlayMode tests when ValueSource is combined with parameterized TestFixture
- RIDER-49947 Invoking `PlayerSettings.SetScriptingDefineSymbolsForGroup()` does not update definitions in Rider.
- Add static entrypoint `Packages.Rider.Editor.RiderScriptEditor.SyncSolution` to allow generating solution from commandline.

## [2.0.7] - 2020-08-18
- Improve performance
- Add support for asmdef Root Namespace in .csproj generation
- ProjectGeneration for custom roslyn analysers https://docs.unity3d.com/2020.2/Documentation/Manual/roslyn-analyzers.html
- Switch target platform in Unity would regenerate csproj files (https://github.com/JetBrains/resharper-unity/issues/1740)


## [2.0.6] - 2020-08-10
- Improve performance
- Add support for asmdef Root Namespace in .csproj generation
- ProjectGeneration for custom roslyn analysers https://docs.unity3d.com/2020.2/Documentation/Manual/roslyn-analyzers.html
- Switch target platform in Unity would regenerate csproj files (https://github.com/JetBrains/resharper-unity/issues/1740)


## [2.0.5] - 2020-05-27
- Fix Regression in 2.0.3: In Unity 2019.2.9 on Mac, changing csproj and calling AssetDatabase.Refresh is not regenerating csproj.
- Regenerate projects on changes in manifest.json and Project Settings (EditorOnlyScriptingUserSettings.json) (#51)
- Fix: Assembly references to package assemblies break IDE projects.
- Fix: Reporting test duration.


## [2.0.2] - 2020-03-18
- fix bug in searching Rider path on MacOS


## [2.0.1] - 2020-03-05
- Speed improvements,
- ProjectTypeGuids for unity-generated project
- Improve UI for Project Generation settings
- Changes in csc.rsp would cause project-generation
- Remove NoWarn 0169 from generated csproj
- Support custom JetBrains Toolbox installation location

## [1.2.1] - 2019-12-09

- Load optimised EditorPlugin version compiled to net 461, with fallback to previous version.
- On ExternalEditor settings page: reorder Generate all ... after Extensions handled
- Better presentation for Rider of some version in ExternalEditors list
- Initial support for Code Coverage with dotCover plugin in Rider
- Added support for Player Project generation

## [1.1.4] - 2019-11-21
 - Fix warning - unreachable code

## [1.1.3] - 2019-10-17

 - Update External Editor, when new toolbox build was installed
 - Add xaml to default list of extensions to include in csproj
 - Avoid initializing Rider package in secondary Unity process, which does Asset processing
 - Reflect multiple csc.rsp arguments to generated csproj files: https://github.com/JetBrains/resharper-unity/issues/1337
 - Setting, which allowed to override LangVersion removed in favor of langversion in csc.rsp
 - Environment.NewLine is used in generated project files instead of Windows line separator.

## [1.1.2] - 2019-09-18

performance optimizations:
 - avoid multiple evaluations
 - avoid reflection in DisableSyncSolutionOnceCallBack
 - project generation optimization
fixes:
 - avoid compilation error with incompatible `Test Framework` package

## [1.1.1] - 2019-08-26

parse nowarn in csc.rsp
warning, when Unity was started from Rider, but external editor was different
improved unit test support
workaround to avoid Unity internal project-generation (fix #28)


## [1.1.0] - 2019-07-02

new setting to manage list of extensions to be opened with Rider
avoid breaking everything on any unhandled exception in RiderScriptEditor cctor
hide Rider settings, when different Editor is selected
dynamically load only newer rider plugins
path detection (work on unix symlinks)
speed up for project generation
lots of bug fixing

## [1.0.8] - 2019-05-20

Fix NullReferenceException when External editor was pointing to non-existing Rider everything was broken by null-ref.

## [1.0.7] - 2019-05-16

Initial migration steps from rider plugin to package.
Fix OSX check and opening of files.

## [1.0.6] - 2019-04-30

Ensure asset database is refreshed when generating csproj and solution files.

## [1.0.5] - 2019-04-27

Add support for generating all csproj files.

## [1.0.4] - 2019-04-18

Fix relative package paths.
Fix opening editor on mac.

## [1.0.3] - 2019-04-12

Fixing null reference issue for callbacks to Asset pipeline.

## [1.0.2] - 2019-01-01

### This is the first release of *Unity Package rider_editor*.

Using the newly created api to integrate Rider with Unity.
