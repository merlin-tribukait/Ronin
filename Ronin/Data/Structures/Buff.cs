using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Data.Structures
{
    public class Buff
    {
        public int Id;
        public int Level;
        private int timeLeft;
        private long stamp;

        public Buff(int id, int level, int timeLeft)
        {
            this.Id = id;
            this.Level = level;
            this.timeLeft = timeLeft;
            this.stamp = Environment.TickCount;
        }

        public int TimeLeft
        {
            get { return timeLeft; }
            set { timeLeft = value; }
        }

        public long SecondsLeft
        {
            get
            {
                long result = (timeLeft - (Math.Abs(Environment.TickCount - this.stamp)/1000));
                return result < 0 ? 0 : result;
            }
        }

        public long MinutesLeft
        {
            get
            {
                long result = (timeLeft - (Math.Abs(Environment.TickCount - this.stamp)/1000))/60;
                return result < 0 ? 0 : result;
            }
        }

        public string BuffName
        {
            get { return ExportedData.SkillsIdToName.ContainsKey(Id) ? ExportedData.SkillsIdToName[this.Id] : "(PARSE ERROR)"; }
        }
    }
}
