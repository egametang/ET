using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;

namespace ProtoBuf
{
    /// <summary>
    /// This class acts as an internal wrapper allowing us to do a dynamic
    /// methodinfo invoke; an't put into Serializer as don't want on public
    /// API; can't put into Serializer&lt;T&gt; since we need to invoke
    /// across classes
    /// </summary>
    internal static class ExtensibleUtil
    {

#if !NO_RUNTIME
        /// <summary>
        /// All this does is call GetExtendedValuesTyped with the correct type for "instance";
        /// this ensures that we don't get issues with subclasses declaring conflicting types -
        /// the caller must respect the fields defined for the type they pass in.
        /// </summary>
        internal static IEnumerable<TValue> GetExtendedValues<TValue>(IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            foreach (TValue value in GetExtendedValues(RuntimeTypeModel.Default, typeof(TValue), instance, tag, format, singleton, allowDefinedTag))
            {
                yield return value;
            }
        }
#endif
        /// <summary>
        /// All this does is call GetExtendedValuesTyped with the correct type for "instance";
        /// this ensures that we don't get issues with subclasses declaring conflicting types -
        /// the caller must respect the fields defined for the type they pass in.
        /// </summary>
        internal static IEnumerable GetExtendedValues(TypeModel model, Type type, IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (tag <= 0) throw new ArgumentOutOfRangeException(nameof(tag));
            IExtension extn = instance.GetExtensionObject(false);

            if (extn == null)
            {
                yield break;
            }

            Stream stream = extn.BeginQuery();
            object value = null;
            ProtoReader reader = null;
            try
            {
                SerializationContext ctx = new SerializationContext();
                reader = ProtoReader.Create(stream, model, ctx, ProtoReader.TO_EOF);
                while (model.TryDeserializeAuxiliaryType(reader, format, tag, type, ref value, true, true, false, false, null) && value != null)
                {
                    if (!singleton)
                    {
                        yield return value;

                        value = null; // fresh item each time
                    }
                }
                if (singleton && value != null)
                {
                    yield return value;
                }
            }
            finally
            {
                ProtoReader.Recycle(reader);
                extn.EndQuery(stream);
            }
        }

        internal static void AppendExtendValue(TypeModel model, IExtensible instance, int tag, DataFormat format, object value)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (value == null) throw new ArgumentNullException(nameof(value));

            // TODO
            //model.CheckTagNotInUse(tag);

            // obtain the extension object and prepare to write
            IExtension extn = instance.GetExtensionObject(true);
            if (extn == null) throw new InvalidOperationException("No extension object available; appended data would be lost.");
            bool commit = false;
            Stream stream = extn.BeginAppend();
            try
            {
                using (ProtoWriter writer = ProtoWriter.Create(stream, model, null))
                {
                    model.TrySerializeAuxiliaryType(writer, null, format, tag, value, false, null);
                    writer.Close();
                }
                commit = true;
            }
            finally
            {
                extn.EndAppend(stream, commit);
            }
        }

        //        /// <summary>
        //        /// Stores the given value into the instance's stream; the serializer
        //        /// is inferred from TValue and format.
        //        /// </summary>
        //        /// <remarks>Needs to be public to be callable thru reflection in Silverlight</remarks>
        //        public static void AppendExtendValueTyped<TSource, TValue>(
        //            TypeModel model, TSource instance, int tag, DataFormat format, TValue value)
        //            where TSource : class, IExtensible
        //        {
        //            AppendExtendValue(model, instance, tag, format, value);
        //        }

    }

}