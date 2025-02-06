
namespace YooAsset
{
    internal sealed class CompletedProvider : ProviderBase
    {
        public CompletedProvider(AssetInfo assetInfo) : base(null, string.Empty, assetInfo)
        {
        }

        internal override void InternalOnStart()
        {
        }
        internal override void InternalOnUpdate()
        {
        }

        public void SetCompleted(string error)
        {
            if (_steps == ESteps.None)
            {
                InvokeCompletion(error, EOperationStatus.Failed);
            }
        }
    }
}