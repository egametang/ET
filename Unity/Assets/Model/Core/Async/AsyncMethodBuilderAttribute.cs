namespace System.Runtime.CompilerServices
{
#if !NOT_CLIENT
    public sealed class AsyncMethodBuilderAttribute: Attribute
    {
        public Type BuilderType
        {
            get;
        }

        public AsyncMethodBuilderAttribute(Type builderType)
        {
            BuilderType = builderType;
        }
    }
#endif
}