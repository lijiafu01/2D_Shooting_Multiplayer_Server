using GameClient.Constructor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Helper
{
    public class UserDataHelper
    {

        public static void AddPoint(int point,UserData userData)
        {
            if (point <= 0) return;
            lock (userData)
            {
                userData.point += point;
            }
            World.Instance.AddQuery($"UPDATE `users` SET `point`='{userData.point}' WHERE userID={userData.userID} limit 1;");
        }
    }
}
