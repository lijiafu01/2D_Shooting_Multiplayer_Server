using System.Collections.Generic;

namespace GameClient.Constructor
{
    [System.Serializable]
    public class Player
    {
        public string quaternion;
        public bool isOwner = false;
        public RoleProperties properties;
        public int category;
        public Vector3 position;
        //qua bi loi nen chuyen qua vector 3
        public Vector3 rotation1;
        public int userID;
        public string name;
        public int point;
        public Dictionary<int, Inventory> inventoryDict = new Dictionary<int, Inventory>();      
    }
}
