using System.Collections.Generic;

namespace ET
{
    public static class Definition
    {
        public const string EntityType = "ET.Entity";

        public const string LSEntityType = "ET.LSEntity";
        
        public const string ETTask = "ETTask";

        public const string ETTaskFullName = "ET.ETTask";

        public static readonly string[] AddChildMethods = { "AddChild", "AddChildWithId" };

        public static readonly string[] ComponentMethod = {"AddComponent","GetComponent"};

        public const string ISystemType = "ET.ISystemType";
        
        public const string ObjectSystemAttribute = "ET.SystemAttribute";

        public const string EnableMethodAttribute = "ET.EnableMethodAttribute";
        
        public const string FriendOfAttribute = "ET.FriendOfAttribute";
        
        public const string UniqueIdAttribute = "ET.UniqueIdAttribute";

        public const string ChildOfAttribute = "ET.ChildOfAttribute";

        public const string ComponentOfAttribute = "ET.ComponentOfAttribute";
        
        public const string EnableAccessEntiyChildAttribute = "ET.EnableAccessEntiyChildAttribute";

        public const string StaticFieldAttribute = "ET.StaticFieldAttribute";

        public const string ETCancellationToken = "ET.ETCancellationToken";

        public const string ETTaskCompleteTask = "ETTask.CompletedTask";

        public const string ETClientNameSpace = "ET.Client";

        public const string ClientDirInServer = @"Unity\Assets\Scripts\Hotfix\Client\";

        public const string EntitySystemAttribute = "EntitySystem";
        public const string EntitySystemAttributeMetaName = "ET.EntitySystemAttribute";

        public const string EntitySystemOfAttribute = "ET.EntitySystemOfAttribute";
        public const string EntitySystemInterfaceSequence = "EntitySystemInterfaceSequence";

        public static string[] FriendAttributes =
        [
            FriendOfAttribute,
            EntitySystemOfAttribute,
            LSEntitySystemOfAttribute
        ];   

        public const string IAwakeInterface = "ET.IAwake";
        public const string AwakeMethod = "Awake";

        public const string IUpdateInterface = "ET.IUpdate";
        public const string UpdateMethod = "Update";

        public const string IDestroyInterface = "ET.IDestroy";
        public const string DestroyMethod = "Destroy";

        public const string IAddComponentInterface = "ET.IAddComponent";
        public const string AddComponentMethod = "AddComponent";

        public const string IDeserializeInterface = "ET.IDeserialize";
        public const string DeserializeMethod = "Deserialize";

        public const string IGetComponentInterface = "ET.IGetComponentSys";
        public const string GetComponentMethod = "GetComponentSys";

        public const string ILoadInterface = "ET.ILoad";
        public const string LoadMethod = "Load";

        public const string ILateUpdateInterface = "ET.ILateUpdate";
        public const string LateUpdateMethod = "LateUpdate";

        public const string ISerializeInterface = "ET.ISerialize";
        public const string SerializeMethod = "Serialize";

        public const string LSEntitySystemAttribute = "LSEntitySystem";
        public const string LSEntitySystemAttributeMetaName = "ET.LSEntitySystemAttribute";
        public const string LSEntitySystemOfAttribute = "ET.LSEntitySystemOfAttribute";

        public const string ILSRollbackInterface = "ET.ILSRollback";
        public const string LSRollbackMethod = "LSRollback";

        public const string ILSUpdateInterface = "ET.ILSUpdate";
        public const string LSUpdateMethod = "LSUpdate";

        public const string ETLog = "ET.Log";

        public const string IMessageInterface = "ET.IMessage";

        public const string EntityRefType = "EntityRef";
        
        public const string EntityWeakRefType = "EntityWeakRef";

        public const string DisableNewAttribute = "ET.DisableNewAttribute";

        public const string EnableClassAttribute = "ET.EnableClassAttribute";
    }
}

