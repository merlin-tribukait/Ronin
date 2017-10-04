using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Protocols.HighFive.Incoming
{
    public class Dialog : H5IncomingPacket
    {
        private H5PacketIds.ServerPrimary _id = H5PacketIds.ServerPrimary.ConfirmDialog;

        public Dialog(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int id = reader.ReadInt();
            if (id == 1510) //Ressurection dialog
            {
                int paramCount = reader.ReadByte();
                reader.SkipBytes(3);
                for (int i = 0; i < paramCount; i++)
                {
                    int paramId = reader.ReadInt();
                    switch (paramId)
                    {
                        case 12:
                            data.LastRessurector = reader.ReadString();
                            break;

                        case 6: //long number
                            reader.ReadLong();
                            break;

                        case 1: //int num
                            reader.ReadInt();
                            break;
                        case 0:
                            reader.ReadString();
                            break;
                    }
                }
                reader.ReadInt(); //time to respond
                data.LastDialogToken = reader.ReadInt();
                //LogHelper.GetLogger().Debug(data.LastDialogToken);
                data.PendingRessurectDialog = true;
            }
        }

        public override H5PacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
