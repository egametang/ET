
namespace YooAsset.Editor
{
    internal enum ErrorCode
    {
        // TaskPrepare
        ThePipelineIsBuiding = 100,
        FoundUnsavedScene = 101,
        NoBuildTarget = 110,
        PackageNameIsNullOrEmpty = 111,
        PackageVersionIsNullOrEmpty = 112,
        BuildOutputRootIsNullOrEmpty = 113,
        BuildinFileRootIsNullOrEmpty = 114,
        PackageOutputDirectoryExists = 115,
        RecommendScriptBuildPipeline = 130,
        BuildPipelineNotSupportBuildMode = 140,
        BuildPipelineNotSupportSharePackRule = 141,

        // TaskGetBuildMap
        RemoveInvalidTags = 200,
        FoundUndependedAsset = 201,
        PackAssetListIsEmpty = 202,
        NotSupportMultipleRawAsset = 210,

        // TaskBuilding
        UnityEngineBuildFailed = 300,
        UnityEngineBuildFatal = 301,

        // TaskUpdateBundleInfo
        CharactersOverTheLimit = 400,
        NotFoundUnityBundleHash = 401,
        NotFoundUnityBundleCRC = 402,
        BundleTempSizeIsZero = 403,

        // TaskVerifyBuildResult
        UnintendedBuildBundle = 500,
        UnintendedBuildResult = 501,

        // TaskCreateManifest
        NotFoundUnityBundleInBuildResult = 600,
        FoundStrayBundle = 601,
    }
}