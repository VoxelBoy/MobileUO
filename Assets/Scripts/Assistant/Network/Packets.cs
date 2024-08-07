using System;
using System.Text;
using System.Collections.Generic;

using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.GameObjects;
using ClassicUO.Network;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

namespace Assistant
{
	internal enum MessageType
	{
		Regular = 0x00,
		System = 0x01,
		Emote = 0x02,
		Label = 0x06,
		Focus = 0x07,
		Whisper = 0x08,
		Yell = 0x09,
		Spell = 0x0A,
		Guild = 0x0D,
		Alliance = 0x0E,
		Encoded = 0xC0,

		Special = 0x20
	}

	internal sealed class RenameReq : PacketWriter
	{
		public RenameReq(uint target, string name) : base(0x75)
		{
			EnsureSize(35);
			WriteUInt(target);
			WriteASCII(name, 30);
		}
	}

	internal sealed class SendPrivatePartyMessage : PacketWriter
	{
		internal SendPrivatePartyMessage(uint serial, string message) : base(0xBF)
		{
			EnsureSize(12 + message.Length * 2);

			WriteUShort(0x06); // party command
			WriteByte(0x03); // private party message
			WriteUInt(serial);
			WriteUnicode(message);
		}
	}

	internal sealed class SendPartyMessage : PacketWriter
	{
		internal SendPartyMessage(string message) : base(0xBF)
		{
			EnsureSize(8 + message.Length * 2);

			WriteUShort(0x06); // party command
			WriteByte(0x04); // tell party
			WriteUnicode(message);
		}
	}

	internal sealed class AcceptParty : PacketWriter
	{
		internal AcceptParty(uint leader) : base(0xBF)
		{
			EnsureSize(10);

			WriteUShort(0x06); // party command
			WriteByte(0x08); // accept
			WriteUInt(leader);
		}
	}

	internal sealed class DeclineParty : PacketWriter
	{
		internal DeclineParty(uint leader) : base(0xBF)
		{
			EnsureSize(10);

			WriteUShort(0x06); // party command
			WriteByte(0x09); // decline
			WriteUInt(leader);
		}
	}

	internal sealed class ContainerContent : PacketWriter
	{
		internal ContainerContent(List<UOItem> items) : this(items, Engine.UsePostKRPackets)
		{
		}

		internal ContainerContent(List<UOItem> items, bool useKR) : base(0x3C)
		{
			WriteUShort((ushort)items.Count);

			foreach (UOItem item in items)
			{
				WriteUInt(item.Serial);
				WriteUShort(item.ItemID);
				WriteSByte(0);
				WriteUShort(item.Amount);
				WriteUShort((ushort)item.Position.X);
				WriteUShort((ushort)item.Position.Y);

				if (useKR)
					WriteByte(item.GridNum);//gridline

				if (item.Container is UOItem cont)
					WriteUInt(cont.Serial);
				else if (item.Container is uint ser)
					WriteUInt(ser);
				else
					WriteUInt(0);
				WriteUShort(item.Hue);
			}
		}
	}

	internal sealed class ContainerItem : PacketWriter
	{
		internal ContainerItem(UOItem item) : this(item, Engine.UsePostKRPackets)
		{
		}

		internal ContainerItem(UOItem item, bool isKR) : base(0x25)
		{
			WriteUInt(item.Serial);

			WriteUShort(item.ItemID);
			WriteByte(0);
			WriteUShort(item.Amount);
			WriteUShort((ushort)item.Position.X);
			WriteUShort((ushort)item.Position.Y);

			if (isKR)
				WriteByte(item.GridNum);//gridline

			object cont = item.Container;
			if (cont is UOEntity)
				WriteUInt(((UOEntity)item.Container).Serial);
			else if (cont is uint ser)
				WriteUInt(ser);
			else
				WriteUInt(0x7FFFFFFF);

			/*if (SearchExemptionAgent.Contains(item))
				WriteUShort((ushort)Config.GetInt("ExemptColor"));
			else*/
				WriteUShort(item.Hue);
		}
	}

	internal sealed class SingleClick : PacketWriter
	{
		internal SingleClick(UOEntity clicked) : base(0x09)
		{
			WriteUInt(clicked.Serial);
		}

		internal SingleClick(uint clicked) : base(0x09)
		{
			WriteUInt(clicked);
		}
	}

	internal sealed class DoubleClick : PacketWriter
	{
		internal DoubleClick(uint clicked) : base(0x06)
		{
			WriteUInt(clicked);
		}
	}

	internal sealed class Target : PacketWriter
	{
		internal Target(uint tid) : this(tid, false, 0)
		{
		}

		internal Target(uint tid, byte flags) : this(tid, false, flags)
		{
		}

		internal Target(uint tid, bool ground) : this(tid, ground, 0)
		{
		}

		internal Target(uint tid, bool ground, byte flags) : base(0x6C)
		{
			EnsureSize(19);
			WriteBool(ground);
			WriteUInt(tid);
			WriteByte(flags);
		}
	}

	internal sealed class TargetResponse : PacketWriter
	{
		internal TargetResponse(TargetInfo info) : base(0x6C)
		{
			EnsureSize(19);
			WriteByte(info.Type);
			WriteUInt(info.TargID);
			WriteByte(info.Flags);
			WriteUInt(info.Serial);
			WriteUShort((ushort)info.X);
			WriteUShort((ushort)info.Y);
			WriteUShort((ushort)info.Z);
			WriteUShort(info.Gfx);
		}

		internal TargetResponse(uint id, UOMobile m) : base(0x6C)
		{
			EnsureSize(19);
			WriteByte(0x00); // target object
			WriteUInt(id);
			WriteByte(0); // flags
			WriteUInt(m.Serial);
			WriteUShort((ushort)m.Position.X);
			WriteUShort((ushort)m.Position.Y);
			WriteUShort((ushort)m.Position.Z);
			WriteUShort(m.Body);
		}

		internal TargetResponse(uint id, UOItem item) : base(0x6C)
		{
			EnsureSize(19);
			WriteByte((byte)0x00); // target object
			WriteUInt(id);
			WriteByte(0); // flags
			WriteUInt(item.Serial);
			WriteUShort((ushort)item.Position.X);
			WriteUShort((ushort)item.Position.Y);
			WriteUShort((ushort)item.Position.Z);
			WriteUShort(item.ItemID);
		}
	}

	internal sealed class TargetCancelResponse : PacketWriter
	{
		internal TargetCancelResponse(uint id) : base(0x6C)
		{
			EnsureSize(19);
			WriteByte(0);
			WriteUInt(id);
			WriteByte(0);
			WriteUInt(0);
			WriteUShort(0xFFFF);
			WriteUShort(0xFFFF);
			WriteUShort(0);
			WriteUShort(0);
		}
	}

	internal sealed class AttackReq : PacketWriter
	{
		internal AttackReq(uint serial) : base(0x05)
		{
			EnsureSize(5);
			WriteUInt(serial);
		}
	}

	internal sealed class SetWeather : PacketWriter
	{
		internal SetWeather(int type, int num) : base(0x65)
		{
			EnsureSize(4);
			WriteByte((byte)type); //types: 0x00 - "It starts to rain", 0x01 - "A fierce storm approaches.", 0x02 - "It begins to snow", 0x03 - "A storm is brewing.", 0xFF - None (turns off sound effects), 0xFE (no effect?? Set temperature?) 
			WriteByte((byte)num); //number of weather effects on screen
			WriteByte(0xFE);
		}
	}

	internal sealed class PlayMusic : PacketWriter
	{
		internal PlayMusic(int num) : base(0x6D)
		{
			EnsureSize(3);
			WriteUInt((uint)num);
		}
	}

	internal sealed class CancelTarget : PacketWriter
	{
		internal CancelTarget(uint id) : base(0x6C)
		{
			EnsureSize(19);
			WriteByte(0);
			WriteUInt(id);
			WriteByte(3);
		}
	}

	internal sealed class SkillsQuery : PacketWriter
	{
		internal SkillsQuery(UOMobile m) : base(0x34)
		{
			EnsureSize(10);
			WriteUInt(0xEDEDEDED); // que el fuck, osi
			WriteByte(0x05);
			WriteUInt(m.Serial);
		}
	}

	internal sealed class StatusQuery : PacketWriter
	{
		internal StatusQuery(UOMobile m) : base(0x34)
		{
			EnsureSize(10);
			WriteUInt(0xEDEDEDED);
			WriteByte(0x04);
			WriteUInt(m.Serial);
		}
	}

	internal sealed class StatLockInfo : PacketWriter
	{
		internal StatLockInfo(PlayerMobile m) : base(0xBF)
		{
			EnsureSize(12);

			WriteUShort(0x19);
			WriteByte(2);
			WriteUInt(m.Serial);
			WriteByte(0);

			int lockBits = 0;

			lockBits |= (int)m.StrLock << 4;
			lockBits |= (int)m.DexLock << 2;
			lockBits |= (int)m.IntLock;

			WriteByte((byte)lockBits);
		}
	}

	internal sealed class SkillsList : PacketWriter
	{
		internal SkillsList() : base(0x3A)
		{
			EnsureSize(3 + 1 + UOSObjects.Player.Skills.Length * 9 + 2);

			WriteByte((byte)0x02);
			for (int i = 0; i < UOSObjects.Player.Skills.Length; i++)
			{
				WriteUShort((ushort)(i + 1));
				WriteUShort(UOSObjects.Player.Skills[i].FixedValue);
				WriteUShort(UOSObjects.Player.Skills[i].FixedBase);
				WriteByte((byte)UOSObjects.Player.Skills[i].Lock);
				WriteUShort(UOSObjects.Player.Skills[i].FixedCap);
			}
			WriteUShort(0);
		}
	}

	internal sealed class SkillUpdate : PacketWriter
	{
		internal SkillUpdate(Skill s) : base(0x3A)
		{
			EnsureSize(3 + 1 + 9);

			WriteByte((byte)0xDF);

			WriteUShort((ushort)s.Index);
			WriteUShort(s.FixedValue);
			WriteUShort(s.FixedBase);
			WriteByte((byte)s.Lock);
			WriteUShort(s.FixedCap);
		}
	}

	internal sealed class SetSkillLock : PacketWriter
	{
		internal SetSkillLock(int skill, Lock type) : base(0x3A)
		{
			EnsureSize(6);
			WriteUShort((ushort)skill);
			WriteByte((byte)type);
		}
	}

	internal sealed class AsciiMessage : PacketWriter
	{
		internal AsciiMessage(uint serial, int graphic, MessageType type, int hue, int font, string name, string text) : base(0x1C)
		{
			if (name == null) name = "";
			if (text == null) text = "";

			if (hue == 0)
				hue = 0x3B2;

			this.EnsureSize(45 + text.Length);

			WriteUInt(serial);
			WriteUShort((ushort)graphic);
			WriteByte((byte)type);
			WriteUShort((ushort)hue);
			WriteUShort((ushort)font);
			WriteASCII(name, 30);
			WriteASCII(text);
		}
	}

	internal sealed class ClientAsciiMessage : PacketWriter
	{
		internal ClientAsciiMessage(MessageType type, int hue, int font, string str) : base(0x03)
		{
			EnsureSize(1 + 2 + 1 + 2 + 2 + str.Length + 1);

			WriteByte((byte)type);
			WriteUShort((ushort)hue);
			WriteUShort((ushort)font);
			WriteASCII(str);
		}
	}

	internal sealed class UnicodeMessage : PacketWriter
	{
		internal UnicodeMessage(uint serial, int graphic, MessageType type, int hue, int font, string lang, string name, string text) : base(0xAE)
		{
			if (lang == null || lang == "") lang = "ENU";
			if (name == null) name = "";
			if (text == null) text = "";

			if (hue == 0)
				hue = 0x3B2;

			this.EnsureSize(50 + (text.Length * 2));

			WriteUInt(serial);
			WriteUShort((ushort)graphic);
			WriteByte((byte)type);
			WriteUShort((ushort)hue);
			WriteUShort((ushort)font);
			WriteASCII(lang.ToUpper(), 4);
			WriteASCII(name, 30);
			WriteUnicode(text);
		}
	}

	internal sealed class ClientUniMessage : PacketWriter
	{
		internal ClientUniMessage(MessageType type, int hue, int font, string lang, List<byte> keys, string text) : base(0xAD)
		{
			if (lang == null || lang == "") lang = "ENU";
			if (text == null) text = "";

			this.EnsureSize(50 + (text.Length * 2) + (keys == null ? 0 : keys.Count + 1));
			if (keys == null || keys.Count <= 1)
				WriteByte((byte)type);
			else
				WriteByte((byte)(type | MessageType.Encoded));
			WriteUShort((ushort)hue);
			WriteUShort((ushort)font);
			WriteASCII(lang, 4);
			if (keys != null && keys.Count > 1)
			{
				WriteUShort(keys[0]);
				for (int i = 1; i < keys.Count; i++)
					WriteByte(keys[i]);
				WriteUTF8(text);
			}
			else
			{
				WriteUnicode(text);
			}
		}
	}

	internal sealed class ClientUniEncodedCommandMessage : PacketWriter
	{
		internal ClientUniEncodedCommandMessage(MessageType type, int hue, int font, string text, string lang = "ENU") : base(0xAD)
		{
			var entries = SpeechesLoader.Instance.GetKeywords(text);

            bool encoded = entries != null && entries.Count != 0;
            if(encoded)
                type |= MessageType.Encoded;

            WriteByte((byte)type);
            WriteUShort((ushort)hue);
            WriteUShort((ushort)font);
            WriteASCII(lang, 4);

            if (encoded)
            {
                List<byte> codeBytes = new List<byte>();
                byte[] utf8 = Encoding.UTF8.GetBytes(text);
                int length = entries.Count;
                codeBytes.Add((byte) (length >> 4));
                int num3 = length & 15;
                bool flag = false;
                int index = 0;

                while (index < length)
                {
                    int keywordID = entries[index].KeywordID;

                    if (flag)
                    {
                        codeBytes.Add((byte) (keywordID >> 4));
                        num3 = keywordID & 15;
                    }
                    else
                    {
                        codeBytes.Add((byte) ((num3 << 4) | ((keywordID >> 8) & 15)));
                        codeBytes.Add((byte) keywordID);
                    }

                    index++;
                    flag = !flag;
                }

                if (!flag) codeBytes.Add((byte) (num3 << 4));

                for (int i = 0; i < codeBytes.Count; i++)
                    WriteByte(codeBytes[i]);

                WriteBytes(utf8, 0, utf8.Length);
                WriteByte(0);
            }
            else
            {
                WriteUnicode(text);
            }
		}
	}

	internal sealed class LiftRequest : PacketWriter
	{
		internal LiftRequest(uint ser, int amount) : base(0x07)
		{
			EnsureSize(7);
			WriteUInt(ser);
			WriteUShort((ushort)amount);
		}

		internal LiftRequest(UOItem i, int amount) : this(i.Serial, amount)
		{
		}

		internal LiftRequest(UOItem i) : this(i.Serial, i.Amount)
		{
		}
	}

	internal sealed class LiftRej : PacketWriter
	{
		internal LiftRej() : this(5) // reason = Inspecific
		{
		}

		internal LiftRej(byte reason) : base(0x27)
		{
			EnsureSize(2);
			WriteByte(reason);
		}
	}

	internal sealed class EquipRequest : PacketWriter
	{
		internal EquipRequest(uint item, UOMobile to, Layer layer) : base(0x13)
		{
			EnsureSize(10);
			WriteUInt(item);
			WriteByte((byte)layer);
			WriteUInt(to.Serial);
		}

		internal EquipRequest(uint item, uint to, Layer layer) : base(0x13)
		{
			EnsureSize(10);
			WriteUInt(item);
			WriteByte((byte)layer);
			WriteUInt(to);
		}
	}

	internal sealed class DropRequest : PacketWriter
	{
		internal DropRequest(UOItem item, uint destSer) : base(0x08)
		{
			WriteUInt(item.Serial);
			WriteUShort(ushort.MaxValue);
			WriteUShort(ushort.MaxValue);
			WriteSByte(0);
			if (Engine.UsePostKRPackets)
				WriteByte(item.GridNum);
			WriteUInt(destSer);
		}

		internal DropRequest(UOItem item, UOItem to) : this(item, to.Serial)
		{
		}

		internal DropRequest(uint item, Point3D p, uint dest) : base(0x08)
		{
			WriteUInt(item);
			WriteUShort((ushort)p.X);
			WriteUShort((ushort)p.Y);
			WriteSByte((sbyte)p.Z);
			if (Engine.UsePostKRPackets)
				WriteByte(UOSObjects.FindItem(item)?.GridNum ?? 0);
			WriteUInt(dest);
		}
	}

	internal class SellListItem
	{
		internal uint Serial;
		internal ushort Amount;

		internal SellListItem(uint s, ushort a)
		{
			Serial = s;
			Amount = a;
		}
	}

	internal sealed class VendorSellResponse : PacketWriter
	{
		internal VendorSellResponse(UOMobile vendor, List<SellListItem> list) : base(0x9F)
		{
			EnsureSize(1 + 2 + 4 + 2 + list.Count * 6);

			WriteUInt(vendor.Serial);
			WriteUShort((ushort)list.Count);

			for (int i = 0; i < list.Count; i++)
			{
				SellListItem sli = list[i];
				WriteUInt(sli.Serial);
				WriteUShort(sli.Amount);
			}
		}
	}

	internal sealed class MobileStatusExtended : PacketWriter
	{
		internal MobileStatusExtended(PlayerMobile m) : base(0x11)
		{
			string name = m.Name;
			if (name == null) name = "";

			EnsureSize(88);

			WriteUInt(m.Serial);
			WriteASCII(name, 30);

			WriteUShort(m.Hits);
			WriteUShort(m.HitsMax);

			WriteBool(false); // cannot edit name

			WriteByte(0x03); // no aos info

			WriteBool(m.IsFemale);

			WriteUShort(m.Strength);
			WriteUShort(m.Dexterity);
			WriteUShort(m.Intelligence);

			WriteUShort(m.Stamina);
			WriteUShort(m.StaminaMax);

			WriteUShort(m.Mana);
			WriteUShort(m.ManaMax);

			WriteUInt(m.Gold);
			WriteUShort((ushort)m.PhysicalResistance);
			WriteUShort(m.Weight);
			WriteUShort((ushort)m.StatsCap);
			WriteByte(m.Followers);
			WriteByte(m.FollowersMax);
		}
	}

	internal sealed class MobileStatusCompact : PacketWriter
	{
		internal MobileStatusCompact(UOMobile m) : base(0x11)
		{
			string name = m.Name;
			if (name == null) name = "";

			EnsureSize(88);

			WriteUInt(m.Serial);
			WriteASCII(name, 30);

			WriteUShort(m.Hits);
			WriteUShort(m.HitsMax);

			WriteBool(false); // cannot edit name

			WriteByte(0x00); // no aos info
		}
	}

	internal sealed class GumpTextEntry
	{
		internal GumpTextEntry(ushort id, string s)
		{
			EntryID = id;
			Text = s;
		}

		internal ushort EntryID;
		internal string Text;
	}

	internal sealed class GumpResponse : PacketWriter
	{
		internal GumpResponse(uint serial, uint tid, int bid, List<int> switches, List<GumpTextEntry> entries) : base(0xB1)
		{
			EnsureSize(3 + 4 + 4 + 4 + 4 + switches.Count * 4 + 4 + entries.Count * 4);

			WriteUInt(serial);
			WriteUInt(tid);

			WriteUInt((uint)bid);

			WriteUInt((uint)switches.Count);
			for (int i = 0; i < switches.Count; i++)
				WriteUInt((uint)switches[i]);
			WriteUInt((uint)entries.Count);
			for (int i = 0; i < entries.Count; i++)
			{
				GumpTextEntry gte = entries[i];
				WriteUShort(gte.EntryID);
				WriteUShort((ushort)(gte.Text.Length * 2));
				WriteUnicode(gte.Text, gte.Text.Length);
			}
		}
	}

	internal sealed class CompressedGump : PacketWriter
	{
		internal CompressedGump(uint serial, uint tid, int bid, int[] switches, GumpTextEntry[] entries) : base(0xDD)
		{
			EnsureSize(3 + 4 + 4 + 4 + 4 + switches.Length * 4 + 4 + entries.Length * 4);

			WriteUInt(serial);
			WriteUInt(tid);

			WriteUInt((uint)bid);

			WriteUInt((uint)switches.Length);
			for (int i = 0; i < switches.Length; i++)
				WriteUInt((uint)switches[i]);
			WriteUInt((uint)entries.Length);
			for (int i = 0; i < entries.Length; i++)
			{
				GumpTextEntry gte = entries[i];
				WriteUShort(gte.EntryID);
				WriteUShort((ushort)(gte.Text.Length * 2));
				WriteUnicode(gte.Text, gte.Text.Length);
			}
		}
	}

	internal sealed class UseSkill : PacketWriter
	{
		internal UseSkill(int sk) : base(0x12)
		{
			string cmd = String.Format("{0} 0", sk);
			EnsureSize(4 + cmd.Length + 1);
			WriteByte((byte)0x24);
			WriteASCII(cmd);
		}

		internal UseSkill(Skill sk) : this(sk.Index)
		{
		}
	}

	internal sealed class ExtCastSpell : PacketWriter
	{
		internal ExtCastSpell(uint book, ushort spell) : base(0xBF)
		{
			EnsureSize(1 + 2 + 2 + 2 + 4 + 2);
			WriteUShort(0x1C);
			if (SerialHelper.IsItem(book))
			{
				WriteUShort(1);
				WriteUInt(book);
			}
			else
				WriteUShort(2);
			WriteUShort(spell);
		}
	}

	internal sealed class CastSpellFromBook : PacketWriter
	{
		internal CastSpellFromBook(uint book, ushort spell) : base(0x12)
		{
			string cmd;
			if (SerialHelper.IsItem(book))
				cmd = $"{spell} {book}";
			else
				cmd = $"{spell}";
			EnsureSize(3 + 1 + cmd.Length + 1);
			WriteByte(0x27);
			WriteASCII(cmd);
		}
	}

	internal sealed class CastSpellFromMacro : PacketWriter
	{
		internal CastSpellFromMacro(ushort spell) : base(0x12)
		{
			string cmd = spell.ToString();
			EnsureSize(3 + 1 + cmd.Length + 1);
			WriteByte((byte)0x56);
			WriteASCII(cmd);
		}
	}

	internal sealed class DisarmRequest : PacketWriter
	{
		internal DisarmRequest() : base(0xBF)
		{
			EnsureSize(3);
			WriteUShort(0x09);
		}
	}

	internal sealed class StunRequest : PacketWriter
	{
		internal StunRequest() : base(0xBF)
		{
			EnsureSize(3);
			WriteUShort(0x0A);
		}
	}

	internal sealed class CloseGump : PacketWriter
	{
		internal CloseGump(uint typeID, uint buttonID) : base(0xBF)
		{
			EnsureSize(13);

			WriteUShort(0x04);
			WriteUInt(typeID);
			WriteUInt(buttonID);
			UOSObjects.Player.OpenedGumps?.Remove(typeID);
		}

		internal CloseGump(uint typeID) : base(0xBF)
		{
			EnsureSize(13);

			WriteUShort(0x04);
			WriteUInt(typeID);
			WriteUInt(0);
			UOSObjects.Player.OpenedGumps?.Remove(typeID);
		}
	}

	internal sealed class ChangeCombatant : PacketWriter
	{
		internal ChangeCombatant(uint ser) : base(0xAA)
		{
			EnsureSize(5);
			WriteUInt(ser);
		}

		internal ChangeCombatant(UOMobile m) : this(m.Serial)
		{
		}
	}

	internal sealed class UseAbility : PacketWriter
	{
		// ints are 'encoded' with a leading bool, if true then the number is 0, if flase then followed by all 4 bytes (lame :-)
		internal UseAbility(Ability a) : base(0xD7)
		{
			EnsureSize(1 + 2 + 4 + 2 + 4);

			WriteUInt((uint)UOSObjects.Player.Serial);
			WriteUShort((ushort)0x19);
			if (a == Ability.None)
			{
				WriteBool(true);
			}
			else
			{
				WriteBool(false);
				WriteUInt((uint)a);
			}
		}
	}

	internal sealed class ClearAbility : PacketWriter
	{
		internal static readonly PacketWriter Instance = new ClearAbility();

		internal ClearAbility() : base(0xBF)
		{
			EnsureSize(5);

			WriteUShort(0x21);
		}
	}

	internal sealed class PingPacket : PacketWriter
	{
		internal PingPacket(byte seq) : base(0x73)
		{
			EnsureSize(2);
			WriteByte(seq);
		}
	}

	internal sealed class MobileUpdate : PacketWriter
	{
		internal MobileUpdate(UOMobile m) : base(0x20)
		{
			EnsureSize(19);
			WriteUInt(m.Serial);
			WriteUShort(m.Body);
			WriteByte(0);
			ushort ltHue = UOSObjects.Gump.HLTargetHue;
			if (ltHue > 0 && Targeting.LastTargetInfo != null && Targeting.LastTargetInfo.Serial == m.Serial)
				WriteUShort((ushort)(ltHue | 0x8000));
			else
				WriteUShort(m.Hue);
			WriteByte((byte)m.GetPacketFlags());
			WriteUShort((ushort)m.Position.X);
			WriteUShort((ushort)m.Position.Y);
			WriteUShort(0);
			WriteByte((byte)m.Direction);
			WriteSByte((sbyte)m.Position.Z);
		}
	}

	internal sealed class MobileIncoming : PacketWriter
	{
		internal MobileIncoming(UOMobile m) : base(0x78)
		{
			int count = m.Contains.Count;
			int ltHue = UOSObjects.Gump.HLTargetHue;
			bool isLT;
			if (ltHue != 0)
				isLT = Targeting.LastTargetInfo != null && Targeting.LastTargetInfo.Serial == m.Serial;
			else
				isLT = false;

			EnsureSize(3 + 4 + 2 + 2 + 2 + 1 + 1 + 2 + 1 + 1 + 4 + count * (4 + 2 + 1 + 2));
			WriteUInt(m.Serial);
			WriteUShort(m.Body);
			WriteUShort((ushort)m.Position.X);
			WriteUShort((ushort)m.Position.Y);
			WriteSByte((sbyte)m.Position.Z);
			WriteByte((byte)m.Direction);
			WriteUShort((ushort)(isLT ? ltHue | 0x8000 : m.Hue));
			WriteByte((byte)m.GetPacketFlags());
			WriteByte(m.Notoriety);

			for (int i = 0; i < count; ++i)
			{
				UOItem item = m.Contains[i];
				ushort itemID = (ushort)(item.ItemID & 0x3FFF);
				bool writeHue = item.Hue != 0;
				if (writeHue || isLT)
					itemID |= 0x8000;

				WriteUInt(item.Serial);
				WriteUShort(itemID);
				WriteByte((byte)item.Layer);
				if (isLT)
					WriteUShort((ushort)(ltHue & 0x3FFF));
				else if (writeHue)
					WriteUShort(item.Hue);
			}
			WriteUInt(0); // terminate
		}
	}

	internal class VendorBuyItem
	{
		internal VendorBuyItem(uint ser, int amount, int price)
		{
			Serial = ser;
			Amount = amount;
			Price = price;
		}

		internal readonly uint Serial;
		internal int Amount;
		internal int Price;

		internal int TotalCost { get { return Amount * Price; } }
	}

	internal sealed class VendorBuyResponse : PacketWriter
	{
		internal VendorBuyResponse(uint vendor, IList<VendorBuyItem> list) : base(0x3B)
		{
			EnsureSize(4 + 1 + list.Count * 7);

			WriteUInt(vendor);
			if (list.Count > 0)
			{
				WriteByte(0x02); // flag

				for (int i = 0; i < list.Count; ++i)
				{
					VendorBuyItem vbi = list[i];
					WriteByte(0x1A); // layer?
					WriteUInt(vbi.Serial);
					WriteUShort((ushort)vbi.Amount);
				}
			}
			else
				WriteByte(0x00);
		}
	}

	internal sealed class MenuResponse : PacketWriter
	{
		internal MenuResponse(uint serial, ushort menuid, ushort index, ushort itemid, ushort hue) : base(0x7D)
		{
			EnsureSize(13);
			WriteUInt(serial);
			WriteUShort(menuid);
			WriteUShort(index);
			WriteUShort(itemid);
			WriteUShort(hue);
		}
	}

	internal sealed class HuePicker : PacketWriter
	{
		internal HuePicker() : this(uint.MaxValue, 0x0FAB)
		{
		}

		internal HuePicker(ushort itemid) : this(uint.MaxValue, itemid)
		{
		}

		internal HuePicker(uint serial, ushort itemid) : base(0x95)
		{
			EnsureSize(9);
			WriteUInt(serial);
			WriteUShort(0);
			WriteUShort(itemid);
		}
	}

	internal sealed class WalkRequest : PacketWriter
	{
		internal WalkRequest(Direction dir, byte seq) : base(0x02)
		{
			EnsureSize(7);
			WriteByte((byte)dir);
			WriteUInt(seq);
			WriteUInt(uint.MaxValue); // key
		}
	}

	internal sealed class ResyncReq : PacketWriter
	{
		internal ResyncReq() : base(0x22)
		{
			EnsureSize(3);
			WriteUShort(0);
		}
	}

	internal sealed class EquipmentItem : PacketWriter
	{
		internal EquipmentItem(UOItem item, uint owner) : this(item, item.Hue, owner)
		{
		}

		internal EquipmentItem(UOItem item, ushort hue, uint owner) : base(0x2E)
		{
			EnsureSize(15);
			WriteUInt(item.Serial);
			WriteUShort(item.ItemID);
			WriteSByte(0);
			WriteByte((byte)item.Layer);
			WriteUInt(owner);
			WriteUShort(hue);
		}
	}

	internal sealed class ForceWalk : PacketWriter
	{
		internal ForceWalk(Direction d) : base(0x97)
		{
			EnsureSize(2);
			WriteByte((byte)d);
		}
	}

	internal sealed class PathFindTo : PacketWriter
	{
		internal PathFindTo(ushort x, ushort y, short z) : base(0x38)
		{
			EnsureSize(7 * 20);
			for (int i = 0; i < 20; i++)
			{
				if (i != 0)
					WriteByte((byte)0x38);
				WriteUShort(x);
				WriteUShort(y);
				WriteUShort((ushort)z);
			}
		}
	}

	internal sealed class LoginConfirm : PacketWriter
	{
		internal LoginConfirm(UOMobile m) : base(0x1B)
		{
			EnsureSize(37);
			WriteUInt(m.Serial);
			WriteUInt(0);
			WriteUShort(m.Body);
			WriteUShort((ushort)m.Position.X);
			WriteUShort((ushort)m.Position.Y);
			WriteUShort((ushort)m.Position.Z);
			WriteByte((byte)m.Direction);
			WriteByte(0);
			WriteUInt(uint.MaxValue);

			WriteUShort(0);
			WriteUShort(0);
			WriteUShort(6144);
			WriteUShort(4096);
		}
	}

	internal sealed class LoginComplete : PacketWriter
	{
		internal LoginComplete() : base(0x55)
		{
		}
	}

	internal sealed class DeathStatus : PacketWriter
	{
		internal DeathStatus(bool dead) : base(0x2C)
		{
			EnsureSize(2);
			WriteByte((byte)(dead ? 0 : 2));
		}
	}

	internal sealed class CurrentTime : PacketWriter
	{
		internal CurrentTime() : base(0x5B)
		{
			DateTime now = DateTime.UtcNow;
			EnsureSize(4);
			WriteByte((byte)now.Hour);
			WriteByte((byte)now.Minute);
			WriteByte((byte)now.Second);
		}
	}

	internal sealed class MapChange : PacketWriter
	{
		internal MapChange(byte map) : base(0xBF)
		{
			this.EnsureSize(6);

			WriteUShort(0x08);
			WriteByte(map);
		}
	}

	internal sealed class SeasonChange : PacketWriter
	{
		internal SeasonChange(int season, bool playSound) : base(0xBC)
		{
			EnsureSize(3);
			WriteByte((byte)season);
			WriteBool(playSound);
		}
	}

	internal sealed class SupportedFeatures : PacketWriter
	{
		//private static int m_Value = 0x801F;
		internal SupportedFeatures(ushort val) : base(0xB9)
		{
			EnsureSize(3);
			WriteUShort(val); // 0x01 = T2A, 0x02 = LBR
		}
	}

	internal sealed class MapPatches : PacketWriter
	{
		internal MapPatches(int[] patches) : base(0xBF)
		{
			EnsureSize(9 + (4 * patches.Length));

			WriteUShort(0x0018);

			WriteUInt((uint)(patches.Length / 2));

			for (int i = 0; i < patches.Length; i++)
				WriteUInt((uint)patches[i]);
		}
	}

	internal sealed class MobileAttributes : PacketWriter
	{
		internal MobileAttributes(PlayerMobile m) : base(0x2D)
		{
			EnsureSize(17);
			WriteUInt(m.Serial);

			WriteUShort(m.HitsMax);
			WriteUShort(m.Hits);

			WriteUShort(m.ManaMax);
			WriteUShort(m.Mana);

			WriteUShort(m.StaminaMax);
			WriteUShort(m.Stamina);
		}
	}

	internal sealed class SetWarMode : PacketWriter
	{
		internal SetWarMode(bool mode) : base(0x72)
		{
			EnsureSize(5);
			WriteBool(mode);
			WriteByte(0x00);
			WriteByte(0x32);
			WriteByte(0x00);
			//Fill();
		}
	}

	internal sealed class OpenDoorMacro : PacketWriter
	{
		internal OpenDoorMacro() : base(0x12)
		{
			EnsureSize(5);
			WriteByte((byte)0x58);
			WriteByte((byte)0);
		}
	}

	internal sealed class PersonalLightLevel : PacketWriter
	{
		internal PersonalLightLevel(sbyte level) : base(0x4E)
		{
			EnsureSize(6);
			WriteUInt(UOSObjects.Player.Serial);
			WriteSByte(level);
		}
	}

	internal sealed class GlobalLightLevel : PacketWriter
	{
		internal GlobalLightLevel(sbyte level) : base(0x4F)
		{
			EnsureSize(2);
			WriteSByte(level);
		}
	}

	internal sealed class DisplayPaperdoll : PacketWriter
	{
		internal DisplayPaperdoll(UOMobile m, string text) : base(0x88)
		{
			EnsureSize(66);
			WriteUInt(m.Serial);
			WriteASCII(text, 60);
			WriteByte((byte)(m.Warmode ? 1 : 0));
		}
	}

	internal sealed class ContextMenuRequest : PacketWriter
	{
		internal ContextMenuRequest(uint entity) : base(0xBF)
		{
			EnsureSize(1 + 2 + 2 + 4);
			WriteUShort(0x13);
			WriteUInt(entity);
		}
	}

	internal sealed class ContextMenuResponse : PacketWriter
	{
		internal ContextMenuResponse(uint entity, ushort idx) : base(0xBF)
		{
			EnsureSize(1 + 2 + 2 + 4 + 2);

			WriteUShort(0x15);
			WriteUInt(entity);
			WriteUShort(idx);
		}
	}

	internal sealed class SetUpdateRange : PacketWriter
	{
		internal SetUpdateRange(int range) : base(0xC8)
		{
			EnsureSize(2);
			WriteByte((byte)range);
		}
	}

	internal sealed class PlaySound : PacketWriter
	{
		internal PlaySound(int sound) : base(0x54)
		{
			EnsureSize(12);
			WriteByte(0x01); //(0x00=quiet, repeating, 0x01=single normally played sound effect)
			WriteUShort((ushort)sound);
			WriteUShort(0);
			WriteUShort((ushort)UOSObjects.Player.Position.X);
			WriteUShort((ushort)UOSObjects.Player.Position.Y);
			WriteUShort((ushort)UOSObjects.Player.Position.Z);
		}
	}

	internal sealed class DesignStateGeneral : PacketWriter
	{
		internal DesignStateGeneral(House house) : base(0xBF)
		{
			EnsureSize(13);

			WriteUShort(0x1D);
			WriteUInt(house.Serial);
			WriteUInt(house.Revision);
		}
	}

	internal sealed class StringQueryResponse : PacketWriter
	{
		internal StringQueryResponse(uint serial, byte type, byte index, bool ok, string resp) : base(0xAC)
		{
			if (resp == null)
				resp = String.Empty;

			this.EnsureSize(1 + 2 + 4 + 1 + 1 + 1 + 2 + resp.Length + 1);

			WriteUInt(serial);
			WriteByte(type);
			WriteByte(index);
			WriteBool(ok);
			WriteUShort((ushort)(resp.Length + 1));
			WriteASCII(resp);
		}
	}

	internal class DesignStateDetailed : PacketWriter
	{
		internal const int MaxItemsPerStairBuffer = 750;

		private static byte[][] m_PlaneBuffers;
		private static bool[] m_PlaneUsed;

		private static byte[][] m_StairBuffers;

		private static byte[] m_InflatedBuffer = new byte[0x2000];
		private static byte[] m_DeflatedBuffer = new byte[0x2000];

		internal static void Clear(byte[] buffer, int size)
		{
			for (int i = 0; i < size; ++i)
				buffer[i] = 0;
		}

		internal DesignStateDetailed(uint serial, uint revision, int xMin, int yMin, int xMax, int yMax, MultiTileEntry[] tiles) : base(0xD8)
		{
			EnsureSize(17 + (tiles.Length * 5));

			WriteByte(0x03); // Compression Type
			WriteByte(0x00); // Unknown
			WriteUInt(serial);
			WriteUInt(revision);
			WriteUShort((ushort)tiles.Length);
			WriteUShort(0); // Buffer length : reserved
			WriteByte(0); // Plane count : reserved

			int totalLength = 1; // includes plane count

			int width = (xMax - xMin) + 1;
			int height = (yMax - yMin) + 1;

			if (m_PlaneBuffers == null)
			{
				m_PlaneBuffers = new byte[9][];
				m_PlaneUsed = new bool[9];

				for (int i = 0; i < m_PlaneBuffers.Length; ++i)
					m_PlaneBuffers[i] = new byte[0x400];

				m_StairBuffers = new byte[6][];

				for (int i = 0; i < m_StairBuffers.Length; ++i)
					m_StairBuffers[i] = new byte[MaxItemsPerStairBuffer * 5];
			}
			else
			{
				for (int i = 0; i < m_PlaneUsed.Length; ++i)
					m_PlaneUsed[i] = false;

				Clear(m_PlaneBuffers[0], width * height * 2);

				for (int i = 0; i < 4; ++i)
				{
					Clear(m_PlaneBuffers[1 + i], (width - 1) * (height - 2) * 2);
					Clear(m_PlaneBuffers[5 + i], width * (height - 1) * 2);
				}
			}

			int totalStairsUsed = 0;

			for (int i = 0; i < tiles.Length; ++i)
			{
				MultiTileEntry mte = tiles[i];
				int x = mte.m_OffsetX - xMin;
				int y = mte.m_OffsetY - yMin;
				int z = mte.m_OffsetZ;
				int plane, size;
				bool floor = false;
				try
				{
					floor = (TileDataLoader.Instance.StaticData[mte.m_ItemID & (TileDataLoader.Instance.StaticData.Length - 1)].Height <= 0);
				}
				catch
				{
				}

				switch (z)
				{
					case 0: plane = 0; break;
					case 7: plane = 1; break;
					case 27: plane = 2; break;
					case 47: plane = 3; break;
					case 67: plane = 4; break;
					default:
					{
						int stairBufferIndex = (totalStairsUsed / MaxItemsPerStairBuffer);
						byte[] stairBuffer = m_StairBuffers[stairBufferIndex];

						int byteIndex = (totalStairsUsed % MaxItemsPerStairBuffer) * 5;

						stairBuffer[byteIndex++] = (byte)((mte.m_ItemID >> 8) & 0x3F);
						stairBuffer[byteIndex++] = (byte)mte.m_ItemID;

						stairBuffer[byteIndex++] = (byte)mte.m_OffsetX;
						stairBuffer[byteIndex++] = (byte)mte.m_OffsetY;
						stairBuffer[byteIndex++] = (byte)mte.m_OffsetZ;

						++totalStairsUsed;

						continue;
					}
				}

				if (plane == 0)
				{
					size = height;
				}
				else if (floor)
				{
					size = height - 2;
					x -= 1;
					y -= 1;
				}
				else
				{
					size = height - 1;
					plane += 4;
				}

				int index = ((x * size) + y) * 2;

				m_PlaneUsed[plane] = true;
				m_PlaneBuffers[plane][index] = (byte)((mte.m_ItemID >> 8) & 0x3F);
				m_PlaneBuffers[plane][index + 1] = (byte)mte.m_ItemID;
			}

			int planeCount = 0;

			for (int i = 0; i < m_PlaneBuffers.Length; ++i)
			{
				if (!m_PlaneUsed[i])
					continue;

				++planeCount;

				int size = 0;

				if (i == 0)
					size = width * height * 2;
				else if (i < 5)
					size = (width - 1) * (height - 2) * 2;
				else
					size = width * (height - 1) * 2;

				byte[] inflatedBuffer = m_PlaneBuffers[i];

				int deflatedLength = m_DeflatedBuffer.Length;
				ZLibManaged.Compress(m_DeflatedBuffer, ref deflatedLength, inflatedBuffer);
				WriteByte((byte)(0x20 | i));
				WriteByte((byte)size);
				WriteByte((byte)deflatedLength);
				WriteByte((byte)(((size >> 4) & 0xF0) | ((deflatedLength >> 8) & 0xF)));
				WriteBytes(m_DeflatedBuffer, 0, deflatedLength);

				totalLength += 4 + deflatedLength;
			}

			int totalStairBuffersUsed = (totalStairsUsed + (MaxItemsPerStairBuffer - 1)) / MaxItemsPerStairBuffer;

			for (int i = 0; i < totalStairBuffersUsed; ++i)
			{
				++planeCount;

				int count = (totalStairsUsed - (i * MaxItemsPerStairBuffer));

				if (count > MaxItemsPerStairBuffer)
					count = MaxItemsPerStairBuffer;

				int size = count * 5;

				byte[] inflatedBuffer = m_StairBuffers[i];

				int deflatedLength = m_DeflatedBuffer.Length;
				ZLibManaged.Compress(m_DeflatedBuffer, ref deflatedLength, inflatedBuffer);
				WriteByte((byte)(9 + i));
				WriteByte((byte)size);
				WriteByte((byte)deflatedLength);
				WriteByte((byte)(((size >> 4) & 0xF0) | ((deflatedLength >> 8) & 0xF)));
				WriteBytes(m_DeflatedBuffer, 0, deflatedLength);

				totalLength += 4 + deflatedLength;
			}

			Seek(15);

			WriteUShort((ushort)totalLength); // Buffer length
			WriteByte((byte)planeCount); // Plane count
		}
	}

	internal sealed class PromptResponse : PacketWriter
	{
		internal PromptResponse(uint serial, uint promptid, uint operation, string text, string lang = "ENU")
			: base(0xC2)
		{
			if (text != "")
				EnsureSize(2 + 4 + 4 + 4 + 4 + (text.Length * 2));
			else
			{
				EnsureSize(18);
			}

			WriteUInt(serial);
			WriteUInt(promptid);
			WriteUInt(operation);

			if (string.IsNullOrEmpty(lang))
				lang = "ENU";

			WriteASCII(lang.ToUpper(), 4);

			if (text != "")
				WriteUnicode(text);
		}
	}

	internal sealed class RemoveObject : PacketWriter
	{
		internal RemoveObject(UOEntity ent) : base(0x1D)
		{
			EnsureSize(5);
			WriteUInt(ent.Serial);
		}

		internal RemoveObject(uint serial) : base(0x1D)
		{
			EnsureSize(5);
			WriteUInt(serial);
		}
	}

	internal sealed class WorldItem : PacketWriter
	{
		internal WorldItem(UOItem item) : base(0x1A)
		{
			EnsureSize(20);

			// 14 base length
			// +2 - Amount
			// +2 - Hue
			// +1 - Flags

			uint serial = item.Serial;
			ushort itemID = item.ItemID;
			ushort amount = item.Amount;
			int x = item.Position.X;
			int y = item.Position.Y;
			ushort hue = item.Hue;
			byte flags = item.GetPacketFlags();
			byte direction = (byte)item.Direction;

			if (amount != 0)
				serial |= 0x80000000;
			else
				serial &= 0x7FFFFFFF;
			WriteUInt(serial);
			WriteUShort((ushort)(itemID & 0x7FFF));
			if (amount != 0)
				WriteUShort(amount);

			x &= 0x7FFF;
			if (direction != 0)
				x |= 0x8000;
			WriteUShort((ushort)x);

			y &= 0x3FFF;
			if (hue != 0)
				y |= 0x8000;
			if (flags != 0)
				y |= 0x4000;

			WriteUShort((ushort)y);
			if (direction != 0)
				WriteByte(direction);
			WriteSByte((sbyte)item.Position.Z);
			if (hue != 0)
				WriteUShort(hue);
			if (flags != 0)
				WriteByte(flags);
		}
	}
}
