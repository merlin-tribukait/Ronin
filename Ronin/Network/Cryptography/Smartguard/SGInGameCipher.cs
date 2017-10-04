using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ronin.Utilities;

namespace Ronin.Network.Cryptography.SmartGuard
{
    public class SGInGameCipher : InGameObfuscator
    {
        public SGKey clientKeySend = new SGKey();
        public SGKey clientKeyReceive = new SGKey();
        public SGKey legitKeySend = new SGKey();
        public SGKey legitKeyReceive = new SGKey();

        private readonly Mutex mutex1 = new Mutex();
        private readonly Mutex mutex2 = new Mutex();
        private readonly Mutex mutex3 = new Mutex();
        private readonly Mutex mutex4 = new Mutex();

        public SGInGameCipher(byte[] legacyKey, int seed) : base(legacyKey, seed)
        {
        }

        private void crypt(byte[] packet, SGKey key)
        {
            for (int i = 2; i < packet.Length; i++)
            {
                key.var1++;
                key.var1 %= 256;
                key.var2 += key.ContentBytes[key.var1];
                key.var2 %= 256;
                var tmp = key.ContentBytes[key.var1];
                key.ContentBytes[key.var1] = key.ContentBytes[key.var2];
                key.ContentBytes[key.var2] = tmp;
                var index = key.ContentBytes[key.var1] + key.ContentBytes[key.var2];
                index %= 256;
                packet[i] = (byte)(packet[i] ^ key.ContentBytes[index]);
            }
        }

        public void reverseCrypt(byte[] packet, SGKey key)
        {
            for (int i = packet.Length - 1; i > 1; i--)
            {
                var index = key.ContentBytes[key.var1] + key.ContentBytes[key.var2];
                index %= 256;
                packet[i] = (byte)(packet[i] ^ key.ContentBytes[index]);
                var tmp = key.ContentBytes[key.var1];
                key.ContentBytes[key.var1] = key.ContentBytes[key.var2];
                key.ContentBytes[key.var2] = tmp;

                key.var2 -= key.ContentBytes[key.var1];
                key.var2 += key.var2 < 0 ? 256 : 0;

                key.var1--;
                key.var1 += key.var1 < 0 ? 256 : 0;
            }
        }

        public override void DeobfuscatePacketFromClient(byte[] packet)
        {
            mutex1.WaitOne();
            try
            {
                crypt(packet, clientKeySend);
            }
            finally
            {
                mutex1.ReleaseMutex();
            }
        }

        public override void ObfuscatePacketForServer(byte[] packet)
        {
            mutex2.WaitOne();
            try
            {
                crypt(packet, legitKeySend);
            }
            finally
            {
                mutex2.ReleaseMutex();
            }
        }

        public override void DeobfuscatePacketFromServer(byte[] packet)
        {
            mutex3.WaitOne();
            try
            {
                crypt(packet, legitKeyReceive);
            }
            finally
            {
                mutex3.ReleaseMutex();
            }
        }

        public override void ObfuscatePacketForClient(byte[] packet)
        {
            mutex4.WaitOne();
            try
            {
                crypt(packet, clientKeyReceive);
            }
            finally
            {
                mutex4.ReleaseMutex();
            }
        }
    }
}
