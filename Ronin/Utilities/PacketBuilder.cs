using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data.Constants;

namespace Ronin.Utilities
{
    public class PacketBuilder
    {
        private MemoryStream data = new MemoryStream();

        private byte id;

        private short subId;

        private bool idSet = false;

        public void SetId(byte id)
        {
            this.id = id;
            this.idSet = true;
        }

        public void SetSubId(short subId)
        {
            this.subId = subId;
        }

        public void SetSubId(H5PacketIds.ClientSecondary subId)
        {
            this.subId = (short)subId;
        }

        public void SetSubId(H5PacketIds.ServerSecondary subId)
        {
            this.subId = (short)subId;
        }

        public void SetId(H5PacketIds.ClientPrimary id)
        {
            this.id = (byte)id;
            this.idSet = true;
        }

        public void SetId(H5PacketIds.ServerPrimary id)
        {
            this.id = (byte)id;
            this.idSet = true;
        }

        public void Append(int value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            buff.Write(value);
        }

        public void Append(long value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            buff.Write(value);
        }

        public void Append(short value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            buff.Write(value);
        }

        public void Append(byte value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            buff.Write(value);
        }

        public void Append(byte[] value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            buff.Write(value);
        }

        public void Append(bool value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            buff.Write(value);
        }

        public void Append(string value)
        {
            BinaryWriter buff = new BinaryWriter(this.data);
            byte[] arr = Encoding.Unicode.GetBytes(value);
            buff.Write(arr);
            buff.Write((short)0); //null termination
        }

        public byte[] GetBytes()
        {
            if (!idSet)
            {
                throw new Exception("Packet without id is invalid!");
            }

            if (id == 0xD0 && subId == 0)
            {
                throw new Exception("Invalid subId!");
            }

            if (id == 0xFE && subId == 0)
            {
                throw new Exception("Invalid subId!");
            }

            byte[] buffer = new byte[3 + (subId != 0 ? 2 : 0) + this.data.Length];
            short size = (short)buffer.Length;
            int nativeLayerSize = 3;
            buffer[0] = (byte)(size & 0xff);
            buffer[1] = (byte)((size >> 8) & 0xff);
            buffer[2] = this.id;
            if (subId != 0)
            {
                buffer[3] = (byte)(subId & 0xff);
                buffer[4] = (byte)((subId >> 8) & 0xff);
                nativeLayerSize += 2;
            }

            byte[] data = this.data.GetBuffer();
            for (int i = 0; i < this.data.Length; i++)
            {
                buffer[i + nativeLayerSize] = data[i];
            }

            return buffer;
        }
    }
}
