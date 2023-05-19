//-----------------------------------------------------------------------
// <copyright file="EnsureOdinValidatorDefine.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Sirenix.Utilities
{
    using System;
    using System.Linq;
    using UnityEditor;

    /// <summary>
    /// Defines the ODIN_VALIDATOR symbol.
    /// </summary>
    internal static class EnsureOdinValidatorDefine
    {
        private const string DEFINE = "ODIN_VALIDATOR";

        [InitializeOnLoadMethod]
        private static void EnsureScriptingDefineSymbol()
        {
            var currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            if (currentTarget == BuildTargetGroup.Unknown)
            {
                return;
            }

            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Trim();
            var defines = definesString.Split(';');

            if (defines.Contains(DEFINE) == false)
            {
                if (definesString.EndsWith(";", StringComparison.InvariantCulture) == false)
                {
                    definesString += ";";
                }

                definesString += DEFINE;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
            }
        }
    }

    //
    // If you have a project where only some users have Odin Validator, and you want to utilize the ODIN_VALIDATOR 
    // define symbol. Then, in order to only define the symbol for those with Odin Validator, you can delete this script, 
    // which prevents ODIN_VALIDATOR from being added to Unity's player settings.
    // 
    // And instead automatically add the ODIN_VALIDATOR define to an mcs.rsp file if Odin exists using the script below.
    // You can then ignore the mcs.rsp file in source control.
    // 
    // Remember to manually remove the ODIN_VALIDATOR define symbol in player settings after removing this script.
    //
    //    static class AddOdinValidatorDefineIfOdinExists
    //    {
    //        private const string ODIN_MCS_DEFINE = "-define:ODIN_VALIDATOR";
    //
    //        [InitializeOnLoadMethod]
    //        private static void AddOrRemoveOdinDefine()
    //        {
    //            var addDefine = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.StartsWith("Sirenix.OdinInspector.Editor"));
    //
    // #if ODIN_VALIDATOR
    //            var hasDefine = true;
    // #else
    //            var hasDefine = false;
    // #endif
    //
    //            if (addDefine == hasDefine)
    //            {
    //                return;
    //            }
    //
    //            var mcsPath = Path.Combine(Application.dataPath, "mcs.rsp");
    //            var hasMcsFile = File.Exists(mcsPath);
    //
    //            if (addDefine)
    //            {
    //                var lines = hasMcsFile ? File.ReadAllLines(mcsPath).ToList() : new List<string>();
    //                if (!lines.Any(x => x.Trim() == ODIN_MCS_DEFINE))
    //                {
    //                    lines.Add(ODIN_MCS_DEFINE);
    //                    File.WriteAllLines(mcsPath, lines.ToArray());
    //                    AssetDatabase.Refresh();
    //                }
    //            }
    //            else if (hasMcsFile)
    //            {
    //                var linesWithoutOdinDefine = File.ReadAllLines(mcsPath).Where(x => x.Trim() != ODIN_MCS_DEFINE).ToArray();
    //
    //                if (linesWithoutOdinDefine.Length == 0)
    //                {
    //                    // Optional - Remove the mcs file instead if it doesn't contain any lines.
    //                    File.Delete(mcsPath);
    //                }
    //                else
    //                {
    //                    File.WriteAllLines(mcsPath, linesWithoutOdinDefine);
    //                }
    //
    //                AssetDatabase.Refresh();
    //            }
    //        }
    //    }
}

#endif // UNITY_EDITOR