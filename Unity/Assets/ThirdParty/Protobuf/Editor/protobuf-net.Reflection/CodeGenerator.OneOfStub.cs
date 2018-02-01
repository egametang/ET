using Google.Protobuf.Reflection;

namespace ProtoBuf.Reflection
{
    partial class CommonCodeGenerator
    {
        /// <summary>
        /// Represents the union summary of a one-of declaration
        /// </summary>
        protected class OneOfStub
        {
            /// <summary>
            /// The underlying descriptor
            /// </summary>
            public OneofDescriptorProto OneOf { get; }

            internal OneOfStub(OneofDescriptorProto decl)
            {
                OneOf = decl;
            }
            internal int Count32 { get; private set; }
            internal int Count64 { get; private set; }
            internal int Count128 { get; private set; }
            internal int CountRef { get; private set; }
            internal int CountTotal => CountRef + Count32 + Count64;

            private void AccountFor(FieldDescriptorProto.Type type, string typeName)
            {
                switch (type)
                {
                    case FieldDescriptorProto.Type.TypeBool:
                    case FieldDescriptorProto.Type.TypeEnum:
                    case FieldDescriptorProto.Type.TypeFixed32:
                    case FieldDescriptorProto.Type.TypeFloat:
                    case FieldDescriptorProto.Type.TypeInt32:
                    case FieldDescriptorProto.Type.TypeSfixed32:
                    case FieldDescriptorProto.Type.TypeSint32:
                    case FieldDescriptorProto.Type.TypeUint32:
                        Count32++;
                        break;
                    case FieldDescriptorProto.Type.TypeDouble:
                    case FieldDescriptorProto.Type.TypeFixed64:
                    case FieldDescriptorProto.Type.TypeInt64:
                    case FieldDescriptorProto.Type.TypeSfixed64:
                    case FieldDescriptorProto.Type.TypeSint64:
                    case FieldDescriptorProto.Type.TypeUint64:
                        Count32++;
                        Count64++;
                        break;
                    case FieldDescriptorProto.Type.TypeMessage:
                        switch(typeName)
                        {
                            case ".google.protobuf.Timestamp":
                            case ".google.protobuf.Duration":
                                Count64++;
                                break;
                            case ".bcl.Guid":
                                Count128++;
                                break;
                            default:
                                CountRef++;
                                break;
                        }
                        break;
                    default:
                        CountRef++;
                        break;
                }
            }
            internal string GetStorage(FieldDescriptorProto.Type type, string typeName)
            {
                switch (type)
                {
                    case FieldDescriptorProto.Type.TypeBool:
                        return "Boolean";
                    case FieldDescriptorProto.Type.TypeInt32:
                    case FieldDescriptorProto.Type.TypeSfixed32:
                    case FieldDescriptorProto.Type.TypeSint32:
                    case FieldDescriptorProto.Type.TypeFixed32:
                    case FieldDescriptorProto.Type.TypeEnum:
                        return "Int32";
                    case FieldDescriptorProto.Type.TypeFloat:
                        return "Single";
                    case FieldDescriptorProto.Type.TypeUint32:
                        return "UInt32";
                    case FieldDescriptorProto.Type.TypeDouble:
                        return "Double";
                    case FieldDescriptorProto.Type.TypeFixed64:
                    case FieldDescriptorProto.Type.TypeInt64:
                    case FieldDescriptorProto.Type.TypeSfixed64:
                    case FieldDescriptorProto.Type.TypeSint64:
                        return "Int64";
                    case FieldDescriptorProto.Type.TypeUint64:
                        return "UInt64";
                    case FieldDescriptorProto.Type.TypeMessage:
                        switch (typeName)
                        {
                            case ".google.protobuf.Timestamp":
                                return "DateTime";
                            case ".google.protobuf.Duration":
                                return "TimeSpan";
                            case ".bcl.Guid":
                                return "Guid";
                            default:
                                return "Object";
                        }
                    default:
                        return "Object";
                }
            }
            internal static OneOfStub[] Build(GeneratorContext context, DescriptorProto message)
            {
                if (message.OneofDecls.Count == 0) return null;
                var stubs = new OneOfStub[message.OneofDecls.Count];
                int index = 0;
                foreach (var decl in message.OneofDecls)
                {
                    stubs[index++] = new OneOfStub(decl);
                }
                foreach (var field in message.Fields)
                {
                    if (field.ShouldSerializeOneofIndex())
                    {
                        stubs[field.OneofIndex].AccountFor(field.type, field.TypeName);
                    }
                }
                return stubs;
            }
            private bool isFirst = true;
            internal bool IsFirst()
            {
                if (isFirst)
                {
                    isFirst = false;
                    return true;
                }
                return false;
            }

            internal string GetUnionType()
            {
                if (Count128 != 0)
                {
                    return CountRef == 0 ? "DiscriminatedUnion128" : "DiscriminatedUnion128Object";
                }
                if (Count64 != 0)
                {
                    return CountRef == 0 ? "DiscriminatedUnion64" : "DiscriminatedUnion64Object";
                }
                if (Count32 != 0)
                {
                    return CountRef == 0 ? "DiscriminatedUnion32" : "DiscriminatedUnion32Object";
                }
                return "DiscriminatedUnionObject";
            }
        }
    }
}
