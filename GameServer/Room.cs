using ExitGames.Logging;
using GameClient.Enums;
using GameServer;
using GameServer.RoomMode;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    public class Room
    {
        //danh sach id cua cac phong
        public static List<int> listID = new List<int>();
        //danh sach user trong phong
        public List<User> users;
       
        public RoomSetting settings;
        public CancellationTokenSource cts;
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public RoomExtension extension;
        public bool isLobby;
        public delegate void RoomTaskInvoke();
        public RoomTaskInvoke roomTask;
        public int ownerID = 0;
        public List<int> kickList = new List<int>();
        public delegate void KickEvent(int userID);
        public KickEvent onKickUser;

        public Room(RoomSetting settings, RoomExtension extension)
        {
            this.settings = settings;
            this.extension = extension;
            users = new List<User>();
            cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { RoomTask(state); }), cts.Token);
            Log.Debug("Create room id: " + settings.id + " name: " + settings.name);
        }

        public void SendAllPlayer(byte code, Dictionary<byte, object> data, bool useUDP = false)
        {
            for (int i = 0; i < users.Count; i++)
            {
                users[i].SendEvent(new EventData(code, data), new SendParameters { Unreliable = useUDP });
            }
        }

        public void SendAllPlayerOther(byte code, Dictionary<byte, object> data, User otherUser, bool useUDP = false)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i] != otherUser)
                    users[i].SendEvent(new EventData(code, data), new SendParameters { Unreliable = useUDP });
            }
        }
        //demo send one player
        public void SendOnePlayer(byte code, Dictionary<byte, object> data, User otherUser, bool useUDP = false)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i] == otherUser)
                    users[i].SendEvent(new EventData(code, data), new SendParameters { Unreliable = useUDP });
            }
        }
        //xoa phong hien tai
        public void Destroy()
        {
            cts.Cancel();
            cts.Dispose();
            int roomID = settings.id;
            listID.Remove(roomID);
            World.Instance.rooms.Remove(roomID);
            foreach (User user in users)
            {
                user.lastJoinRoom = null;
            }
            Log.Debug("Remove room id: " + settings.id + " name: " + settings.name);
        }

        void RoomTask(object obj)
        {
            CancellationToken token = (CancellationToken)obj;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (roomTask != null) roomTask.Invoke();
                Thread.Sleep(1000);
            }
        }

        public void JoinRoom(List<User> users)
        {
            List<User> listUser = new List<User>(users);
            for (int i = 0; i < listUser.Count; i++)
            {
                JoinRoom(listUser[i]);
            }
        }

        public void JoinRoom(User user)
        {
            if(kickList.Contains(user.userData.userID))
            {
                user.SendNotification("ban khong the gia nhap phong ma ban da bi duoi!");
                return;
            }
            if (users.Count >= settings.maxPlayer)
            {
                throw new Exception("RoomHasFull");
            }
            if (user.lastJoinRoom != null) user.lastJoinRoom.LeaveRoom(user);
            user.lastJoinRoom = this;
            users.Add(user);
            //settings.removeLeaveRoom = true;
            extension.OnUserJoin(user);
            
        }
        public void LeaveRoom(User user)
        {
            users.Remove(user);
            /*if(user.userData.userID == ownerID && settings.removeLeaveRoom)
            {
                if (users.Count > 0)
                {
                    ownerID = users[0].userData.userID;
                    Log.Debug("chuyen chu phong cho nguoi choi " + users[0].userData.userID);
                }
                else
                {
                    //ko can xet cung dc
                    ownerID = 0;
                    Log.Debug("khong con ai trong phong");
                }
            }*/
            user.lastJoinRoom = null;
            extension.OnUserLeave(user);
            
            /*if (settings.removeLeaveRoom)
            {
                if (users.Count == 0) Destroy();
            }*/
        }

        public static Room CreateRoom(RoomSetting settings, RoomExtension extension)
        {
            lock (listID)
            {
                int id = 0;
                int i = 1;
                while (id == 0)
                {
                    if (!listID.Contains(i))
                    {
                        id = i;
                        listID.Add(i);
                    }
                    i++;
                }
                settings.id = id;
            }
            Room room = new Room(settings, extension);
            room.extension.room = room;
            World.Instance.rooms.Add(settings.id, room);
            return room;
        }

        public void Kick(string username,User ownerUser)
        {
            if (ownerID != ownerUser.userData.userID) return;
            var user = users.Find(x => x.userData.username == username);
            if(user!= null )
            {
                if (user == ownerUser) return;
                onKickUser?.Invoke(user.userData.userID);
                LeaveRoom(user);
                kickList.Add(user.userData.userID);
                var data = new Dictionary<byte, object>();
                data[1] = RoomCode.Kick;
                user.SendEvent(new EventData((byte)RequestCode.Room, data), new SendParameters());
            }
        }
    }
}
