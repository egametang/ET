namespace DotRecast.Recast
{
    public interface IRecastBuilderProgressListener
    {
        void OnProgress(int completed, int total);
    }
}