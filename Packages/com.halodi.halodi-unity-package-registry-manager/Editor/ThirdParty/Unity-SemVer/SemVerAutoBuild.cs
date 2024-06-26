using System.Collections.Generic;
using UnityEngine;

namespace Artees.UnitySemVer
{
    /// <summary>
    /// Sets the <see cref="SemVer.Build">build</see> metadata automatically
    /// </summary>
    /// <seealso cref="SemVer.Build"/>
    public abstract class SemVerAutoBuild
    {
        /// <summary>
        /// <see cref="SemVerAutoBuild"/> implementations
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Disables automatic <see cref="SemVer.Build">build</see> metadata
            /// </summary>
            Manual,

            /// <summary>
            /// Sets the <see cref="SemVer.Build">build</see> metadata to the
            /// <a href="https://docs.unity3d.com/Manual/UnityCloudBuildManifest.html">Unity Cloud Build</a>
            /// <see cref="Artees.UnitySemVer.CloudBuildManifest.BuildNumber">“build number”</see>
            /// </summary>
            /// <seealso cref="Artees.UnitySemVer.CloudBuildManifest"/>
            CloudBuildNumber
        }

        public static readonly IReadOnlyDictionary<Type, SemVerAutoBuild> Instances =
            new Dictionary<Type, SemVerAutoBuild>
            {
                {Type.Manual, new ManualBuild()},
                {Type.CloudBuildNumber, new CloudBuildNumberBuild()}
            };

        internal abstract string Get(string build);

        internal abstract string Set(string build);

        private class ManualBuild : SemVerAutoBuild
        {
            internal override string Get(string build)
            {
                return build;
            }

            internal override string Set(string build)
            {
                return build;
            }
        }

        private class CloudBuildNumberBuild : ReadOnly
        {
            internal override string Get(string build)
            {
                return CloudBuildManifest.Instance.IsLoaded
                    ? CloudBuildManifest.Instance.BuildNumber.ToString()
                    : string.Empty;
            }
        }

        public abstract class ReadOnly : SemVerAutoBuild
        {
            internal sealed override string Set(string build)
            {
                Debug.LogWarning("The build metadata is read-only");
                return build;
            }
        }
    }
}