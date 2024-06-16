using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Constructor
{
    [System.Serializable]
    public class Quaternion
    {
        public float x, y, z,w;

        public Quaternion(float x, float y, float z,float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
}
