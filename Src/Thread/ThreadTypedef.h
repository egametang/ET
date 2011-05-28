#ifndef THREAD_TYPEDEF_H
#define THREAD_TYPEDEF_H
#include <boost/shared_ptr.hpp>
#include <boost/thread.hpp>

namespace Egametang {

// boost
typedef boost::shared_ptr<boost::thread> ThreadPtr;

} // namespace Egametang

#endif // THREAD_TYPEDEF_H
