using GameClient.Constructor;
using GameServer.Constructor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
namespace GameServer.Helper
{
    public class InventoryHelper
    {
        public static AutoIncrement AUTO_INCREMENT = new AutoIncrement();
        public static Dictionary<int, Inventory> GetInventoryDict(int userID)
        {
            Dictionary<int, Inventory> result = new Dictionary<int, Inventory>();
            MySqlConnection conn = DBUtils.GetMySqlConnection();
            conn.Open();
            string query = $"Select * from inventory Where IDUser={userID}";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;

            using (var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    int inventoryID = reader.GetInt16(reader.GetOrdinal("inventoryID"));
                    int IDItem = reader.GetInt16(reader.GetOrdinal("IDItem"));
                    int IDUser = reader.GetInt16(reader.GetOrdinal("IDUser"));
                    result.Add(inventoryID, new Inventory { inventoryID = inventoryID, IDitem = IDItem, IDUser = IDUser });
                }
            }
            conn.Close();
            conn.Dispose();
            return result;

        }
        public static void AddInventory(int itemID,UserData userData)
        {
            int inventoryID = 0;
            lock (AUTO_INCREMENT)
            {
                inventoryID = AUTO_INCREMENT.id;
                AUTO_INCREMENT.id++;
            }
            var inventory = new Inventory { IDitem = itemID, IDUser = userData.userID, inventoryID = AUTO_INCREMENT.id };
            userData.inventoryDict.Add(AUTO_INCREMENT.id, inventory);
            World.Instance.AddQuery($"INSERT INTO `inventory`(`inventoryID`, `IDItem`, `IDUser`) VALUES ({inventoryID},{itemID},{userData.userID});");
        }

    }
}
