﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Utilities;

namespace Ronin.Protocols.Interlude.Incoming
{
    public class SystemMessage : ILIncomingPacket
    {
        private ILPacketIds.ServerPrimary _id = ILPacketIds.ServerPrimary.SystemMessage;

        public SystemMessage(PacketReader reader, bool fromServer) : base(reader, fromServer)
        {
        }

        public override void Parse(L2PlayerData data)
        {
            int systemId = reader.ReadInt();
            int paramCount = reader.ReadInt();
            switch (systemId)
            {
                case 105:
                    //log.Debug("Sent invite");
                    data.PendingInvite = true;

                    break;
                case 145:
                    //log.Debug("Sent invite");
                    data.PendingInvite = false;

                    break;
                case 107:
                    //log.Debug("Invite accepted");
                    data.PendingInvite = true;

                    break;
                case 612:
                    //log.Debug($"Spoil Activated");

                    break;
                case 1595:
                    //log.Debug($"Skill Land");
                    for (int i = 0; i < paramCount; i++)
                    {
                        int paramType = reader.ReadInt();
                        switch (paramType)
                        {
                            case 4: //TYPE_SKILL_NAME
                                int skillId = reader.ReadInt();
                                int skillLevel = reader.ReadInt();
                                break;
                        }
                    }

                    break;
                case 1597:
                    //log.Debug($"Skill Fail");
                    for (int i = 0; i < paramCount; i++)
                    {
                        int paramType = reader.ReadInt();
                        switch (paramType)
                        {
                            case 4: //TYPE_SKILL_NAME
                                int skillId = reader.ReadInt();
                                int skillLevel = reader.ReadInt();
                                data.LandedSkills.Remove(skillId);
                                break;
                        }
                    }

                    break;

            }
        }

        public override ILPacketIds.ServerPrimary Id
        {
            get { return _id; }
        }
    }
}