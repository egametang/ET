namespace ET.Analyzer
{
    public static class Definition
    {
        public const string EntityType = "ET.Entity";
        
        public const string ETTask = "ETTask";

        public static readonly string[] AddChildMethods = { "AddChild", "AddChildWithId" };

        public static readonly string[] ComponentMethod = {"AddComponent","GetComponent"};

        public const string ISystemType = "ET.ISystemType";
        
        public const string BaseAttribute = "ET.BaseAttribute";
        
        public const string ObjectSystemAttribute = "ET.ObjectSystemAttribute";

        public const string EnableMethodAttribute = "ET.EnableMethodAttribute";
        
        public const string FriendOfAttribute = "ET.FriendOfAttribute";
        
        public const string UniqueIdAttribute = "ET.UniqueIdAttribute";

        public const string ChildOfAttribute = "ET.ChildOfAttribute";

        public const string ComponentOfAttribute = "ET.ComponentOfAttribute";
        
        public const string EnableAccessEntiyChildAttribute = "ET.EnableAccessEntiyChildAttribute";

        public const string StaticFieldAttribute = "ET.StaticFieldAttribute";
    }
}

