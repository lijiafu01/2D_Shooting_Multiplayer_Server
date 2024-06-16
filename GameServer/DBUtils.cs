using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace GameServer
{
    class DBUtils
    {
        public static MySqlConnection GetMySqlConnection()
        {
            string host = "127.0.0.1";
            int port = 3306;
            string database = "gamedemo";
            string username = "root";
            string password = "Viethoan2001";

            return GetDBConnection(host, port, database, username, password);
        }

        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            string connString = "Server=" + host + ";Database=" + database + ";port=" + port + ";User ID=" + username + ";password=" + password;
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }
    }
}