using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Network.Cryptography.Rpg_club
{
    public class RpgInGameCipher : InGameObfuscator
    {
        public static List<string> OffsetList = new List<string>();

        public static void Init()
        {
            OffsetList.Add("-13284461858463872225922389526972055736677");//-132844618584638722259223895269720557366
        }

        public byte[] SecretKey = new byte[8];
        private byte[] dynamicKeyBytes;

        public RpgInGameCipher(byte[] dynamicKeyBytes, int seed) : base(dynamicKeyBytes, seed)
        {
            this.dynamicKeyBytes = dynamicKeyBytes;
        }

        public override void DeobfuscatePacketFromClient(byte[] packet)
        {
            byte[] xorKey = new byte[16];
            for (int i = 0; i < 8; i++)
            {
                xorKey[i] = (byte)(SecretKey[i] ^ dynamicKeyBytes[i]);
            }

            for (int i = 2; i < packet.Length; i++)
            {
                packet[i] ^= xorKey[(i - 2)%16];
            }
        }

        public override void ObfuscatePacketForServer(byte[] packet)
        {
            byte[] xorKey = new byte[16];
            for (int i = 0; i < 8; i++)
            {
                xorKey[i] = (byte)(SecretKey[i] ^ dynamicKeyBytes[i]);
            }

            for (int i = 2; i < packet.Length; i++)
            {
                packet[i] ^= xorKey[(i - 2) % 16];
            }
        }

        public override void DeobfuscatePacketFromServer(byte[] packet)
        {
            byte[] xorKey = new byte[16];
            for (int i = 0; i < 8; i++)
            {
                xorKey[i] = (byte)(SecretKey[i] ^ dynamicKeyBytes[i]);
            }

            for (int i = 2; i < packet.Length; i++)
            {
                packet[i] ^= xorKey[(i - 2) % 16];
            }
        }

        public override void ObfuscatePacketForClient(byte[] packet)
        {
            byte[] xorKey = new byte[16];
            for (int i = 0; i < 8; i++)
            {
                xorKey[i] = (byte)(SecretKey[i] ^ dynamicKeyBytes[i]);
            }

            for (int i = 2; i < packet.Length; i++)
            {
                packet[i] ^= xorKey[(i - 2) % 16];
            }
        }
    }
}
