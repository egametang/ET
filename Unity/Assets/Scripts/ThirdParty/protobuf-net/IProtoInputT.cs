namespace ProtoBuf
{
    /// <summary>
    /// Represents the ability to deserialize values from an input of type <typeparamref name="TInput"/>
    /// </summary>
    public interface IProtoInput<TInput>
    {
        /// <summary>
        /// Deserialize a value from the input
        /// </summary>
        T Deserialize<T>(TInput source, T value = default, object userState = null);
    }
}
