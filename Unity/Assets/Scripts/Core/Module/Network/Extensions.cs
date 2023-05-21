using System.Net;
using System.Net.Sockets;

namespace ET
{
    public static class Extensions
    {
        // always pass the same IPEndPointNonAlloc instead of allocating a new
        // one each time.
        //
        // use IPEndPointNonAlloc.temp to get the latest SocketAdddress written
        // by ReceiveFrom_Internal!
        //
        // IMPORTANT: .temp will be overwritten in next call!
        //            hash or manually copy it if you need to store it, e.g.
        //            when adding a new connection.
        public static int ReceiveFrom_NonAlloc(
            this Socket socket,
            byte[] buffer,
            int offset,
            int size,
            SocketFlags socketFlags,
            EndPoint remoteEndPoint)
        {
            // call ReceiveFrom with IPEndPointNonAlloc.
            // need to wrap this in ReceiveFrom_NonAlloc because it's not
            // obvious that IPEndPointNonAlloc.Create does NOT create a new
            // IPEndPoint. it saves the result in IPEndPointNonAlloc.temp!
#if UNITY
            EndPoint casted = remoteEndPoint;
            return  socket.ReceiveFrom(buffer, offset, size, socketFlags, ref casted);
#else
            return  socket.ReceiveFrom(buffer, offset, size, socketFlags, ref remoteEndPoint);
#endif
        }

        // same as above, different parameters
        public static int ReceiveFrom_NonAlloc(this Socket socket, byte[] buffer, ref EndPoint remoteEndPoint)
        {
#if UNITY
            EndPoint casted = remoteEndPoint;
            return socket.ReceiveFrom(buffer, ref casted);
#else
            return socket.ReceiveFrom(buffer, ref remoteEndPoint);
#endif

        }
        
        // SendTo allocates too:
        // https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L2240
        // -> the allocation is in EndPoint.Serialize()
        // NOTE: technically this function isn't necessary.
        //       could just pass IPEndPointNonAlloc.
        //       still good for strong typing.
        //public static int SendTo_NonAlloc(
        //    this Socket socket,
        //    byte[] buffer,
        //    int offset,
        //    int size,
        //    SocketFlags socketFlags,
        //    IPEndPointNonAlloc remoteEndPoint)
        //{
        //    EndPoint casted = remoteEndPoint;
        //    return socket.SendTo(buffer, offset, size, socketFlags, casted);
        //}
    }
}