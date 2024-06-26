namespace Artees.UnitySemVer
{
    public static class SemVerErrorMessage
    {
        public const string Empty = "Pre-release and build identifiers must not be empty";

        public const string Invalid = 
            "Pre-release and build identifiers must comprise only ASCII alphanumerics and hyphen";

        public const string LeadingZero = "Numeric pre-release identifiers must not include leading zeroes";
    }
}