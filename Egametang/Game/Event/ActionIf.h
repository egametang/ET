#ifndef EVENT_ACTIONIF_H
#define EVENT_ACTIONIF_H

namespace Egametang {

class ActionIf
{
public:
	virtual void Excute(ContexIf* contex) = 0;
};

} // namespace Egametang

#endif // EVENT_ACTIONIF_H
