using ExitGames.Logging;
using GameServer.Handler;
using GameServer.Handlers;
using GameServer.RoomMode;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameClient.Enums;
using GameClient.Constructor;
using System.Threading;
using MySqlConnector;
using System.Data.Common;
using GameServer.Helper;
using GameServer.Constructor;
using DevOne.Security.Cryptography.BCrypt;

namespace GameServer
{
    public class World
    {
        public static World Instance;
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public delegate bool HanlderRequest(OperationRequest request, SendParameters data, User user);

        public Dictionary<int, User> users = new Dictionary<int, User>();
        public Dictionary<int, Room> rooms = new Dictionary<int, Room>();
        public List<BaseHandler> handlers = new List<BaseHandler>();
        public int currentID = 0;
        public Dictionary<int, UserData> userDataDict = new Dictionary<int, UserData>();
        public List <Bullet> bulletList = new List<Bullet>();
        public int bulletID = 0;
        public CancellationTokenSource cts;
        Queue<string> queryUpdateQueue = new Queue<string>();
        public Pack pack = new Pack();
        public void Init()
        {
          
            Instance = this;
            InventoryHelper.AUTO_INCREMENT.id = GetAUTO_INCREAMENT("inventory");
            cts = new CancellationTokenSource();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { SendWarning(state); }), cts.Token);
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { ExecuteAllQuery(state); }), cts.Token);
            handlers.Add(new LoginHandler());
            handlers.Add(new RegisterHandler());
            handlers.Add(new RoomHandler());
            handlers.Add(new GamePlayHandler());
            handlers.Add(new ChatHandler());
            handlers.Add(new FriendHandler());
            CreateRoom();
           
        }
        void CreateRoom()
        {
            string name = "";
            string password = "";
            for(int i=1;i<10;i++)
            {
                name = i.ToString();
                RoomSetting setting = new RoomSetting();
                setting.name = name;
                setting.maxPlayer = 5;
                setting.password = password;
                setting.removeLeaveRoom = false;
                var extension = new GamePlayExtension();
                Room room = Room.CreateRoom(setting, extension);
            }
            
        }
        void ExecuteAllQuery(object obj)
        {
            CancellationToken token = (CancellationToken)obj;  
            while(true)
            {
                if(token.IsCancellationRequested)
                {
                    return;
                }
                ExecuteAllQuery();
                Thread.Sleep(20000);
            }
        }

        void ExecuteAllQuery()
        {
            if(queryUpdateQueue.Count == 0) return;
            string query = "";
            Log.Debug("Query" + queryUpdateQueue.Count);
            MySqlConnection conn = DBUtils.GetMySqlConnection();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            while(queryUpdateQueue.Count > 0)
            {
                string str = queryUpdateQueue.Dequeue();
                query += str;
                try
                {
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Log.Error(query);
                    Log.Error(e.StackTrace);
                }
            }
            conn.Close();
            conn.Dispose();

        }
        public void AddQuery(string query)
        {
            lock (queryUpdateQueue)
            {
                queryUpdateQueue.Enqueue(query);
            }
        }
        public static int GetAUTO_INCREAMENT(string table)
        {
            MySqlConnection conn = DBUtils.GetMySqlConnection();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();

            //lien hop command voi connection
            int rs = 0;
            cmd.Connection = conn;
            cmd.CommandText = $"SELECT AUTO_INCREMENT FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'gamedemo' AND TABLE_NAME = @table";
            cmd.Parameters.Add("@table", MySqlDbType.String).Value = table;
            
            using (DbDataReader reader = cmd.ExecuteReader())
            {

                if (reader.Read())
                {
                    long autoIncrementValue = reader.GetInt64(reader.GetOrdinal("AUTO_INCREMENT"));
                    rs = Convert.ToInt32(autoIncrementValue);
                }

            }
            conn.Close();
            conn.Dispose();
            return rs;  
        }
       
        void SendWarning(object obj)
        {

            CancellationToken token = (CancellationToken)obj;
            int count  = 0;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if(count == 30)
                {
                    foreach (User user in Instance.users.Values)
                    {
                        user.Kick("bi kick boi server trong vong 30 giay");
                    }
                }     
                count++;
                if(count <= 30)
                    Log.Debug($"chuan bi kick toan bo nguoi choi tai may chu trong vong {30 - count}");
                //Notification("canh bao den may chu moi 10 giay");
                Thread.Sleep(1000);
            }
        }
        public static void Notification(string notice)
        {
            SendParameters sendParameters = new SendParameters();
            foreach(User user in World.Instance.users.Values)
            {
                user.SendEvent(new EventData((byte)RequestCode.Notification,new Dictionary<byte, object>() { { 1,notice} }),new SendParameters());
            }
        }
        public void BroadCastList(byte eventCode, Dictionary<byte, object> data, bool useUdp= false)
        {
            SendParameters sendParameters = new SendParameters();
            sendParameters.Unreliable = useUdp;
            foreach (User user in users.Values)
            {
                user.SendEvent(new EventData(eventCode, data), sendParameters);
            }
        }
        public void BroadCast(byte eventCode,Dictionary<byte,object> data,bool useUdp=false)
        {
            SendParameters sendParameters = new SendParameters();
            sendParameters.Unreliable = useUdp;
            foreach(User user in users.Values)
            {
                user.SendEvent(new EventData(eventCode,data),sendParameters);
            }
        }
        public void BroadCastOther(byte eventCode, Dictionary<byte, object> data,User otherUser, bool useUdp = false)
        {
            SendParameters sendParameters = new SendParameters();
            sendParameters.Unreliable = useUdp;
            foreach (User user in users.Values)
            {
                if(user != otherUser)
                {
                    user.SendEvent(new EventData(eventCode, data), sendParameters);
                }
                
            }
        }

    }
}
