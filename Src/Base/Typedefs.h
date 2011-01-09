#ifndef BASE_TYPEDEFS_H
#define BASE_TYPEDEFS_H
#include <boost/smart_ptr.hpp>

namespace Hainan {

typedef boost::int8_t   int8;
typedef boost::int16_t  int16;
typedef boost::int32_t  int32;
typedef boost::int64_t  int64;
typedef boost::uint8_t  uint8;
typedef boost::uint16_t uint16;
typedef boost::uint32_t uint32;
typedef boost::uint64_t uint64;

// smart_ptr typedef

typedef boost::shared_ptr<int> IntPtr;
typedef boost::shared_ptr<std::string> StringPtr;

// boost
namespace boost {
class thread;
}
typedef boost::shared_ptr<boost::thread> ThreadPtr;

// google
namespace google {
namespace protobuf {
class Service;
class Message;
}
}
typedef boost::shared_ptr<google::protobuf::Service> ProtobufServicePtr;
typedef boost::shared_ptr<google::protobuf::Message> ProtobufMessagePtr;

// Hainan
class RpcSession;
class RpcRequest;
typedef boost::shared_ptr<RpcSession> RpcSessionPtr;
typedef boost::shared_ptr<RpcRequest> RpcRequestPtr;

} // namespace Hainan

#endif // BASE_TYPEDEFS_H
