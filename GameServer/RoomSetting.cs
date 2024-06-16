using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class RoomSetting
    {
        public int id;
        public bool removeLeaveRoom;//khi player out ,phong se auto xoa
        public string name;
        public string password;
        public int maxPlayer;//so luog toi da trong phong

        public RoomSetting(string name)
        {
            this.name = name;
        }
        public RoomSetting() { }
    }
}
