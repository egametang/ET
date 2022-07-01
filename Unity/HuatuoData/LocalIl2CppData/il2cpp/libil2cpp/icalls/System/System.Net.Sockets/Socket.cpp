#include "il2cpp-config.h"

#include "icalls/System/System.Net.Sockets/Socket.h"

#include "il2cpp-class-internals.h"
#include "os/Socket.h"
#include "os/Mutex.h"
#include "os/Thread.h"
#include "utils/StringUtils.h"
#include "vm/Array.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/Object.h"
#include "vm/String.h"
#include "vm/Thread.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Net
{
namespace Sockets
{
    static os::AddressFamily convert_address_family(AddressFamily family)
    {
        switch (family)
        {
            case kAddressFamilyUnknown:
            case kAddressFamilyImpLink:
            case kAddressFamilyPup:
            case kAddressFamilyChaos:
            case kAddressFamilyIso:
            case kAddressFamilyEcma:
            case kAddressFamilyDataKit:
            case kAddressFamilyCcitt:
            case kAddressFamilyDataLink:
            case kAddressFamilyLat:
            case kAddressFamilyHyperChannel:
            case kAddressFamilyNetBios:
            case kAddressFamilyVoiceView:
            case kAddressFamilyFireFox:
            case kAddressFamilyBanyan:
            case kAddressFamilyAtm:
            case kAddressFamilyCluster:
            case kAddressFamilyIeee12844:
            case kAddressFamilyNetworkDesigners:
                // Unsupported
                return os::kAddressFamilyError;

            case kAddressFamilyUnspecified:
                return os::kAddressFamilyUnspecified;

            case kAddressFamilyUnix:
                return os::kAddressFamilyUnix;

            case kAddressFamilyInterNetwork:
                return os::kAddressFamilyInterNetwork;

            case kAddressFamilyIpx:
                return os::kAddressFamilyIpx;

            case kAddressFamilySna:
                return os::kAddressFamilySna;

            case kAddressFamilyDecNet:
                return os::kAddressFamilyDecNet;

            case kAddressFamilyAppleTalk:
                return os::kAddressFamilyAppleTalk;

            case kAddressFamilyInterNetworkV6:
                return os::kAddressFamilyInterNetworkV6;

            case kAddressFamilyIrda:
                return os::kAddressFamilyIrda;

            default:
                return os::kAddressFamilyError;
        }

        return os::kAddressFamilyError;
    }

    static os::SocketType convert_socket_type(SocketType type)
    {
        switch (type)
        {
            case kSocketTypeStream:
                return os::kSocketTypeStream;

            case kSocketTypeDgram:
                return os::kSocketTypeDgram;

            case kSocketTypeRaw:
                return os::kSocketTypeRaw;

            case kSocketTypeRdm:
                return os::kSocketTypeRdm;

            case kSocketTypeSeqpacket:
                return os::kSocketTypeSeqpacket;

            default:
                return os::kSocketTypeError;
        }

        return os::kSocketTypeError;
    }

    static os::ProtocolType convert_socket_protocol(ProtocolType protocol)
    {
        switch (protocol)
        {
            case kProtocolTypeIP:
            case kProtocolTypeIPv6:
            case kProtocolTypeIcmp:
            case kProtocolTypeIgmp:
            case kProtocolTypeGgp:
            case kProtocolTypeTcp:
            case kProtocolTypePup:
            case kProtocolTypeUdp:
            case kProtocolTypeIdp:
                // In this case the enum values map exactly.
                return (os::ProtocolType)protocol;

            case kProtocolTypeND:
            case kProtocolTypeRaw:
            case kProtocolTypeIpx:
            case kProtocolTypeSpx:
            case kProtocolTypeSpxII:
            case kProtocolTypeUnknown:
                // Everything else in unsupported
                return os::kProtocolTypeUnknown;

            default:
                return os::kProtocolTypeUnknown;
        }

        return os::kProtocolTypeUnknown;
    }

    static AddressFamily convert_from_os_address_family(os::AddressFamily family)
    {
        switch (family)
        {
            case os::kAddressFamilyUnspecified:
                return kAddressFamilyUnspecified;

            case os::kAddressFamilyUnix:
                return kAddressFamilyUnix;

            case os::kAddressFamilyInterNetwork:
                return kAddressFamilyInterNetwork;

//  #ifdef AF_IPX
            case os::kAddressFamilyIpx:
                return kAddressFamilyIpx;
//  #endif

            case os::kAddressFamilySna:
                return kAddressFamilySna;

            case os::kAddressFamilyDecNet:
                return kAddressFamilyDecNet;

            case os::kAddressFamilyAppleTalk:
                return kAddressFamilyAppleTalk;

//  #ifdef AF_INET6
            case os::kAddressFamilyInterNetworkV6:
                return kAddressFamilyInterNetworkV6;
//  #endif

//  #ifdef AF_IRDA
            case os::kAddressFamilyIrda:
                return kAddressFamilyIrda;
//  #endif
            default:
                break;
        }

        return kAddressFamilyUnknown;
    }

    static os::SocketFlags convert_socket_flags(SocketFlags flags)
    {
        return (os::SocketFlags)flags;
    }

    static Il2CppSocketAddress* end_point_info_to_socket_address(const os::EndPointInfo &info)
    {
        static Il2CppClass *System_Net_SocketAddress = NULL;

        Il2CppSocketAddress *socket_address = NULL;

        if (!System_Net_SocketAddress)
        {
            System_Net_SocketAddress = vm::Class::FromName(
                vm::Assembly::GetImage(
                    vm::Assembly::Load("System.dll")),
                "System.Net", "SocketAddress");
        }

        socket_address = (Il2CppSocketAddress*)vm::Object::New(System_Net_SocketAddress);

        const AddressFamily family = convert_from_os_address_family(info.family);

        if (info.family == os::kAddressFamilyInterNetwork)
        {
            socket_address->m_Size = 8;
            IL2CPP_OBJECT_SETREF(socket_address, data, vm::Array::New(il2cpp_defaults.byte_class, 8));

            const uint16_t port = info.data.inet.port;
            const uint32_t address = info.data.inet.address;

            il2cpp_array_set(socket_address->data, uint8_t, 0, (family >> 0) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 1, (family >> 8) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 2, (port >> 8) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 3, (port >> 0) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 4, (address >> 24) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 5, (address >> 16) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 6, (address >>  8) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 7, (address >>  0) & 0xFF);
        }
        else if (info.family == os::kAddressFamilyUnix)
        {
            const int32_t path_len = (int32_t)strlen(info.data.path);

            socket_address->m_Size = 3 + path_len;
            IL2CPP_OBJECT_SETREF(socket_address, data, vm::Array::New(il2cpp_defaults.byte_class, 3 + path_len));

            il2cpp_array_set(socket_address->data, uint8_t, 0, (family >> 0) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 1, (family >> 8) & 0xFF);

            for (int32_t i = 0; i <= path_len; i++)
                il2cpp_array_set(socket_address->data, uint8_t, i + 2, info.data.path[i]);

            il2cpp_array_set(socket_address->data, uint8_t, 2 + path_len, 0);
        }
        else if (info.family == os::kAddressFamilyInterNetworkV6)
        {
            socket_address->m_Size = 28;
            IL2CPP_OBJECT_SETREF(socket_address, data, vm::Array::New(il2cpp_defaults.byte_class, 28));

            il2cpp_array_set(socket_address->data, uint8_t, 0, (family >> 0) & 0xFF);
            il2cpp_array_set(socket_address->data, uint8_t, 1, (family >> 8) & 0xFF);

            // Note that we start at the 3rd byte in both the managed array, where the first
            // two bytes are the family, set just above this. We also start at the third byte
            // in the info.data.raw array, as the first two bytes are unused and garbage data.
            for (int i = 2; i < 28; ++i)
                il2cpp_array_set(socket_address->data, uint8_t, i, info.data.raw[i]);
        }
        else
        {
            // Not supported
            return NULL;
        }

        return socket_address;
    }

    static bool check_thread_status()
    {
        static baselib::ReentrantLock _mutex;

        os::FastAutoLock lock(&_mutex);

        Il2CppThread *current_thread = vm::Thread::Current();
        const vm::ThreadState state = vm::Thread::GetState(current_thread);

        if (state & vm::kThreadStateAbortRequested)
            return false;

        if (state & vm::kThreadStateSuspendRequested)
        {
            IL2CPP_ASSERT(0 && "kThreadStateSuspendRequested not supported yet!");
            return true;
        }

        if (state & vm::kThreadStateStopRequested)
            return false;

        if (current_thread->GetInternalThread()->interruption_requested)
        {
            IL2CPP_ASSERT(0 && "thread->interruption_requested not supported yet!");
            return false;
        }

        return true;
    }

/// Acquire os::SocketHandle in IntPtr "socket" for the current scope.
#define AUTO_ACQUIRE_SOCKET \
    os::SocketHandleWrapper socketHandle (os::PointerToSocketHandle (reinterpret_cast<void*>(socket)))

#define RETURN_IF_SOCKET_IS_INVALID(...) \
    if (!socketHandle.IsValid ()) \
    { \
        *error = os::kErrorCodeInvalidHandle; \
        return __VA_ARGS__; \
    }

    intptr_t Socket::Accept(intptr_t socket, int32_t* error, bool blocking)
    {
        *error = 0;

        os::Socket *new_sock = NULL;

        AUTO_ACQUIRE_SOCKET;

        if (socketHandle.IsValid())
        {
            const os::WaitStatus status = socketHandle->Accept(&new_sock);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kErrorCodeInvalidHandle;
        }

        intptr_t ret;
        if (new_sock)
            ret = static_cast<uintptr_t>(os::CreateSocketHandle(new_sock));
        else
            ret = 0;

        return ret;
    }

    int32_t Socket::Available(intptr_t socket, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        int32_t available = 0;

        const os::WaitStatus status = socketHandle->Available(&available);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return 0;
        }

        return available;
    }

    void UnpackIPv6AddressFromBuffer(const uint8_t* buffer, int32_t length, uint16_t* port, uint8_t* address, uint32_t* scope)
    {
        if (length < 28)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return;
        }

        *port = ((buffer[2] << 8) | buffer[3]);

        for (int i = 0; i < ipv6AddressSize; i++)
            address[i] = buffer[i + 8];

        *scope = (uint32_t)((buffer[27] << 24) +
            (buffer[26] << 16) +
            (buffer[25] << 8) +
            (buffer[24]));
    }

    void Socket::Bind(intptr_t socket, Il2CppSocketAddress* socket_address, int32_t* error)
    {
        *error = 0;

        const int32_t length = ARRAY_LENGTH_AS_INT32(socket_address->data->max_length);
        const uint8_t *buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(socket_address->data);

        if (length < 2)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return;
        }

        const os::AddressFamily family = convert_address_family((AddressFamily)(buffer[0] | (buffer[1] << 8)));

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        if (family == os::kAddressFamilyInterNetwork)
        {
            if (length < 8)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return;
            }

            const uint16_t port = ((buffer[2] << 8) | buffer[3]);
            const uint32_t address = ((buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | buffer[7]);

            const os::WaitStatus status = socketHandle->Bind(address, port);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyUnix)
        {
            if (length - 2 >= END_POINT_MAX_PATH_LEN)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return;
            }

            char path[END_POINT_MAX_PATH_LEN] = {0};

            for (int32_t i = 0; i < (length - 2); ++i)
            {
                path[i] = (char)buffer[i + 2];
            }

            const os::WaitStatus status = socketHandle->Bind(path);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyInterNetworkV6)
        {
            uint16_t port;
            uint8_t address[ipv6AddressSize] = {0};
            uint32_t scope;
            UnpackIPv6AddressFromBuffer(buffer, length, &port, address, &scope);

            const os::WaitStatus status = socketHandle->Bind(address, scope, port);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kWSAeafnosupport;
            return;
        }
    }

    void Socket::Blocking(intptr_t socket, bool block, int32_t* error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        const os::WaitStatus status = socketHandle->SetBlocking(block);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();
    }

    void Socket::Close(intptr_t socket, int32_t* error)
    {
        *error = 0;

        // Socket::Close get invoked when running the finalizers; in case Socket_internal
        // didn't succeed, the socket has a NULL value and thus we don't need to do
        // anything here.
        if (os::PointerToSocketHandle(reinterpret_cast<void*>(socket)) == os::kInvalidSocketHandle)
            return;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        socket = static_cast<intptr_t>(os::kInvalidSocketHandle);

        socketHandle->Close();

        // There is an implicit acquisition happening when we create the socket which we undo
        // now that we have closed the socket.
        os::ReleaseSocketHandle(socketHandle.GetHandle());
    }

    void Socket::Connect(intptr_t socket, Il2CppSocketAddress* socket_address, int32_t* error)
    {
        *error = 0;

        const int32_t length = ARRAY_LENGTH_AS_INT32(socket_address->data->max_length);
        const uint8_t *buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(socket_address->data);

        if (length < 2)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return;
        }

        const os::AddressFamily family = convert_address_family((AddressFamily)(buffer[0] | (buffer[1] << 8)));

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        if (family == os::kAddressFamilyInterNetwork)
        {
            if (length < 8)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return;
            }

            const uint16_t port = ((buffer[2] << 8) | buffer[3]);
            const uint32_t address = ((buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | buffer[7]);

            const os::WaitStatus status = socketHandle->Connect(address, port);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyUnix)
        {
            if (length - 2 >= END_POINT_MAX_PATH_LEN)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return;
            }

            char path[END_POINT_MAX_PATH_LEN] = {0};

            for (int32_t i = 0; i < (length - 2); ++i)
            {
                path[i] = (char)buffer[i + 2];
            }

            const os::WaitStatus status = socketHandle->Connect(path);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyInterNetworkV6)
        {
            uint16_t port;
            uint8_t address[ipv6AddressSize] = {0};
            uint32_t scope;
            UnpackIPv6AddressFromBuffer(buffer, length, &port, address, &scope);

            const os::WaitStatus status = socketHandle->Connect(address, scope, port);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kWSAeafnosupport;
            return;
        }
    }

    void Socket::Disconnect(intptr_t socket, bool reuse, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        const os::WaitStatus status = socketHandle->Disconnect(reuse);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();
    }

    void Socket::GetSocketOptionArray(intptr_t socket, SocketOptionLevel level, SocketOptionName name, Il2CppArray **byte_val, int32_t *error)
    {
        *error = 0;

        // Note: for now the options map one to one.
        const os::SocketOptionName system_name = (os::SocketOptionName)(name);
        const os::SocketOptionLevel system_level = (os::SocketOptionLevel)(level);

        int32_t length = ARRAY_LENGTH_AS_INT32((*byte_val)->max_length);
        uint8_t *buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress((*byte_val));

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        const os::WaitStatus status = socketHandle->GetSocketOption(system_level, system_name, buffer, &length);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();
    }

    void Socket::GetSocketOptionObj(intptr_t socket, SocketOptionLevel level, SocketOptionName name, Il2CppObject **obj_val, int32_t *error)
    {
        *error = 0;

        // Note: for now the options map one to one.
        const os::SocketOptionName system_name = (os::SocketOptionName)(name);
        const os::SocketOptionLevel system_level = (os::SocketOptionLevel)(level);

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        int32_t first = 0;
        int32_t second = 0;

        const os::WaitStatus status = socketHandle->GetSocketOptionFull(system_level, system_name, &first, &second);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return;
        }

        switch (name)
        {
            case kSocketOptionNameLinger:
            {
                static Il2CppClass *System_Net_Sockets_LingerOption = NULL;

                if (!System_Net_Sockets_LingerOption)
                {
                    System_Net_Sockets_LingerOption = vm::Class::FromName(
                        vm::Assembly::GetImage(
                            vm::Assembly::Load("System.dll")),
                        "System.Net.Sockets", "LingerOption");
                }

                *obj_val = vm::Object::New(System_Net_Sockets_LingerOption);

                const FieldInfo *enabled_field_info = vm::Class::GetFieldFromName(System_Net_Sockets_LingerOption, "enabled");
                const FieldInfo *seconds_field_info = vm::Class::GetFieldFromName(System_Net_Sockets_LingerOption, "lingerTime");

                *((bool*)((char*)(*obj_val) + enabled_field_info->offset)) = (first ? 1 : 0);
                *((int32_t*)((char*)(*obj_val) + seconds_field_info->offset)) = second;
            }

            break;

            case kSocketOptionNameDontLinger:
            case kSocketOptionNameSendTimeout:
            case kSocketOptionNameReceiveTimeout:
            default:
                *obj_val = vm::Object::Box(il2cpp_defaults.int32_class, &first);
                break;
        }
    }

    void Socket::Listen(intptr_t socket, int32_t backlog, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        const os::WaitStatus status = socketHandle->Listen(backlog);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();
    }

    static os::PollFlags select_mode_to_poll_flags(SelectMode mode)
    {
        switch (mode)
        {
            case kSelectModeSelectRead:
                return os::kPollFlagsIn;

            case kSelectModeSelectWrite:
                return os::kPollFlagsOut;

            case kSelectModeSelectError:
                return os::kPollFlagsErr;
        }

        return os::kPollFlagsNone;
    }

    bool Socket::Poll(intptr_t socket, SelectMode mode, int32_t timeout, int32_t *error)
    {
        *error = 0;

        os::PollRequest request;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(false);

        request.fd = socketHandle.GetSocket()->GetDescriptor();
        request.events = select_mode_to_poll_flags(mode);
        request.revents = os::kPollFlagsNone;

        if (request.events == os::kPollFlagsNone)
        {
            *error = os::kWSAefault;
            return false;
        }

        std::vector<os::PollRequest> requests;

        requests.push_back(request);

        // The timeout from managed code is in microseconds. Convert it to milliseconds
        // for the poll implementation.
        timeout = (timeout >= 0) ? (timeout / 1000) : -1;

        int32_t results = 0;
        const os::WaitStatus result = os::Socket::Poll(requests, timeout, &results, error);

        if (result == kWaitStatusFailure || results == 0)
            return false;

        return (requests[0].revents != os::kPollFlagsNone);
    }

    int32_t Socket::ReceiveArray(intptr_t socket, Il2CppArray *buffers, SocketFlags flags, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        const int32_t count = ARRAY_LENGTH_AS_INT32(buffers->max_length);
        const os::SocketFlags c_flags = convert_socket_flags(flags);

        os::WSABuf *wsabufs = il2cpp_array_addr(buffers, os::WSABuf, 0);

        int32_t len = 0;

        const os::WaitStatus status = socketHandle->ReceiveArray(wsabufs, count, &len, c_flags);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return 0;
        }

        return len;
    }

    int32_t Socket::Receive(intptr_t socket, Il2CppArray *buffer, int32_t offset, int32_t count, SocketFlags flags, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        const int32_t a_len = ARRAY_LENGTH_AS_INT32(buffer->max_length);
        if (offset > a_len - count)
            return 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = il2cpp_array_addr(buffer, uint8_t, offset);

        int32_t len = 0;

        const os::WaitStatus status = socketHandle->Receive(data, count, c_flags, &len);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();

        return len;
    }

    int32_t Socket::RecvFrom(intptr_t socket, Il2CppArray *buffer, int32_t offset, int32_t count, SocketFlags flags, Il2CppSocketAddress **socket_address, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        const int32_t a_len = ARRAY_LENGTH_AS_INT32(buffer->max_length);
        if (offset > a_len - count)
            return 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = il2cpp_array_addr(buffer, uint8_t, offset);

        int32_t len = 0;

        const int32_t length = ARRAY_LENGTH_AS_INT32((*socket_address)->data->max_length);
        const uint8_t *socket_buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress((*socket_address)->data);

        if (length < 2)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return 0;
        }

        const os::AddressFamily family = convert_address_family((AddressFamily)(socket_buffer[0] | (socket_buffer[1] << 8)));

        os::EndPointInfo info;

        info.family = os::kAddressFamilyError;

        if (family == os::kAddressFamilyInterNetwork)
        {
            if (length < 8)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            const uint16_t port = ((socket_buffer[2] << 8) | socket_buffer[3]);
            const uint32_t address = ((socket_buffer[4] << 24) | (socket_buffer[5] << 16) | (socket_buffer[6] << 8) | socket_buffer[7]);

            const os::WaitStatus status = socketHandle->RecvFrom(address, port, data, count, c_flags, &len, info);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyUnix)
        {
            if (length - 2 >= END_POINT_MAX_PATH_LEN)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            char path[END_POINT_MAX_PATH_LEN] = {0};

            for (int32_t i = 0; i < (length - 2); ++i)
            {
                path[i] = (char)socket_buffer[i + 2];
            }

            const os::WaitStatus status = socketHandle->RecvFrom(path, data, count, c_flags, &len, info);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyInterNetworkV6)
        {
            uint16_t port;
            uint8_t address[ipv6AddressSize] = {0};
            uint32_t scope;
            UnpackIPv6AddressFromBuffer(socket_buffer, length, &port, address, &scope);

            const os::WaitStatus status = socketHandle->RecvFrom(address, scope, port, data, count, c_flags, &len, info);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kWSAeafnosupport;
            return 0;
        }

        *socket_address = (info.family == os::kAddressFamilyError) ? NULL : end_point_info_to_socket_address(info);

        return len;
    }

    Il2CppSocketAddress* Socket::LocalEndPoint(intptr_t socket, int32_t* error)
    {
        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(NULL);

        os::EndPointInfo info;

        memset(&info, 0x00, sizeof(os::EndPointInfo));

        const os::WaitStatus status = socketHandle->GetLocalEndPointInfo(info);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return NULL;
        }

        Il2CppSocketAddress *socket_address = end_point_info_to_socket_address(info);

        if (socket_address == NULL)
            *error = os::kWSAeafnosupport;

        return socket_address;
    }

    Il2CppSocketAddress* Socket::RemoteEndPoint(intptr_t socket, int32_t* error)
    {
        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(NULL);

        os::EndPointInfo info;

        memset(&info, 0x00, sizeof(os::EndPointInfo));

        const os::WaitStatus status = socketHandle->GetRemoteEndPointInfo(info);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return NULL;
        }

        Il2CppSocketAddress *socket_address = end_point_info_to_socket_address(info);

        if (socket_address == NULL)
            *error = os::kWSAeafnosupport;

        return socket_address;
    }

    void Socket::Select(Il2CppArray **sockets, int32_t timeout, int32_t *error)
    {
        *error = 0;

        // Layout: READ, null, WRITE, null, ERROR, null
        const uint32_t input_sockets_count = ARRAY_LENGTH_AS_INT32((*sockets)->max_length);

        std::vector<os::PollRequest> requests;
        std::vector<os::SocketHandleWrapper> socketHandles;

        requests.reserve(input_sockets_count - 3);

        int32_t mode = 0;

        for (uint32_t i = 0; i < input_sockets_count; ++i)
        {
            Il2CppObject *obj = il2cpp_array_get(*sockets, Il2CppObject*, i);

            if (obj == NULL)
            {
                if (++mode > 3)
                {
                    // Something very bad happened here (ie: the input array was wrong)
                    // so we gracefully terminate the method.
                    *error = os::kWSAefault;
                    return;
                }

                continue;
            }

            FieldInfo *safe_handle_field_info = vm::Class::GetFieldFromName(obj->klass, "m_Handle");
            const Il2CppObject* value = NULL;
            vm::Field::GetValue(obj, safe_handle_field_info, &value);

            const FieldInfo *handle_field_info = vm::Class::GetFieldFromName(value->klass, "handle");
            intptr_t& intPtr = *((intptr_t*)((char*)value + handle_field_info->offset));

            // Acquire socket.
            socketHandles.push_back(os::SocketHandleWrapper());
            os::SocketHandleWrapper& socketHandle = socketHandles.back();
            socketHandle.Acquire(os::PointerToSocketHandle(reinterpret_cast<void*>(intPtr)));

            os::PollRequest request;
            // May 'invalid socket' (-1); we want the error from Poll() in that case.
            request.fd = socketHandle.GetSocket() == NULL ? -1 : socketHandle.GetSocket()->GetDescriptor();
            request.events = (mode == 0 ? os::kPollFlagsIn : (mode == 1 ? os::kPollFlagsOut : os::kPollFlagsErr));
            request.revents = os::kPollFlagsNone;

            requests.push_back(request);
        }

        if (requests.size() == 0)
            return;

        int32_t results = 0;

        // The timeout from managed code is in microseconds. Convert it to milliseconds
        // for the poll implementation.
        timeout = (timeout >= 0) ? (timeout / 1000) : -1;

        const os::WaitStatus result = os::Socket::Poll(requests, timeout, &results, error);

        if (result == kWaitStatusFailure)
        {
            *sockets = NULL;
            return;
        }

        Il2CppArray *new_sockets = vm::Array::New(((Il2CppObject*)(*sockets))->klass->element_class, results + 3);

        if (results > 0)
        {
            mode = 0;

            uint32_t request_index = 0;

            // This odd loop is due to the layout of the sockets input array:
            // Layout: READ, null, WRITE, null, ERROR, null
            // We need to iterate each request and iterate the sockets array, skipping
            // the null entries. We try to avoid an infinite loop here as well.
            uint32_t add_index = 0;
            while (request_index < requests.size())
            {
                const uint32_t input_sockets_index = (request_index + mode);
                if (input_sockets_index > input_sockets_count - 1)
                    break; // We have exhausted the input sockets array, so exit.

                Il2CppObject *obj = il2cpp_array_get(*sockets, Il2CppObject*, input_sockets_index);

                if (obj == NULL)
                {
                    ++mode;
                    continue; // Here is a null entry, skip it without updated the next request index.
                }

                os::PollRequest &request = requests[request_index];

                if (request.revents != os::kPollFlagsNone)
                {
                    switch (mode)
                    {
                        case 0:
                            if (request.revents & (os::kPollFlagsIn | os::kPollFlagsErr))
                            {
                                il2cpp_array_setref(new_sockets, (add_index + mode), obj);
                                add_index++;
                            }
                            break;

                        case 1:
                            if (request.revents & (os::kPollFlagsOut | os::kPollFlagsErr))
                            {
                                il2cpp_array_setref(new_sockets, (add_index + mode), obj);
                                add_index++;
                            }
                            break;

                        default:
                            if (request.revents & os::kPollFlagsErr)
                            {
                                il2cpp_array_setref(new_sockets, (add_index + mode), obj);
                                add_index++;
                            }
                            break;
                    }
                }

                ++request_index;
            }
        }

        *sockets = new_sockets;
    }

    bool Socket::SendFile(intptr_t socket, Il2CppString *filename, Il2CppArray *pre_buffer, Il2CppArray *post_buffer, TransmitFileOptions flags)
    {
        if (filename == NULL)
            return false;

        os::TransmitFileBuffers t_buffers = {0};

        if (pre_buffer != NULL)
        {
            t_buffers.head = il2cpp_array_addr(pre_buffer, uint8_t, 0);
            t_buffers.head_length = ARRAY_LENGTH_AS_INT32(pre_buffer->max_length);
        }

        if (post_buffer != NULL)
        {
            t_buffers.tail = il2cpp_array_addr(post_buffer, uint8_t, 0);
            t_buffers.tail_length = ARRAY_LENGTH_AS_INT32(post_buffer->max_length);
        }

        AUTO_ACQUIRE_SOCKET;
        if (!socketHandle.IsValid())
            return false;

        const Il2CppChar* ustr = utils::StringUtils::GetChars(filename);
        const std::string str = utils::StringUtils::Utf16ToUtf8(ustr);

        // Note: for now they map 1-1
        const os::TransmitFileOptions o_flags = (os::TransmitFileOptions)flags;
        const os::WaitStatus status = socketHandle->SendFile(str.c_str(), &t_buffers, o_flags);

        if (status == kWaitStatusFailure)
        {
            // TODO: mono stores socketHandle->GetLastError into a threadlocal global variable
            // that can be retrieved later by other icalls.
            return false;
        }

        if ((flags & kTransmitFileOptionsDisconnect) == kTransmitFileOptionsDisconnect)
            socketHandle->Disconnect(true);

        return true;
    }

    int32_t Socket::SendTo(intptr_t socket, Il2CppArray *buffer, int32_t offset, int32_t count, SocketFlags flags, Il2CppSocketAddress *socket_address, int32_t *error)
    {
        *error = 0;

        const int32_t a_len = ARRAY_LENGTH_AS_INT32(buffer->max_length);
        if (offset > a_len - count)
            return 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = il2cpp_array_addr(buffer, uint8_t, offset);

        int32_t len = 0;

        const int32_t length = ARRAY_LENGTH_AS_INT32(socket_address->data->max_length);
        const uint8_t *socket_buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(socket_address->data);

        if (length < 2)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return 0;
        }

        const os::AddressFamily family = convert_address_family((AddressFamily)(socket_buffer[0] | (socket_buffer[1] << 8)));

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        if (family == os::kAddressFamilyInterNetwork)
        {
            if (length < 8)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            const uint16_t port = ((socket_buffer[2] << 8) | socket_buffer[3]);
            const uint32_t address = ((socket_buffer[4] << 24) | (socket_buffer[5] << 16) | (socket_buffer[6] << 8) | socket_buffer[7]);

            const os::WaitStatus status = socketHandle->SendTo(address, port, data, count, c_flags, &len);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyUnix)
        {
            if (length - 2 >= END_POINT_MAX_PATH_LEN)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            char path[END_POINT_MAX_PATH_LEN] = {0};

            for (int32_t i = 0; i < (length - 2); ++i)
            {
                path[i] = (char)socket_buffer[i + 2];
            }

            const os::WaitStatus status = socketHandle->SendTo(path, data, count, c_flags, &len);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyInterNetworkV6)
        {
            uint16_t port;
            uint8_t address[ipv6AddressSize] = {0};
            uint32_t scope;
            UnpackIPv6AddressFromBuffer(socket_buffer, length, &port, address, &scope);

            const os::WaitStatus status = socketHandle->SendTo(address, scope, port, data, count, c_flags, &len);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kWSAeafnosupport;
            return 0;
        }

        return len;
    }

    int32_t Socket::SendArray(intptr_t socket, Il2CppArray *buffers, SocketFlags flags, int32_t *error)
    {
        *error = 0;

        const int32_t count = ARRAY_LENGTH_AS_INT32(buffers->max_length);
        const os::SocketFlags c_flags = convert_socket_flags(flags);

        os::WSABuf *wsabufs = il2cpp_array_addr(buffers, os::WSABuf, 0);

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        int32_t sent = 0;

        const os::WaitStatus status = socketHandle->SendArray(wsabufs, count, &sent, c_flags);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return 0;
        }

        return sent;
    }

    int32_t Socket::Send(intptr_t socket, Il2CppArray *buffer, int32_t offset, int32_t count, SocketFlags flags, int32_t *error)
    {
        *error = 0;

        const int32_t a_len = ARRAY_LENGTH_AS_INT32(buffer->max_length);
        if (offset > a_len - count)
            return 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = il2cpp_array_addr(buffer, uint8_t, offset);

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        int32_t len = 0;

        const os::WaitStatus status = socketHandle->Send(data, count, c_flags, &len);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();

        return len;
    }

#if IL2CPP_SUPPORT_IPV6
    static os::IPv6Address ipaddress_to_ipv6_addr(Il2CppObject *ipaddr)
    {
        FieldInfo* numbersFieldInfo = vm::Class::GetFieldFromName(ipaddr->klass, "m_Numbers");
        IL2CPP_ASSERT(numbersFieldInfo);
        Il2CppArray* data = (Il2CppArray*)vm::Field::GetValueObject(numbersFieldInfo, ipaddr);

        os::IPv6Address ipv6;
        for (int i = 0; i < 8; i++)
        {
            uint16_t s = il2cpp_array_get(data, uint16_t, i);
            ipv6.addr[2 * i] = (s >> 8) & 0xff;
            ipv6.addr[2 * i + 1] = s & 0xff;
        }

        return ipv6;
    }

    static void GetAddressAndInterfaceFromObject(Il2CppObject* object, const char* groupField, const char* interfaceField,
        os::IPv6Address& ipv6, uint64_t& interfaceOffset)
    {
        FieldInfo* groupFieldInfo = vm::Class::GetFieldFromName(object->klass, groupField);
        IL2CPP_ASSERT(groupFieldInfo);
        Il2CppObject* address = vm::Field::GetValueObject(groupFieldInfo, object);

        if (address)
            ipv6 = ipaddress_to_ipv6_addr(address);

        FieldInfo* interfaceFieldInfo = vm::Class::GetFieldFromName(object->klass, interfaceField);
        IL2CPP_ASSERT(interfaceFieldInfo);
        vm::Field::GetValue(object, interfaceFieldInfo, &interfaceOffset);
    }

#endif // IL2CPP_SUPPORT_IPV6

    void Socket::SetSocketOption(intptr_t socket, SocketOptionLevel level, SocketOptionName name, Il2CppObject *obj_val, Il2CppArray *byte_val, int32_t int_val, int32_t *error)
    {
        *error = 0;

        // Note: for now the options map one to one.
        const os::SocketOptionName system_name = (os::SocketOptionName)(name);
        const os::SocketOptionLevel system_level = (os::SocketOptionLevel)(level);

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        os::WaitStatus status = kWaitStatusFailure;

        if (byte_val != NULL)
        {
            const int32_t length = ARRAY_LENGTH_AS_INT32(byte_val->max_length);
            const uint8_t *buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(byte_val);

            status = socketHandle->SetSocketOptionArray(system_level, system_name, buffer, length);
        }
        else if (obj_val != NULL)
        {
            switch (name)
            {
                case kSocketOptionNameLinger:
                {
                    const FieldInfo *enabled_field_info = vm::Class::GetFieldFromName(obj_val->klass, "enabled");
                    const FieldInfo *seconds_field_info = vm::Class::GetFieldFromName(obj_val->klass, "lingerTime");

                    const bool enabled = *((bool*)((char*)obj_val + enabled_field_info->offset));
                    const int32_t seconds = *((int32_t*)((char*)obj_val + seconds_field_info->offset));

                    status = socketHandle->SetSocketOptionLinger(system_level, system_name, enabled, seconds);
                }

                break;

                case kSocketOptionNameAddMembership:
                case kSocketOptionNameDropMembership:
                {
#if IL2CPP_SUPPORT_IPV6
                    if (system_level == (os::SocketOptionLevel)kSocketOptionLevelIPv6)
                    {
                        os::IPv6Address ipv6 = {{0}};
                        uint64_t interfaceOffset;
                        GetAddressAndInterfaceFromObject(obj_val, "m_Group", "m_Interface", ipv6, interfaceOffset);
                        status = socketHandle->SetSocketOptionMembership(system_level, system_name, ipv6, interfaceOffset);
                    }
                    else if (system_level == (os::SocketOptionLevel)kSocketOptionLevelIP)
#endif // IL2CPP_SUPPORT_IPV6
                    {
                        FieldInfo *group_field_info = vm::Class::GetFieldFromName(obj_val->klass, "group");
                        Il2CppObject* group_obj = vm::Field::GetValueObject(group_field_info, obj_val);
                        const FieldInfo *group_address_field_info = vm::Class::GetFieldFromName(group_obj->klass, "m_Address");
                        const uint32_t group_address = *((uint32_t*)(uint64_t*)((char*)group_obj + group_address_field_info->offset));

                        uint32_t local_address = 0;
                        FieldInfo *local_field_info = vm::Class::GetFieldFromName(obj_val->klass, "localAddress");
                        if (local_field_info != NULL)
                        {
                            Il2CppObject* local_obj = vm::Field::GetValueObject(local_field_info, obj_val);
                            if (local_obj != NULL)
                            {
                                const FieldInfo *local_address_field_info = vm::Class::GetFieldFromName(local_obj->klass, "m_Address");
                                local_address = *((uint32_t*)(uint64_t*)((char*)local_obj + local_address_field_info->offset));
                            }
                        }

                        status = socketHandle->SetSocketOptionMembership(system_level, system_name, group_address, local_address);
                    }
                }

                break;

                default:
                    *error = os::kWSAeinval;
                    return; // early out
            }
        }
        else
            status = socketHandle->SetSocketOption(system_level, system_name, int_val);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();
    }

    void Socket::Shutdown(intptr_t socket, SocketShutdown how, int32_t* error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID();

        const os::WaitStatus status = socketHandle->Shutdown(how);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();
    }

    intptr_t Socket::Socket_internal(Il2CppObject *self, AddressFamily family, SocketType type, ProtocolType protocol, int32_t* error)
    {
        intptr_t socket = 0;

        *error = 0;

        os::AddressFamily n_family = convert_address_family(family);
        os::SocketType n_type = convert_socket_type(type);
        os::ProtocolType n_protocol = convert_socket_protocol(protocol);

        if (n_family == os::kAddressFamilyError)
        {
            *error = os::kWSAeafnosupport;
            return socket;
        }

        if (n_type == os::kSocketTypeError)
        {
            *error = os::kWSAesocktnosupport;
            return socket;
        }

        if (n_protocol == os::kProtocolTypeUnknown)
        {
            *error = os::kWSAeprotonosupport;
            return socket;
        }

        os::Socket *sock = new os::Socket(check_thread_status);

        const os::WaitStatus status = sock->Create(n_family, n_type, n_protocol);

        if (status == kWaitStatusFailure)
        {
            *error = sock->GetLastError();

            // Okay to delete socket directly. We haven't created a handle yet.
            delete sock;

            return socket;
        }

        os::SocketHandle socketHandle = os::CreateSocketHandle(sock);
        socket = static_cast<intptr_t>(socketHandle);

        return socket;
    }

    int32_t Socket::WSAIoctl(intptr_t socket, int32_t code, Il2CppArray *input, Il2CppArray *output, int32_t *error)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(-1);

        if (code == 0x5421 /* FIONBIO */)
        {
            // Invalid command. Must use Socket.Blocking
            return -1;
        }

        const int32_t in_length = (input ? ARRAY_LENGTH_AS_INT32(input->max_length) : 0);
        const uint8_t *in_buffer = (input ? (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(input) : NULL);

        const int32_t out_length = (output ? ARRAY_LENGTH_AS_INT32(output->max_length) : 0);
        uint8_t *out_buffer = (output ? (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(output) : NULL);

        int32_t output_bytes = 0;

        const os::WaitStatus status = socketHandle->Ioctl(code, in_buffer, in_length, out_buffer, out_length, &output_bytes);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return -1;
        }

        return output_bytes;
    }

    bool Socket::SendFile_internal(intptr_t sock, Il2CppString* filename, Il2CppArray* pre_buffer, Il2CppArray* post_buffer, int32_t flags, int32_t* error, bool blocking)
    {
        return SendFile(sock, filename, pre_buffer, post_buffer, static_cast<TransmitFileOptions>(flags));
    }

    bool Socket::SupportsPortReuse(ProtocolType proto)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Socket::SupportsPortReuse);
        IL2CPP_UNREACHABLE;
        return false;
    }

    int32_t Socket::IOControl_internal(intptr_t sock, int32_t ioctl_code, Il2CppArray* input, Il2CppArray* output, int32_t* error)
    {
        return WSAIoctl(sock, ioctl_code, input, output, error);
    }

    int32_t Socket::ReceiveFrom_internal(intptr_t socket, uint8_t* buffer, int32_t count, SocketFlags flags, Il2CppSocketAddress** socket_address, int32_t* error, bool blocking)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = buffer;

        int32_t len = 0;

        const int32_t length = ARRAY_LENGTH_AS_INT32((*socket_address)->data->max_length);
        const uint8_t *socket_buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress((*socket_address)->data);

        if (length < 2)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return 0;
        }

        const os::AddressFamily family = convert_address_family((AddressFamily)(socket_buffer[0] | (socket_buffer[1] << 8)));

        os::EndPointInfo info;

        info.family = os::kAddressFamilyError;

        if (family == os::kAddressFamilyInterNetwork)
        {
            if (length < 8)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            const uint16_t port = ((socket_buffer[2] << 8) | socket_buffer[3]);
            const uint32_t address = ((socket_buffer[4] << 24) | (socket_buffer[5] << 16) | (socket_buffer[6] << 8) | socket_buffer[7]);

            const os::WaitStatus status = socketHandle->RecvFrom(address, port, data, count, c_flags, &len, info);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyUnix)
        {
            if (length - 2 >= END_POINT_MAX_PATH_LEN)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            char path[END_POINT_MAX_PATH_LEN] = { 0 };

            for (int32_t i = 0; i < (length - 2); ++i)
            {
                path[i] = (char)socket_buffer[i + 2];
            }

            const os::WaitStatus status = socketHandle->RecvFrom(path, data, count, c_flags, &len, info);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyInterNetworkV6)
        {
            uint16_t port;
            uint8_t address[ipv6AddressSize] = { 0 };
            uint32_t scope;
            UnpackIPv6AddressFromBuffer(socket_buffer, length, &port, address, &scope);

            const os::WaitStatus status = socketHandle->RecvFrom(address, scope, port, data, count, c_flags, &len, info);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kWSAeafnosupport;
            return 0;
        }

        *socket_address = (info.family == os::kAddressFamilyError) ? NULL : end_point_info_to_socket_address(info);

        return len;
    }

    int32_t Socket::SendTo_internal(intptr_t socket, uint8_t* buffer, int32_t count, SocketFlags flags, Il2CppSocketAddress* socket_address, int32_t* error, bool blocking)
    {
        *error = 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = buffer;

        int32_t len = 0;

        const int32_t length = ARRAY_LENGTH_AS_INT32(socket_address->data->max_length);
        const uint8_t *socket_buffer = (uint8_t*)il2cpp::vm::Array::GetFirstElementAddress(socket_address->data);

        if (length < 2)
        {
            vm::Exception::Raise(vm::Exception::GetSystemException());
            return 0;
        }

        const os::AddressFamily family = convert_address_family((AddressFamily)(socket_buffer[0] | (socket_buffer[1] << 8)));

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        if (family == os::kAddressFamilyInterNetwork)
        {
            if (length < 8)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            const uint16_t port = ((socket_buffer[2] << 8) | socket_buffer[3]);
            const uint32_t address = ((socket_buffer[4] << 24) | (socket_buffer[5] << 16) | (socket_buffer[6] << 8) | socket_buffer[7]);

            const os::WaitStatus status = socketHandle->SendTo(address, port, data, count, c_flags, &len);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyUnix)
        {
            if (length - 2 >= END_POINT_MAX_PATH_LEN)
            {
                vm::Exception::Raise(vm::Exception::GetSystemException());
                return 0;
            }

            char path[END_POINT_MAX_PATH_LEN] = { 0 };

            for (int32_t i = 0; i < (length - 2); ++i)
            {
                path[i] = (char)socket_buffer[i + 2];
            }

            const os::WaitStatus status = socketHandle->SendTo(path, data, count, c_flags, &len);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else if (family == os::kAddressFamilyInterNetworkV6)
        {
            uint16_t port;
            uint8_t address[ipv6AddressSize] = { 0 };
            uint32_t scope;
            UnpackIPv6AddressFromBuffer(socket_buffer, length, &port, address, &scope);

            const os::WaitStatus status = socketHandle->SendTo(address, scope, port, data, count, c_flags, &len);

            if (status == kWaitStatusFailure)
                *error = socketHandle->GetLastError();
        }
        else
        {
            *error = os::kWSAeafnosupport;
            return 0;
        }

        return len;
    }

    Il2CppSocketAddress* Socket::LocalEndPoint_internal(intptr_t socket, int32_t family, int32_t* error)
    {
        // We should be able to ignore the family, as the socket should already have that information.
        return LocalEndPoint(socket, error);
    }

    Il2CppSocketAddress* Socket::RemoteEndPoint_internal(intptr_t socket, int32_t family, int32_t* error)
    {
        // We should be able to ignore the family, as the socket should already have that information.
        return RemoteEndPoint(socket, error);
    }

    static void STDCALL
    abort_apc(void* param)
    {
    }

    void Socket::cancel_blocking_socket_operation(Il2CppObject* thread)
    {
        Il2CppThread* t = reinterpret_cast<Il2CppThread*>(thread);
        t->internal_thread->handle->QueueUserAPC(abort_apc, NULL);
        // IL2CPP_NOT_IMPLEMENTED_ICALL(Socket::cancel_blocking_socket_operation);
        //IL2CPP_UNREACHABLE;
    }

    void Socket::Connect_internal(intptr_t sock, Il2CppSocketAddress* sa, int32_t* error, bool blocking)
    {
        Connect(sock, sa, error);
    }

    bool Socket::Duplicate_internal(intptr_t handle, int32_t targetProcessId, intptr_t *duplicate_handle, int32_t *werror)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Socket::Duplicate_internal);
        return false;
    }

    int32_t Socket::ReceiveArray40(intptr_t socket, void *buffers, int32_t count, SocketFlags flags, int32_t *error, bool blocking)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        const os::SocketFlags c_flags = convert_socket_flags(flags);

        int32_t len = 0;

        const os::WaitStatus status = socketHandle->ReceiveArray((os::WSABuf*)buffers, count, &len, c_flags);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return 0;
        }

        return len;
    }

    int32_t Socket::Receive40(intptr_t socket, uint8_t *buffer, int32_t count, SocketFlags flags, int32_t *error, bool blocking)
    {
        *error = 0;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        const os::SocketFlags c_flags = convert_socket_flags(flags);

        int32_t len = 0;

        const os::WaitStatus status = socketHandle->Receive(buffer, count, c_flags, &len);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();

        return len;
    }

    int32_t Socket::SendArray40(intptr_t socket, void *wsabufs, int32_t count, SocketFlags flags, int32_t *error, bool blocking)
    {
        *error = 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        int32_t sent = 0;

        const os::WaitStatus status = socketHandle->SendArray((os::WSABuf*)wsabufs, count, &sent, c_flags);

        if (status == kWaitStatusFailure)
        {
            *error = socketHandle->GetLastError();
            return 0;
        }

        return sent;
    }

    int32_t Socket::Send40(intptr_t socket, uint8_t *buffer, int32_t count, SocketFlags flags, int32_t *error, bool blocking)
    {
        *error = 0;

        const os::SocketFlags c_flags = convert_socket_flags(flags);
        const uint8_t *data = buffer;

        AUTO_ACQUIRE_SOCKET;
        RETURN_IF_SOCKET_IS_INVALID(0);

        int32_t len = 0;

        const os::WaitStatus status = socketHandle->Send(data, count, c_flags, &len);

        if (status == kWaitStatusFailure)
            *error = socketHandle->GetLastError();

        return len;
    }

    bool Socket::IsProtocolSupported_internal(int32_t networkInterface)
    {
        // The networkInterface argument is from the
        // System.Net.NetworkInformation.NetworkInterfaceComponent enum
        // 0 => IPv4
        // 1 => IPv6
#if IL2CPP_SUPPORT_IPV6_SUPPORT_QUERY
        return networkInterface == 1 ? os::Socket::IsIPv6Supported() : true;
#elif IL2CPP_SUPPORT_IPV6
        // This platform supports both IPv6 and IPv4.
        return true;
#else
        // This platform only supports IPv4.
        return networkInterface == 0;
#endif
    }
} /* namespace Sockets */
} /* namespace Net */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
