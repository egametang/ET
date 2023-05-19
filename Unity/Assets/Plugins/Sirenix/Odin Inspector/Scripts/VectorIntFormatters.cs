#if UNITY_2017_2_OR_NEWER

//-----------------------------------------------------------------------
// <copyright file="VectorIntFormatters.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.Serialization.RegisterFormatter(typeof(Sirenix.Serialization.Vector2IntFormatter))]
[assembly: Sirenix.Serialization.RegisterFormatter(typeof(Sirenix.Serialization.Vector3IntFormatter))]
namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Vector2Int"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Vector2Int}" />
    public class Vector2IntFormatter : MinimalBaseFormatter<Vector2Int>
    {
        private static readonly Serializer<int> Serializer = Serialization.Serializer.Get<int>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Vector2Int value, IDataReader reader)
        {
            value.x = Vector2IntFormatter.Serializer.ReadValue(reader);
            value.y = Vector2IntFormatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Vector2Int value, IDataWriter writer)
        {
            Vector2IntFormatter.Serializer.WriteValue(value.x, writer);
            Vector2IntFormatter.Serializer.WriteValue(value.y, writer);
        }
    }

    /// <summary>
    /// Custom formatter for the <see cref="Vector3Int"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Vector3Int}" />
    public class Vector3IntFormatter : MinimalBaseFormatter<Vector3Int>
    {
        private static readonly Serializer<int> Serializer = Serialization.Serializer.Get<int>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Vector3Int value, IDataReader reader)
        {
            value.x = Vector3IntFormatter.Serializer.ReadValue(reader);
            value.y = Vector3IntFormatter.Serializer.ReadValue(reader);
            value.z = Vector3IntFormatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Vector3Int value, IDataWriter writer)
        {
            Vector3IntFormatter.Serializer.WriteValue(value.x, writer);
            Vector3IntFormatter.Serializer.WriteValue(value.y, writer);
            Vector3IntFormatter.Serializer.WriteValue(value.z, writer);
        }
    }
}

#endif