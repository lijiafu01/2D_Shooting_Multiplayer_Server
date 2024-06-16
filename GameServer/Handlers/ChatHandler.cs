using GameServer.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameClient.Enums;
using Photon.SocketServer;
using ExitGames.Logging;
using Newtonsoft.Json;
using GameClient.Constructor;

namespace GameServer.Handlers
{
    public class ChatHandler:BaseHandler
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user)
        {
            if(request.OperationCode != (byte)RequestCode.Chat) return false;
            switch (request.Parameters[1])
            {
                case (int) ChatCode.Room:
                    ChatInRoom(request.Parameters, user);
                    break;
                case (int)ChatCode.Global:
                    ChatInGlobal(request.Parameters, user);
                    break;
                case (int)ChatCode.Private:
                    ChatInPrivate(request.Parameters, user);
                    break;
            }
            return true;
        }
        //tu lam chat private
        private void ChatInPrivate(Dictionary<byte, object> dt, User user)
        {
            string username = (string)dt[5];
            User[] users = World.Instance.users.Values.ToArray();
            foreach(User u in users)
            {
                if(u.userData.username == username)
                {
                    dt[3] = user.userData.userID;
                    dt[4] = user.userData.username;
                    u.SendEvent(new EventData((byte)RequestCode.Chat, dt),new SendParameters());
                }
                return;
            }
        }
        private void ChatInGlobal(Dictionary<byte, object> dt, User user)
        {
            dt[3] = user.userData.userID;
            dt[4] = user.userData.username;
            World.Instance.BroadCastOther((byte)RequestCode.Chat, dt, user);
        }
        private void ChatInRoom(Dictionary<byte, object> dt, User user,ChatCode Private = ChatCode.Private)
        {
            
            dt[3] = user.userData.userID;
            dt[4] = user.userData.username;
            user.lastJoinRoom.SendAllPlayerOther((byte)RequestCode.Chat, dt, user);

        }
    }
}
