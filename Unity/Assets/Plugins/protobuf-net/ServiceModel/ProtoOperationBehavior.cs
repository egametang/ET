#if FEAT_SERVICEMODEL && PLAT_XMLSERIALIZER && !NO_GENERICS
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;
using ProtoBuf.Meta;

namespace ProtoBuf.ServiceModel
{
    /// <summary>
    /// Describes a WCF operation behaviour that can perform protobuf serialization
    /// </summary>
    public sealed class ProtoOperationBehavior : DataContractSerializerOperationBehavior
    {
        private TypeModel model;
        /// <summary>
        /// The type-model that should be used with this behaviour
        /// </summary>
        public TypeModel Model
        {
            get { return model; }
            set {
                if (value == null) throw new ArgumentNullException("Model");
                model = value;
            }

        }
        /// <summary>
        /// Create a new ProtoOperationBehavior instance
        /// </summary>
        public ProtoOperationBehavior(OperationDescription operation) : base(operation)
        {
#if !NO_RUNTIME
            model = RuntimeTypeModel.Default;
#endif
        }
        //public ProtoOperationBehavior(OperationDescription operation, DataContractFormatAttribute dataContractFormat) : base(operation, dataContractFormat) { }

        /// <summary>
        /// Creates a protobuf serializer if possible (falling back to the default WCF serializer)
        /// </summary>
        public override XmlObjectSerializer CreateSerializer(Type type, System.Xml.XmlDictionaryString name, System.Xml.XmlDictionaryString ns, IList<Type> knownTypes)
        {
            if (model == null) throw new InvalidOperationException("No Model instance has been assigned to the ProtoOperationBehavior");
            return XmlProtoSerializer.TryCreate(model, type) ?? base.CreateSerializer(type, name, ns, knownTypes);
        }
    }
}
#endif