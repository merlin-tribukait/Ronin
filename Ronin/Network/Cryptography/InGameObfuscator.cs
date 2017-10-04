using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Network.Cryptography
{
    public abstract class InGameObfuscator
    {
        public InGameObfuscator(byte[] dynamicKeyBytes, int seed)
        {

        }

        public abstract void DeobfuscatePacketFromClient(byte[] packet);
        public abstract void ObfuscatePacketForServer(byte[] packet);
        public abstract void DeobfuscatePacketFromServer(byte[] packet);
        public abstract void ObfuscatePacketForClient(byte[] packet);
    }
}
