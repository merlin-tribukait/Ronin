using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class Quest
    {
        public int QuestId;
        public int StateFlags;
        public int ProgressCounter;

        /// <summary>
        /// Dictionary containing the counters of the monsters killed for the quest. (Key = unitId, Value = count)
        /// </summary>
        //public Dictionary<NpcId, int> QuestMobCounter = new Dictionary<NpcId, int>();
    }
}
