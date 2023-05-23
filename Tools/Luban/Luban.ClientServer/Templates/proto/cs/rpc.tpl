{{
    name = x.name
    full_name = x.full_name
    parent = x.parent
    fields = x.fields
    targ_type = x.targ_type
    tres_type = x.tres_type
}}
using Bright.Serialization;

namespace {{x.namespace_with_top_module}}
{
   
{{~if x.comment != '' ~}}
    /// <summary>
    /// {{x.escape_comment}}
    /// </summary>
{{~end~}}
    public sealed class {{name}} : Bright.Net.Codecs.Rpc<{{cs_define_type targ_type}}, {{cs_define_type tres_type}}>
    {
        public {{name}}()
        {
        }
        
        public const int __ID__ = {{x.id}};

        public override int GetTypeId()
        {
            return __ID__;
        }

        public override void Reset()
        {
            throw new System.NotImplementedException();
        }

        public override object Clone()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"{{full_name}}{%{ {{arg:{Arg},res:{Res} }} }%}";
        }
    }
}
