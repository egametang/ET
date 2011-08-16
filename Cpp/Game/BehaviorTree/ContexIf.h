#ifndef BEHAVIORTREE_CONTEXIF_H
#define BEHAVIORTREE_CONTEXIF_H

#include <stddef.h>
#include "BehaviorTree/SpellBuff.h"

namespace Egametang {

class ContexIf
{
public:
	virtual ~ContexIf()
	{
	}

	virtual Spell* GetSpell()
	{
		return NULL;
	}

	virtual Buff* GetBuff()
	{
		return NULL;
	}
};

} // namespace Egametang

#endif // BEHAVIORTREE_CONTEXIF_H

