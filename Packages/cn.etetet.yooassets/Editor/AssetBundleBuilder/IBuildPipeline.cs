
namespace YooAsset.Editor
{
    public interface IBuildPipeline
    {
        public BuildResult Run(BuildParameters buildParameters, bool enableLog);
    }
}