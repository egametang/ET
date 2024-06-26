namespace Artees.UnitySemVer
{
    internal static class SemVerConverter
    {
        public static SemVer FromString(string semVerString)
        {
            var strings = semVerString.Split(SemVer.IdentifiersSeparator, SemVer.PreReleasePrefix, SemVer.BuildPrefix);
            var preReleaseStart = semVerString.IndexOf(SemVer.PreReleasePrefix);
            var buildIndex = semVerString.IndexOf(SemVer.BuildPrefix);
            var preReleaseEnd = buildIndex >= 0 ? buildIndex : semVerString.Length;
            var preRelease = preReleaseStart >= 0
                ? semVerString.Substring(preReleaseStart + 1, preReleaseEnd - preReleaseStart - 1)
                : string.Empty;
            var build = buildIndex >= 0 ? semVerString.Substring(buildIndex + 1) : string.Empty;
            uint major = 0;
            if (strings.Length > 0) uint.TryParse(strings[0], out major);
            uint minor = 1;
            if (strings.Length > 1) uint.TryParse(strings[1], out minor);
            uint patch = 0;
            if (strings.Length > 2) uint.TryParse(strings[2], out patch);
            var semVer = new SemVer
            {
                major = major,
                minor = minor,
                patch = patch,
                preRelease = preRelease,
                Build = build
            };
            return semVer;
        }

        public static string ToString(SemVer semVer)
        {
            var preRelease =
                string.IsNullOrEmpty(semVer.preRelease)
                    ? string.Empty
                    : $"{SemVer.PreReleasePrefix}{semVer.preRelease}";
            var build =
                string.IsNullOrEmpty(semVer.Build)
                    ? string.Empty
                    : $"{SemVer.BuildPrefix}{semVer.Build}";
            return string.Format("{1}{0}{2}{0}{3}{4}{5}",
                SemVer.IdentifiersSeparator,
                semVer.major,
                semVer.minor,
                semVer.patch,
                preRelease,
                build);
        }
    }
}