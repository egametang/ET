namespace System.Runtime.CompilerServices
{
    public sealed class AsyncMethodBuilderAttribute: Attribute
    {
        public Type BuilderType { get; }

        public AsyncMethodBuilderAttribute(Type builderType)
        {
            BuilderType = builderType;
        }
    }
}