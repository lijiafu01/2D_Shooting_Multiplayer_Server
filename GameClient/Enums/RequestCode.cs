using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Enums
{
    public enum RequestCode : byte
    {
        Kick,
        Error,
        Notification,
        Room,
        Login,
        Register,
        LoadGamePlay,
        GamePlay,
        Chat,
        Friend
    }
}
