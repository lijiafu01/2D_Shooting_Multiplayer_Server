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
using MySqlConnector;
using GameServer.Helper;

namespace GameServer.Handlers
{
    public class FriendHandler : BaseHandler
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public bool OnHandlerRequest(OperationRequest request, SendParameters sendData, User user)
        {
            if (request.OperationCode != (byte)RequestCode.Friend) return false;
            switch (request.Parameters[1])
            {
                case (int)FriendCode.Search:
                    SearchUser(request.Parameters, user);
                    break;
                case (int)FriendCode.Add:
                    AddFriend(request.Parameters, user);
                    break;
                case (int)FriendCode.Pending:
                    PendingList(request.Parameters, user);
                    break;
                case (int)FriendCode.Respon:
                    FriendRespon(request.Parameters, user);
                    break;
                case (int)FriendCode.Friend:
                    FriendList(request.Parameters, user);
                    break;
                case (int)FriendCode.Remove:
                    RemoveFriend(request.Parameters, user);
                    break;
            }
            return true;
        }

        private void RemoveFriend(Dictionary<byte, object> dt, User user)
        {
            int userID1 = (int)dt[2];
            int userID2 = user.userData.userID;
            MySqlConnection conn = null;
            MySqlCommand cmd = null;

            try
            {
                conn = DBUtils.GetMySqlConnection();
                conn.Open();

                cmd = new MySqlCommand("DELETE FROM Friends WHERE (userID1 = @UserID1 And userID2 = @UserID2) OR (userID1 = @UserID2 And userID2 = @UserID1) AND status = 'friend'", conn);
                cmd.Parameters.AddWithValue("@UserID1", userID1);
                cmd.Parameters.AddWithValue("@UserID2", userID2);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Handle any error that occurred during the execution of the SQL command.
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                // Close the connection
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }


        private void FriendList(Dictionary<byte, object> dt, User user)
        {
            List<Player> players = new List<Player>();
            int id = user.userData.userID;
            MySqlConnection conn = null;
            MySqlCommand cmd = null;

            try
            {
                conn = DBUtils.GetMySqlConnection();
                conn.Open();

                cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT 
                            Users.userID, Users.username 
                        FROM 
                            Friends 
                        INNER JOIN 
                            Users ON (Friends.userID1 = Users.userID AND Friends.userID2 = @id) OR 
                                      (Friends.userID2 = Users.userID AND Friends.userID1 = @id) 
                        WHERE 
                            Friends.status = 'friend'";

                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userID = reader.GetInt32(0);
                        string username = reader.GetString(1);

                        Player player = new Player(); // tạo một đối tượng Player mới
                        player.userID = userID;
                        player.name = username;
                        players.Add(player); // thêm vào danh sách
                    }
                }
            }
            catch 
            {
                Log.Debug("loi hien thi danh sach ban be");
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();

                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            var data = new Dictionary<byte, object>();
            data[1] = FriendCode.Friend;
            data[2] = JsonConvert.SerializeObject(players);
            user.SendEvent(new EventData((byte)RequestCode.Friend, data), new SendParameters());

        }

        private void FriendRespon(Dictionary<byte, object> dt, User user)
        {
            int userID1 = (int)dt[2];
            int userID2 = user.userData.userID;

            MySqlConnection conn = null;
            MySqlCommand cmd = null;

            try
            {
                conn = DBUtils.GetMySqlConnection();
                conn.Open();

                if ((bool)dt[3])
                {

                    // If the friend request is accepted
                    cmd = new MySqlCommand(
                        "UPDATE Friends SET status = 'friend' WHERE userID1 = @userID1 AND userID2 = @userID2", conn
                    );
                }
                else
                {
                    // If the friend request is declined
                    cmd = new MySqlCommand(
                        "DELETE FROM Friends WHERE userID1 = @userID1 AND userID2 = @userID2 AND status = 'pending'", conn
                    );
                }

                cmd.Parameters.Add("@userID1", MySqlDbType.Int32).Value = userID1;
                cmd.Parameters.Add("@userID2", MySqlDbType.Int32).Value = userID2;

                cmd.ExecuteNonQuery();
            }
            catch 
            {
                Log.Debug("loi chap nhan ket ban");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }


        private void PendingList(Dictionary<byte, object> dt, User user)
        {
            List<Player> players = new List<Player>();
            int userID2 = user.userData.userID;
            players = FriendHelper.PendingList(user.userData.userID);

            var data = new Dictionary<byte, object>();
            data[1] = FriendCode.Pending;
            data[2] = JsonConvert.SerializeObject(players);
            user.SendEvent(new EventData((byte)RequestCode.Friend, data), new SendParameters());
        }



        private void AddFriend(Dictionary<byte, object> dt, User user)
        {
            int userID1 = user.userData.userID;
            int userID2 = (int)dt[2];
            if (FriendHelper.CheckIsFriend(userID1, userID2))
            {
                var data = new Dictionary<byte, object>();
                data[1] = FriendCode.Add;
                data[2] = false;
                user.SendEvent(new EventData((byte)RequestCode.Friend, data), new SendParameters());
            }
            else 
            {
                FriendHelper.AddPendingList(userID1, userID2);
                var data = new Dictionary<byte, object>();
                data[1] = FriendCode.Add;
                data[2] = true;
                user.SendEvent(new EventData((byte)RequestCode.Friend, data), new SendParameters());
            }
        }


        private void SearchUser(Dictionary<byte, object> dt, User user)
        {
            string username = (string)dt[2];
            List<Player> players = new List<Player>();
            players = FriendHelper.SerchUser(username);
            var data = new Dictionary<byte, object>();
            data[1] = FriendCode.Search;
            data[2] = JsonConvert.SerializeObject(players);
            user.SendEvent(new EventData((byte)RequestCode.Friend, data), new SendParameters());
        }
    }
}
