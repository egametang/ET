#if !NO_RUNTIME
using System;
using ProtoBuf.Meta;

#if FEAT_COMPILER
using System.Reflection.Emit;
#endif

namespace ProtoBuf.Serializers
{
    sealed class SubItemSerializer : IProtoTypeSerializer
    {
        bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return ((IProtoTypeSerializer)proxy.Serializer).HasCallbacks(callbackType);
        }

        bool IProtoTypeSerializer.CanCreateInstance()
        {
            return ((IProtoTypeSerializer)proxy.Serializer).CanCreateInstance();
        }

#if FEAT_COMPILER
        void IProtoTypeSerializer.EmitCallback(Compiler.CompilerContext ctx, Compiler.Local valueFrom, TypeModel.CallbackType callbackType)
        {
            ((IProtoTypeSerializer)proxy.Serializer).EmitCallback(ctx, valueFrom, callbackType);
        }

        void IProtoTypeSerializer.EmitCreateInstance(Compiler.CompilerContext ctx)
        {
            ((IProtoTypeSerializer)proxy.Serializer).EmitCreateInstance(ctx);
        }
#endif

        void IProtoTypeSerializer.Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            ((IProtoTypeSerializer)proxy.Serializer).Callback(value, callbackType, context);
        }

        object IProtoTypeSerializer.CreateInstance(ProtoReader source)
        {
            return ((IProtoTypeSerializer)proxy.Serializer).CreateInstance(source);
        }

        private readonly int key;
        private readonly Type type;
        private readonly ISerializerProxy proxy;
        private readonly bool recursionCheck;
        public SubItemSerializer(Type type, int key, ISerializerProxy proxy, bool recursionCheck)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            this.key = key;
            this.recursionCheck = recursionCheck;
        }

        Type IProtoSerializer.ExpectedType => type;

        bool IProtoSerializer.RequiresOldValue => true;

        bool IProtoSerializer.ReturnsValue => true;

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            if (recursionCheck)
            {
                ProtoWriter.WriteObject(value, key, dest);
            }
            else
            {
                ProtoWriter.WriteRecursionSafeObject(value, key, dest);
            }
        }

        object IProtoSerializer.Read(object value, ProtoReader source)
        {
            return ProtoReader.ReadObject(value, key, source);
        }

#if FEAT_COMPILER
        bool EmitDedicatedMethod(Compiler.CompilerContext ctx, Compiler.Local valueFrom, bool read)
        {
            MethodBuilder method = ctx.GetDedicatedMethod(key, read);
            if (method == null) return false;

            using (Compiler.Local token = new ProtoBuf.Compiler.Local(ctx, ctx.MapType(typeof(SubItemToken))))
            {
                Type rwType = ctx.MapType(read ? typeof(ProtoReader) : typeof(ProtoWriter));
                ctx.LoadValue(valueFrom);
                if (!read) // write requires the object for StartSubItem; read doesn't
                {  // (if recursion-check is disabled [subtypes] then null is fine too)
                    if (Helpers.IsValueType(type) || !recursionCheck) { ctx.LoadNullRef(); }
                    else { ctx.CopyValue(); }
                }
                ctx.LoadReaderWriter();
                ctx.EmitCall(Helpers.GetStaticMethod(rwType, "StartSubItem",
                    read ? new Type[] { rwType } : new Type[] { ctx.MapType(typeof(object)), rwType }));
                ctx.StoreValue(token);

                // note: value already on the stack
                ctx.LoadReaderWriter();
                ctx.EmitCall(method);
                // handle inheritance (we will be calling the *base* version of things,
                // but we expect Read to return the "type" type)
                if (read && type != method.ReturnType) ctx.Cast(this.type);
                ctx.LoadValue(token);
                ctx.LoadReaderWriter();
                ctx.EmitCall(Helpers.GetStaticMethod(rwType, "EndSubItem", new Type[] { ctx.MapType(typeof(SubItemToken)), rwType }));
            }
            return true;
        }
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (!EmitDedicatedMethod(ctx, valueFrom, false))
            {
                ctx.LoadValue(valueFrom);
                if (Helpers.IsValueType(type)) ctx.CastToObject(type);
                ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(key)); // re-map for formality, but would expect identical, else dedicated method
                ctx.LoadReaderWriter();
                ctx.EmitCall(Helpers.GetStaticMethod(ctx.MapType(typeof(ProtoWriter)), recursionCheck ? "WriteObject" : "WriteRecursionSafeObject", new Type[] { ctx.MapType(typeof(object)), ctx.MapType(typeof(int)), ctx.MapType(typeof(ProtoWriter)) }));
            }
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (!EmitDedicatedMethod(ctx, valueFrom, true))
            {
                ctx.LoadValue(valueFrom);
                if (Helpers.IsValueType(type)) ctx.CastToObject(type);
                ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(key)); // re-map for formality, but would expect identical, else dedicated method
                ctx.LoadReaderWriter();
                ctx.EmitCall(Helpers.GetStaticMethod(ctx.MapType(typeof(ProtoReader)), "ReadObject"));
                ctx.CastFromObject(type);
            }
        }
#endif
    }
}
#endif