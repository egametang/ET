#if !NO_RUNTIME
using System;

using ProtoBuf.Meta;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif



namespace ProtoBuf.Serializers
{
    sealed class PropertyDecorator : ProtoDecoratorBase
    {
        public override Type ExpectedType { get { return forType; } }
        private readonly PropertyInfo property;
        private readonly Type forType;
        public override bool RequiresOldValue { get { return true; } }
        public override bool ReturnsValue { get { return false; } }
        private readonly bool readOptionsWriteValue;
        private readonly MethodInfo shadowSetter;
        public PropertyDecorator(TypeModel model, Type forType, PropertyInfo property, IProtoSerializer tail) : base(tail)
        {
            Helpers.DebugAssert(forType != null);
            Helpers.DebugAssert(property != null);
            this.forType = forType;
            this.property = property;
            SanityCheck(model, property, tail, out readOptionsWriteValue, true, true);
            shadowSetter = GetShadowSetter(model, property);
        }
        private static void SanityCheck(TypeModel model, PropertyInfo property, IProtoSerializer tail, out bool writeValue, bool nonPublic, bool allowInternal) {
            if(property == null) throw new ArgumentNullException("property");
            
            writeValue = tail.ReturnsValue && (GetShadowSetter(model, property) != null || (property.CanWrite && Helpers.GetSetMethod(property, nonPublic, allowInternal) != null));
            if (!property.CanRead || Helpers.GetGetMethod(property, nonPublic, allowInternal) == null)
            {
                throw new InvalidOperationException("Cannot serialize property without a get accessor");
            }
            if (!writeValue && (!tail.RequiresOldValue || Helpers.IsValueType(tail.ExpectedType)))
            { // so we can't save the value, and the tail doesn't use it either... not helpful
                // or: can't write the value, so the struct value will be lost
                throw new InvalidOperationException("Cannot apply changes to property " + property.DeclaringType.FullName + "." + property.Name);
            }
        }
        static MethodInfo GetShadowSetter(TypeModel model, PropertyInfo property)
        {
#if WINRT            
            MethodInfo method = Helpers.GetInstanceMethod(property.DeclaringType.GetTypeInfo(), "Set" + property.Name, new Type[] { property.PropertyType });
#else
            
#if FEAT_IKVM
            Type reflectedType = property.DeclaringType;
#else
            Type reflectedType = property.ReflectedType;
#endif
            MethodInfo method = Helpers.GetInstanceMethod(reflectedType, "Set" + property.Name, new Type[] { property.PropertyType });
#endif
            if (method == null || !method.IsPublic || method.ReturnType != model.MapType(typeof(void))) return null;
            return method;
        }
#if !FEAT_IKVM
        public override void Write(object value, ProtoWriter dest)
        {
            Helpers.DebugAssert(value != null);
            value = property.GetValue(value, null);
            if(value != null) Tail.Write(value, dest);
        }
        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value != null);

            object oldVal = Tail.RequiresOldValue ? property.GetValue(value, null) : null;
            object newVal = Tail.Read(oldVal, source);
            if (readOptionsWriteValue && newVal != null) // if the tail returns a null, intepret that as *no assign*
            {
                if (shadowSetter == null)
                {
                    property.SetValue(value, newVal, null);
                }
                else
                {
                    shadowSetter.Invoke(value, new object[] { newVal });
                }
            }
            return null;
        }
#endif

#if FEAT_COMPILER
        protected override void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.LoadAddress(valueFrom, ExpectedType);
            ctx.LoadValue(property);
            ctx.WriteNullCheckedTail(property.PropertyType, Tail, null);
        }
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {

            bool writeValue;
            SanityCheck(ctx.Model, property, Tail, out writeValue, ctx.NonPublic, ctx.AllowInternal(property));
            if (ExpectedType.IsValueType && valueFrom == null)
            {
                throw new InvalidOperationException("Attempt to mutate struct on the head of the stack; changes would be lost");
            }

            using (Compiler.Local loc = ctx.GetLocalWithValue(ExpectedType, valueFrom))
            {
                if (Tail.RequiresOldValue)
                {
                    ctx.LoadAddress(loc, ExpectedType); // stack is: old-addr
                    ctx.LoadValue(property); // stack is: old-value
                }
                Type propertyType = property.PropertyType;
                ctx.ReadNullCheckedTail(propertyType, Tail, null); // stack is [new-value]

                if (writeValue)
                {
                    using (Compiler.Local newVal = new Compiler.Local(ctx, property.PropertyType))
                    {
                        ctx.StoreValue(newVal); // stack is empty

                        Compiler.CodeLabel allDone = new Compiler.CodeLabel(); // <=== default structs
                        if (!propertyType.IsValueType)
                        { // if the tail returns a null, intepret that as *no assign*
                            allDone = ctx.DefineLabel();
                            ctx.LoadValue(newVal); // stack is: new-value
                            ctx.BranchIfFalse(@allDone, true); // stack is empty
                        }
                        // assign the value
                        ctx.LoadAddress(loc, ExpectedType); // parent-addr
                        ctx.LoadValue(newVal); // parent-obj|new-value
                        if (shadowSetter == null)
                        {
                            ctx.StoreValue(property); // empty
                        }
                        else
                        {
                            ctx.EmitCall(shadowSetter); // empty
                        }
                        if (!propertyType.IsValueType)
                        {
                            ctx.MarkLabel(allDone);
                        }
                    }

                }
                else
                { // don't want return value; drop it if anything there
                    // stack is [new-value]
                    if (Tail.ReturnsValue) { ctx.DiscardValue(); }
                }
            }
        }
#endif

        internal static bool CanWrite(TypeModel model, MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException("member");

            PropertyInfo prop = member as PropertyInfo;
            if (prop != null) return prop.CanWrite || GetShadowSetter(model, prop) != null;

            return member is FieldInfo; // fields are always writeable; anything else: JUST SAY NO!
        }
    }
}
#endif