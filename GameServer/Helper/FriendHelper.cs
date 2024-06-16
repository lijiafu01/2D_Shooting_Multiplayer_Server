using GameClient.Constructor;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Helper
{
    public class FriendHelper
    {
        public static List<Player> PendingList(int userID2)
        {
            List<Player> result = new List<Player>();
            using (var conn = DBUtils.GetMySqlConnection())
            {
                conn.Open();
                string query = @"
                SELECT U.userID, U.username
                FROM Users U
                INNER JOIN Friends F ON U.userID = F.userID1
                WHERE F.userID2 = @userID2 AND F.status = 'pending'";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userID2", userID2);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Player player = new Player();
                        player.userID = reader.GetInt32(0);
                        player.name = reader.GetString(1);
                        result.Add(player);
                    }
                }
            }
            return result;
        }

        public static bool CheckIsFriend(int userID1, int userID2)
        {
            using (var conn = DBUtils.GetMySqlConnection())
            {
                conn.Open();
                try
                {
                    string query = "SELECT COUNT(*) FROM Friends WHERE ((userID1 = @userID1 AND userID2 = @userID2) OR (userID1 = @userID2 AND userID2 = @userID1)) AND status = 'friend'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userID1", userID1);
                    cmd.Parameters.AddWithValue("@userID2", userID2);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // Return true if the users are friends.
                    return count > 0;
                }
                catch (Exception ex)
                {
                    // Handle any errors that may have occurred
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        public static void AddPendingList(int userID1, int userID2)
        {
            using (var conn = DBUtils.GetMySqlConnection())
            {
                conn.Open();
                try
                {
                    string checkQuery = "SELECT COUNT(*) FROM Friends WHERE (userID1 = @userID1 AND userID2 = @userID2) AND status = 'pending'";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@userID1", userID1);
                    checkCmd.Parameters.AddWithValue("@userID2", userID2);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        string query = "INSERT INTO Friends(userID1, userID2, status) VALUES(@userID1, @userID2, 'pending')";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@userID1", userID1);
                        cmd.Parameters.AddWithValue("@userID2", userID2);

                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("Record already exists.");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that may have occurred
                    Console.WriteLine(ex.Message);
                }
            }
        }


        public static void AddFriend(int userID1, int userID2)
        {
            using (var conn = DBUtils.GetMySqlConnection())
            {
                conn.Open();
                try
                {
                    // Insert the first record
                    string query = "INSERT INTO Friends(userID1, userID2, status) VALUES(@userID1, @userID2, 'friend')";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userID1", userID1);
                    cmd.Parameters.AddWithValue("@userID2", userID2);

                    cmd.ExecuteNonQuery();

                    // Clear the parameters
                    cmd.Parameters.Clear();

                    // Insert the second record
                    query = "INSERT INTO Friends(userID1, userID2, status) VALUES(@userID1, @userID2, 'friend')";
                    cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userID1", userID2);
                    cmd.Parameters.AddWithValue("@userID2", userID1);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle any errors that may have occurred
                    Console.WriteLine(ex.Message);
                }
            }
        }


        public static List<Player> SerchUser(string username)
        {
            var conn = DBUtils.GetMySqlConnection();
            conn.Open();
            string query = "SELECT userID, username FROM Users WHERE username LIKE @username LIMIT 5;";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Parameters.AddWithValue("@username", "%" + username + "%");

            cmd.Connection = conn;
            cmd.CommandText = query;
            List<Player> result = new List<Player>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {

                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    result.Add(new Player { userID = id,name = name });
                }
            }
            conn.Close();
            conn.Dispose();
            return result;
        }
    }
}
