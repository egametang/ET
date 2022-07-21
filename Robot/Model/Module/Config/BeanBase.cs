using Bright.Serialization;

namespace Bright.Config
{
    public abstract class BeanBase : ITypeId
    {
        public abstract int GetTypeId();
    }
}
