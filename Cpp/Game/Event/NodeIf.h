#ifndef EVENT_NODEIF_H
#define EVENT_NODEIF_H

namespace Egametang {

class ContexIf;
class EventNode;

class NodeIf
{
public:
	virtual bool Run(ContexIf* contex) = 0;

	virtual void AddChildNode(NodeIf *node)
	{
	}
};

class NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const EventNode& conf) = 0;
};

} // namespace Egametang

#endif // EVENT_NODEIF_H
