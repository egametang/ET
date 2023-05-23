#pragma once
#include <functional>

#include "bright/serialization/ByteBuf.h"
#include "bright/CfgBean.hpp"

using ByteBuf = ::bright::serialization::ByteBuf;

namespace {{assembly.top_module}}
{

{{~for e in x.enum_codes~}}
{{e}}
{{~end~}}

{{~for b in x.beans~}}
{{b.cpp_namespace_begin}} class {{b.name}}; {{b.cpp_namespace_end}}
{{~end~}}

{{~for b in x.bean_codes~}}
{{b}}
{{~end~}}

{{~for t in x.table_codes~}}
{{t}}
{{~end~}}

{{x.tables_code}}

}
