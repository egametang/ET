using System;
using Google.Protobuf;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;


public class Adapt_IMessage : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get { return typeof(IMessage); }
    }

    public override Type AdaptorType
    {
        get { return typeof(Adaptor); }
    }

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    public class Adaptor : MyAdaptor, IMessage
    {
        public Adaptor(AppDomain appdomain, ILTypeInstance instance) : base(appdomain,instance)
        {
        }

        protected override AdaptHelper.AdaptMethod[] GetAdaptMethods()
        {
            AdaptHelper.AdaptMethod[] methods = 
            {
                new AdaptHelper.AdaptMethod {Name = "MergeFrom", ParamCount = 1}, 
                new AdaptHelper.AdaptMethod {Name = "WriteTo", ParamCount = 1}, 
                new AdaptHelper.AdaptMethod {Name = "CalculateSize", ParamCount = 0}, 
            };
            return methods;
        }

        public void MergeFrom(CodedInputStream input)
        {
            Invoke(0, input);
        }

        public void WriteTo(CodedOutputStream output)
        {
            Invoke(1, output);
        }

        public int CalculateSize()
        {
            return (int)Invoke(2);
        }
    }
}
