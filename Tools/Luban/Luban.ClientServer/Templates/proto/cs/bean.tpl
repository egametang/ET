{{
    is_value_type = x.is_value_type
    is_abstract_type = x.is_abstract_type
    name = x.name
    full_name = x.full_name
    parent_def_type = x.parent_def_type
    parent = x.parent
    fields = x.fields
    hierarchy_fields = x.hierarchy_fields
}}
using Bright.Serialization;

namespace {{x.namespace_with_top_module}}
{

{{~if x.comment != '' ~}}
    /// <summary>
    /// {{x.escape_comment}}
    /// </summary>
{{~end~}}
    public  {{if is_value_type}}struct{{else}}{{x.cs_class_modifier}} class{{end}} {{name}} : {{if parent_def_type}} {{parent}} {{else}} Bright.Serialization.BeanBase {{end}}
    {
        {{~if !is_value_type~}}
        public {{name}}()
        {
        }
        {{~end~}}

        public {{name}}(Bright.Common.NotNullInitialization _) {{if parent_def_type}} : base(_) {{end}}
        {
            {{~ for field in fields ~}}
                {{~if cs_need_init field.ctype~}}
            {{cs_init_field_ctor_value field.convention_name field.ctype}}
                {{~else if is_value_type~}}
            {{field.convention_name}} = default;
                {{~end~}}
            {{~end~}}
        }

        public static void Serialize{{name}}(ByteBuf _buf, {{name}} x)
        {
    {{~if is_abstract_type~}}
            _buf.WriteInt(x.GetTypeId());
            x.Serialize(_buf);
    {{~else~}}
            x.Serialize(_buf);
    {{~end~}}
        }

        public static {{name}} Deserialize{{name}}(ByteBuf _buf)
        {
        {{~if is_abstract_type~}}
           {{full_name}} x;
            switch (_buf.ReadInt())
            {
            {{~ for child in x.hierarchy_not_abstract_children~}}
                case {{child.full_name}}.__ID__: x = new {{child.full_name}}(); break;
            {{~end~}}
                default: throw new SerializationException();
            }
            x.Deserialize(_buf);
        {{~else~}}
            var x = new {{full_name}}();
            x.Deserialize(_buf);
        {{~end~}}
            return x;
        }

        {{~ for field in fields ~}}
{{~if field.comment != '' ~}}
        /// <summary>
        /// {{field.escape_comment}}
        /// </summary>
{{~end~}}
         public {{cs_define_type field.ctype}} {{field.convention_name}};

        {{~end~}}

        {{~if !is_abstract_type~}}
        public const int __ID__ = {{x.id}};
        public override int GetTypeId() => __ID__;

        public override void Serialize(ByteBuf _buf)
        {
            {{~ for field in hierarchy_fields ~}}
            {{cs_serialize '_buf' field.convention_name field.ctype}}
            {{~end~}}
        }

        public override void Deserialize(ByteBuf _buf)
        {
            {{~ for field in hierarchy_fields ~}}
            {{cs_deserialize '_buf' field.convention_name field.ctype}}
            {{~end~}}
        }

        public override string ToString()
        {
            return "{{full_name}}{ "
        {{~ for field in hierarchy_fields ~}}
            + "{{field.convention_name}}:" + {{cs_to_string field.convention_name field.ctype}} + ","
        {{~end~}}
            + "}";
        }
        {{~end~}}
    }

}
