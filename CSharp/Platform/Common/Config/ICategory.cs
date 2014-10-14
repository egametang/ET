using System.ComponentModel;

namespace Common.Config
{
    public interface ICategory: ISupportInitialize
    {
        string Name { get; }
    }
}