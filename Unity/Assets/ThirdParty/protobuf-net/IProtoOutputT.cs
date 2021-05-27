using System;

namespace ProtoBuf
{
    /// <summary>
    /// Represents the ability to serialize values to an output of type <typeparamref name="TOutput"/>
    /// </summary>
    public interface IProtoOutput<TOutput>
    {
        /// <summary>
        /// Serialize the provided value
        /// </summary>
        void Serialize<T>(TOutput destination, T value, object userState = null);
    }

    /// <summary>
    /// Represents the ability to serialize values to an output of type <typeparamref name="TOutput"/>
    /// with pre-computation of the length
    /// </summary>
    public interface IMeasuredProtoOutput<TOutput> : IProtoOutput<TOutput>
    {
        /// <summary>
        /// Measure the length of a value in advance of serialization
        /// </summary>
        MeasureState<T> Measure<T>(T value, object userState = null);

        /// <summary>
        /// Serialize the previously measured value
        /// </summary>
        void Serialize<T>(MeasureState<T> measured, TOutput destination);
    }

    /// <summary>
    /// Represents the outcome of computing the length of an object; since this may have required computing lengths
    /// for multiple objects, some metadata is retained so that a subsequent serialize operation using
    /// this instance can re-use the previously calculated lengths. If the object state changes between the
    /// measure and serialize operations, the behavior is undefined.
    /// </summary>
    public struct MeasureState<T> : IDisposable
        // note: 2.4.* does not actually implement this API;
        // it only advertises it for 3.* capability/feature-testing, i.e.
        // callers can check whether a model implements
        // IMeasuredProtoOutput<Foo>, and *work from that*
    {
        /// <summary>
        /// Releases all resources associated with this value
        /// </summary>
        public void Dispose() => throw new NotImplementedException();

        /// <summary>
        /// Gets the calculated length of this serialize operation, in bytes
        /// </summary>
        public long Length => throw new NotImplementedException();
    }
}
