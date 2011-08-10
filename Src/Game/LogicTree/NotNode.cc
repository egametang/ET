#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/NotNode.h"

namespace Egametang {

bool NotNode::Run()
{
	return !node->Run();
}

} // namespace Egametang

