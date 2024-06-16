using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Constructor
{
    public class UserData
    {
        public bool isOwner = false;
        public string username;
        public string password;
        public int userID;
        public string name;
        public int point;
        public int category;
        public Dictionary<int, Inventory> inventoryDict;
    }
}
