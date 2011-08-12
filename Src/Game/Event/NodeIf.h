#ifndef EVENT_NODEIF_H
#define EVENT_NODEIF_H

namespace Egametang {

class ContexIf
{
};

enum ChildNodeType
{
	LEFT  = 0,
	RIGHT = 1,
};

class NodeIf
{
public:
	virtual bool Check(ContexIf* contex) = 0;
	virtual void AddChildNode(NodeIf *node, int type);
};

class NodeFactoryIf
{
public:
	virtual NodeIf* GetInstance(const LogicNode& conf) = 0;
};

} // namespace Egametang

#endif // EVENT_NODEIF_H
