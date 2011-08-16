#ifndef EVENT_NODEIF_H
#define EVENT_NODEIF_H

#include <string>

namespace Egametang {

class ContexIf;
class EventNode;

class NodeIf
{
public:
	virtual ~NodeIf()
	{
	}
	virtual bool Run(ContexIf* contex) = 0;

	virtual void AddChildNode(NodeIf *node)
	{
	}

	virtual std::string ToString() = 0;
};

class NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf) = 0;
};

} // namespace Egametang

#endif // EVENT_NODEIF_H
