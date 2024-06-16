using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotonHostRuntimeInterfaces;
using ExitGames.Logging;
using Photon.SocketServer.ServerToServer;
using System.Threading;
using GameClient.Enums;
using GameServer;
using GameClient.Constructor;

namespace GameServer
{
    public class User : ClientPeer
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public Room lastJoinRoom;
        public string name;
        public UserData userData;
        public User(InitRequest initRequest) : base(initRequest)
        {
            Log.Debug("User Connect to Server");
            //add login success
            //World.Instance.users.Add(initRequest.ConnectionId,this);
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            if(userData != null)
            {
                if(lastJoinRoom != null)
                {
                    lastJoinRoom.LeaveRoom(this);
                }
                World.Instance.users.Remove(userData.userID);
                World.Notification($"nguoi choi {userData.username}thoat khoi may chu");
            }
        }

        protected override void OnOperationRequest(OperationRequest request, SendParameters data)
        {
            bool haveRequest = false;
            for (int i = 0; i < World.Instance.handlers.Count; i++)
            {
                haveRequest = World.Instance.handlers[i].OnHandlerRequest(request, data, this);
                if (haveRequest) return;
            }
            if (!haveRequest) Log.Error("Dont have request type " + (RequestCode)request.OperationCode);
        }

        public void Kick(string reason)
        {
            SendEvent(new EventData((byte)RequestCode.Kick, new Dictionary<byte, object> { { 1, reason } }), new SendParameters());
            Log.Error(reason);
            Thread.Sleep(500);
            this.Disconnect();
        }
        public void SendNotification(string notification)
        {
            var data = new Dictionary<byte, object>();
            data[1] = notification;
            SendEvent(new EventData((byte)RequestCode.Notification, data), new SendParameters());
        }
        public void SendError(Exception e)
        {
            int code = e.GetHashCode();
            Log.Error($"{e.Message} {code} {e.StackTrace}");
            var data = new Dictionary<byte, object>();
            data[1] = e.Message;
            data[2] = code;
            SendEvent(new EventData((byte)RequestCode.Error, data), new SendParameters());
        }
        public void Ban()
        {

        }

        public void UnBan()
        {

        }
    }
}
