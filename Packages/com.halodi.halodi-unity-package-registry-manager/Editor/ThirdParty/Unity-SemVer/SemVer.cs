using System;
using UnityEngine;

namespace Artees.UnitySemVer
{
    /// <summary>
    /// A semantic version based on the <a href="https://semver.org/">Semantic Versioning 2.0.0</a> specification.
    /// </summary>
    [Serializable]
    public class SemVer : IComparable<SemVer>, IEquatable<SemVer>
    {
        public const char IdentifiersSeparator = '.';
        public const char PreReleasePrefix = '-';
        public const char BuildPrefix = '+';

        public static bool operator ==(SemVer left, SemVer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SemVer left, SemVer right)
        {
            return !Equals(left, right);
        }

        public static bool operator >(SemVer left, SemVer right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(SemVer left, SemVer right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(SemVer left, SemVer right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(SemVer left, SemVer right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static implicit operator string(SemVer s)
        {
            return s.ToString();
        }

        public static implicit operator SemVer(string s)
        {
            return Parse(s);
        }

        public static SemVer Parse(string semVer)
        {
            return SemVerConverter.FromString(semVer);
        }

        /// <summary>
        /// Major version X (X.y.z | X > 0) MUST be incremented if any backwards incompatible changes are introduced
        /// to the public API. It MAY include minor and patch level changes. Patch and minor version MUST be reset to 0
        /// when major version is incremented.
        /// <seealso cref="IncrementMajor"/>
        /// </summary>
        public uint major;

        /// <summary>
        /// Minor version Y (x.Y.z | x > 0) MUST be incremented if new, backwards compatible functionality is
        /// introduced to the public API. It MUST be incremented if any public API functionality is marked as
        /// deprecated. It MAY be incremented if substantial new functionality or improvements are introduced within
        /// the private code. It MAY include patch level changes. Patch version MUST be reset to 0 when minor version
        /// is incremented.
        /// <seealso cref="IncrementMinor"/>
        /// </summary>
        public uint minor;

        /// <summary>
        /// Patch version Z (x.y.Z | x > 0) MUST be incremented if only backwards compatible bug fixes are introduced.
        /// </summary>
        public uint patch;

        /// <summary>
        /// A pre-release version indicates that the version is unstable and might not satisfy the intended
        /// compatibility requirements as denoted by its associated normal version.
        /// </summary>
        /// <example>1.0.0-<b>alpha</b>, 1.0.0-<b>alpha.1</b>, 1.0.0-<b>0.3.7</b>, 1.0.0-<b>x.7.z.92</b></example>
        public string preRelease;

        /// <summary>
        /// Set the <see cref="Build">build</see> metadata automatically
        /// </summary>
        /// <seealso cref="Build"/>
        public SemVerAutoBuild.Type autoBuild;

        [SerializeField] private string build = string.Empty;

        /// <summary>
        /// Build metadata MUST be ignored when determining version precedence. Thus two versions that differ only in
        /// the build metadata, have the same precedence.
        /// </summary>
        /// <example>1.0.0-alpha+<b>001</b>, 1.0.0+<b>20130313144700</b>, 1.0.0-beta+<b>exp.sha.5114f85</b></example>
        public string Build
        {
            get => SemVerAutoBuild.Instances[autoBuild].Get(build);
            set => build = SemVerAutoBuild.Instances[autoBuild].Set(value);
        }

        /// <summary>
        /// The base part of the version number (Major.Minor.Patch).
        /// </summary>
        /// <example>1.9.0</example>
        /// <returns>Major.Minor.Patch</returns>
        public string Core => $"{major}.{minor}.{patch}";

        /// <summary>
        /// An internal version number. This number is used only to determine whether one version is more recent than
        /// another, with higher numbers indicating more recent versions.
        /// <a href="https://developer.android.com/studio/publish/versioning"/>
        /// </summary>
        /// <returns><c>Major * 10000 + Minor * 100 + Patch</c></returns>
        public int AndroidBundleVersionCode
        {
            get
            {
                var clampedPatch = ClampAndroidBundleVersionCode(patch, "Patch");
                var clampedMinor = ClampAndroidBundleVersionCode(minor, "Minor");
                return (int) (major * 10000 + clampedMinor * 100 + clampedPatch);
            }
        }

        private static uint ClampAndroidBundleVersionCode(uint value, string name)
        {
            uint clamped;
            const uint max = 100;
            if (value >= max)
            {
                clamped = max - 1;
                Debug.LogWarning(name + " should be less than " + max);
            }
            else
            {
                clamped = value;
            }

            return clamped;
        }

        public SemVer()
        {
            minor = 1;
            preRelease = string.Empty;
            autoBuild = SemVerAutoBuild.Type.Manual;
        }

        /// <summary>
        /// Increment the major version, reset the patch and the minor version to 0.
        /// </summary>
        public void IncrementMajor()
        {
            major++;
            minor = patch = 0;
        }

        /// <summary>
        /// Increment the minor version, reset the patch version to 0.
        /// </summary>
        public void IncrementMinor()
        {
            minor++;
            patch = 0;
        }

        /// <summary>
        /// Increment the patch version.
        /// </summary>
        public void IncrementPatch()
        {
            patch++;
        }

        /// <summary>
        /// Check if this semantic version meets the <a href="https://semver.org/">Semantic Versioning 2.0.0</a>
        /// specification.
        /// </summary>
        /// <returns>The result of validation and automatically corrected version number.</returns>
        public SemVerValidationResult Validate()
        {
            return new SemVerValidator().Validate(this);
        }

        /// <summary>
        /// Creates a copy of this semantic version.
        /// </summary>
        public SemVer Clone()
        {
            return new SemVer
            {
                major = major,
                minor = minor,
                patch = patch,
                preRelease = preRelease,
                Build = Build,
                autoBuild = autoBuild,
            };
        }

        public int CompareTo(SemVer other)
        {
            return new SemVerComparer().Compare(this, other);
        }

        public bool Equals(SemVer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CompareTo(other) == 0;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SemVer) obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return SemVerConverter.ToString(this);
        }
    }
}