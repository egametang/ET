#ifndef EVENT_NODEFACTORIES_H
#define EVENT_NODEFACTORIES_H

#include <vector>
#include "Event/NodeIf.h"

namespace Egametang {

class NodeFactories
{
private:
	std::vector<NodeFactoryIf*> factories;

public:
	NodeFactories();

	virtual ~NodeFactories();

	virtual NodeIf* GetInstance(const EventNode& conf);
};

} // namespace Egametang

#endif // EVENT_NODEFACTORIES_H
