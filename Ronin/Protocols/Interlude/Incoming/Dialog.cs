using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class Dialog : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.Dialog;

        public Dialog(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int id = reader.ReadInt();
            if (id == 1510) //Ressurection dialog
            {
                int paramCount = reader.ReadByte();
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

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}
