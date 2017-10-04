using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Protocols.Abstract.Interfaces
{
    interface IBuffRemover
    {
        void RemoveBuff(int objectId,int buffId ,int buffLevel);
    }
}
