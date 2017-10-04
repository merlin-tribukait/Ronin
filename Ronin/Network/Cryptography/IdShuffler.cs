using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Network.Cryptography
{
    internal class IdShuffler
    {
        /// <summary>
        /// The seed for the random number generator given from the server in the initialization packet.
        /// </summary>
        private int _seed;

        private int initialSeed;

        private static int twoByteTableSize = 294;

        private int PseudoRand()
        {
            int a;
            a = (int)((((long)_seed * 0x343fd) + 0x269ec3) & 0xFFFFFFFF);
            _seed = a;
            return (_seed >> 0x10) & 0x7FFF;
        }

        /// <summary>
        /// The table containing the obfuscation values for the single byte id packets. (not containing subids)
        /// </summary>
        private byte[] _oneByteTable = new byte[0xD1];

        /// <summary>
        /// The table containing the obfuscation values for the multi-byte id packets. (containing subids)
        /// Multi-byte id obfuscation is untested!
        /// </summary>
        private char[] _twoByteTable = new char[twoByteTableSize + 1];

        public IdShuffler(int seed)
        {
            _seed = seed;
            initialSeed = seed;
            Init();
        }

        private void Init()
        {
            _seed = initialSeed;
            _twoByteTable = new char[twoByteTableSize + 1];
            //Fill the tables with the index values.
            for (int i = 0; i < _oneByteTable.Length; i++)
            {
                _oneByteTable[i] = (byte)(i);
            }

            for (int i = 0; i < _twoByteTable.Length; i++)
            {
                _twoByteTable[i] = (char)i;
            }

            //Apllying the cryptography algorithm for single byte obfuscation values.
            for (int i = 2; i <= 0xD1; i++)
            {
                int rand_pos = (PseudoRand() % i) + 1;
                int x = _oneByteTable[rand_pos - 1];
                _oneByteTable[rand_pos - 1] = _oneByteTable[i - 1];
                _oneByteTable[i - 1] = (byte)x;
            }

            //Apllying the cryptography algorithm for multi-byte obfuscation values.
            for (int i = 1; i < _twoByteTable.Length; i++)
            {
                int rand_pos = PseudoRand() % (i + 1);
                char tmp = _twoByteTable[rand_pos];
                _twoByteTable[rand_pos] = _twoByteTable[i];
                _twoByteTable[i] = tmp;
            }

            //Some ids don't participate in the obfuscation, so they should be excluded.
            byte[] constants = { 0x12, 0xb1, 0x11, 0xd0 };
            for (int i = 0; i < constants.Length; i++)
            {
                byte constant = constants[i];
                int curPos = 0;
                for (int k = 0; k < 0xD1; k++)
                {
                    if (_oneByteTable[k] == constant) curPos = k;
                }
                byte x1 = _oneByteTable[constant];
                _oneByteTable[constant] = constant;
                _oneByteTable[curPos] = x1;
            }

            int cur_pos = 0;
            char constant2 = (char)0x70;
            for (int i = 0; i < _twoByteTable.Length; i++)
            {
                if (_twoByteTable[i] == constant2) cur_pos = i;
            }
            char x2 = _twoByteTable[constant2];
            _twoByteTable[constant2] = constant2;
            _twoByteTable[cur_pos] = x2;

            constant2 = (char)0x71;
            for (int i = 0; i < _twoByteTable.Length; i++)
            {
                if (_twoByteTable[i] == constant2) cur_pos = i;
            }
            x2 = _twoByteTable[constant2];
            _twoByteTable[constant2] = constant2;
            _twoByteTable[cur_pos] = x2;
        }

        public byte DeobfuscateId(byte obfId)
        {
            byte deobfId = _oneByteTable[obfId];
            return deobfId;
        }

        public byte ObfuscateId(byte deobfId)
        {
            byte obfId = 0;
            for (int i = 0; i < _oneByteTable.Length; i++)
            {
                if (_oneByteTable[i] == deobfId)
                    obfId = (byte)i;
            }
            return obfId;
        }

        public char DeobfuscateId(char obfId)
        {
            if (obfId > twoByteTableSize)
            {
                twoByteTableSize = obfId;
                //MessageBox.Show(twoByteTableSize.ToString());
                Init();
            }
            char deobfId = _twoByteTable[obfId];
            return deobfId;
        }

        public char ObfuscateId(char deobfId)
        {
            char obfId = (char)0;
            for (int i = 0; i < _twoByteTable.Length; i++)
            {
                if (_twoByteTable[i] == deobfId)
                    obfId = (char)i;
            }
            return obfId;
        }
    }
}
