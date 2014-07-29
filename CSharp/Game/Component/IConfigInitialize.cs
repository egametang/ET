namespace Component
{
    public interface IConfigInitialize
    {
        string ConfigName { get; }
        void Init(string dir);
    }
}