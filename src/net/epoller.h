// author: tanghai

#include <boost/function.hpp>
#include <boost/unordered_map.hpp>

namespace hainan {

using namespace boost;
typedef unordered_map< int, function<void (void)> > HandlerMap;

class Epoller: private noncopyable
{
private:
	HandlerMap handlers;
public:
	void Register();
	void Add();
};
} // hainan
