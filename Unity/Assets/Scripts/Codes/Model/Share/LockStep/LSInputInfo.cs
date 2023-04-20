using TrueSync;

namespace ET
{
    public partial class LSInputInfo
    {
        public static bool operator==(LSInputInfo a, LSInputInfo b)
        {
            if (a.V != b.V)
            {
                return false;
            }

            if (a.Button != b.Button)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(LSInputInfo a, LSInputInfo b)
        {
            return !(a == b);
        }
    }
}