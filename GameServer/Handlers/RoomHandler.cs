using GameServer.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using GameClient.Enums;
using ExitGames.Logging;
using GameServer.RoomMode;
using GameClient.Constructor;
using Newtonsoft.Json;

namespace GameServer.Handlers
{
    public class RoomHandler : BaseHandler
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user)
        {
            if ((byte)RequestCode.Room != request.OperationCode) return false;

            switch ((int)request.Parameters[1])
            {
                case(int)RoomCode.CreateRoom:
                    CreateRoom(request.Parameters,user);
                    break;
                case (int)RoomCode.GetListRoom:
                    GetListRoom(user);
                    break;
                case (int)RoomCode.JoinRoom:
                    JoinRoom(request.Parameters, user);
                    break;
                case (int)RoomCode.LeaveRoom:
                    LeaveRoom(request.Parameters, user);
                    break;
            }
            return true;
        }

        private void LeaveRoom(Dictionary<byte, object> dt, User user)
        {
            int roomId = (int)dt[2];
            if (World.Instance.rooms.ContainsKey(roomId))
            {
                Room room = World.Instance.rooms[roomId];
                room.LeaveRoom(user);
            }
        }

        private void JoinRoom(Dictionary <byte,object> dt,User user)
        {
            user.userData.category = (int)dt[4];
            int roomId = (int)dt[2];
            string password = (string)dt[3];
            if(World.Instance.rooms.ContainsKey(roomId))
            {
                Room room = World.Instance.rooms[roomId];
                if(room.settings.password == password)
                {
                    room.JoinRoom(user);
                }
                else
                {
                    user.SendNotification("Password  Error!");
                }
            }
            else
            {
                user.SendNotification("Room not exist!");
            }
        }
        private void GetListRoom(User user)
        {
            Log.Debug("GetListRoom");
            var data = new Dictionary<byte, object>();
            data[1] = RoomCode.GetListRoom;
            List<RoomInfo> roomInfos = new List<RoomInfo>();
            foreach (var room in World.Instance.rooms.Values.ToArray())
            {
                roomInfos.Add(new RoomInfo { havePassword = room.settings.password != "" ? true : false, roomName = room.settings.name, roomID = room.settings.id });
            }
            data[2] = JsonConvert.SerializeObject(roomInfos);
            user.SendEvent(new EventData((byte)RequestCode.Room, data), new SendParameters());

        }
        void CreateRoom(Dictionary<byte,object> data,User user)
        {
            Log.Debug($"tao phong{data[2]} {data[3]}");
            RoomSetting setting = new RoomSetting();
            setting.name = data[2] as string;
            setting.maxPlayer = 4;
            setting.password = data[3] as string;
            setting.removeLeaveRoom = true;
            var extension = new GamePlayExtension();
            
            Room room = Room.CreateRoom(setting,extension);
            room.ownerID = user.userData.userID;
            try
            {
                Log.Debug($"gia nhập phòng {data[3]}");
                room.JoinRoom(user);
            }
            catch(Exception e)
            {
                if(e.Message == "RoomHasFull")
                {
                    user.SendNotification("phòng đã đầy!");
                }
                else
                {
                    user.SendNotification(e.Message);
                }
            }
        }
    }
}
