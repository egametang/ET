namespace ET.PackageManager.Editor
{
    public class ETPackageCreateData
    {
        public string                   PackageAuthor;
        public string                   PackagePath;
        public int                      PackageId;
        public string                   PackageName;
        public string                   AssemblyName;
        public string                   DisplayName;
        public string                   Description;
        public EPackageCreateType       PackageCreateType;
        public EPackageRuntimeRefType   RuntimeRefType;
        public EPackageCreateFolderType FolderType;
    }
}
