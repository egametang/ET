using System.IO;

namespace ET
{
    public interface ISessionStreamDispatcher
    {
        void Dispatch(Session session, MemoryStream stream);
    }
}