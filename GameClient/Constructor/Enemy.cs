using System.Collections.Generic;


namespace GameClient.Constructor
{
    [System.Serializable]
    public class Enemy
    {
        public int enemyID;
        public string name;
        public RoleProperties enemyProper;
        public int category;
        public Vector3 pos;
        public int hotPos;
        public Quaternion quaternion;
    }
}
