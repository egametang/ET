#include <boost/foreach.hpp>
#include "Base/Marcos.h"
#include "Event/NotNode.h"

namespace Egametang {
NotNode::~NotNode()
{
	delete node;
}

bool NotNode::Check(ContexIf* contex)
{
	return !node->Check(contex);
}

} // namespace Egametang

