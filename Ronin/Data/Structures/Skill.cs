using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class Skill
    {
        public int SkillId;
        public int SkillLevel;
        public bool IsPassive;
        public long LastLaunched;
        public int Cooldown;
        public int CastTime;
        public bool IsDisabled;
        public bool IsEnchanted;
        
        public string SkillName
        {
            get { return ExportedData.SkillsIdToName.ContainsKey(SkillId) ? ExportedData.SkillsIdToName[this.SkillId] : "(ERROR)"; }
        }

        public bool CanBeUsed()
        {
            if (Math.Abs(Environment.TickCount - LastLaunched) > Cooldown &&
                Math.Abs(Environment.TickCount - LastLaunched) > CastTime)
                return true;
            return false;
        }
    }
}
