using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;

namespace Ronin.Utilities
{
    public class PacketReader
    {
        private MemoryStream data;
        private BinaryReader reader;
        public int Id;
        public int SubId;
        public int Size;

        public PacketReader(byte[] packet, bool fromServer = true)
        {
            this.data = new MemoryStream(packet);
            this.reader = new BinaryReader(this.data);

            //skip the network layer size
            var rawsize = this.reader.ReadUInt16();

            if(rawsize <= 2)
                return;

            this.Id = this.reader.ReadByte();
            if ((H5PacketIds.ServerPrimary)this.Id == H5PacketIds.ServerPrimary.Extended && fromServer)
            {
                this.SubId = this.reader.ReadInt16();
            }

            if (!fromServer && (H5PacketIds.ClientPrimary)this.Id == H5PacketIds.ClientPrimary.Extended)
            {
                this.SubId = this.reader.ReadInt16();
            }

            this.Size = packet.Length;
        }

        public byte ReadByte()
        {
            byte result = this.reader.ReadByte();
            return result;
        }

        public double ReadDouble()
        {
            double result = this.reader.ReadDouble();
            return result;
        }

        public bool ReadBool()
        {
            byte log = this.reader.ReadByte();
            bool result = log == 1 ? true : false;
            return result;
        }

        public short ReadShort()
        {
            short result = this.reader.ReadInt16();
            return result;
        }

        public int ReadInt()
        {
            int result = this.reader.ReadInt32();
            return result;
        }

        public long ReadLong()
        {
            long result = this.reader.ReadInt64();
            return result;
        }

        public string ReadString(int strLength)
        {
            byte[] arr = this.reader.ReadBytes(strLength * 2);
            string result = System.Text.Encoding.Unicode.GetString(arr);
            return result;
        }

        public string ReadString()
        {
            List<byte> log = new List<byte>();
            short res = this.reader.ReadInt16();
            while (res != 0)
            {
                log.Add((byte)(res & 0xff));
                log.Add((byte)(res >> 8));
                res = this.reader.ReadInt16();
            }
            var arr = log.ToArray();
            string result = System.Text.Encoding.Unicode.GetString(arr);
            return result;
        }

        public void SkipBytes(int count)
        {
            //move the cursor position
            this.reader.ReadBytes(count);
        }
    }
}
