using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ClassicUO.Network;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

using Assistant.Core;
using BuffIcon = Assistant.Core.BuffIcon;
using ClassicUO.Game.UI.Gumps;
using UOScript;

namespace Assistant
{
    internal class PacketHandlers
    {
        private static List<UOItem> m_IgnoreGumps = new List<UOItem>();
        internal static List<UOItem> IgnoreGumps { get { return m_IgnoreGumps; } }

        internal static void Initialize()
        {
            //Client -> Server handlers
            PacketHandler.RegisterClientToServerViewer(0x00, new PacketViewerCallback(CreateCharacter));
            //PacketHandler.RegisterClientToServerViewer(0x01, new PacketViewerCallback(Disconnect));
            PacketHandler.RegisterClientToServerViewer(0x02, new PacketViewerCallback(MovementRequest));
            //PacketHandler.RegisterClientToServerFilter(0x05, new PacketFilterCallback(AttackRequest));
            PacketHandler.RegisterClientToServerViewer(0x06, new PacketViewerCallback(ClientDoubleClick));
            PacketHandler.RegisterClientToServerViewer(0x07, new PacketViewerCallback(LiftRequest));
            PacketHandler.RegisterClientToServerViewer(0x08, new PacketViewerCallback(DropRequest));
            PacketHandler.RegisterClientToServerViewer(0x09, new PacketViewerCallback(ClientSingleClick));
            PacketHandler.RegisterClientToServerViewer(0x12, new PacketViewerCallback(ClientTextCommand));
            PacketHandler.RegisterClientToServerViewer(0x13, new PacketViewerCallback(EquipRequest));
            // 0x29 - UOKR confirm drop.  0 bytes payload (just a single byte, 0x29, no length or data)
            PacketHandler.RegisterClientToServerViewer(0x3A, new PacketViewerCallback(SetSkillLock));
            PacketHandler.RegisterClientToServerViewer(0x5D, new PacketViewerCallback(PlayCharacter));
            PacketHandler.RegisterClientToServerViewer(0x7D, new PacketViewerCallback(MenuResponse));
            //PacketHandler.RegisterClientToServerFilter(0x80, new PacketFilterCallback(ServerListLogin));
            //PacketHandler.RegisterClientToServerFilter(0x91, new PacketFilterCallback(GameLogin));
            PacketHandler.RegisterClientToServerViewer(0x95, new PacketViewerCallback(HueResponse));
            //PacketHandler.RegisterClientToServerViewer(0xA0, new PacketViewerCallback(PlayServer));
            PacketHandler.RegisterClientToServerViewer(0xB1, new PacketViewerCallback(ClientGumpResponse));
            PacketHandler.RegisterClientToServerViewer(0xBF, new PacketViewerCallback(ExtendedClientCommand));
            //PacketHandler.RegisterClientToServerViewer( 0xD6, new PacketViewerCallback( BatchQueryProperties ) );
            PacketHandler.RegisterClientToServerViewer(0xC2, new PacketViewerCallback(UnicodePromptSend));
            PacketHandler.RegisterClientToServerViewer(0xD7, new PacketViewerCallback(ClientEncodedPacket));
            PacketHandler.RegisterClientToServerViewer(0xF8, new PacketViewerCallback(CreateCharacter));

            //Server -> Client handlers
            PacketHandler.RegisterServerToClientViewer(0x0B, new PacketViewerCallback(Damage));
            PacketHandler.RegisterServerToClientViewer(0x11, new PacketViewerCallback(MobileStatus));
            PacketHandler.RegisterServerToClientViewer(0x17, new PacketViewerCallback(NewMobileStatus));
            PacketHandler.RegisterServerToClientViewer(0x1A, new PacketViewerCallback(WorldItem));
            PacketHandler.RegisterServerToClientViewer(0x1B, new PacketViewerCallback(LoginConfirm));
            PacketHandler.RegisterServerToClientFilter(0x1C, new PacketFilterCallback(AsciiSpeech));
            PacketHandler.RegisterServerToClientViewer(0x1D, new PacketViewerCallback(RemoveObject));
            PacketHandler.RegisterServerToClientFilter(0x20, new PacketFilterCallback(MobileUpdate));
            PacketHandler.RegisterServerToClientViewer(0x24, new PacketViewerCallback(BeginContainerContent));
            PacketHandler.RegisterServerToClientFilter(0x25, new PacketFilterCallback(ContainerContentUpdate));
            PacketHandler.RegisterServerToClientViewer(0x27, new PacketViewerCallback(LiftReject));
            PacketHandler.RegisterServerToClientViewer(0x2D, new PacketViewerCallback(MobileStatInfo));
            PacketHandler.RegisterServerToClientFilter(0x2E, new PacketFilterCallback(EquipmentUpdate));
            PacketHandler.RegisterServerToClientViewer(0x3A, new PacketViewerCallback(Skills));
            PacketHandler.RegisterServerToClientFilter(0x3C, new PacketFilterCallback(ContainerContent));
            PacketHandler.RegisterServerToClientViewer(0x4E, new PacketViewerCallback(PersonalLight));
            PacketHandler.RegisterServerToClientViewer(0x4F, new PacketViewerCallback(GlobalLight));
            PacketHandler.RegisterServerToClientViewer(0x72, new PacketViewerCallback(ServerSetWarMode));
            PacketHandler.RegisterServerToClientViewer(0x73, new PacketViewerCallback(PingResponse));
            PacketHandler.RegisterServerToClientViewer(0x76, new PacketViewerCallback(ServerChange));
            PacketHandler.RegisterServerToClientFilter(0x77, new PacketFilterCallback(MobileMoving));
            PacketHandler.RegisterServerToClientFilter(0x78, new PacketFilterCallback(MobileIncoming));
            PacketHandler.RegisterServerToClientViewer(0x7C, new PacketViewerCallback(SendMenu));
            //PacketHandler.RegisterServerToClientFilter(0x8C, new PacketFilterCallback(ServerAddress));
            PacketHandler.RegisterServerToClientViewer(0xA1, new PacketViewerCallback(HitsUpdate));
            PacketHandler.RegisterServerToClientViewer(0xA2, new PacketViewerCallback(ManaUpdate));
            PacketHandler.RegisterServerToClientViewer(0xA3, new PacketViewerCallback(StamUpdate));
            //PacketHandler.RegisterServerToClientViewer(0xA8, new PacketViewerCallback(ServerList));
            PacketHandler.RegisterServerToClientViewer(0xAB, new PacketViewerCallback(DisplayStringQuery));
            PacketHandler.RegisterServerToClientViewer(0xAF, new PacketViewerCallback(DeathAnimation));
            PacketHandler.RegisterServerToClientViewer(0xAE, new PacketViewerCallback(UnicodeSpeech));
            PacketHandler.RegisterServerToClientViewer(0xB0, new PacketViewerCallback(UncompressedGump));
            PacketHandler.RegisterServerToClientViewer(0xB9, new PacketViewerCallback(Features));
            PacketHandler.RegisterServerToClientViewer(0xBC, new PacketViewerCallback(ChangeSeason));
            PacketHandler.RegisterServerToClientViewer(0xBE, new PacketViewerCallback(OnAssistVersion));
            PacketHandler.RegisterServerToClientViewer(0xBF, new PacketViewerCallback(ExtendedPacket));
            PacketHandler.RegisterServerToClientViewer(0xC1, new PacketViewerCallback(OnLocalizedMessage));
            PacketHandler.RegisterServerToClientViewer(0xC2, new PacketViewerCallback(UnicodePromptReceived));
            PacketHandler.RegisterServerToClientViewer(0xC8, new PacketViewerCallback(SetUpdateRange));
            PacketHandler.RegisterServerToClientViewer(0xCC, new PacketViewerCallback(OnLocalizedMessageAffix));
            PacketHandler.RegisterServerToClientViewer(0xD6, new PacketViewerCallback(EncodedPacket));//0xD6 "encoded" packets
            PacketHandler.RegisterServerToClientViewer(0xD8, new PacketViewerCallback(CustomHouseInfo));
            //PacketHandler.RegisterServerToClientFilter( 0xDC, new PacketFilterCallback( ServOPLHash ) );
            PacketHandler.RegisterServerToClientViewer(0xDD, new PacketViewerCallback(CompressedGump));
            PacketHandler.RegisterServerToClientViewer(0xF0, new PacketViewerCallback(RunUOProtocolExtention)); // Special RunUO protocol extentions (for KUOC/Razor)

            PacketHandler.RegisterServerToClientViewer(0xF3, new PacketViewerCallback(SAWorldItem));

            PacketHandler.RegisterServerToClientViewer(0x2C, new PacketViewerCallback(ResurrectionGump));

            PacketHandler.RegisterServerToClientViewer(0xDF, new PacketViewerCallback(BuffDebuff));
        }

        private static void OnAssistVersion(Packet p, PacketHandlerEventArgs args)
        {
            args.Block = true;
        }

        private static void DisplayStringQuery(Packet p, PacketHandlerEventArgs args)
        {
            // See also Packets.cs: StringQueryResponse
            /*if ( MacroManager.AcceptActions )
            {
                 int serial = p.ReadInt32();
                 byte type = p.ReadByte();
                 byte index = p.ReadByte();

                 MacroManager.Action( new WaitForTextEntryAction( serial, type, index ) );
            }*/
        }

        private static void SetUpdateRange(Packet p, PacketHandlerEventArgs args)
        {
            UOSObjects.ClientViewRange = p.ReadByte();
        }

        private static void EncodedPacket(Packet p, PacketHandlerEventArgs args)
        {
            ushort id = p.ReadUShort();

            switch ( id )
            {
                case 1: // object property list
                {
                    uint serial = p.ReadUInt();
                    if (SerialHelper.IsItem(serial))
                    {
                        UOItem item = UOSObjects.FindItem( serial );
                        if ( item == null )
                            UOSObjects.AddItem( item=new UOItem( serial ) );

                        item.ReadPropertyList( p, out string name );
                        if (!string.IsNullOrEmpty(name))
                            item.Name = name;
                        if ( item.ModifiedOPL )
                        {
                            args.Block = true;
                            Engine.Instance.SendToClient( item.ObjPropList.BuildPacket() );
                        }
                    }
                    else if (SerialHelper.IsMobile(serial))
                    {
                        UOMobile m = UOSObjects.FindMobile( serial );
                        if ( m == null )
                            UOSObjects.AddMobile( m=new UOMobile( serial ) );

                        m.ReadPropertyList( p, out _ );
                        if ( m.ModifiedOPL )
                        {
                            args.Block = true;
                            Engine.Instance.SendToClient( m.ObjPropList.BuildPacket() );
                        }
                    }
                    break;
                }
            }
        }

        private static void ServOPLHash(Packet p, PacketHandlerEventArgs args)
        {
            uint s = p.ReadUInt();
            uint hash = p.ReadUInt();

            if ( SerialHelper.IsItem(s) )
            {
                 UOItem item = UOSObjects.FindItem( s );
                 if ( item != null && item.OPLHash != hash )
                 {
                      item.OPLHash = hash;
                      p.Seek(p.Position - 4);
                      p.WriteUInt(item.OPLHash);
                }
            }
            else if ( SerialHelper.IsMobile(s) )
            {
                 UOMobile m = UOSObjects.FindMobile( s );
                 if ( m != null && m.OPLHash != hash )
                 {
                      m.OPLHash = hash;
                      p.Seek( p.Position - 4 );
                      p.WriteUInt( m.OPLHash );
                 }
            }
        }

        private static void ClientSingleClick(Packet p, PacketHandlerEventArgs args)
        {
            uint ser = p.ReadUInt();

            UOMobile m = UOSObjects.FindMobile(ser);

            if (m == null)
                return;

            // if you modify this, don't forget to modify the allnames hotkey
            //if (Config.GetBool("LastTargTextFlags"))
            Targeting.CheckTextFlags(m);

            if (FriendsManager.IsFriend(m.Serial))//Config.GetBool("ShowFriendOverhead")
            {
                m.OverheadMessage(63, "[Friend]");
            }
        }

        private static void ClientDoubleClick(Packet p, PacketHandlerEventArgs args)
        {
            uint ser = p.ReadUInt();
            if (UOSObjects.Gump.PreventDismount && UOSObjects.Player != null && ser == UOSObjects.Player.Serial && UOSObjects.Player.Warmode && UOSObjects.Player.GetItemOnLayer(Layer.Mount) != null)
            { // mount layer = 0x19
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "Dismount Blocked");
                args.Block = true;
                return;
            }

            if (UOSObjects.Gump.UseObjectsQueue)
                args.Block = !PlayerData.DoubleClick(ser, false);
            if (SerialHelper.IsItem(ser) && World.Player != null)
                UOSObjects.Player.LastObject = ser;

            if (ScriptManager.Recording)
            {
                ushort gfx = 0;
                bool world = false;
                uint cont = 0;
                if (SerialHelper.IsItem(ser))
                {
                    UOItem i = UOSObjects.FindItem(ser);
                    if (i != null)
                    {
                        gfx = i.ItemID;
                        if (i.RootContainer != null)
                        {
                            if (i.RootContainer is UOEntity ent)
                                cont = ent.Serial;
                            else if (i.RootContainer is uint cser)
                                cont = cser;
                        }
                        else
                            world = true;
                    }
                }
                else if(SerialHelper.IsMobile(ser))
                {
                    UOMobile m = UOSObjects.FindMobile(ser);
                    if (m != null)
                        gfx = m.Body;
                    world = true;
                }

                if (gfx != 0)
                {
                    if (ScriptManager.Recording)
                    {
                        if (UOSObjects.Gump.RecordTypeUse)//usetype (graphic) [color] [source] [range]
                        {
                            ScriptManager.AddToScript(string.Format("usetype 0x{0:X4} -1{1}", gfx, world ? " 'world'" : (cont > 0 ? $" 0x{cont:X8}" : "")));
                        }
                        else
                            ScriptManager.AddToScript($"useobject 0x{ser:X}");
                    }
                }
            }
        }

        private static HashSet<UOMobile> _RecentlyDead = new HashSet<UOMobile>();
        private static void DeathAnimation(Packet p, PacketHandlerEventArgs args)
        {
            UOMobile m = UOSObjects.FindMobile(p.ReadUInt());
            //TODO: take snapshot of own kill and other kill
            if (m != null)
            {
                if (_RecentlyDead.Contains(m))
                    return;
                _RecentlyDead.Add(m);
                Timer.DelayedCallbackState(TimeSpan.FromMilliseconds(150), OnAfterMobileDeath, m).Start();
            }
        }
        
        private static void OnAfterMobileDeath(UOMobile m)
        {
            if (m.IsGhost && ((m == UOSObjects.Player && UOSObjects.Gump.SnapOwnDeath) || UOSObjects.Gump.SnapOtherDeath))
            {
                UOSObjects.SnapShot();
            }
            _RecentlyDead.Remove(m);
        }

        private static void ExtendedClientCommand(Packet p, PacketHandlerEventArgs args)
        {
            ushort ext = p.ReadUShort();
            switch (ext)
            {
                case 0x10: // query object properties
                {
                    break;
                }
                case 0x15: // context menu response
                {
                    if (ScriptManager.Recording)
                    {
                        UOEntity ent = null;
                        uint ser = p.ReadUInt();
                        ushort idx = p.ReadUShort();

                        if (SerialHelper.IsMobile(ser))
                            ent = UOSObjects.FindMobile(ser);
                        else if (SerialHelper.IsItem(ser))
                            ent = UOSObjects.FindItem(ser);

                        if (ent != null && ent.ContextMenu != null)
                        {
                            ScriptManager.AddToScript($"contextmenu {(ser == World.Player.Serial ? "'self'" : $"0x{ser:X}")} {idx}");
                        }
                    }
                    break;
                }
                case 0x1C:// cast spell
                {
                    uint ser = uint.MaxValue;
                    if (p.ReadUShort() == 1)
                        ser = p.ReadUInt();
                    ushort sid = p.ReadUShort();
                    Spell s = Spell.Get(sid);
                    if (s != null)
                    {
                        s.OnCast(p);
                        args.Block = true;
                        if (ScriptManager.Recording)
                            ScriptManager.AddToScript($"cast '{Spell.GetName(sid)}'");
                    }
                    break;
                }
            }
        }

        private static void ClientTextCommand(Packet p, PacketHandlerEventArgs args)
        {
            int type = p.ReadByte();
            string command = p.ReadASCII();
            if (UOSObjects.Player != null && !string.IsNullOrEmpty(command))
            {
                switch (type)
                {
                    case 0x24: // Use skill
                    {
                        if (!int.TryParse(command.Split(' ')[0], out int skillIndex) || skillIndex >= SkillsLoader.Instance.SkillsCount)
                            break;

                        UOSObjects.Player.LastSkill = skillIndex;
                        if (ScriptManager.Recording)
                            ScriptManager.AddToScript($"useskill '{SkillsLoader.Instance.Skills[skillIndex].Name}'");
                        if (skillIndex == (int)SkillName.Stealth && !UOSObjects.Player.Visible)
                            StealthSteps.Hide();
                        SkillTimer.Start();
                        break;
                    }
                    case 0x27: // Cast spell from book
                    {
                        string[] split = command.Split(' ');
                        if (split.Length > 0)
                        {
                            if (ushort.TryParse(split[0], out ushort spellID))
                            {
                                uint serial = 0;
                                if (split.Length > 1)
                                    serial = Utility.ToUInt32(split[1], uint.MaxValue);
                                Spell s = Spell.Get(spellID);
                                if (s != null)
                                {
                                    s.OnCast(p);
                                    args.Block = true;
                                    if (ScriptManager.Recording)
                                    {
                                        if (SerialHelper.IsValid(serial))
                                            ScriptManager.AddToScript($"cast '{Spell.GetName(spellID)}' 0x{serial:X}");
                                        else
                                            ScriptManager.AddToScript($"cast '{Spell.GetName(spellID)}'");
                                    }
                                }
                            }
                        }

                        break;
                    }

                    case 0x56: // Cast spell from macro
                    {
                        if(ushort.TryParse(command, out ushort spellID))
                        {
                            Spell s = Spell.Get(spellID);
                            if (s != null)
                            {
                                s.OnCast(p);
                                args.Block = true;
                                if (ScriptManager.Recording)
                                    ScriptManager.AddToScript($"cast '{Spell.GetName(spellID)}'");
                            }
                        }

                        break;
                    }
                }
            }
        }

        internal static DateTime PlayCharTime = DateTime.MinValue;

        private static void CreateCharacter(Packet p, PacketHandlerEventArgs args)
        {
            p.Seek(1 + 4 + 4 + 1); // skip begining crap
            UOSObjects.OrigPlayerName = p.ReadASCII(30);

            PlayCharTime = DateTime.UtcNow;
        }

        private static void PlayCharacter(Packet p, PacketHandlerEventArgs args)
        {
            p.ReadUInt(); //0xedededed
            UOSObjects.OrigPlayerName = p.ReadASCII(30);

            PlayCharTime = DateTime.UtcNow;
        }

        /*private static void ServerList(Packet p, PacketHandlerEventArgs args)
        {
            p.ReadByte(); //unknown
            ushort numServers = p.ReadUShort();

            for (int i = 0; i < numServers; ++i)
            {
                ushort num = p.ReadUShort();
                UOSObjects.Servers[num] = p.ReadString(32);
                p.ReadByte(); // full %
                p.ReadSByte(); // time zone
                p.ReadUInt(); // ip
            }
        }*/

        /*private static void PlayServer(Packet p, PacketHandlerEventArgs args)
        {
            ushort index = p.ReadUShort();
            string name;
            if (UOSObjects.Servers.TryGetValue(index, out name) && !string.IsNullOrEmpty(name))
                UOSObjects.ShardName = name;
            else
                UOSObjects.ShardName = "[Unknown]";
        }*/

        private static object _ParentLifted;
        private static void LiftRequest(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();
            ushort amount = p.ReadUShort();

            UOItem item = UOSObjects.FindItem(serial);
            _ParentLifted = item?.Container ?? null;

            if (UOSObjects.Gump.UseObjectsQueue)
            {
                if (item == null)
                {
                    UOSObjects.AddItem(item = new UOItem(serial));
                    item.Amount = amount;
                }
                DragDropManager.Drag(item, amount, true);
                args.Block = true;
            }
        }

        private static void LiftReject(Packet p, PacketHandlerEventArgs args)
        {
            int reason = p.ReadByte();
            if (!DragDropManager.LiftReject())
                args.Block = true;
            _ParentLifted = null;
            //MacroManager.PlayError( MacroError.LiftRej );
        }

        private static void EquipRequest(Packet p, PacketHandlerEventArgs args)
        {
            uint iser = p.ReadUInt(); // item being dropped serial
            Layer layer = (Layer)p.ReadByte();
            uint mser = p.ReadUInt();//mobile dropped to

            UOItem item = UOSObjects.FindItem(iser);

            if (layer == Layer.Invalid || layer > Layer.LastValid)
            {
                if (item != null)
                {
                    layer = item.Layer;
                    if (layer == Layer.Invalid || layer > Layer.LastValid)
                        layer = (Layer)item.TileDataInfo.Layer;
                }
            }

            if (item == null)
                return;

            UOMobile m = UOSObjects.FindMobile(mser);
            if (m == null)
                return;
            if (UOSObjects.Gump.UseObjectsQueue)
                args.Block = DragDropManager.Drop(item, m, layer);
            if (ScriptManager.Recording && layer > Layer.Invalid && layer <= Layer.LastUserValid)
                ScriptManager.AddToScript($"equipitem {mser} {layer}");
            _ParentLifted = null;
        }

        private static void DropRequest(Packet p, PacketHandlerEventArgs args)
        {
            uint iser = p.ReadUInt();
            int x = (short)p.ReadUShort();
            int y = (short)p.ReadUShort();
            int z = p.ReadSByte();

            UOItem i = UOSObjects.FindItem(iser);
            if (i == null)
                return;
            if (Engine.UsePostKRPackets)
                i.GridNum = p.ReadByte();
            uint dser = p.ReadUInt();

            UOItem dest = UOSObjects.FindItem(dser);
            if (dest != null && dest.IsContainer && UOSObjects.Player != null && (dest.IsChildOf(UOSObjects.Player.Backpack) || dest.IsChildOf(UOSObjects.Player.Quiver)))
                i.IsNew = true;
            if (UOSObjects.Gump.UseObjectsQueue)
                args.Block = DragDropManager.Drop(i, dser, new Point3D(x, y, z));

            if (ScriptManager.Recording)
            {
                if (UOSObjects.Gump.RecordTypeUse)
                {
                    //movetype (graphic) (source) (destination) [(x y z)] [color] [amount] [range]
                    string source, destination;
                    if (_ParentLifted is uint ui)
                        source = $"0x{ui:X}";
                    else if (_ParentLifted is UOEntity ent)
                        source = $"0x{ent.Serial:X}";
                    else
                        source = "'world'";
                    if (dser == uint.MaxValue || dser == 0)
                        destination = "'ground'";
                    else
                        destination = $"0x{dser:X}";
                    if (destination[0] == '0')
                    {
                        ScriptManager.AddToScript($"movetype 0x{i.Graphic:X} {source} {destination} {x} {y} {z} -1 {i.Amount}");
                    }
                    else
                    {
                        x -= UOSObjects.Player.Position.X;
                        y -= UOSObjects.Player.Position.Y;
                        z -= UOSObjects.Player.Position.Z;
                        ScriptManager.AddToScript($"movetypeoffset 0x{i.Graphic:X} {source} {destination} {x} {y} {z} -1 {i.Amount}");
                    }
                }
                else ////moveitem (serial) (destination) [(x y z)] [amount]
                {
                    string destination;
                    if (dser == uint.MaxValue || dser == 0)
                        destination = "'ground'";
                    else
                        destination = $"0x{dser:X}";
                    ScriptManager.AddToScript($"moveitem 0x{iser:X} {destination} {x} {y} {z} {i.Amount}");
                }
            }
            _ParentLifted = null;
        }

        private static void MovementRequest(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player != null)
            {
                Direction dir = (Direction)p.ReadByte();
                byte seq = p.ReadByte();

                UOSObjects.Player.Direction = (dir & Direction.Up);
                if (ScriptManager.Recording)
                    ScriptManager.AddToScript($"walk '{dir}'");
            }
        }

        private static void ContainerContentUpdate(Packet p, PacketHandlerEventArgs args)
        {
            // This function will ignore the item if the container item has not been sent to the client yet.
            // We can do this because we can't really count on getting all of the container info anyway.
            // (So we'd need to request the container be updated, so why bother with the extra stuff required to find the container once its been sent?)
            uint serial = p.ReadUInt();
            ushort itemid = p.ReadUShort();
            itemid = (ushort)(itemid + p.ReadSByte()); // signed, itemID offset
            ushort amount = p.ReadUShort();
            if (amount == 0)
                amount = 1;
            Point3D pos = new Point3D(p.ReadUShort(), p.ReadUShort(), 0);
            byte gridPos = 0;
            if (Engine.UsePostKRPackets)
                gridPos = p.ReadByte();
            uint cser = p.ReadUInt();
            ushort hue = p.ReadUShort();

            UOItem i = UOSObjects.FindItem(serial);
            if (i == null)
            {
                if (!SerialHelper.IsItem(serial))
                    return;

                UOSObjects.AddItem(i = new UOItem(serial));
                i.IsNew = i.AutoStack = true;
            }
            else
            {
                i.CancelRemove();
            }
            if (serial != DragDropManager.Pending)
            {
                if (!DragDropManager.EndHolding(serial))
                    return;
            }

            i.ItemID = itemid;
            i.Amount = amount;
            i.Position = pos;
            i.GridNum = gridPos;
            i.Hue = hue;
            //TODO: SearchException
            /*if (SearchExemptionAgent.Contains(i))
            {
                p.Seek(p.Position - 2);
                p.Write((short)Config.GetInt("ExemptColor"));
            }*/

            i.Container = cser;
            if (i.IsNew)
                UOItem.UpdateContainers();
        }

        private static void BeginContainerContent(Packet p, PacketHandlerEventArgs args)
        {
            uint ser = p.ReadUInt();
            if (!SerialHelper.IsItem(ser))
                return;
            UOItem item = UOSObjects.FindItem(ser);
            if (item != null)
            {
                if (m_IgnoreGumps.Contains(item))
                {
                    m_IgnoreGumps.Remove(item);
                    args.Block = true;
                }
            }
            else
            {
                UOSObjects.AddItem(new UOItem(ser));
                UOItem.UpdateContainers();
            }
        }
        private static void ContainerContent(Packet p, PacketHandlerEventArgs args)
        {
            int count = p.ReadUShort();
            for (int i = 0; i < count; i++)
            {
                uint serial = p.ReadUInt();
                // serial is purposely not checked to be valid, sometimes buy lists dont have "valid" item serials (and we are okay with that).
                UOItem item = UOSObjects.FindItem(serial);
                if (item == null)
                {
                    UOSObjects.AddItem(item = new UOItem(serial));
                    item.IsNew = true;
                    item.AutoStack = false;
                }
                else
                {
                    item.CancelRemove();
                }
                if (!DragDropManager.EndHolding(serial))
                    continue;

                item.ItemID = p.ReadUShort();
                item.ItemID = (ushort)(item.ItemID + p.ReadSByte());// signed, itemID offset
                item.Amount = p.ReadUShort();
                if (item.Amount == 0)
                    item.Amount = 1;
                item.Position = new Point3D(p.ReadUShort(), p.ReadUShort(), 0);
                if (Engine.UsePostKRPackets)
                    item.GridNum = p.ReadByte();
                uint cont = p.ReadUInt();
                item.Hue = p.ReadUShort();
                //TODO: SearchException + Counters
                /*
                if (SearchExemptionAgent.Contains(item))
                {
                    p.Seek(p.Position - 2);
                    p.Write((short)Config.GetInt("ExemptColor"));
                }*/

                item.Container = cont; // must be done after hue is set (for counters)
            }
            UOItem.UpdateContainers();
        }

        private static void EquipmentUpdate(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();

            UOItem i = UOSObjects.FindItem(serial);
            bool isNew = false;
            if (i == null)
            {
                UOSObjects.AddItem(i = new UOItem(serial));
                isNew = true;
                UOItem.UpdateContainers();
            }
            else
            {
                i.CancelRemove();
            }
            if (!DragDropManager.EndHolding(serial))
                return;

            ushort iid = p.ReadUShort();
            i.ItemID = (ushort)(iid + p.ReadSByte()); // signed, itemID offset
            i.Layer = (Layer)p.ReadByte();
            uint ser = p.ReadUInt();// cont must be set after hue (for counters)
            i.Hue = p.ReadUShort();

            i.Container = ser;

            int ltHue = UOSObjects.Gump.HLTargetHue;
            if (ltHue != 0 && Targeting.IsLastTarget(i.Container as UOMobile))
            {
                p.Seek(p.Position - 2);
                p.WriteUShort((ushort)(ltHue & 0x3FFF));
            }

            if (i.Layer == Layer.Backpack && isNew && UOSObjects.Gump.AutoSearchContainers && ser == UOSObjects.Player.Serial)
            {
                m_IgnoreGumps.Add(i);
                PlayerData.DoubleClick(i.Serial);
            }
        }

        private static void SetSkillLock(Packet p, PacketHandlerEventArgs args)
        {
            int i = p.ReadUShort();

            if (i >= 0 && i < Skill.Count)
            {
                Skill skill = UOSObjects.Player.Skills[i];

                skill.Lock = (LockType)p.ReadByte();
            }
        }

        private static void Skills(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null || UOSObjects.Player.Skills == null)
                return;
            byte type = p.ReadByte();

            switch (type)
            {
                case 0x02://list (with caps, 3.0.8 and up)
                {
                    int i;
                    while ((i = p.ReadUShort()) > 0)
                    {
                        if (i > 0 && i <= Skill.Count)
                        {
                            Skill skill = UOSObjects.Player.Skills[i - 1];

                            if (skill == null)
                                continue;

                            skill.FixedValue = p.ReadUShort();
                            skill.FixedBase = p.ReadUShort();
                            skill.Lock = (LockType)p.ReadByte();
                            skill.FixedCap = p.ReadUShort();
                            if (!UOSObjects.Player.SkillsSent)
                                skill.Delta = 0;
                        }
                        else
                        {
                            p.Seek(p.Position + 7);
                        }
                    }

                    UOSObjects.Player.SkillsSent = true;
                    break;
                }

                case 0x00: // list (without caps, older clients)
                {
                    int i;
                    while ((i = p.ReadUShort()) > 0)
                    {
                        if (i > 0 && i <= Skill.Count)
                        {
                            Skill skill = UOSObjects.Player.Skills[i - 1];

                            if (skill == null)
                                continue;

                            skill.FixedValue = p.ReadUShort();
                            skill.FixedBase = p.ReadUShort();
                            skill.Lock = (LockType)p.ReadByte();
                            skill.FixedCap = 100;//p.ReadUShort();
                            if (!UOSObjects.Player.SkillsSent)
                                skill.Delta = 0;
                        }
                        else
                        {
                            p.Seek(p.Position + 5);
                        }
                    }

                    UOSObjects.Player.SkillsSent = true;
                    break;
                }

                case 0xDF: //change (with cap, new clients)
                {
                    int i = p.ReadUShort();

                    if (i >= 0 && i < Skill.Count)
                    {
                        Skill skill = UOSObjects.Player.Skills[i];

                        if (skill == null)
                            break;

                        ushort old = skill.FixedBase;
                        skill.FixedValue = p.ReadUShort();
                        skill.FixedBase = p.ReadUShort();
                        skill.Lock = (LockType)p.ReadByte();
                        skill.FixedCap = p.ReadUShort();
                    }
                    break;
                }

                case 0xFF: //change (without cap, older clients)
                {
                    int i = p.ReadUShort();

                    if (i >= 0 && i < Skill.Count)
                    {
                        Skill skill = UOSObjects.Player.Skills[i];

                        if (skill == null)
                            break;

                        ushort old = skill.FixedBase;
                        skill.FixedValue = p.ReadUShort();
                        skill.FixedBase = p.ReadUShort();
                        skill.Lock = (LockType)p.ReadByte();
                        skill.FixedCap = 100;
                    }
                    break;
                }
            }
        }

        private static void LoginConfirm(Packet p, PacketHandlerEventArgs args)
        {
            UOSObjects.Items.Clear();
            UOSObjects.Mobiles.Clear();

            UseNewStatus = false;

            uint serial = p.ReadUInt();

            PlayerData m = new PlayerData(serial)
            {
                Name = UOSObjects.OrigPlayerName
            };

            UOMobile test = UOSObjects.FindMobile(serial);
            if (test != null)
                test.Remove();

            UOSObjects.AddMobile(UOSObjects.Player = m);
            //Config.LoadProfileFor(UOSObjects.Player);

            p.ReadUInt(); // always 0?
            m.Body = p.ReadUShort();
            m.Position = new Point3D(p.ReadUShort(), p.ReadUShort(), (short)p.ReadUShort());
            m.Direction = (Direction)p.ReadByte();

            //TODO: check if this is really needed
            //Engine.Instance.SetPosition((uint)m.Position.X, (uint)m.Position.Y, (uint)m.Position.Z, m.Direction);

            if (UOSObjects.Player != null)
                UOSObjects.Player.SetSeason();
        }

        private static void MobileMoving(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();
            UOMobile m = UOSObjects.FindMobile(serial);

            if(m == null)
            {
                UOSObjects.AddMobile(m = new UOMobile(serial));
                UOSObjects.RequestMobileStatus(m);
            }

            if (m != null)
            {
                m.Body = p.ReadUShort();
                m.Position = new Point3D(p.ReadUShort(), p.ReadUShort(), p.ReadSByte());

                if (UOSObjects.Player != null && !Utility.InRange(UOSObjects.Player.Position, m.Position, UOSObjects.Player.VisRange))
                {
                    m.Remove();
                    return;
                }

                Targeting.CheckLastTargetRange(m);

                m.Direction = (Direction)p.ReadByte();
                m.Hue = p.ReadUShort();
                int ltHue = UOSObjects.Gump.HLTargetHue;
                if (ltHue != 0 && Targeting.IsLastTarget(m))
                {
                    p.Seek(p.Position - 2);
                    p.WriteUShort((ushort)(ltHue | 0x8000));
                }

                bool wasPoisoned = m.Poisoned;
                m.ProcessPacketFlags(p.ReadByte());
                byte oldNoto = m.Notoriety;
                m.Notoriety = p.ReadByte();

                if (m == UOSObjects.Player)
                {
                    //TODO: check if this is really needed
                    //Engine.Instance.SetPosition((uint)m.Position.X, (uint)m.Position.Y, (uint)m.Position.Z, (byte)m.Direction);
                }
            }
        }

        private static readonly int[] HealthHues = new int[] { 428, 333, 37, 44, 49, 53, 158, 263, 368, 473, 578 };

        private static void HitsUpdate(Packet p, PacketHandlerEventArgs args)
        {
            //present in CUO, no need to replicate it
            /*UOMobile m = UOSObjects.FindMobile(p.ReadUInt());

            if (m != null)
            {
                int oldPercent = (int)(m.Hits * 100 / (m.HitsMax == 0 ? (ushort)1 : m.HitsMax));

                m.HitsMax = p.ReadUShort();
                m.Hits = p.ReadUShort();

                if (Client.Instance.AllowBit(FeatureBit.OverheadHealth) && Config.GetBool("ShowHealth"))
                {
                    int percent = (int)(m.Hits * 100 / (m.HitsMax == 0 ? (ushort)1 : m.HitsMax));

                    // Limit to people who are on screen and check the previous value so we dont get spammed.
                    if (oldPercent != percent && UOSObjects.Player != null && Utility.Distance(UOSObjects.Player.Position, m.Position) <= 12)
                    {
                        try
                        {
                            m.OverheadMessageFrom(HealthHues[((percent + 5) / 10) % HealthHues.Length],
                                 m.Name ?? string.Empty,
                                 Config.GetString("HealthFmt"), percent);
                        }
                        catch
                        {
                        }
                    }
                }
            }*/
        }

        private static void StamUpdate(Packet p, PacketHandlerEventArgs args)
        {
            //present in CUO, no need to replicate it
            /*UOMobile m = UOSObjects.FindMobile(p.ReadUInt());

            if (m != null)
            {
                int oldPercent = (int)(m.Stam * 100 / (m.StamMax == 0 ? (ushort)1 : m.StamMax));

                m.StamMax = p.ReadUShort();
                m.Stam = p.ReadUShort();

                if (m == UOSObjects.Player)
                {
                    Client.Instance.RequestTitlebarUpdate();
                    UOAssist.PostStamUpdate();
                }

                if (m != UOSObjects.Player && Client.Instance.AllowBit(FeatureBit.OverheadHealth) && Config.GetBool("ShowPartyStats"))
                {
                    int stamPercent = (int)(m.Stam * 100 / (m.StamMax == 0 ? (ushort)1 : m.StamMax));
                    int manaPercent = (int)(m.Mana * 100 / (m.ManaMax == 0 ? (ushort)1 : m.ManaMax));

                    // Limit to people who are on screen and check the previous value so we dont get spammed.
                    if (oldPercent != stamPercent && UOSObjects.Player != null && Utility.Distance(UOSObjects.Player.Position, m.Position) <= 12)
                    {
                        try
                        {
                            m.OverheadMessageFrom(0x63,
                                 m.Name ?? string.Empty,
                                 Config.GetString("PartyStatFmt"), manaPercent, stamPercent);
                        }
                        catch
                        {
                        }
                    }
                }
            }*/
        }

        private static void ManaUpdate(Packet p, PacketHandlerEventArgs args)
        {
            //present in CUO, no need to replicate it
            /*UOMobile m = UOSObjects.FindMobile(p.ReadUInt());

            if (m != null)
            {
                int oldPercent = (int)(m.Mana * 100 / (m.ManaMax == 0 ? (ushort)1 : m.ManaMax));

                m.ManaMax = p.ReadUShort();
                m.Mana = p.ReadUShort();

                if (m == UOSObjects.Player)
                {
                    Client.Instance.RequestTitlebarUpdate();
                    UOAssist.PostManaUpdate();
                }

                if (m != UOSObjects.Player && Client.Instance.AllowBit(FeatureBit.OverheadHealth) && Config.GetBool("ShowPartyStats"))
                {
                    int stamPercent = (int)(m.Stam * 100 / (m.StamMax == 0 ? (ushort)1 : m.StamMax));
                    int manaPercent = (int)(m.Mana * 100 / (m.ManaMax == 0 ? (ushort)1 : m.ManaMax));

                    // Limit to people who are on screen and check the previous value so we dont get spammed.
                    if (oldPercent != manaPercent && UOSObjects.Player != null && Utility.Distance(UOSObjects.Player.Position, m.Position) <= 12)
                    {
                        try
                        {
                            m.OverheadMessageFrom(0x63,
                                 Language.Format(LocString.sStatsA1, m.Name),
                                 Config.GetString("PartyStatFmt"), manaPercent, stamPercent);
                        }
                        catch
                        {
                        }
                    }
                }
            }*/
        }

        private static void MobileStatInfo(Packet pvSrc, PacketHandlerEventArgs args)
        {
            UOMobile m = UOSObjects.FindMobile(pvSrc.ReadUInt());
            if (m == null)
                return;

            m.HitsMax = pvSrc.ReadUShort();
            m.Hits = pvSrc.ReadUShort();

            m.ManaMax = pvSrc.ReadUShort();
            m.Mana = pvSrc.ReadUShort();

            m.StamMax = pvSrc.ReadUShort();
            m.Stam = pvSrc.ReadUShort();
        }

        internal static bool UseNewStatus = false;

        private static void NewMobileStatus(Packet p, PacketHandlerEventArgs args)
        {
            UOMobile m = UOSObjects.FindMobile(p.ReadUInt());

            if (m == null)
                return;

            UseNewStatus = true;

            // 00 01
            p.ReadUShort();

            // 00 01 Poison
            // 00 02 Yellow Health Bar

            ushort id = p.ReadUShort();

            // 00 Off
            // 01 On
            // For Poison: Poison Level + 1

            byte flag = p.ReadByte();

            if (id == 1)
            {
                bool wasPoisoned = m.Poisoned;
                m.Poisoned = (flag != 0);

                /*if (m == UOSObjects.Player && wasPoisoned != m.Poisoned)
                    Client.Instance.RequestTitlebarUpdate();*/
            }
        }

        private static void Damage(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();
            ushort damage = p.ReadUShort();

            //not available on UOS, sincerely, I don't think we need this on CUO
            /*if (Config.GetBool("ShowDamageTaken") || Config.GetBool("ShowDamageDealt"))
            {
                UOMobile m = UOSObjects.FindMobile(serial);

                if (m == null)
                    return;

                if (serial == UOSObjects.Player.Serial && Config.GetBool("ShowDamageTaken"))
                {
                    if (Config.GetBool("ShowDamageTakenOverhead"))
                    {
                        UOSObjects.Player.OverheadMessage(38, $"[{damage}]", true);
                    }
                    else
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Force, $"{damage} dmg->{m.Name}");
                    }
                }

                if (Config.GetBool("ShowDamageDealt"))
                {
                    if (Config.GetBool("ShowDamageDealtOverhead"))
                    {
                        m.OverheadMessageFrom(38, m.Name, $"[{damage}]", true);
                    }
                    else
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Force, $"{UOSObjects.Player.Name}->{m.Name}: {damage} damage");
                    }
                }
            }*/

            //todo: implement this ASAP 
            /*if (DamageTracker.Running)
                DamageTracker.AddDamage(serial, damage);*/
        }

        private static void MobileStatus(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();
            UOMobile m = UOSObjects.FindMobile(serial);
            if (m == null)
                UOSObjects.AddMobile(m = new UOMobile(serial));

            m.Name = p.ReadASCII(30);

            m.Hits = p.ReadUShort();
            m.HitsMax = p.ReadUShort();

            //p.ReadBoolean();//CanBeRenamed
            if (p.ReadBool())
                m.CanRename = true;

            byte type = p.ReadByte();

            if (m == UOSObjects.Player && type != 0x00)
            {
                PlayerData player = (PlayerData)m;

                player.Female = p.ReadBool();

                int oStr = player.Str, oDex = player.Dex, oInt = player.Int;

                player.Str = p.ReadUShort();
                player.Dex = p.ReadUShort();
                player.Int = p.ReadUShort();

                //Your strength has changed by {0}{1}, it is now {2}.
                //no need to implement this on CUO
                /*if (player.Str != oStr && oStr != 0 && Config.GetBool("DisplaySkillChanges"))
                {
                    if (Config.GetBool("DisplaySkillChangesOverhead"))
                    {
                        UOSObjects.Player.OverheadMessage(LocString.StrChangeOverhead, player.Str - oStr > 0 ? "+" : "", player.Str - oStr, player.Str);
                    }
                    else
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Force, LocString.StrChanged, player.Str - oStr > 0 ? "+" : "", player.Str - oStr, player.Str);
                    }
                }

                if (player.Dex != oDex && oDex != 0 && Config.GetBool("DisplaySkillChanges"))
                {
                    if (Config.GetBool("DisplaySkillChangesOverhead"))
                    {
                        UOSObjects.Player.OverheadMessage(LocString.DexChangeOverhead, player.Dex - oDex > 0 ? "+" : "", player.Dex - oDex, player.Dex);
                    }
                    else
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Force, LocString.DexChanged, player.Dex - oDex > 0 ? "+" : "", player.Dex - oDex, player.Dex);
                    }
                }

                if (player.Int != oInt && oInt != 0 && Config.GetBool("DisplaySkillChanges"))
                {
                    if (Config.GetBool("DisplaySkillChangesOverhead"))
                    {
                        UOSObjects.Player.OverheadMessage(LocString.IntChangeOverhead, player.Int - oInt > 0 ? "+" : "", player.Int - oInt, player.Int);
                    }
                    else
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Force, LocString.IntChanged, player.Int - oInt > 0 ? "+" : "", player.Int - oInt, player.Int);
                    }
                }*/

                player.Stam = p.ReadUShort();
                player.StamMax = p.ReadUShort();
                player.Mana = p.ReadUShort();
                player.ManaMax = p.ReadUShort();

                player.Gold = p.ReadUInt();
                player.AR = p.ReadUShort(); // ar / physical resist
                player.Weight = p.ReadUShort();

                if (type >= 0x03)
                {
                    if (type > 0x04)
                    {
                        player.MaxWeight = p.ReadUShort();

                        p.ReadByte(); // race?
                    }

                    player.StatCap = p.ReadUShort();

                    player.Followers = p.ReadByte();
                    player.FollowersMax = p.ReadByte();

                    if (type > 0x03)
                    {
                        player.FireResistance = (short)p.ReadUShort();
                        player.ColdResistance = (short)p.ReadUShort();
                        player.PoisonResistance = (short)p.ReadUShort();
                        player.EnergyResistance = (short)p.ReadUShort();

                        player.Luck = (short)p.ReadUShort();

                        player.DamageMin = p.ReadUShort();
                        player.DamageMin = p.ReadUShort();
                        player.DamageMax = p.ReadUShort();

                        player.Tithe = (int)p.ReadUInt();
                    }
                }
            }
        }

        private static void MobileUpdate(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            uint serial = p.ReadUInt();
            UOMobile m = UOSObjects.FindMobile(serial);
            if (m == null)
                UOSObjects.AddMobile(m = new UOMobile(serial));

            bool wasHidden = !m.Visible;

            m.Body = (ushort)(p.ReadUShort() + p.ReadSByte());
            m.Hue = p.ReadUShort();
            int ltHue = UOSObjects.Gump.HLTargetHue;
            if (ltHue != 0 && Targeting.IsLastTarget(m))
            {
                p.Seek(p.Position - 2);
                p.WriteUShort((ushort)(ltHue | 0x8000));
            }

            bool wasPoisoned = m.Poisoned;
            m.ProcessPacketFlags(p.ReadByte());

            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            p.ReadUShort(); //always 0?
            m.Direction = (Direction)p.ReadByte();
            m.Position = new Point3D(x, y, p.ReadSByte());

            if (m == UOSObjects.Player)
            {
                //Engine.Instance.SetPosition((uint)m.Position.X, (uint)m.Position.Y, (uint)m.Position.Z, (byte)m.Direction);

                if (!wasHidden && !m.Visible)
                {
                    if (UOSObjects.Gump.CountStealthSteps)
                        StealthSteps.Hide();
                }
                else if (wasHidden && m.Visible)
                {
                    StealthSteps.Unhide();
                }
            }

            UOItem.UpdateContainers();
        }

        private static void MobileIncoming(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            uint serial = p.ReadUInt();
            ushort body = p.ReadUShort();

            Point3D position = new Point3D(p.ReadUShort(), p.ReadUShort(), p.ReadSByte());

            UOMobile m = UOSObjects.FindMobile(serial);
            if (m == null)
                UOSObjects.AddMobile(m = new UOMobile(serial));

            bool wasHidden = !m.Visible;
            
            if (UOSObjects.Gump.ShowMobileFlags)
                Targeting.CheckTextFlags(m);

            int ltHue = UOSObjects.Gump.HLTargetHue;
            bool isLT;
            if (ltHue != 0)
                isLT = Targeting.IsLastTarget(m);
            else
                isLT = false;

            m.Body = body;
            if (m != UOSObjects.Player)
                m.Position = position;
            m.Direction = (Direction)p.ReadByte();
            m.Hue = p.ReadUShort();
            if (isLT)
            {
                p.Seek(p.Position - 2);
                p.WriteUShort((ushort)(ltHue | 0x8000));
            }

            bool wasPoisoned = m.Poisoned;
            m.ProcessPacketFlags(p.ReadByte());
            byte oldNoto = m.Notoriety;
            m.Notoriety = p.ReadByte();

            if (m == UOSObjects.Player)
            {
                if (!wasHidden && !m.Visible)
                {
                    if (UOSObjects.Gump.CountStealthSteps)
                        StealthSteps.Hide();
                }
                else if (wasHidden && m.Visible)
                {
                    StealthSteps.Unhide();
                }
            }

            while (true)
            {
                serial = p.ReadUInt();
                if (!SerialHelper.IsItem(serial))
                    break;

                UOItem item = UOSObjects.FindItem(serial);
                bool isNew = false;
                if (item == null)
                {
                    isNew = true;
                    UOSObjects.AddItem(item = new UOItem(serial));
                }
                if (!DragDropManager.EndHolding(serial))
                    continue;

                item.Container = m;

                ushort id = p.ReadUShort();

                if (Engine.UseNewMobileIncoming)
                    item.ItemID = (ushort)(id & 0xFFFF);
                else if (Engine.UsePostSAChanges)
                    item.ItemID = (ushort)(id & 0x7FFF);
                else
                    item.ItemID = (ushort)(id & 0x3FFF);

                item.Layer = (Layer)p.ReadByte();

                if (Engine.UseNewMobileIncoming)
                {
                    item.Hue = p.ReadUShort();
                    if (isLT)
                    {
                        p.Seek(p.Position - 2);
                        p.WriteUShort((ushort)(ltHue & 0x3FFF));
                    }
                }
                else
                {
                    if ((id & 0x8000) != 0)
                    {
                        item.Hue = p.ReadUShort();
                        if (isLT)
                        {
                            p.Seek(p.Position - 2);
                            p.WriteUShort((ushort)(ltHue & 0x3FFF));
                        }
                    }
                    else
                    {
                        item.Hue = 0;
                        if (isLT)
                            Engine.Instance.SendToClient(new EquipmentItem(item, (ushort)(ltHue & 0x3FFF), m.Serial));
                    }
                }

                if (item.Layer == Layer.Backpack && isNew && UOSObjects.Gump.AutoSearchContainers && m == UOSObjects.Player && m != null)
                {
                    m_IgnoreGumps.Add(item);
                    PlayerData.DoubleClick(item.Serial);
                }
            }

            UOItem.UpdateContainers();
        }

        private static void RemoveObject(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();

            if (SerialHelper.IsMobile(serial))
            {
                UOMobile m = UOSObjects.FindMobile(serial);
                if (m != null && m != UOSObjects.Player)
                    m.Remove();
            }
            else if (SerialHelper.IsItem(serial))
            {
                UOItem i = UOSObjects.FindItem(serial);
                if (i != null)
                {
                    if (DragDropManager.Holding == i)
                    {
                        i.Container = null;
                    }
                    else
                        i.RemoveRequest();
                }
            }
        }

        private static void ServerChange(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player != null)
                UOSObjects.Player.Position = new Point3D(p.ReadUShort(), p.ReadUShort(), (short)p.ReadUShort());
        }

        private static void WorldItem(Packet p, PacketHandlerEventArgs args)
        {
            UOItem item;
            uint serial = p.ReadUInt();
            item = UOSObjects.FindItem(serial & 0x7FFFFFFF);
            bool isNew = false;
            if (item == null)
            {
                UOSObjects.AddItem(item = new UOItem(serial & 0x7FFFFFFF));
                isNew = true;
            }
            else
            {
                item.CancelRemove();
            }
            if (!DragDropManager.EndHolding(serial))
                return;

            item.Container = null;

            ushort itemID = p.ReadUShort();
            item.ItemID = (ushort)(itemID & 0x7FFF);

            if ((serial & 0x80000000) != 0)
                item.Amount = p.ReadUShort();
            else
                item.Amount = 1;

            if ((itemID & 0x8000) != 0)
                item.ItemID = (ushort)(item.ItemID + p.ReadSByte());

            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();

            if ((x & 0x8000) != 0)
                item.Direction = p.ReadByte();
            else
                item.Direction = 0;

            short z = p.ReadSByte();

            item.Position = new Point3D(x & 0x7FFF, y & 0x3FFF, z);

            if ((y & 0x8000) != 0)
                item.Hue = p.ReadUShort();
            else
                item.Hue = 0;

            byte flags = 0;
            if ((y & 0x4000) != 0)
                flags = p.ReadByte();

            item.ProcessPacketFlags(flags);

            if (isNew && UOSObjects.Player != null)
            {
                if (item.ItemID == 0x2006)// corpse itemid = 0x2006
                {
                    if (UOSObjects.Gump.ShowCorpseNames)
                        Engine.Instance.SendToServer(new SingleClick(item));

                    if (UOSObjects.Gump.OpenCorpses && Utility.InRange(item.Position, UOSObjects.Player.Position, UOSObjects.Gump.OpenCorpsesRange) && UOSObjects.Player != null && UOSObjects.Player.Visible)
                    {
                        PlayerData.DoubleClick(item.Serial);
                    }
                }
                else if (!item.IsMulti)
                {
                    int dist = Utility.Distance(item.GetWorldPosition(), UOSObjects.Player.Position);
                    if (!UOSObjects.Player.IsGhost && UOSObjects.Player.Visible && dist <= 2 && Scavenger.Enabled && item.Movable)
                        Scavenger.Scavenge(item);
                }
            }

            UOItem.UpdateContainers();

            //convert static walls?! we really need this?
            /*if ()
                args.Block = WallStaticFilter.MakeWallStatic(item);*/
        }

        private static void SAWorldItem(Packet p, PacketHandlerEventArgs args)
        {
            /*
            New World UOItem Packet
            PacketID: 0xF3
            PacketLen: 24
            Format:

                 BYTE - 0xF3 packetId
                 WORD - 0x01
                 BYTE - ArtDataID: 0x00 if the item uses art from TileData table, 0x02 if the item uses art from MultiData table)
                 DWORD - item Serial
                 WORD - item ID
                 BYTE - item direction (same as old)
                 WORD - amount
                 WORD - amount
                 WORD - X
                 WORD - Y
                 SBYTE - Z
                 BYTE - item light
                 WORD - item Hue
                 BYTE - item flags (same as old packet)
            */

            // Post-7.0.9.0
            /*
            New World UOItem Packet
            PacketID: 0xF3
            PacketLen: 26
            Format:

                 BYTE - 0xF3 packetId
                 WORD - 0x01
                 BYTE - ArtDataID: 0x00 if the item uses art from TileData table, 0x02 if the item uses art from MultiData table)
                 DWORD - item Serial
                 WORD - item ID
                 BYTE - item direction (same as old)
                 WORD - amount
                 WORD - amount
                 WORD - X
                 WORD - Y
                 SBYTE - Z
                 BYTE - item light
                 WORD - item Hue
                 BYTE - item flags (same as old packet)
                 WORD ???
            */

            ushort _unk1 = p.ReadUShort();

            byte _artDataID = p.ReadByte();

            UOItem item;
            uint serial = p.ReadUInt();
            item = UOSObjects.FindItem(serial);
            bool isNew = false;
            if (item == null)
            {
                UOSObjects.AddItem(item = new UOItem(serial));
                isNew = true;
            }
            else
            {
                item.CancelRemove();
            }
            if (!DragDropManager.EndHolding(serial))
                return;

            item.Container = null;

            ushort itemID = p.ReadUShort();
            item.ItemID = (ushort)(_artDataID == 0x02 ? itemID | 0x4000 : itemID);

            item.Direction = p.ReadByte();

            ushort _amount = p.ReadUShort();
            item.Amount = _amount = p.ReadUShort();

            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            short z = p.ReadSByte();

            item.Position = new Point3D(x, y, z);

            byte _light = p.ReadByte();

            item.Hue = p.ReadUShort();

            byte flags = p.ReadByte();

            item.ProcessPacketFlags(flags);

            if (Engine.UsePostHSChanges)
            {
                p.ReadUShort();
            }

            if (isNew && UOSObjects.Player != null)
            {
                if (item.ItemID == 0x2006)// corpse itemid = 0x2006
                {
                    if (UOSObjects.Gump.ShowCorpseNames)
                        Engine.Instance.SendToServer(new SingleClick(item));
                    if (UOSObjects.Gump.OpenCorpses && Utility.InRange(item.Position, UOSObjects.Player.Position, UOSObjects.Gump.OpenCorpsesRange) && UOSObjects.Player != null && UOSObjects.Player.Visible)
                        PlayerData.DoubleClick(item.Serial);
                }
                else if (!item.IsMulti)
                {
                    int dist = Utility.Distance(item.GetWorldPosition(), UOSObjects.Player.Position);
                    if (!UOSObjects.Player.IsGhost && UOSObjects.Player.Visible && dist <= 2 && Scavenger.Enabled && item.Movable)
                        Scavenger.Scavenge(item);
                }
            }

            UOItem.UpdateContainers();

            //show static walls ?! we need this on CUO?!
            /*if (Config.GetBool("ShowStaticWalls"))
                args.Block = WallStaticFilter.MakeWallStatic(item);*/
        }

        internal static void HandleSpeech(Packet p, PacketHandlerEventArgs args, uint ser, ushort body, MessageType type, ushort hue, ushort font, string lang, string name, string text)
        {
            if (UOSObjects.Player == null)
                return;

            if (SerialHelper.IsMobile(ser) && type == MessageType.Label)
            {
                UOMobile m = UOSObjects.FindMobile(ser);
                if (m != null /*&& ( m.Name == null || m.Name == "" || m.Name == "(Not Seen)" )*/&& m.Name.IndexOf(text) != 5 && m != UOSObjects.Player && !(text.StartsWith("(") && text.EndsWith(")")))
                    m.Name = text;
            }
            else
            {
                if (ser == uint.MaxValue && name == "System")
                {
                    if (text.StartsWith("You've committed a criminal act") || text.StartsWith("You are now a criminal"))
                    {
                        UOSObjects.Player.ResetCriminalTimer();
                    }
                }

                //is this mess really needed?
                /*if (Config.GetBool("ShowContainerLabels") && ser.IsItem)
                {
                    UOItem item = UOSObjects.FindItem(ser);

                    if (item == null || !item.IsContainer)
                        return;

                    foreach (ContainerLabels.ContainerLabel label in ContainerLabels.ContainerLabelList)
                    {
                        // Check if its the serial match and if the text matches the name (since we override that for the label)
                        if (Serial.Parse(label.Id) == ser && (item.DisplayName.Equals(text) || label.Alias.Equals(text, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            string labelDisplay = $"{Config.GetString("ContainerLabelFormat").Replace("{label}", label.Label).Replace("{type}", text)}";

                            //ContainerLabelStyle
                            if (Config.GetInt("ContainerLabelStyle") == 0)
                            {
                                Client.Instance.SendToClient(new AsciiMessage(ser, item.ItemID.Value, MessageType.Label, label.Hue, 3, Language.CliLocName, labelDisplay));

                            }
                            else
                            {
                                Client.Instance.SendToClient(new UnicodeMessage(ser, item.ItemID.Value, MessageType.Label, label.Hue, 3, Language.CliLocName, "", labelDisplay));
                            }

                            // block the actual message from coming through since we have it in the label
                            args.Block = true;

                            ContainerLabels.LastContainerLabelDisplayed = ser;

                            break;
                        }
                    }
                }*/

                if ((type == MessageType.Emote || type == MessageType.Regular || type == MessageType.Whisper || type == MessageType.Yell) && SerialHelper.IsValid(ser))
                {
                    /*if (SerialHelper.IsMobile(ser) && IgnoreAgent.IsIgnored(ser))
                    {
                        args.Block = true;
                        return;
                    }

                    if (Config.GetBool("ForceSpeechHue"))
                    {
                        p.Seek(10, SeekOrigin.Begin);
                        p.Write((ushort)Config.GetInt("SpeechHue"));
                    }*/
                    Journal.AddLine($"{name}: {text}", type);
                }
                else if (!SerialHelper.IsValid(ser))
                {
                    Journal.AddLine(text, MessageType.System);
                }
            }
        }

        internal static void AsciiSpeech(Packet p, PacketHandlerEventArgs args)
        {
            // 0, 1, 2
            uint serial = p.ReadUInt(); // 3, 4, 5, 6
            ushort body = p.ReadUShort(); // 7, 8
            MessageType type = (MessageType)p.ReadByte(); // 9
            ushort hue = p.ReadUShort(); // 10, 11
            ushort font = p.ReadUShort();
            string name = p.ReadASCII(30);
            string text = p.ReadASCII();

            if (UOSObjects.Player != null && serial == 0 && body == 0 && type == MessageType.Regular && hue == 0xFFFF && font == 0xFFFF && name == "SYSTEM")
            {
                return;
            }
            HandleSpeech(p, args, serial, body, type, hue, font, "A", name, text);

            if (!SerialHelper.IsValid(serial))
            {
                BandageTimer.OnAsciiMessage(text);
            }
            GateTimer.OnAsciiMessage(text);
        }

        internal static void UnicodeSpeech(Packet p, PacketHandlerEventArgs args)
        {
            // 0, 1, 2
            uint serial = p.ReadUInt(); // 3, 4, 5, 6
            ushort body = p.ReadUShort(); // 7, 8
            MessageType type = (MessageType)p.ReadByte(); // 9
            ushort hue = p.ReadUShort(); // 10, 11
            ushort font = p.ReadUShort();
            string lang = p.ReadASCII(4);
            string name = p.ReadASCII(30);
            string text = p.ReadUnicode();

            HandleSpeech(p, args, serial, body, type, hue, font, lang, name, text);
            if (!SerialHelper.IsValid(serial))
            {
                BandageTimer.OnAsciiMessage(text);
            }
        }

        private static void OnLocalizedMessage(Packet p, PacketHandlerEventArgs args)
        {
            // 0, 1, 2
            uint serial = p.ReadUInt(); // 3, 4, 5, 6
            ushort body = p.ReadUShort(); // 7, 8
            MessageType type = (MessageType)p.ReadByte(); // 9
            ushort hue = p.ReadUShort(); // 10, 11
            ushort font = p.ReadUShort();
            int num = (int)p.ReadUInt();
            string name = p.ReadASCII(30);
            string ext_str = p.ReadUnicodeReversed();

            if ((num >= 3002011 && num < 3002011 + 64) || // reg spells
                 (num >= 1060509 && num < 1060509 + 16) || // necro
                 (num >= 1060585 && num < 1060585 + 10) || // chiv
                 (num >= 1060493 && num < 1060493 + 10) || // chiv
                 (num >= 1060595 && num < 1060595 + 6) || // bush
                 (num >= 1060610 && num < 1060610 + 8)) // ninj
            {
                type = MessageType.Spell;
            }
            BandageTimer.OnLocalizedMessage(num);
 
            string text = ClilocLoader.Instance.Translate(num, ext_str);// Language.ClilocFormat(num, ext_str);
            if (text == null)
                return;
            HandleSpeech(p, args, serial, body, type, hue, font, "ENU", name, text);
        }

        private static void OnLocalizedMessageAffix(Packet p, PacketHandlerEventArgs phea)
        {
            // 0, 1, 2
            uint serial = p.ReadUInt(); // 3, 4, 5, 6
            ushort body = p.ReadUShort(); // 7, 8
            MessageType type = (MessageType)p.ReadByte(); // 9
            ushort hue = p.ReadUShort(); // 10, 11
            ushort font = p.ReadUShort();
            int num = (int)p.ReadUInt();
            byte affixType = p.ReadByte();
            string name = p.ReadASCII(30);
            string affix = p.ReadASCII();
            string args = p.ReadUnicode();

            if ((num >= 3002011 && num < 3002011 + 64) || // reg spells
                 (num >= 1060509 && num < 1060509 + 16) || // necro
                 (num >= 1060585 && num < 1060585 + 10) || // chiv
                 (num >= 1060493 && num < 1060493 + 10) || // chiv
                 (num >= 1060595 && num < 1060595 + 6) || // bush
                 (num >= 1060610 && num < 1060610 + 8)     // ninj
                 )
            {
                type = MessageType.Spell;
            }

            string text;
            if ((affixType & 1) != 0) // prepend
                text = String.Format("{0}{1}", affix, ClilocLoader.Instance.Translate(num, args));
            else // 0 == append, 2 = system
                text = String.Format("{0}{1}", ClilocLoader.Instance.Translate(num, args), affix);
            HandleSpeech(p, phea, serial, body, type, hue, font, "ENU", name, text);
        }

        private static void ClientGumpResponse(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            uint ser = p.ReadUInt();
            uint tid = p.ReadUInt();
            int bid = (int)p.ReadUInt();

            if(UOSObjects.Player.OpenedGumps.TryGetValue(tid, out var list))
                list.Remove(list.First(g => g.ServerID == ser));

            if (!ScriptManager.Recording)
                return;

            int sc = (int)p.ReadUInt();
            if (sc < 0 || sc > 2000)
                return;
            //int[] switches = new int[sc];
            for (int i = 0; i < sc; i++, AssistantGump._InstanceSB.Append(' '))
                AssistantGump._InstanceSB.Append(p.ReadUInt());

            /*int ec = (int)p.ReadUInt();
            if (ec < 0 || ec > 2000)
                return;
            GumpTextEntry[] entries = new GumpTextEntry[ec];
            for (int x = 0; x < ec; x++)
            {
                ushort id = p.ReadUShort();
                ushort len = p.ReadUShort();
                string text = p.ReadUnicode(len);
                if (len >= 240)
                    text.Remove(240, text.Length - 240);
                entries[x] = new GumpTextEntry(id, text);
            }*/
            ScriptManager.AddToScript($"waitforgump 0x{tid:X} 15000");
            ScriptManager.AddToScript($"replygump 0x{tid:X} {bid} {AssistantGump._InstanceSB}");
            AssistantGump._InstanceSB.Clear();
            //MacroActions are non present in UOS
            //UOSObjects.Player.LastGumpResponseAction = new GumpResponseAction(bid, switches, entries);
        }

        private static void ChangeSeason(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player != null)
            {
                byte season = p.ReadByte();
                UOSObjects.Player.SetSeason(season);
            }

        }

        private static void ExtendedPacket(Packet p, PacketHandlerEventArgs args)
        {
            ushort type = p.ReadUShort();

            switch (type)
            {
                case 0x04: // close gump
                {
                    uint ser = p.ReadUInt();
                    // int serial, int tid
                    UOSObjects.Player.OpenedGumps.Remove(ser);
                    break;
                }
                case 0x06: // party messages
                {
                    OnPartyMessage(p, args);
                    break;
                }
                case 0x08: // map change
                {
                    if (UOSObjects.Player != null)
                        UOSObjects.Player.Map = p.ReadByte();
                    break;
                }
                case 0x14: // context menu
                {
                    p.ReadUInt(); // 0x01
                    UOEntity ent = null;
                    uint ser = p.ReadUInt();
                    if (SerialHelper.IsMobile(ser))
                        ent = UOSObjects.FindMobile(ser);
                    else if (SerialHelper.IsItem(ser))
                        ent = UOSObjects.FindItem(ser);

                    if (ent != null)
                    {
                        byte count = p.ReadByte();

                        try
                        {
                            ent.ContextMenu.Clear();

                            for (int i = 0; i < count; i++)
                            {
                                ushort idx = p.ReadUShort();
                                ushort num = p.ReadUShort();
                                ushort flags = p.ReadUShort();
                                ushort color = 0;

                                if ((flags & 0x02) != 0)
                                    color = p.ReadUShort();

                                ent.ContextMenu.Add(idx, num);
                            }
                        }
                        catch
                        {
                        }
                    }
                    break;
                }
                /*case 0x18: // map patches
                {
                    if (UOSObjects.Player != null)
                    {
                        int count = (int)p.ReadUInt() * 2;
                        try
                        {
                            UOSObjects.Player.MapPatches = new int[count];
                            for (int i = 0; i < count; i++)
                                UOSObjects.Player.MapPatches[i] = (int)p.ReadUInt();
                        }
                        catch
                        {
                        }
                    }
                    break;
                }*/
                case 0x19: //  stat locks
                {
                    if (p.ReadByte() == 0x02)
                    {
                        UOMobile m = UOSObjects.FindMobile(p.ReadUInt());
                        if (UOSObjects.Player == m && m != null)
                        {
                            p.ReadByte();// 0?

                            byte locks = p.ReadByte();

                            UOSObjects.Player.StrLock = (LockType)((locks >> 4) & 3);
                            UOSObjects.Player.DexLock = (LockType)((locks >> 2) & 3);
                            UOSObjects.Player.IntLock = (LockType)(locks & 3);
                        }
                    }
                    break;
                }
                /*case 0x1D: // Custom House "General Info"
                {
                    //only really used on packetsave, we don't need it on UOS+CUO, and latest razor packetsave don't actually work, so it's bloatware
                    UOItem i = UOSObjects.FindItem(p.ReadUInt());
                    if (i != null)
                        i.HouseRevision = (int)p.ReadUInt();
                    break;
                }*/
            }
        }

        internal static int SpecialPartySent = 0;
        internal static int SpecialPartyReceived = 0;
        internal static int SpecialFactionSent = 0;
        internal static int SpecialFactionReceived = 0;
 
        private static void RunUOProtocolExtention(Packet p, PacketHandlerEventArgs args)
        {
            //this is currently handled in ClassicUO
            //args.Block = true;

            switch (p.ReadByte())
            {
                case 1: // Custom Party information
                {
                    uint serial;

                    SpecialPartyReceived++;

                    while ((serial = p.ReadUInt()) > 0)
                    {
                        if (!Party.Contains(serial))
                            break;//Party.Add(serial);
                        UOMobile mobile = UOSObjects.FindMobile(serial);

                        short x = (short)p.ReadUShort();
                        short y = (short)p.ReadUShort();
                        byte map = p.ReadByte();

                        if (mobile == null)
                        {
                            UOSObjects.AddMobile(mobile = new UOMobile(serial));
                            mobile.Visible = false;
                        }

                        if (mobile.Name == null || mobile.Name.Length <= 0)
                            mobile.Name = "(Not Seen)";

                        if (map == UOSObjects.Player.Map)
                            mobile.Position = new Point3D(x, y, mobile.Position.Z);
                        else
                            mobile.Position = Point3D.Zero;
                    }
                    break;
                }
                case 2: // Faction track information...
                {
                    bool locations = p.ReadByte() != 0;
                    uint serial;
                    SpecialFactionReceived++;
                    if (!locations)
                    {
                        Faction.Clear();
                    }

                    while ((serial = p.ReadUInt()) > 0)
                    {
                        UOMobile mobile = UOSObjects.FindMobile(serial);
                        if (mobile == null)
                        {
                            UOSObjects.AddMobile(mobile = new UOMobile(serial));
                            mobile.Visible = false;
                        }
                        if (!locations || !Faction.Contains(serial) && (!UOSObjects.Gump.FriendsParty || !PacketHandlers.Party.Contains(serial)))
                        {
                            Faction.Add(serial);
                        }

                        if (locations)
                        {
                            short x = (short)p.ReadUShort();
                            short y = (short)p.ReadUShort();
                            byte map = p.ReadByte();
                            byte hits = p.ReadByte();
                            if (map == UOSObjects.Player.Map)
                            {
                                mobile.Position = new Point3D(x, y, mobile.Position.Z);
                            }
                            else
                            {
                                mobile.Position = Point3D.Zero;
                            }
                        }
                        if (string.IsNullOrEmpty(mobile.Name))
                        {
                            mobile.Name = "(Not Seen)";
                        }
                    }
                    break;
                }
                /*case 0xF0:
                {
                    if (UOSObjects.Player != null)
                    {
                        UOSObjects.Player.Position = new Point3D((int)p.ReadUInt(), (int)p.ReadUInt(), (int)p.ReadUInt());
                    }
                    break;
                }*/
                case 0xFE: // Begin Handshake/Features Negotiation
                {
                    ulong features = p.ReadULong();
                    Engine.Instance.SetFeatures(features);
                    //Engine.Instance.SendToServer(new PRazorAnswer());
                    //args.Block = true;
                    break;
                }
            }
        }

        internal static List<uint> Party { get; } = new List<uint>();
        internal static HashSet<uint> Faction { get; } = new HashSet<uint>();
        private static Timer m_PartyDeclineTimer = null;
        internal static uint PartyLeader = 0;

        private static void OnPartyMessage(Packet p, PacketHandlerEventArgs args)
        {
            switch (p.ReadByte())
            {
                case 0x01: // List
                {
                    Party.Clear();

                    int count = p.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        uint s = p.ReadUInt();
                        if (UOSObjects.Player == null || s != UOSObjects.Player.Serial)
                            Party.Add(s);
                    }

                    break;
                }
                case 0x02: // Remove Member/Re-list
                {
                    Party.Clear();
                    int count = p.ReadByte();
                    uint remSerial = p.ReadUInt(); // the serial of who was removed

                    if (UOSObjects.Player != null)
                    {
                        UOMobile rem = UOSObjects.FindMobile(remSerial);
                        if (rem != null && !Utility.InRange(UOSObjects.Player.Position, rem.Position, UOSObjects.Player.VisRange))
                            rem.Remove();
                    }

                    for (int i = 0; i < count; i++)
                    {
                        uint s = p.ReadUInt();
                        if (UOSObjects.Player == null || s != UOSObjects.Player.Serial)
                            Party.Add(s);
                    }

                    break;
                }
                case 0x03: // text message

                case 0x04: // 3 = private, 4 = public
                {
                    //Serial from = p.ReadUInt();
                    //string text = p.ReadUnicodeStringSafe();
                    break;
                }
                case 0x07: // party invite
                {
                    //Serial leader = p.ReadUInt();
                    PartyLeader = p.ReadUInt();

                    //in UOS we can't auto-accept party
                    /*if (Config.GetBool("BlockPartyInvites"))
                    {
                        Client.Instance.SendToServer(new DeclineParty(PacketHandlers.PartyLeader));
                    }*/

                    if (UOSObjects.Gump.AutoAcceptParty)
                    {
                        UOMobile leaderMobile = UOSObjects.FindMobile(PartyLeader);
                        if (leaderMobile != null && UOSObjects.Gump.IsFriend(leaderMobile.Serial))
                        {
                            if (PartyLeader != 0)
                            {
                                UOSObjects.Player.SendMessage($"Auto accepted party invite from: {leaderMobile.Name}");

                                Engine.Instance.SendToServer(new AcceptParty(PartyLeader));
                                PartyLeader = 0;
                            }
                        }
                    }
                    else
                    {
                        if (m_PartyDeclineTimer == null)
                            m_PartyDeclineTimer = Timer.DelayedCallback(TimeSpan.FromSeconds(10.0), new TimerCallback(PartyAutoDecline));
                        m_PartyDeclineTimer.Start();
                    }

                    break;
                }
            }
        }

        private static void PartyAutoDecline()
        {
            PartyLeader = 0;
        }

        private static void PingResponse(Packet p, PacketHandlerEventArgs args)
        {
            if (Ping.Response(p.ReadByte()))
                args.Block = true;
        }

        private static void ClientEncodedPacket(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();
            ushort packetID = p.ReadUShort();
            switch (packetID)
            {
                case 0x19: // set ability
                {
                    uint ability = 0;
                    if (p.ReadByte() == 0)
                        ability = p.ReadUInt();

                    /*if (ability >= 0 && ability < (int)Ability.Invalid)
                        ScriptManager.AddToScript($"setability '{ability}'");*/
                    break;
                }
            }
        }

        private static void MenuResponse(Packet pvSrc, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            uint serial = pvSrc.ReadUInt();
            ushort menuID = pvSrc.ReadUShort();
            ushort index = pvSrc.ReadUShort();
            ushort itemID = pvSrc.ReadUShort();
            ushort hue = pvSrc.ReadUShort();

            UOSObjects.Player.HasMenu = false;
            if (ScriptManager.Recording)
                ScriptManager.AddToScript($"menuresponse {index} {itemID} {hue}");
        }

        private static void SendMenu(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            UOSObjects.Player.CurrentMenuS = p.ReadUInt();
            UOSObjects.Player.CurrentMenuI = p.ReadUShort();
            UOSObjects.Player.HasMenu = true;
            if (ScriptManager.Recording)
            {
                ScriptManager.AddToScript($"replymenu {UOSObjects.Player.CurrentMenuI}");
                args.Block = true;
            }
        }

        private static void HueResponse(Packet p, PacketHandlerEventArgs args)
        {
            uint serial = p.ReadUInt();
            ushort iid = p.ReadUShort();
            ushort hue = p.ReadUShort();

            if (serial == uint.MaxValue)
            {
                //TODO: HueEntry - Callback
                //HueEntry.Callback?.Invoke(hue);
                //args.Block = true;
            }
        }

        private static void Features(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player != null)
                UOSObjects.Player.Features = p.ReadUShort();
        }

        private static void PersonalLight(Packet p, PacketHandlerEventArgs args)
        {
            /*if (UOSObjects.Player != null && !args.Block)
            {
                p.ReadUInt(); // serial

                UOSObjects.Player.LocalLightLevel = p.ReadSByte();

                if (EnforceLightLevels(UOSObjects.Player.LocalLightLevel))
                    args.Block = true;
            }*/
        }

        private static void GlobalLight(Packet p, PacketHandlerEventArgs args)
        {
            /*if (UOSObjects.Player != null && !args.Block)
            {
                UOSObjects.Player.GlobalLightLevel = p.ReadByte();

                if (EnforceLightLevels(UOSObjects.Player.GlobalLightLevel))
                    args.Block = true;
            }*/
        }

        private static bool EnforceLightLevels(int lightLevel)
        {
            /*if (Config.GetBool("MinMaxLightLevelEnabled"))
            {
                // 0 bright, 30 is dark

                if (lightLevel < Config.GetInt("MaxLightLevel"))
                {
                    lightLevel = Convert.ToByte(Config.GetInt("MaxLightLevel")); // light level is too light
                }
                else if (lightLevel > Config.GetInt("MinLightLevel")) // light level is too dark
                {
                    lightLevel = Convert.ToByte(Config.GetInt("MinLightLevel"));
                }
                else // No need to block or do anything special
                {
                    return false;
                }

                UOSObjects.Player.LocalLightLevel = 0;
                UOSObjects.Player.GlobalLightLevel = (byte)lightLevel;

                Client.Instance.SendToClient(new GlobalLightLevel(lightLevel));
                Client.Instance.SendToClient(new PersonalLightLevel(UOSObjects.Player));

                return true;
            }*/

            return false;
        }

        private static void ServerSetWarMode(Packet p, PacketHandlerEventArgs args)
        {
            UOSObjects.Player.Warmode = p.ReadBool();
        }

        private static void CustomHouseInfo(Packet p, PacketHandlerEventArgs args)
        {
            /*p.ReadByte(); // compression
            p.ReadByte(); // Unknown

            UOItem i = UOSObjects.FindItem(p.ReadUInt());
            if (i != null)
            {
                i.HouseRevision = p.ReadInt32();
                i.HousePacket = p.CopyBytes(0, p.Length);
            }*/
        }

        /*
        Packet Build
        1.  BYTE[1] Cmd
        2.  BYTE[2] len
        3.  BYTE[4] Player Serial
        4.  BYTE[4] Gump ID
        5.  BYTE[4] x
        6.  BYTE[4] y
        7.  BYTE[4] Compressed Gump Layout Length (CLen)
        8.  BYTE[4] Decompressed Gump Layout Length (DLen)
        9.  BYTE[CLen-4] Gump Data, zlib compressed
        10. BYTE[4] Number of text lines
        11. BYTE[4] Compressed Text Line Length (CTxtLen)
        12. BYTE[4] Decompressed Text Line Length (DTxtLen)
        13. BYTE[CTxtLen-4] Gump's Compressed Text data, zlib compressed
         */
        private static void CompressedGump(Packet p, PacketHandlerEventArgs args)
        {
            List<string> gumpStrings = new List<string>();
            uint progserial = p.ReadUInt(), typeidserial = p.ReadUInt();
            uint x = p.ReadUInt();
            uint y = p.ReadUInt();
            uint clen = p.ReadUInt() - 4;
            int dlen = (int)p.ReadUInt();

            byte[] decData = new byte[dlen];
            ref var buffer = ref p.ToArray();
            string layout;
            unsafe
            {
                fixed (byte* srcPtr = &buffer[p.Position], destPtr = decData)
                {
                    ZLib.Decompress((IntPtr)srcPtr, (int)clen, 0, (IntPtr)destPtr, dlen);
                    layout = Encoding.UTF8.GetString(destPtr, dlen);
                }
            }
            p.Skip((int)clen);
            // Split on one or more non-digit characters.
            string[] numbers = Regex.Split(layout, @"\D+");

            foreach (string value in numbers)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (int.TryParse(value, out int i) && ((i >= 500000 && i <= 600000) || (i >= 1000000 && i <= 1200000) || (i >= 3000000 && i <= 3100000)))
                        gumpStrings.Add(ClilocLoader.Instance.GetString(i));
                }
            }
            uint linesNum = p.ReadUInt();
            if (linesNum < 0 || linesNum > 256)
                linesNum = 0;
            if (linesNum > 0)
            {
                clen = p.ReadUInt() - 4;
                dlen = (int)p.ReadUInt();
                decData = new byte[dlen];

                unsafe
                {
                    fixed (byte* srcPtr = &buffer[p.Position], destPtr = decData)
                        ZLib.Decompress((IntPtr)srcPtr, (int)clen, 0, (IntPtr)destPtr, dlen);
                }

                p.Skip((int)clen);

                for (int i = 0, index = 0; i < linesNum; i++)
                {
                    int length = ((decData[index++] << 8) | decData[index++]) << 1;

                    int true_length = 0;

                    while (true_length < length)
                    {
                        if (((decData[index + true_length++] << 8) | decData[index + true_length++]) << 1 == '\0')
                            break;
                    }

                    gumpStrings.Add(Encoding.BigEndianUnicode.GetString(decData, index, true_length));
                    index += length;
                }
            }
            TryParseGump(layout, gumpStrings);
            //Timer.DelayedCallbackState(TimeSpan.FromMilliseconds(UOSObjects.Gump.ActionDelay), AddObservedGump, new PlayerData.GumpData(typeidserial, progserial, gumpStrings));
            AddObservedGump(new PlayerData.GumpData(typeidserial, progserial, gumpStrings));
            /*if (Config.GetBool("CaptureMibs") && MessageInBottleCapture.IsMibGump(layout))
            {
                switch (gumpStrings.Count)
                {
                    //Classic, non-custom MIB
                    case 3:
                        MessageInBottleCapture.CaptureMibCoordinates(gumpStrings[2], false);
                        break;
                    case 4:
                        MessageInBottleCapture.CaptureMibCoordinates(gumpStrings[2], true);
                        break;
                }
            }*/
        }

        private static void UncompressedGump(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            List<string> gumpStrings = new List<string>();
            uint progserial = p.ReadUInt(), typeidserial = p.ReadUInt();
            int x = (int)p.ReadUInt();
            int y = (int)p.ReadUInt();
            ushort cmdLen = p.ReadUShort();
            string cmd = p.ReadASCII(cmdLen);
            ushort textLinesCount = p.ReadUShort();
            if (textLinesCount < 0 || textLinesCount > 256)
                textLinesCount = 0;
            // Split on one or more non-digit characters.
            string[] numbers = Regex.Split(cmd, @"\D+");
            foreach (string value in numbers)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (int.TryParse(value, out int i) && ((i >= 500000 && i <= 600000) || (i >= 1000000 && i <= 1200000) || (i >= 3000000 && i <= 3100000)))
                        gumpStrings.Add(ClilocLoader.Instance.GetString(i));
                }
            }

            ref var buffer = ref p.ToArray();
            for (int i = 0, index = p.Position; i < textLinesCount; i++)
            {
                int length = ((buffer[index++] << 8) | buffer[index++]) << 1;
                int true_length = 0;

                while (true_length < length)
                {
                    if (((buffer[index + true_length++] << 8) | buffer[index + true_length++]) << 1 == '\0')
                        break;
                }

                gumpStrings.Add(Encoding.BigEndianUnicode.GetString(buffer, index, true_length));
                index += length;
            }
            TryParseGump(cmd, gumpStrings);
            AddObservedGump(new PlayerData.GumpData(typeidserial, progserial, gumpStrings));
        }

        private static void AddObservedGump(PlayerData.GumpData data)
        {
            if (!UOSObjects.Player.OpenedGumps.TryGetValue(data.GumpID, out List<PlayerData.GumpData> glist))
                glist = UOSObjects.Player.OpenedGumps[data.GumpID] = new List<PlayerData.GumpData>();
            glist.Add(data);
        }

        private static void TryParseGump(string gumpData, List<string> pieces)
        {
            int dataIndex = 0;
            while (dataIndex < gumpData.Length)
            {
                if (gumpData.Substring(dataIndex) == "\0")
                {
                    break;
                }
                else
                {
                    int begin = gumpData.IndexOf("{", dataIndex);
                    int end = gumpData.IndexOf("}", dataIndex + 1);
                    if ((begin != -1) && (end != -1))
                    {
                        string sub = gumpData.Substring(begin + 1, end - begin - 1).Trim();
                        pieces.Add(sub);
                        dataIndex = end;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static List<string> ParseGumpString(string[] gumpPieces, string[] gumpLines)
        {
            List<string> gumpText = new List<string>();
            for (int i = 0; i < gumpPieces.Length; i++)
            {
                string[] gumpParams = Regex.Split(gumpPieces[i], @"\s+");
                switch (gumpParams[0].ToLower())
                {

                    case "croppedtext":
                        gumpText.Add(gumpLines[int.Parse(gumpParams[6])]);
                        // CroppedText [x] [y] [width] [height] [color] [text-id]
                        // Adds a text field to the gump. gump is similar to the text command, but the text is cropped to the defined area.
                        //gump.AddControl(new CroppedText(gump, gumpParams, gumpLines), currentGUMPPage);
                        //(gump.LastControl as CroppedText).Hue = 1;
                        break;

                    case "htmlgump":
                        gumpText.Add(gumpLines[int.Parse(gumpParams[5])]);
                        // HtmlGump [x] [y] [width] [height] [text-id] [background] [scrollbar]
                        // Defines a text-area where Html-commands are allowed.
                        // [background] and [scrollbar] can be 0 or 1 and define whether the background is transparent and a scrollbar is displayed.
                        //	gump.AddControl(new HtmlGumpling(gump, gumpParams, gumpLines), currentGUMPPage);
                        break;

                    case "text":
                        gumpText.Add(gumpLines[int.Parse(gumpParams[4])]);
                        // Text [x] [y] [color] [text-id]
                        // Defines the position and color of a text (data) entry.
                        //gump.AddControl(new TextLabel(gump, gumpParams, gumpLines), currentGUMPPage);
                        break;
                }
            }

            return gumpText;
        }

        private static void ResurrectionGump(Packet p, PacketHandlerEventArgs args)
        {
            /*if (Config.GetBool("AutoCap"))
            {
                ScreenCapManager.DeathCapture(0.10);
                ScreenCapManager.DeathCapture(0.25);
                ScreenCapManager.DeathCapture(0.50);
                ScreenCapManager.DeathCapture(0.75);
            }*/
        }

        private static void BuffDebuff(Packet p, PacketHandlerEventArgs args)
        {
            uint ser = p.ReadUInt();
            ushort icon = p.ReadUShort();
            ushort action = p.ReadUShort();

            if (Enum.IsDefined(typeof(BuffIcon), icon))
            {
                BuffIcon buff = (BuffIcon)icon;
                switch (action)
                {
                    case 0x01: // show

                        p.ReadUInt(); //0x000
                        p.ReadUShort(); //icon # again..?
                        p.ReadUShort(); //0x1 = show
                        p.ReadUInt(); //0x000
                        ushort duration = p.ReadUShort();
                        p.ReadUShort(); //0x0000
                        p.ReadByte(); //0x0

                        BuffsDebuffs buffInfo = new BuffsDebuffs
                        {
                            IconNumber = icon,
                            BuffIcon = (BuffIcon)icon,
                            ClilocMessage1 = ClilocLoader.Instance.GetString((int)p.ReadUInt()),
                            ClilocMessage2 = ClilocLoader.Instance.GetString((int)p.ReadUInt()),
                            Duration = duration,
                            Timestamp = DateTime.UtcNow
                        };

                        if (UOSObjects.Player != null && UOSObjects.Player.BuffsDebuffs.All(b => b.BuffIcon != buff))
                        {
                            UOSObjects.Player.BuffsDebuffs.Add(buffInfo);
                        }

                        break;

                    case 0x0: // remove
                        if (UOSObjects.Player != null)// && UOSObjects.Player.BuffsDebuffs.Any(b => b.BuffIcon == buff))
                        {
                            UOSObjects.Player.BuffsDebuffs.RemoveAll(b => b.BuffIcon == buff);
                        }

                        break;
                }
            }
        }

        /*private static void AttackRequest(Packet p, PacketHandlerEventArgs args)
        {
            //TODO: Show attack overhead?! really?
            if (UOSObjects.Gump.ShowMobileFlags)
            {
                uint serial = p.ReadUInt();

                UOMobile m = UOSObjects.FindMobile(serial);

                if (m == null) return;

                UOSObjects.Player.OverheadMessage(UOSObjects.Gump.IsFriend(serial) ? 63 : m.GetNotorietyColorInt(), $"Attack: {m.Name}");
            }
        }*/

        private static void UnicodePromptSend(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            //uint serial = p.ReadUInt();
            //uint id = p.ReadUInt();
            //uint type = p.ReadUInt();

            uint serial = p.ReadUInt();
            uint id = p.ReadUInt();
            uint type = p.ReadUInt();

            string lang = p.ReadASCII(4);
            string text = p.ReadUnicodeReversed();

            UOSObjects.Player.HasPrompt = false;
            UOSObjects.Player.PromptSenderSerial = serial;
            UOSObjects.Player.PromptID = id;
            UOSObjects.Player.PromptType = type;
            UOSObjects.Player.PromptInputText = text;

            if (ScriptManager.Recording && !string.IsNullOrEmpty(UOSObjects.Player.PromptInputText))
                ScriptManager.AddToScript($"promptresponse '{UOSObjects.Player.PromptInputText}'");
        }

        private static void UnicodePromptReceived(Packet p, PacketHandlerEventArgs args)
        {
            if (UOSObjects.Player == null)
                return;

            uint serial = p.ReadUInt();
            uint id = p.ReadUInt();
            uint type = p.ReadUInt();

            UOSObjects.Player.HasPrompt = true;
            UOSObjects.Player.PromptSenderSerial = serial;
            UOSObjects.Player.PromptID = id;
            UOSObjects.Player.PromptType = type;
            if (ScriptManager.Recording)
                ScriptManager.AddToScript($"waitforprompt {id}");
        }
    }
}
