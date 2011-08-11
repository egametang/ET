#ifndef LOGICTREE_NOTNODE_H
#define LOGICTREE_NOTNODE_H

#include "LogicTree/LogicNodeIf.h"

namespace Egametang {

class NotNode: public LogicNodeIf
{
private:
	LogicNodeIf* node;

public:
	virtual bool Run(LogicContex* contex);
};

} // namespace Egametang


#endif // LOGICTREE_NOTNODE_H
