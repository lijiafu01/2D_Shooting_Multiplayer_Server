using GameClient.Enums;
using GameServer.Handler;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ExitGames.Logging;
using Newtonsoft.Json;
using GameClient.Constructor;
using MySqlConnector;
using System.Data.Common;
using GameServer.Helper;
using DevOne.Security.Cryptography.BCrypt;

namespace GameServer.Handlers
{
    public class LoginHandler : BaseHandler
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user)
        {
            if ((byte)RequestCode.Login != request.OperationCode) return false;
            //du lieu dc gui tu client len de check voi database
            string username = request.Parameters[1] as string;
            string password = request.Parameters[2] as string;
            bool loginFail = false;
            Dictionary<byte, object> data = new Dictionary<byte, object>();
           
            var conn = DBUtils.GetMySqlConnection();
            conn.Open();
            
            string sql = "SELECT * FROM users WHERE username = @username";
            var cmd = new MySqlCommand();
            cmd.Parameters.Add("@username", MySqlDbType.String).Value = username;
            cmd.Connection = conn;
            cmd.CommandText = sql;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if(reader.Read())
                {
                    int userID = reader.GetInt16(reader.GetOrdinal("userID"));
                    string dbusername = reader.GetString(reader.GetOrdinal("username"));
                    string dbpassword = reader.GetString(reader.GetOrdinal("password"));
                    string name = reader.IsDBNull(reader.GetOrdinal("name")) ? "" : reader.GetString(reader.GetOrdinal("name"));
                    int point = reader.GetInt16(reader.GetOrdinal("point"));
                    Log.Debug($"userID = {userID} username = {username} password = {password} name = {name} point = {point}");
                    bool checkPass = BCryptHelper.CheckPassword(password, dbpassword.Replace("$2y$10$", "$2a$10$"));
                    if (checkPass) 
                    { 
                        if(World.Instance.users.ContainsKey(userID)) World.Instance.users[userID].Disconnect();
                        World.Instance.users.Add(userID, user);
                        //user.userData = new UserData { name = "", point = 0, userID = userID, username = username, inventoryDict = InventoryHelper.GetInventoryDict(userID) };
                        if(World.Instance.userDataDict.ContainsKey(userID))
                        {
                            user.userData = World.Instance.userDataDict[userID];
                        }
                        else
                        {
                            user.userData = new UserData { name = "", point = 0, userID = userID, username = username, inventoryDict = InventoryHelper.GetInventoryDict(userID) };
                            World.Instance.userDataDict.Add(userID, user.userData);
                        }
                        data[2] = JsonConvert.SerializeObject(user.userData);
                    }
                    else
                    {
                        loginFail = true;
                        user.SendNotification("sai mat khau !");
                    }
                }
                else
                {
                    loginFail = true; Log.Debug($"tai khoan  {username} khong ton tai");
                }
            }
            conn.Close();
            conn.Dispose();
            data[1] = loginFail;         
            user.SendEvent(new EventData((byte)RequestCode.Login, data), new SendParameters { Unreliable = true});
            //neu dang nhap that bai server se dung khoang 1s xong moi disconect
           /* Thread.Sleep(1000);
            if (loginFail) user.Disconnect();*/
            return true;

            
        }
    }
}
