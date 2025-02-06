using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityFS
{
    public class BundleSubFile
    {
        public string file;
        public byte[] data;
    }

    public class BundleFileInfo
    {
        public string signature;
        public uint version;
        public string unityVersion;
        public string unityRevision;
        public ArchiveFlags flags;
        public List<BundleSubFile> files;
    }
}
