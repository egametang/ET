// author: tanghai

#include <boost/function.hpp>
#include <boost/unordered_map.hpp>

namespace hainan {
typedef boost::unordered_map< int, boost::function<void (void)> > HandlerMap;
class Epoller: private boost::noncopyable
{
private:
	HandlerMap handlers;
public:
	void Add()
};
} // hainan
