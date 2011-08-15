#ifndef EVENT_CONTEXIF_H
#define EVENT_CONTEXIF_H

#include <stddef.h>
#include "Event/SpellBuff.h"

namespace Egametang {

class ContexIf
{
public:
	virtual ~ContexIf();

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

#endif // EVENT_CONTEXIF_H

