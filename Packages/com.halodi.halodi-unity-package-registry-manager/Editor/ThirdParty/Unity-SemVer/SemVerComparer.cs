using System;
using System.Collections.Generic;
using UnityEngine;

namespace Artees.UnitySemVer
{
    internal class SemVerComparer : IComparer<SemVer>
    {
        public int Compare(SemVer x, SemVer y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(x, null)) return -1;
            var majorComparison = x.major.CompareTo(y.major);
            if (majorComparison != 0) return majorComparison;
            var minorComparison = x.minor.CompareTo(y.minor);
            if (minorComparison != 0) return minorComparison;
            var patchComparison = x.patch.CompareTo(y.patch);
            return patchComparison != 0 ? patchComparison : ComparePreReleaseVersions(x, y);
        }

        private static int ComparePreReleaseVersions(SemVer x, SemVer y)
        {
            if (IsPreRelease(x))
            {
                if (!IsPreRelease(y)) return -1;
            }
            else
            {
                return IsPreRelease(y) ? 1 : 0;
            }

            var xIdentifiers = x.preRelease.Split(SemVer.IdentifiersSeparator);
            var yIdentifiers = y.preRelease.Split(SemVer.IdentifiersSeparator);
            var length = Mathf.Min(xIdentifiers.Length, yIdentifiers.Length);
            for (var i = 0; i < length; i++)
            {
                var xIdentifier = xIdentifiers[i];
                var yIdentifier = yIdentifiers[i];
                if (Equals(xIdentifier, yIdentifier)) continue;
                return ComparePreReleaseIdentifiers(xIdentifier, yIdentifier);
            }

            return xIdentifiers.Length.CompareTo(yIdentifiers.Length);
        }

        private static bool IsPreRelease(SemVer semVer)
        {
            return !string.IsNullOrEmpty(semVer.preRelease);
        }

        private static int ComparePreReleaseIdentifiers(string xIdentifier, string yIdentifier)
        {
            var isXNumber = int.TryParse(xIdentifier, out var xNumber);
            var isYNumber = int.TryParse(yIdentifier, out var yNumber);
            if (!isXNumber)
            {
                const StringComparison comparison = StringComparison.Ordinal;
                return isYNumber ? 1 : string.Compare(xIdentifier, yIdentifier, comparison);
            }

            if (isYNumber)
            {
                return xNumber.CompareTo(yNumber);
            }

            return -1;
        }
    }
}