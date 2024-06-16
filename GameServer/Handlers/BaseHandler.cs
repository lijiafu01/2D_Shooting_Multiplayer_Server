using GameServer;
using Photon.SocketServer;

namespace GameServer.Handler
{
    public interface BaseHandler
    {
        bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user);
    }
}
