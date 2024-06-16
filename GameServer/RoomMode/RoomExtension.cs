using ExitGames.Logging;
using GameClient.Enums;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.RoomMode
{
    public class RoomExtension
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private Room _room;
        
        public RoomUserEvent onUserJoin;
        public RoomUserEvent onUserLeave;
        public delegate void RoomUserEvent(User user);

        public Room room
        {
            get
            {
                return _room;
            }
            set
            {
                _room = value;
                Init();
            }
        }
        public virtual void Init()
        {
            Log.Debug("Init RoomExtension");
        }
        public virtual void OnUserJoin(User user)
        {
            Log.Debug($"User {user.ConnectionId} ({user.name}) join room {room.settings.id}");
            Dictionary<byte, object> data = new Dictionary<byte, object>();
            data[1] = RoomCode.JoinRoom;
            data[2] = user.ConnectionId;
            data[2] = user.name;
            room.SendAllPlayer((byte)RequestCode.Room, data);
            

        }
        public virtual void OnUserLeave(User user)
        {
            Log.Debug($"User {user.ConnectionId} ({user.name}) leave room {room.settings.id}");
            Dictionary<byte, object> data = new Dictionary<byte, object>();
            data[1] = RoomCode.LeaveRoom;
            data[2] = user.ConnectionId;
            data[2] = user.name;
            room.SendAllPlayer((byte)RequestCode.Room, data);          
        }
    }
}
