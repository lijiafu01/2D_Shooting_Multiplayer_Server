using ExitGames.Logging;
using GameClient.Constructor;
using GameClient.Enums;
using GameServer.Handler;
using GameServer.Helper;
using GameServer.RoomMode;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Handlers
{
    public class GamePlayHandler : BaseHandler
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user)
        {
            if (request.OperationCode != (byte)RequestCode.GamePlay) return false;
            switch ((int)request.Parameters[1])
            {
                case (int)GamePlayCode.GetData:
                    GetData(user);
                    break;
                case (int)GamePlayCode.Move:
                    Move(request.Parameters, user);
                    break;
                case (int)GamePlayCode.LootItem:
                    LootItem(request.Parameters, user);
                    break;
                case (int)GamePlayCode.BulletMove:
                    BulletMove(request.Parameters, user);
                    break;
                case (int)GamePlayCode.TakeDamage:
                    TakeDamage(request.Parameters, user);
                    break;
                case (int)GamePlayCode.Skill:
                    TakeSkill(request.Parameters, user);
                    break;
                /*case (int)GamePlayCode.CheckHp:
                    CheckHp(request.Parameters, user);
                    break;*/
                /*case (int)GamePlayCode.GetEnemyAI:
                    EnemyAI(request.Parameters, user);
                    break;*/
                case (int)GamePlayCode.GetCurrentListEnemy:
                    GetCurrentListEnemy(request.Parameters, user);
                    break;
                case (int)GamePlayCode.EnemyGetDame:
                    EnemyGetDame(request.Parameters, user);
                    break;
                case (int)GamePlayCode.EnemyDeath:
                    EnemyDeath(request.Parameters, user);
                    break;
                case (int)GamePlayCode.SpawnItem:
                    SpawnItem(request.Parameters, user);
                    break;
                case (int)GamePlayCode.GetCurrentPlayer:
                    GetCurrentPlayer(request.Parameters, user);
                    break;
            }
            return true;
        }

        private void GetCurrentPlayer(Dictionary<byte, object> dt, User user)
        {
            GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            var player = extension.playerList.Find(x => x.userID == user.userData.userID);
            RoleProperties proper = JsonConvert.DeserializeObject<RoleProperties>((string)dt[2]);
            player.properties = proper;

            user.lastJoinRoom.SendAllPlayerOther((byte)RequestCode.GamePlay, dt,user, true);
        }

        private void SpawnItem(Dictionary<byte, object> dt, User user)
        {
            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, dt, true);
        }

        private void EnemyDeath(Dictionary<byte, object> dt, User user)
        {
            /*Random rnd = new Random();
            *//*float x = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
            float y = (float)rnd.NextDouble() * (25 - (-25)) + (-25);

            Vector3 vector3 = new Vector3(x, y, 0);*//*

            int randomItem = rnd.Next(0, 6); // includes 0 but excludes 6, giving you 0 to 5

            var data = new Dictionary<byte, object>();
            data[1] = GamePlayCode.SpawnItem;
            data[2] = randomItem;
            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, data, true);*/
            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, dt, true);
        }

        private void EnemyGetDame(Dictionary<byte, object> dt, User user)
        {

            /*GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            var enemy = extension.enemyList.Find(x => x.enemyID == (int)dt[2]);
            if (enemy != null)
            {
                enemy.enemyProper.hp -= (float)dt[3];
                Random rnd = new Random();
                if (enemy.enemyProper.hp <= 0)
                {
                    float x = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                    float y = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                    
                    enemy.pos = new Vector3(x, y, 0);
                    int category = rnd.Next(0, 8);
                    var data = new Dictionary<byte, object>();
                    data[1] = dt[1];
                    data[2] = dt[2];
                    data[3] = dt[3];
                    dt[4] = 50f;
                    dt[5] = category;
                    user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, data, true);
                    return;
                   *//* Log.Debug("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                    Log.Debug($"{(int)dt[5]} " + "aaaaaaaaaaaaaaaaaaa");*//*
                }
            }*/

            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, dt, true);
            //Log.Debug($"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa +{enemy.enemyProper.hp}");
        }

        private void GetCurrentListEnemy(Dictionary<byte, object> dt, User user)
        {
            GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            List<Enemy> enemyClient = JsonConvert.DeserializeObject<List<Enemy>>((string)dt[2]);
            extension.enemyList = enemyClient;
            /*for(int i = 0; i < enemyClient.Count; i++)
            {
                extension.enemyList[i].pos = enemyClient[i].pos;
                extension.enemyList[i].quaternion = enemyClient[i].quaternion;
            }*/

           /* Log.Debug("nhan enemy pos tu owner ppppppppppppppppppppppppp");
            List<int> angleList = new List<int>();
            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                int angle = rnd.Next(0, 360);
                angleList.Add(angle);
            }

            var data = new Dictionary<byte, object>();
            data[1] = GamePlayCode.GetListEnemy;
            data[2] = JsonConvert.SerializeObject(extension.enemyList);
            data[3] = dt[3];
            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, data, true);*/

        }

        private void EnemyAI(Dictionary<byte, object> parameters, User user)
        {
           /* GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            Random rnd = new Random();
            foreach(Enemy enemy in extension.enemyList)
            {
                float x = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                float y = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                enemy.pos = new Vector3(x, y, 0);

                float zAngle = (float)rnd.NextDouble() * 360f;
                enemy.quaternion = new Quaternion(0, 0, zAngle, 1);
            }*/
        }

        private void CheckHp(Dictionary<byte, object> dt, User user)
        {
            /*float hp = (float)dt[3];
            GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            var player = extension.playerList.Find(x => x.userID == user.userData.userID);
            if (player != null)
            {
                player.properties.hp = (float)dt[3];
            }*/
        }

        private void TakeSkill(Dictionary<byte, object> dt, User user)
        {
            
            /*GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            var player = extension.playerList.Find(x => x.userID == user.userData.userID);
            if (player != null)
            {
                player.properties.isSkill = (bool)dt[4];
                player.properties.skillCategoy = (int)dt[3];
                if ((int)dt[3] == 1)
                {
                    player.properties.hp -= 5;
                }            
            }*/
            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, dt, true);

        }

        private void TakeDamage(Dictionary<byte, object> dt, User user)
        {
            
           /* GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            var player = extension.playerList.Find(x => x.userID == user.userData.userID);
            if (player != null)
            {
                //dong bo ben client thi hay hon
                player.properties.hp -= (float)dt[3];
            }*/
            var data = new Dictionary<byte, object>();
            data[1] = GamePlayCode.TakeDamage;
            data[2] = dt[2];
            data[3] = dt[3];
            data[4] = dt[4];
            user.lastJoinRoom.SendAllPlayer((byte)RequestCode.GamePlay, data, true);
        }

        private void BulletMove(Dictionary<byte, object> dt, User user)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>((string)dt[2]);
            var rotation = JsonConvert.DeserializeObject<Quaternion>((string)dt[3]);
            GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            //World.Instance.bulletList[1].bulletID = dt[5];
            Bullet newBullet = new Bullet();
            newBullet.bulletPos = pos;
            Vector3 newRotation = new Vector3(rotation.x, rotation.y, rotation.z);
            newBullet.bulletRot = newRotation;
            newBullet.bulletID = (int)dt[4];
            World.Instance.bulletList.Add(newBullet);
            var data = new Dictionary<byte, object>();
            data[1] = GamePlayCode.BulletMove;
            data[2] = JsonConvert.SerializeObject(World.Instance.bulletList);
            data[3] = dt[2];
            data[4] = dt[3];
            data[5] = dt[5];
            data[6] = dt[6];
            data[7] = dt[7];
            data[8] = dt[8];
            user.lastJoinRoom.SendAllPlayerOther((byte)RequestCode.GamePlay, data, user);
            World.Instance.bulletList.Clear();
        }
        private void LootItem(Dictionary<byte, object> dt, User user)
        {
            //int userID = (int)dt[2];
            //Log.Debug($"User LootItem{user.userData.userID}" + itemID);
            /*UserDataHelper.AddPoint(10, user.userData);
            if (user.userData.inventoryDict.Values.ToList().Find(x => x.IDitem == itemID) == null)
            {
                InventoryHelper.AddInventory(itemID, user.userData);
            }*/
            user.lastJoinRoom.SendAllPlayerOther((byte)RequestCode.GamePlay, dt, user);

        }

        //chuyen pos data cua user vua vao phong len cho sv
        private void Move(Dictionary<byte, object> dt, User user)
        {
            
            var pos = JsonConvert.DeserializeObject<Vector3>((string)dt[2]);
            var rotation = JsonConvert.DeserializeObject<Quaternion>((string)dt[3]);
            GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            extension.playerList.Find(x => x.userID == user.userData.userID).position = pos;
            Vector3 newRotation = new Vector3(rotation.x, rotation.y, rotation.z);
            extension.playerList.Find(x => x.userID == user.userData.userID).rotation1.z = (float)dt[4];
            //Log.Debug(user.userData.userID + " vi tri x line: " + pos.x + "    y line: " + pos.y + "     z line: " + pos.z);
            var data = new Dictionary<byte, object>();
            data[1] = GamePlayCode.Move;
            data[2] = dt[2];
            data[3] = user.userData.userID;
            data[4] = dt[3];
            user.lastJoinRoom.SendAllPlayerOther((byte)RequestCode.GamePlay, data, user);
        }

        void GetData(User user)
        {
            var data = new Dictionary<byte, object>();
            GamePlayExtension extension = (GamePlayExtension)user.lastJoinRoom.extension;
            
            if (extension.playerList.Count == 1)
            {
                
                Random rnd = new Random();
                for (int i = 0; i < 10; i++)
                {

                    Enemy enemy = new Enemy();
                    enemy.enemyID = i;
                    enemy.name = "Enemy_" + i.ToString();


                    enemy.category = rnd.Next(0, 8);

                    enemy.enemyProper = new RoleProperties(500f, 100f, 7f, 0f, 3f,500f);

                    float x = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                    float y = (float)rnd.NextDouble() * (25 - (-25)) + (-25);
                    enemy.pos = new Vector3(x, y, 0);

                    float zAngle = (float)rnd.NextDouble() * 360f;
                    enemy.quaternion = new Quaternion(0, 0, zAngle, 1);

                    extension.enemyList.Add(enemy);
                    
                }
                
                extension.StartEnemyAI();
            }
           // Log.Debug("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" + extension.enemyList.Count);
            data[1] = GamePlayCode.GetData;
            data[2] = JsonConvert.SerializeObject(extension.playerList);
            data[3] = JsonConvert.SerializeObject(extension.enemyList);
            data[4] = JsonConvert.SerializeObject(extension.enemyListAI);
            //truyền data về client 
            user.SendEvent(new EventData((byte)RequestCode.GamePlay, data), new SendParameters());
            extension.isFirst = true;
        }
        
    }
}
