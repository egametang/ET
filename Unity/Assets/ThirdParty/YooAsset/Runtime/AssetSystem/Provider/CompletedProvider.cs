
namespace YooAsset
{
	internal sealed class CompletedProvider : ProviderBase
	{
		public override float Progress
		{
			get
			{
				if (IsDone)
					return 1f;
				else
					return 0;
			}
		}

		public CompletedProvider(AssetInfo assetInfo) : base(assetInfo)
		{
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.Fail;
				LastError = MainAssetInfo.Error;
				InvokeCompletion();
			}
		}
	}
}