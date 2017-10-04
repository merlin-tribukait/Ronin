using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Network;
using Ronin.Utilities;

namespace Ronin.Protocols
{
    public abstract class ActionRequest
    {
        protected PacketBuilder packet = new PacketBuilder();
        public ActionRequest()
        {
            
        }

        public abstract void Build(L2PlayerData data);

        public virtual void Send(L2PlayerData data, Client tcpClient)
        {
            Build(data);
            tcpClient.SendPacketToServer(packet.GetBytes());
        }
    }
}
