using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Network.Cryptography
{
    internal class LegacyInGameCipher : InGameObfuscator
    {
        /// <summary>
        /// Create the keys with the static part hardcoded, it is extracted from the game client.
        /// </summary>
        private byte[] clientKeySend = { 0, 0, 0, 0, 0, 0, 0, 0, 0xc8, 0x27, 0x93, 0x1, 0xa1, 0x6c, 0x31, 0x97 };

        private byte[] clientKeyReceive = { 0, 0, 0, 0, 0, 0, 0, 0, 0xc8, 0x27, 0x93, 0x1, 0xa1, 0x6c, 0x31, 0x97 };
        private byte[] legitKeySend = { 0, 0, 0, 0, 0, 0, 0, 0, 0xc8, 0x27, 0x93, 0x1, 0xa1, 0x6c, 0x31, 0x97 };
        private byte[] legitKeyReceive = { 0, 0, 0, 0, 0, 0, 0, 0, 0xc8, 0x27, 0x93, 0x1, 0xa1, 0x6c, 0x31, 0x97 };

        public byte[] DynamicKeyBytes;

        private IdShuffler idObfuscator;

        public LegacyInGameCipher(byte[] dynamicKeyBytes, int seed) : base(dynamicKeyBytes, seed)
        {
            //The seed for generating the random tables is received at this moment, initialize the id obfuscator with the seed.
            this.idObfuscator = new IdShuffler(seed);

            DynamicKeyBytes = dynamicKeyBytes;

            //Fill in the missing dynamic part of the keys.
            Array.Copy(dynamicKeyBytes, 0, this.clientKeySend, 0, 8);
            Array.Copy(dynamicKeyBytes, 0, this.clientKeyReceive, 0, 8);
            Array.Copy(dynamicKeyBytes, 0, this.legitKeySend, 0, 8);
            Array.Copy(dynamicKeyBytes, 0, this.legitKeyReceive, 0, 8);
        }

        public override void DeobfuscatePacketFromClient(byte[] packet)
        {
            int temp = 0;
            for (int i = 2; i < packet.Length; i++)
            {
                int temp2 = packet[i];
                packet[i] = (byte)(temp2 ^ this.clientKeySend[((i - 2) & 15)] ^ temp);
                temp = temp2;
            }

            //packet[2] = this.idObfuscator.DeobfuscateId(packet[2]);
            //if (packet[2] == (int)0xD0)//H5PacketIds.ClientPrimary.Extended
            //{
            //    char extension = (char)(packet[3] | (char)packet[4] << 8);
            //    char res = this.idObfuscator.DeobfuscateId(extension);
            //    packet[3] = (byte)(res & 0xff);
            //    packet[4] = (byte)(res >> 8);
            //}

            //update key
            long movingPart = (this.clientKeySend[8]) | (this.clientKeySend[9] << 8) | (this.clientKeySend[10] << 16) | (this.clientKeySend[11] << 24);
            movingPart += packet.Length - 2;
            this.clientKeySend[8] = (byte)(movingPart & 0xFF);
            this.clientKeySend[9] = (byte)((movingPart >> 8) & 0xFF);
            this.clientKeySend[10] = (byte)((movingPart >> 16) & 0xFF);
            this.clientKeySend[11] = (byte)((movingPart >> 24) & 0xFF);
        }

        public override void ObfuscatePacketForServer(byte[] packet)
        {
            //packet[2] = this.idObfuscator.ObfuscateId(packet[2]);
            //if (packet[2] == (int)0xD0)//H5PacketIds.ClientPrimary.Extended
            //{
            //    char extension = (char)(packet[3] | (char)packet[4] << 8);
            //    char res = this.idObfuscator.ObfuscateId(extension);
            //    packet[3] = (byte)(res & 0xff);
            //    packet[4] = (byte)(res >> 8);
            //}

            int temp = 0;
            for (int i = 2; i < packet.Length; i++)
            {
                int temp2 = packet[i];
                temp = temp2 ^ (this.legitKeySend[((i - 2) & 15)]) ^ temp;
                packet[i] = (byte)temp;
            }

            long movingPart = (this.legitKeySend[8]) | (this.legitKeySend[9] << 8) | (this.legitKeySend[10] << 16) | (this.legitKeySend[11] << 24);
            movingPart += packet.Length - 2;
            this.legitKeySend[8] = (byte)(movingPart & 0xFF);
            this.legitKeySend[9] = (byte)((movingPart >> 8) & 0xFF);
            this.legitKeySend[10] = (byte)((movingPart >> 16) & 0xFF);
            this.legitKeySend[11] = (byte)((movingPart >> 24) & 0xFF);
        }

        public override void DeobfuscatePacketFromServer(byte[] packet)
        {
            int temp = 0;
            for (int i = 2; i < packet.Length; i++)
            {
                int temp2 = packet[i];
                packet[i] = (byte)(temp2 ^ this.legitKeyReceive[((i - 2) & 15)] ^ temp);
                temp = temp2;
            }

            //update key
            long movingPart = (this.legitKeyReceive[8]) | (this.legitKeyReceive[9] << 8) | (this.legitKeyReceive[10] << 16) | (this.legitKeyReceive[11] << 24);
            movingPart += packet.Length - 2;
            this.legitKeyReceive[8] = (byte)(movingPart & 0xFF);
            this.legitKeyReceive[9] = (byte)((movingPart >> 8) & 0xFF);
            this.legitKeyReceive[10] = (byte)((movingPart >> 16) & 0xFF);
            this.legitKeyReceive[11] = (byte)((movingPart >> 24) & 0xFF);
        }

        public override void ObfuscatePacketForClient(byte[] packet)
        {
            if (packet == null)
            {
                return;
            }

            int temp = 0;
            for (int i = 2; i < packet.Length; i++)
            {
                int temp2 = packet[i];
                temp = temp2 ^ (this.clientKeyReceive[((i - 2) & 15)]) ^ temp;
                packet[i] = (byte)temp;
            }
            long movingPart = (this.clientKeyReceive[8]) | (this.clientKeyReceive[9] << 8) | (this.clientKeyReceive[10] << 16) | (this.clientKeyReceive[11] << 24);
            movingPart += packet.Length - 2;
            this.clientKeyReceive[8] = (byte)(movingPart & 0xFF);
            this.clientKeyReceive[9] = (byte)((movingPart >> 8) & 0xFF);
            this.clientKeyReceive[10] = (byte)((movingPart >> 16) & 0xFF);
            this.clientKeyReceive[11] = (byte)((movingPart >> 24) & 0xFF);
        }
    }
}