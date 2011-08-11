#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "LogicTree/NotNode.h"

namespace Egametang {

bool NotNode::Run(LogicContex* contex)
{
	return !node->Run(contex);
}

} // namespace Egametang

