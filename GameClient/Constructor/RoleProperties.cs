using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Constructor
{
    [System.Serializable]
    public class RoleProperties
    {
        public float damage;
        public float hp;
        public float crit;
        public float moveSpeed;
        public float attackSpeed;
        public bool isSkill;
        public int skillCategoy;
        public float maxHp;
        public RoleProperties(float hp, float damage, float attackSpeed, float crit,float moveSpeed,float maxHp)
        {
            this.hp = hp;
            this.damage = damage;            
            this.attackSpeed = attackSpeed;  
            this.crit = crit;
            this.moveSpeed = moveSpeed;
            this.maxHp = maxHp;
        }
    }
}
