#if FEAT_SERVICEMODEL && PLAT_XMLSERIALIZER
using System.ServiceModel.Description;

namespace ProtoBuf.ServiceModel
{
    /// <summary>
    /// Behavior to swap out DatatContractSerilaizer with the XmlProtoSerializer for a given endpoint.
    ///  <example>
    /// Add the following to the server and client app.config in the system.serviceModel section:
    ///  <behaviors>
    ///    <endpointBehaviors>
    ///      <behavior name="ProtoBufBehaviorConfig">
    ///        <ProtoBufSerialization/>
    ///      </behavior>
    ///    </endpointBehaviors>
    ///  </behaviors>
    ///  <extensions>
    ///    <behaviorExtensions>
    ///      <add name="ProtoBufSerialization" type="ProtoBuf.ServiceModel.ProtoBehaviorExtension, protobuf-net, Version=1.0.0.255, Culture=neutral, PublicKeyToken=257b51d87d2e4d67"/>
    ///    </behaviorExtensions>
    ///  </extensions>
    /// 
    /// Configure your endpoints to have a behaviorConfiguration as follows:
    /// 
    ///  <service name="TK.Framework.Samples.ServiceModel.Contract.SampleService">
    ///    <endpoint address="http://myhost:9003/SampleService" binding="basicHttpBinding" behaviorConfiguration="ProtoBufBehaviorConfig"
    ///     bindingConfiguration="basicHttpBindingConfig" name="basicHttpProtoBuf" contract="ISampleServiceContract" />
    ///  </service>
    ///  <client>
    ///      <endpoint address="http://myhost:9003/SampleService" binding="basicHttpBinding"
    ///          bindingConfiguration="basicHttpBindingConfig" contract="ISampleServiceContract"
    ///          name="BasicHttpProtoBufEndpoint" behaviorConfiguration="ProtoBufBehaviorConfig"/>
    ///   </client>
    /// </example>
    /// </summary>
    public class ProtoEndpointBehavior : IEndpointBehavior
    {
        #region IEndpointBehavior Members

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            ReplaceDataContractSerializerOperationBehavior(endpoint);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            ReplaceDataContractSerializerOperationBehavior(endpoint);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }

        private static void ReplaceDataContractSerializerOperationBehavior(ServiceEndpoint serviceEndpoint)
        {
            foreach (OperationDescription operationDescription in serviceEndpoint.Contract.Operations)
            {
                ReplaceDataContractSerializerOperationBehavior(operationDescription);
            }
        }

        private static void ReplaceDataContractSerializerOperationBehavior(OperationDescription description)
        {
            DataContractSerializerOperationBehavior dcsOperationBehavior = description.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (dcsOperationBehavior != null)
            {
                description.Behaviors.Remove(dcsOperationBehavior);

                ProtoOperationBehavior newBehavior = new ProtoOperationBehavior(description);
                newBehavior.MaxItemsInObjectGraph = dcsOperationBehavior.MaxItemsInObjectGraph;
                description.Behaviors.Add(newBehavior);
            }
        }

        #endregion
    }
}
#endif