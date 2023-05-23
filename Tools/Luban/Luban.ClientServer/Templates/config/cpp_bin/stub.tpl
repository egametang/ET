#include <algorithm>
#include "gen_types.h"

using ByteBuf = bright::serialization::ByteBuf;

namespace {{assembly.top_module}}
{
    {{~for type in x.types~}}

    bool {{type.cpp_full_name}}::deserialize(ByteBuf& _buf)
    {
        {{~if type.parent_def_type~}}
        if (!{{type.parent_def_type.cpp_full_name}}::deserialize(_buf))
        {
            return false;
        }
        {{~end~}}

        {{~ for field in type.export_fields ~}}
        {{cpp_deserialize '_buf' field.convention_name field.ctype}}
        {{~if field.index_field ~}}
        for(auto& _v : this->{{field.convention_name}})
        { 
            {{field.convention_name}}_Index.insert({_v->{{field.index_field.convention_name}}, _v});
        }
        {{~end~}}
        {{~end~}}

        return true;
    }

    bool {{type.cpp_full_name}}::deserialize{{type.name}}(ByteBuf& _buf, ::bright::SharedPtr<{{type.cpp_full_name}}>& _out)
    {
    {{~if type.is_abstract_type~}}
        int id;
        if (!_buf.readInt(id)) return false;
        switch (id)
        {
        {{~for child in type.hierarchy_not_abstract_children~}}
            case {{child.cpp_full_name}}::__ID__: { _out.reset(new {{child.cpp_full_name}}()); if (_out->deserialize(_buf)) { return true; } else { _out.reset(); return false;} }
        {{~end~}}
            default: { _out = nullptr; return false;}
        }
    {{~else~}}
        _out.reset(new {{type.cpp_full_name}}());
        if (_out->deserialize(_buf))
        {
            return true;
        }
        else
        { 
            _out.reset();
            return false;
        }
    {{~end~}}
    }

    void {{type.cpp_full_name}}::resolve(::bright::HashMap<::bright::String, void*>& _tables)
    {
        {{~if type.parent_def_type~}}
        {{type.parent_def_type.name}}::resolve(_tables);
        {{~end~}}
        {{~ for field in type.export_fields ~}}
        {{~if field.gen_ref~}}
        {{cpp_ref_validator_resolve field}}
        {{~else if field.has_recursive_ref~}}
        {{cpp_recursive_resolve field '_tables'}}
        {{~end~}}
        {{~end~}}
    }
    {{~end~}}
}