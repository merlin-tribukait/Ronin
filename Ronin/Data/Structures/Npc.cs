using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class Npc : GameFigure
    {
        public bool IsMonster;
        public bool IsSweepable;
        public bool IsSummonPet;
        private string _name;
        public byte IsInvisible;
        public long AddStamp;

        public string Name
        {
            get
            {
                if(_name == null || _name.Trim()==string.Empty)
                try
                {
                    _name = ExportedData.NpcIdToName[this.UnitId];
                }
                catch
                {

                }
                return _name;
            }
            set { _name = value; }
        }

        public bool IsSummon
        {
            get
            {
                if (UnitId >= 14001 && UnitId <= 14918) //>= Reanimated Man && <= Imperial Phoenix
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsPet
        {
            get
            {
                if (UnitId >= 16013 && UnitId <= 16042)
                {
                    return true;
                }

                if (UnitId >= 12311 && UnitId <= 12313)
                {
                    return true;
                }

                if (UnitId >= 12526 && UnitId <= 12528)
                {
                    return true;
                }

                if (UnitId == 12564)
                {
                    return true;
                }

                if (UnitId == 12621)
                {
                    return true;
                }

                if (UnitId >= 12780 && UnitId <= 12782)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
