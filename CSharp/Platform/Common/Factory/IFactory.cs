using Common.Base;

namespace Common.Factory
{
    public interface IFactory
    {
        Entity Create(int configId);
    }
}