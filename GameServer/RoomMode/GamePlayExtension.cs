using ExitGames.Logging;
using GameClient.Constructor;
using GameClient.Enums;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace GameServer.RoomMode
{
    public class GamePlayExtension:RoomExtension
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        //playerLish chua cac thong tin cau cac obj da vao phong
        public List<Player> playerList = new List<Player>();
        public List<Enemy> enemyList = new List<Enemy>();
        public List<Enemy> enemyListAI = new List<Enemy>();
        private Timer timer;
        public bool isFirst;

        public override void Init()
        {
            base.Init();
            Log.Debug("init GameplayExtension");
            room.onKickUser += OnKickUser;
            //SpawEnemy();
            
        }
        /*private bool isRunning = true;
        public void StartUpdating()
        {
            Task.Run(() =>
            {
                while (isRunning)
                {
                    UpDateEnemyPos();
                    Task.Delay(TimeSpan.FromMilliseconds(50)); // ví dụ nếu bạn muốn chờ 500 mili giây trước khi cập nhật lại
                }
            });
        }

        public void StopUpdating()
        {
            isRunning = false;
        }
        public void UpDateEnemyPos()
        {
            var data = new Dictionary<byte, object>();
            data[1] = GamePlayCode.EnemyPosUpDate;
            data[2] = JsonConvert.SerializeObject(enemyList);
            room.SendAllPlayer((byte)RequestCode.GamePlay, data);
        }*/
        public void StartEnemyAI()
        {
            isFirst = true;
            timer = new Timer();
            timer.Elapsed += OnTimerElapsed;
            timer.Interval = GetRandomInterval(); // Đặt khoảng thời gian ban đầu.
            timer.Start();
            enemyListAI = enemyList;
        }
        public void StopEnemyAI()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= OnTimerElapsed; // Optional: Unsubscribe from the event when not using the timer
                timer = null;
            }

            enemyListAI = null; // Optional: If you want to clear the list of enemies
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpDateEnemyAI(); // Gọi hàm cập nhật AI của enemy.
            timer.Interval = GetRandomInterval(); // Đặt lại khoảng thời gian cho lần sau.
        }
        
        private double GetRandomInterval()
        {
            if(isFirst)
            {
                isFirst = false;
                return 1;
                
            }
            Random rnd = new Random();

            return rnd.Next(2000, 10001); // Trả về một giá trị ngẫu nhiên từ 1000ms (tức là 1 giây) đến 5000ms (tức là 5 giây).
        }
        public void UpDateEnemyAI()
        {
            if (playerList.Count == 0) return;
            enemyListAI = enemyList;
            Random rnd = new Random();
            foreach (Enemy enemy in enemyListAI)
            {
                float x = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                float y = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                enemy.pos = new Vector3(x, y, 0);
                enemy.hotPos = rnd.Next(0,9);

                float num = rnd.Next(-360, 360);
                //float num = (float)(rnd.NextDouble() * 2.0 - 1.0);

                /*     do
                     {
                         num = (float)rnd.NextDouble() * 2 - 1;  // Create a random number between -1 and 1
                     }
                     while (num > -0.2 && num < 0.2);  // Repeat if the number is in the range to avoid*/

                enemy.quaternion = new Quaternion(0.0f, 0.0f, num, 1.0f);

            }
            float timeBetweenBullets = (float)rnd.NextDouble() * 5f;

            int bulletCount = rnd.Next(0,4);
            var data = new Dictionary<byte, object>();

            data[1] = GamePlayCode.GetEnemyAI;
            data[2] = JsonConvert.SerializeObject(this.enemyListAI);
            data[3] = JsonConvert.SerializeObject(this.enemyList);
            data[4] = 1;
            data[5] = 0.1f;
            //truyền data về client 
            room.SendAllPlayer((byte)RequestCode.GamePlay, data);

        }
        /*private void SpawEnemy()
        {
            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {

                Enemy enemy = new Enemy();
                enemy.enemyID = i;
                enemy.name = "Enemy_" + i.ToString();


                enemy.category = rnd.Next(0, 8);

                enemy.enemyProper = new RoleProperties(10f, 500f, 3f, 2f, 0.2f);

                float x = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                float y = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                enemy.pos = new Vector3(x, y, 0);

                float zAngle = (float)rnd.NextDouble() * 360f;
                enemy.quaternion = new Quaternion(0, 0, zAngle, 1);

                this.enemyList.Add(enemy);
            }
        }*/

        public override void OnUserJoin(User user)
        {
            
            //base.OnUserJoin(user);
            Log.Debug($"user gia nhap { user.userData.userID}");
            var player = new Player
            {
                isOwner = false,
                point = user.userData.point,
                userID = user.userData.userID,
                name = user.userData.username,
                position = new Vector3(0, 0, 0),
                rotation1 = new Vector3(0,0,1),
                inventoryDict = user.userData.inventoryDict,
                category = user.userData.category,
                properties = new RoleProperties(500f, 50f, 10f, 15f,3f,500f)

            };
            
            var currentPlayer = playerList.Find(x => x.userID == user.userData.userID);
            if(currentPlayer != null)
            {
                player.position = currentPlayer.position;
                currentPlayer = player;
                //Vector3 lastPos = player.position;
                //currentPlayer = player;
                //urrentPlayer.position = lastPos;
            }
            else
            {
                if (playerList.Count == 0)
                {
                    player.isOwner = true;
                    user.userData.isOwner = true;
                }
                playerList.Add(player);               
            }
            
            var data = new Dictionary<byte, object>();
            data[1] = RoomCode.JoinRoom;
            //data[1] = GamePlayCode.UserJoinRoom;
            data[2] = JsonConvert.SerializeObject(player);
            //truyen cho tat ca nguoi choi tru user(nguoi choi hien tai)
            room.SendAllPlayerOther((byte)RequestCode.Room, data, user);
            user.SendEvent(new EventData((byte)RequestCode.LoadGamePlay), new SendParameters());




        }

        public override void OnUserLeave(User user)
        {
            bool exists = playerList.Exists(x => x.userID == user.userData.userID);
            if (!exists) return;
            
            //base.onUserLeave(user);
            playerList.RemoveAll(x => x.userID == user.userData.userID);
            if (user.userData.isOwner && playerList.Count > 0)
            {
                user.userData.isOwner = false;
                int id = playerList[0].userID;
                Dictionary<byte, object> data2 = new Dictionary<byte, object>();
                data2[1] = GamePlayCode.SetOwner;
                data2[2] = id;
                data2[3] = JsonConvert.SerializeObject(enemyListAI);
                room.SendAllPlayer((byte)RequestCode.GamePlay, data2);

            }
            if (playerList.Count == 0)
            {
                enemyList.Clear();
                StopEnemyAI();
                //StopUpdating();
            }

            Dictionary<byte, object> data = new Dictionary<byte, object>();
            data[1] = RoomCode.LeaveRoom;
            data[2] = user.userData.userID;
            room.SendAllPlayer((byte)RequestCode.Room, data);
            //room.SendAllPlayerOther((byte)RequestCode.Room, data,user);



        }
        public void OnKickUser(int userID)
        {
            playerList.RemoveAll(x => x.userID == userID);
            Log.Debug($"nguoi choi co userID ={userID} da bi kik");
            Log.Debug(JsonConvert.SerializeObject(playerList));
        }
        public void SendOnePlayer(byte code, Dictionary<byte, object> data, User otherUser, bool useUDP = false)
        {
            otherUser.lastJoinRoom.users[0].SendEvent(new EventData(code, data), new SendParameters { Unreliable = useUDP });

        }


    }
}
