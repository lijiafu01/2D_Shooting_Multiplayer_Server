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
using MySqlConnector;
using DevOne.Security.Cryptography.BCrypt;

namespace GameServer.Handlers
{
    public class RegisterHandler : BaseHandler
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user)
        {
            if ((byte)RequestCode.Register != request.OperationCode) return false;
            //du lieu dc gui tu client len de check voi database
            string username = request.Parameters[1] as string;
            string password = request.Parameters[2] as string;
            if (username == "" || password == "") return true;
            bool registerStatus = false;

            var conn = DBUtils.GetMySqlConnection();
            conn.Open();

            string sql = "SELECT * FROM users WHERE username = @username";
            var cmd = new MySqlCommand();
            cmd.Parameters.Add("@username", MySqlDbType.String).Value = username;
            cmd.Connection = conn;
            cmd.CommandText = sql;

            using (var reader = cmd.ExecuteReader())
            {
                if(reader.Read())
                {
                    user.SendNotification("tai khoan da ton tai khong the dang ky!");
                    registerStatus = false;
                }
                else
                {
                    var conn2 = DBUtils.GetMySqlConnection();
                    conn2.Open();
                    string query = $"INSERT INTO users(`username`, `password`) VALUES (@username,@password)";
                    var cmd2 = new MySqlCommand();
                    cmd2.Parameters.Add("@username", MySqlDbType.String).Value = username;
                    string salt = BCryptHelper.GenerateSalt(10);
                    string hashPassword = BCryptHelper.HashPassword(password, salt);
                    hashPassword = hashPassword.Replace("$2a$10$", "$2y$10$");
                    cmd2.Parameters.Add("@password", MySqlDbType.String).Value = hashPassword;
                    cmd2.Connection = conn2;
                    cmd2.CommandText = query;
                    cmd2.ExecuteNonQuery();
                    registerStatus = true;
                    conn2.Close();
                    conn2.Dispose();
                }
                
            }
            conn.Close();
            conn.Dispose();

            Dictionary<byte, object> data = new Dictionary<byte, object>();
            data[1] = registerStatus;
            user.SendEvent(new EventData((byte)RequestCode.Register, data), new SendParameters { Unreliable = true });
            return true;
        }
    }
}
