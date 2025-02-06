using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor
{
    public static class HashUtil
    {
        public static int CombineHash(int hash1, int hash2)
        {
            return hash1 * 1566083941 + hash2;
        }

        public static int ComputHash(List<TypeSig> sigs)
        {
            int hash = 135781321;
            TypeEqualityComparer tc = TypeEqualityComparer.Instance;
            foreach (var sig in sigs)
            {
                hash = hash * 1566083941 + tc.GetHashCode(sig);
            }
            return hash;
        }
    }
}
