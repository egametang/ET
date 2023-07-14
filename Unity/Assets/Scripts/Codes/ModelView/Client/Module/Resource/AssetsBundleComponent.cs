using System.Threading.Tasks;
using YooAsset;

namespace ET.Client;

[ComponentOf]
public class AssetsBundleComponent : Entity, IAwake, IDestroy
{
    public static AssetsBundleComponent Instance { get; set; }

    public InitializeParameters _createParameters;
    public ResourcePackage _defaultPackage;
    private string _locationRoot;
}

[FriendOf(typeof(AssetsBundleComponent))]
public static class AssetsBundleComponentSystem
{
    [ObjectSystem]
    public class AssetsBundleComponentAwakeSystem : AwakeSystem<AssetsBundleComponent>
    {
        protected override void Awake(AssetsBundleComponent self)
        {
            AssetsBundleComponent.Instance = self;
        }
    }
    
    [ObjectSystem]
    public class AssetsBundleComponentDestroySystem : DestroySystem<AssetsBundleComponent>
    {
        protected override void Destroy(AssetsBundleComponent self)
        {
            AssetsBundleComponent.Instance = null;
        }
    }

    public static void Init(this AssetsBundleComponent self,System.Object param)
    {
        YooAssets.Initialize();
    }

    public static async Task<ETTaskCompleted> InitAsync(this AssetsBundleComponent self,string packageName = "DefaultPackage")
    {
        self._defaultPackage = YooAssets.CreatePackage(packageName);
        YooAssets.SetDefaultPackage(self._defaultPackage);
        await self._defaultPackage.InitializeAsync(self._createParameters);

        return ETTask.CompletedTask;
    }
}