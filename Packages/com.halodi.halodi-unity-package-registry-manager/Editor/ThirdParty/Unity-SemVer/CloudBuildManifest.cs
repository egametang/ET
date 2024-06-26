using System;
using UnityEngine;

namespace Artees.UnitySemVer
{
    /// <summary>
    /// A parsed <a href="https://docs.unity3d.com/Manual/UnityCloudBuildManifest.html">Unity Cloud Build manifest</a>.
    /// </summary>
    internal class CloudBuildManifest
    {
        private static CloudBuildManifest _instance;

        public static CloudBuildManifest Instance => _instance ?? (_instance = new CloudBuildManifest());

        /// <summary>
        /// Returns true if the manifest has been successfully loaded.
        /// </summary>
        public readonly bool IsLoaded;

        /// <summary>
        /// The Unity Cloud Build “build number” corresponding to this build.
        /// </summary>
        public readonly int BuildNumber;

        private CloudBuildManifest()
        {
            var manifestAsset = Resources.Load<TextAsset>("UnityCloudBuildManifest.json");
            if (manifestAsset == null) return;
            var manifest = manifestAsset.text;
            IsLoaded = true;
            const string key = "\"buildNumber\"";
            const StringComparison comparison = StringComparison.Ordinal;
            var keyStart = manifest.IndexOf(key, comparison);
            var valueStart = manifest.IndexOf("\"", keyStart + key.Length, comparison) + 1;
            var valueEnd = manifest.IndexOf("\"", valueStart, comparison);
            var buildNumber = manifest.Substring(valueStart, valueEnd - valueStart);
            int.TryParse(buildNumber, out BuildNumber);
        }
    }
}