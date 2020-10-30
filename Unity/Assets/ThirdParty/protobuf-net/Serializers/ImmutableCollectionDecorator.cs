#if !NO_RUNTIME
using System;
using System.Collections;
using System.Reflection;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
    sealed class ImmutableCollectionDecorator : ListDecorator
    {
        protected override bool RequireAdd { get { return false; } }

        static Type ResolveIReadOnlyCollection(Type declaredType, Type t)
        {
#if COREFX || PROFILE259
            if (CheckIsIReadOnlyCollectionExactly(declaredType.GetTypeInfo())) return declaredType;
			foreach (Type intImplBasic in declaredType.GetTypeInfo().ImplementedInterfaces)
            {
                TypeInfo intImpl = intImplBasic.GetTypeInfo();
                if (CheckIsIReadOnlyCollectionExactly(intImpl)) return intImplBasic;
            }
#else
            if (CheckIsIReadOnlyCollectionExactly(declaredType)) return declaredType;
            foreach (Type intImpl in declaredType.GetInterfaces())
            {
                if (CheckIsIReadOnlyCollectionExactly(intImpl)) return intImpl;
            }
#endif
            return null;
        }

#if WINRT || COREFX || PROFILE259
        static bool CheckIsIReadOnlyCollectionExactly(TypeInfo t)
#else
        static bool CheckIsIReadOnlyCollectionExactly(Type t)
#endif
        {
            if (t != null && t.IsGenericType && t.Name.StartsWith("IReadOnlyCollection`"))
            {
#if WINRT || COREFX || PROFILE259
                Type[] typeArgs = t.GenericTypeArguments;
                if (typeArgs.Length != 1 && typeArgs[0].GetTypeInfo().Equals(t)) return false;
#else
                Type[] typeArgs = t.GetGenericArguments();
                if (typeArgs.Length != 1 && typeArgs[0] != t) return false;
#endif
                    
                return true;
            }
            return false;
        }

        internal static bool IdentifyImmutable(TypeModel model, Type declaredType, out MethodInfo builderFactory, out PropertyInfo isEmpty, out PropertyInfo length, out MethodInfo add, out MethodInfo addRange, out MethodInfo finish)
        {
            builderFactory = add = addRange = finish = null;
            isEmpty = length = null;
            if (model == null || declaredType == null) return false;
#if COREFX || PROFILE259
			TypeInfo declaredTypeInfo = declaredType.GetTypeInfo();
#else
            Type declaredTypeInfo = declaredType;
#endif

            // try to detect immutable collections; firstly, they are all generic, and all implement IReadOnlyCollection<T> for some T
            if (!declaredTypeInfo.IsGenericType) return false;

#if COREFX || PROFILE259
			Type[] typeArgs = declaredTypeInfo.GenericTypeArguments, effectiveType;
#else
            Type[] typeArgs = declaredTypeInfo.GetGenericArguments(), effectiveType;
#endif
            switch (typeArgs.Length)
            {
                case 1:
                    effectiveType = typeArgs;
                    break; // fine
                case 2:
                    Type kvp = model.MapType(typeof(System.Collections.Generic.KeyValuePair<,>));
                    if (kvp == null) return false;
                    kvp = kvp.MakeGenericType(typeArgs);
                    effectiveType = new Type[] { kvp };
                    break;
                default:
                    return false; // no clue!
            }

            if (ResolveIReadOnlyCollection(declaredType, null) == null) return false; // no IReadOnlyCollection<T> found

            // and we want to use the builder API, so for generic Foo<T> or IFoo<T> we want to use Foo.CreateBuilder<T>
            string name = declaredType.Name;
            int i = name.IndexOf('`');
            if (i <= 0) return false;
            name = declaredTypeInfo.IsInterface ? name.Substring(1, i - 1) : name.Substring(0, i);

            Type outerType = model.GetType(declaredType.Namespace + "." + name, declaredTypeInfo.Assembly);
            // I hate special-cases...
            if (outerType == null && name == "ImmutableSet")
            {
                outerType = model.GetType(declaredType.Namespace + ".ImmutableHashSet", declaredTypeInfo.Assembly);
            }
            if (outerType == null) return false;

#if PROFILE259
			foreach (MethodInfo method in outerType.GetTypeInfo().DeclaredMethods)
#else
            foreach (MethodInfo method in outerType.GetMethods())
#endif
            {
                if (!method.IsStatic || method.Name != "CreateBuilder" || !method.IsGenericMethodDefinition || method.GetParameters().Length != 0
                    || method.GetGenericArguments().Length != typeArgs.Length) continue;

                builderFactory = method.MakeGenericMethod(typeArgs);
                break;
            }
            Type voidType = model.MapType(typeof(void));
            if (builderFactory == null || builderFactory.ReturnType == null || builderFactory.ReturnType == voidType) return false;

#if COREFX
            TypeInfo typeInfo = declaredType.GetTypeInfo();
#else
            Type typeInfo = declaredType;
#endif
            isEmpty = Helpers.GetProperty(typeInfo, "IsDefaultOrEmpty", false); //struct based immutabletypes can have both a "default" and "empty" state
            if (isEmpty == null) isEmpty = Helpers.GetProperty(typeInfo, "IsEmpty", false);
            if (isEmpty == null)
            {
                //Fallback to checking length if a "IsEmpty" property is not found
                length = Helpers.GetProperty(typeInfo, "Length", false);
                if (length == null) length = Helpers.GetProperty(typeInfo, "Count", false);

                if (length == null) length = Helpers.GetProperty(ResolveIReadOnlyCollection(declaredType, effectiveType[0]), "Count", false);

                if (length == null) return false;
            }

            add = Helpers.GetInstanceMethod(builderFactory.ReturnType, "Add", effectiveType);
            if (add == null) return false;

            finish = Helpers.GetInstanceMethod(builderFactory.ReturnType, "ToImmutable", Helpers.EmptyTypes);
            if (finish == null || finish.ReturnType == null || finish.ReturnType == voidType) return false;

            if (!(finish.ReturnType == declaredType || Helpers.IsAssignableFrom(declaredType, finish.ReturnType))) return false;

            addRange = Helpers.GetInstanceMethod(builderFactory.ReturnType, "AddRange", new Type[] { declaredType });
            if (addRange == null)
            {
                Type enumerable = model.MapType(typeof(System.Collections.Generic.IEnumerable<>), false);
                if (enumerable != null)
                {
                    addRange = Helpers.GetInstanceMethod(builderFactory.ReturnType, "AddRange", new Type[] { enumerable.MakeGenericType(effectiveType) });
                }
            }

            return true;
        }

        private readonly MethodInfo builderFactory, add, addRange, finish;
        private readonly PropertyInfo isEmpty, length;
        internal ImmutableCollectionDecorator(TypeModel model, Type declaredType, Type concreteType, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList, bool overwriteList, bool supportNull,
            MethodInfo builderFactory, PropertyInfo isEmpty, PropertyInfo length, MethodInfo add, MethodInfo addRange, MethodInfo finish)
            : base(model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList, overwriteList, supportNull)
        {
            this.builderFactory = builderFactory;
            this.isEmpty = isEmpty;
            this.length = length;
            this.add = add;
            this.addRange = addRange;
            this.finish = finish;
        }

        public override object Read(object value, ProtoReader source)
        {
            object builderInstance = builderFactory.Invoke(null, null);
            int field = source.FieldNumber;
            object[] args = new object[1];
            if (AppendToCollection && value != null && (isEmpty != null ? !(bool)isEmpty.GetValue(value, null) : (int)length.GetValue(value, null) != 0))
            {
                if (addRange != null)
                {
                    args[0] = value;
                    addRange.Invoke(builderInstance, args);
                }
                else
                {
                    foreach (object item in (ICollection)value)
                    {
                        args[0] = item;
                        add.Invoke(builderInstance, args);
                    }
                }
            }

            if (packedWireType != WireType.None && source.WireType == WireType.String)
            {
                SubItemToken token = ProtoReader.StartSubItem(source);
                while (ProtoReader.HasSubValue(packedWireType, source))
                {
                    args[0] = Tail.Read(null, source);
                    add.Invoke(builderInstance, args);
                }
                ProtoReader.EndSubItem(token, source);
            }
            else
            {
                do
                {
                    args[0] = Tail.Read(null, source);
                    add.Invoke(builderInstance, args);
                } while (source.TryReadFieldHeader(field));
            }

            return finish.Invoke(builderInstance, null);
        }

#if FEAT_COMPILER
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            using (Compiler.Local oldList = AppendToCollection ? ctx.GetLocalWithValue(ExpectedType, valueFrom) : null)
            using (Compiler.Local builder = new Compiler.Local(ctx, builderFactory.ReturnType))
            {
                ctx.EmitCall(builderFactory);
                ctx.StoreValue(builder);

                if (AppendToCollection)
                {
                    Compiler.CodeLabel done = ctx.DefineLabel();
                    if (!Helpers.IsValueType(ExpectedType))
                    {
                        ctx.LoadValue(oldList);
                        ctx.BranchIfFalse(done, false); // old value null; nothing to add
                    }

                    ctx.LoadAddress(oldList, oldList.Type);
                    if (isEmpty != null)
                    {
                        ctx.EmitCall(Helpers.GetGetMethod(isEmpty, false, false));
                        ctx.BranchIfTrue(done, false); // old list is empty; nothing to add
                    }
                    else
                    {
                        ctx.EmitCall(Helpers.GetGetMethod(length, false, false));
                        ctx.BranchIfFalse(done, false); // old list is empty; nothing to add
                    }

                    Type voidType = ctx.MapType(typeof(void));
                    if (addRange != null)
                    {
                        ctx.LoadValue(builder);
                        ctx.LoadValue(oldList);
                        ctx.EmitCall(addRange);
                        if (addRange.ReturnType != null && add.ReturnType != voidType) ctx.DiscardValue();
                    }
                    else
                    {
                        // loop and call Add repeatedly
                        MethodInfo moveNext, current, getEnumerator = GetEnumeratorInfo(ctx.Model, out moveNext, out current);
                        Helpers.DebugAssert(moveNext != null);
                        Helpers.DebugAssert(current != null);
                        Helpers.DebugAssert(getEnumerator != null);

                        Type enumeratorType = getEnumerator.ReturnType;
                        using (Compiler.Local iter = new Compiler.Local(ctx, enumeratorType))
                        {
                            ctx.LoadAddress(oldList, ExpectedType);
                            ctx.EmitCall(getEnumerator);
                            ctx.StoreValue(iter);
                            using (ctx.Using(iter))
                            {
                                Compiler.CodeLabel body = ctx.DefineLabel(), next = ctx.DefineLabel();
                                ctx.Branch(next, false);

                                ctx.MarkLabel(body);
                                ctx.LoadAddress(builder, builder.Type);
                                ctx.LoadAddress(iter, enumeratorType);
                                ctx.EmitCall(current);
                                ctx.EmitCall(add);
                                if (add.ReturnType != null && add.ReturnType != voidType) ctx.DiscardValue();

                                ctx.MarkLabel(@next);
                                ctx.LoadAddress(iter, enumeratorType);
                                ctx.EmitCall(moveNext);
                                ctx.BranchIfTrue(body, false);
                            }
                        }
                    }


                    ctx.MarkLabel(done);
                }

                EmitReadList(ctx, builder, Tail, add, packedWireType, false);

                ctx.LoadAddress(builder, builder.Type);
                ctx.EmitCall(finish);
                if (ExpectedType != finish.ReturnType)
                {
                    ctx.Cast(ExpectedType);
                }
            }
        }
#endif
    }
}
#endif
