using System.Xml;
using System.Collections.Generic;

using ClassicUO.Network;
using ClassicUO.Game.UI.Controls;

namespace Assistant
{
	internal abstract class Filter
	{
		protected Filter(string name)
		{
			Name = name;
			XmlName = $"Filter{Name.Replace(" ", "").Replace("'", "")}";
			m_Callback = new PacketViewerCallback(OnFilter);
		}

        internal static void Initialize()
        {
            SoundFilter.Configure();
			TextMessageFilter.Configure();
			LocMessageFilter.Configure();
			DeathFilter.Configure();
			StaffItemFilter.Configure();
			SnoopMessageFilter.Configure();
			TradeFilter.Configure();
        }

		internal static List<Filter> List { get; } = new List<Filter>();
        internal bool Enabled { get; private set; }

		internal static void Register(Filter filter)
		{
			List.Add(filter);
		}

        internal abstract void OnFilter(Packet p, PacketHandlerEventArgs args);
        internal abstract byte[] PacketIDs { get; }
        internal string Name { get; }
		internal string XmlName { get; }

		private PacketViewerCallback m_Callback { get; }

		public override string ToString()
		{
			return Name;
		}

        internal virtual void OnCheckChanged(bool enabled)
		{
			if (Enabled != enabled)
			{
				Enabled = enabled;
				if (Enabled)
				{
					for (int i = 0; i < PacketIDs.Length; i++)
						PacketHandler.RegisterServerToClientViewer(PacketIDs[i], m_Callback);
				}
				else
				{
					for (int i = 0; i < PacketIDs.Length; i++)
						PacketHandler.RemoveServerToClientViewer(PacketIDs[i], m_Callback);
				}
			}
		}

		internal void SaveProfile(XmlTextWriter xml)
		{
			if (xml != null)
			{
				foreach (Filter filter in List)
				{
					xml.WriteStartElement("data");
					xml.WriteAttributeString("name", XmlName);
					xml.WriteString($"{filter.Enabled}");
					xml.WriteEndElement();
				}
			}
		}
	}

    internal class SoundFilter : Filter
    {
        internal static void Configure()
        {
            Register(new SoundFilter("Bard's Music", GetMultiRange(0x2EA, 0x2ED, 0x3CA, 0x3E2, 0x3FF, 0x417, 0x497, 0x4AF, 0x5D3, 0x5D9, 0x5DA, 0x5E0, 0x5ED, 0x605)));
            Register(new SoundFilter("Dog Sounds", GetRange(0x85, 0x89)));
            Register(new SoundFilter("Cat Sounds", GetRange(0x69, 0x6D)));
            Register(new SoundFilter("Horse Sounds", GetRange(0xA8, 0xAC)));
            Register(new SoundFilter("Sheep Sounds", GetRange(0xD6, 0xDA)));
            Register(new SoundFilter("Spirit Speak Sound", 0x24A));
            Register(new SoundFilter("Fizzle Sound", 0x5C));
            Register(new SoundFilter("Backpack Sounds", 0x48));
            Register(new SoundFilter("Deer Sounds", 0x82, 0x83, 0x84, 0x85, 0x2BE, 0x2BF, 0x2C0, 0x4CB, 0x4CC));
            Register(new SoundFilter("Cyclop Titan Sounds", 0x25D, 0x25E, 0x25F, 0x260, 0x261, 0x262, 0x263, 0x264, 0x265, 0x266));
            Register(new SoundFilter("Bull Sounds", 0x065, 0x066, 0x067, 0x068, 0x069));
            Register(new SoundFilter("Dragon Sounds", 0x2C8, 0x2C9, 0x2CA, 0x2CB, 0x2CC, 0x2CD, 0x2CE, 0x2CF, 0x2D0, 0x2D1, 0x2D2, 0x2D3, 0x2D4, 0x2D5, 0x2D6, 0x16B, 0x16C, 0x16D, 0x16E, 0x16F, 0x15F, 0x160, 0x161));
            Register(new SoundFilter("Chicken Sounds", 0x06F, 0x070, 0x071, 0x072, 0x073));
			Register(new SoundFilter("Emote Sounds", GetMultiRange(0x30A, 0x338, 0x419, 0x44A)));
        }
		internal static void CleanUP()
		{
			_Sounds.Clear();
		}

        internal static ushort[] GetRange(ushort min, ushort max)
        {
            if (max < min)
                return new ushort[0];

            ushort[] range = new ushort[max - min + 1];
            for (ushort i = min; i <= max; i++)
                range[i - min] = i;
            return range;
        }

		internal static ushort[] GetMultiRange(params ushort[] multirange)
		{
			if (multirange.Length == 0 || (multirange.Length % 2) != 0)
				return new ushort[0];
			List<ushort> range = new List<ushort>();
			for(int i = 0; i < multirange.Length; i += 2)
			{
				for(ushort x = multirange[i]; x <= multirange[i+1]; x++)
				{
					range.Add(x);
				}
			}
			return range.ToArray();
		}

		private static HashSet<ushort> _Sounds = new HashSet<ushort>();
        private ushort[] m_Sounds;

        private SoundFilter(string name, params ushort[] blockSounds) : base(name)
        {
            m_Sounds = blockSounds;
        }

		private static byte[] Instance { get; } = new byte[] { 0x54 };
		internal override byte[] PacketIDs => Instance;

        internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
        {
            p.ReadByte(); // flags

            ushort sound = p.ReadUShort();
			if (_Sounds.Contains(sound))
				args.Block = true;
        }

		internal override void OnCheckChanged(bool enabled)
		{
			base.OnCheckChanged(enabled);
			for (int i = 0; i < m_Sounds.Length; i++)
			{
				if(enabled)
					_Sounds.Add(m_Sounds[i]);
				else
					_Sounds.Remove(m_Sounds[i]);
			}
		}
	}

	internal class TextMessageFilter : Filter
	{
		internal static void Configure()
		{
		}

		private string[] m_Strings;
		private MessageType m_Type;

		private TextMessageFilter(string name, MessageType type, string[] msgs) : base(name)
		{
			m_Strings = msgs;
			m_Type = type;
		}

		private static byte[] Instance { get; } = new byte[] { 0x1C };
		internal override byte[] PacketIDs => Instance;

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			if (args.Block)
				return;

			// 0, 1, 2
			uint serial = p.ReadUInt(); // 3, 4, 5, 6
			ushort body = p.ReadUShort(); // 7, 8
			MessageType type = (MessageType)p.ReadByte(); // 9

			if (type != m_Type)
				return;

			ushort hue = p.ReadUShort(); // 10, 11
			ushort font = p.ReadUShort();
			string name = p.ReadASCII(30);
			string text = p.ReadASCII();

			for (int i = 0; i < m_Strings.Length; i++)
			{
				if (text.IndexOf(m_Strings[i]) != -1)
				{
					args.Block = true;
					return;
				}
			}
		}
	}

	internal class LocMessageFilter : Filter
	{
		internal static void Configure()
		{
		}

		private int[] m_Nums;
		private MessageType m_Type;

		private LocMessageFilter(string name, MessageType type, int[] msgs) : base(name)
		{
			m_Nums = msgs;
			m_Type = type;
		}

		private static byte[] Instance { get; } = new byte[] { 0xC1 };
		internal override byte[] PacketIDs => Instance;

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			if (args.Block)
				return;

			uint serial = p.ReadUInt();
			ushort body = p.ReadUShort();
			MessageType type = (MessageType)p.ReadByte();
			ushort hue = p.ReadUShort();
			ushort font = p.ReadUShort();
			int num = (int)p.ReadUInt();

			// paladin spells
			if (num >= 1060718 && num <= 1060727)
				type = MessageType.Spell;
			if (type != m_Type)
				return;

			for (int i = 0; i < m_Nums.Length; i++)
			{
				if (num == m_Nums[i])
				{
					args.Block = true;
					return;
				}
			}
		}
	}

	internal class DeathFilter : Filter
	{
		internal static void Configure()
		{
			Register(new DeathFilter());
		}

		internal DeathFilter() : base("Death")
		{
		}

		private static byte[] Instance { get; } = new byte[] { 0x2C };
		internal override byte[] PacketIDs => Instance;

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			args.Block = true;
		}
	}

	internal class StaffItemFilter : Filter
	{
		internal static void Configure()
		{
			Register(new StaffItemFilter());
		}

		internal StaffItemFilter() : base("Staff Items")
		{
		}

		private static byte[] Instance { get; } = new byte[] { 0x1A };
		internal override byte[] PacketIDs => Instance;

		private static bool IsStaffItem(ushort itemID)
		{
			return itemID == 0x36FF || // LOS blocker
				   itemID == 0x1183; // Movement blocker
		}

		private static bool IsStaffItem(UOItem i)
		{
			return i.OnGround && (IsStaffItem(i.ItemID) || !i.Visible);
		}

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			uint serial = p.ReadUInt();
			ushort itemID = p.ReadUShort();

			if ((serial & 0x80000000) != 0)
				p.ReadUShort(); // amount

			if ((itemID & 0x8000) != 0)
				itemID = (ushort)((itemID & 0x7FFF) + p.ReadSByte()); // itemID offset

			ushort x = p.ReadUShort();
			ushort y = p.ReadUShort();

			if ((x & 0x8000) != 0)
				p.ReadByte(); // direction

			short z = p.ReadSByte();

			if ((y & 0x8000) != 0)
				p.ReadUShort(); // hue

			bool visible = true;
			if ((y & 0x4000) != 0)
			{
				int flags = p.ReadByte();

				visible = ((flags & 0x80) == 0);
			}

			if (IsStaffItem(itemID) || !visible)
				args.Block = true;
		}

		internal override void OnCheckChanged(bool enabled)
		{
			base.OnCheckChanged(enabled);
			if (UOSObjects.Player != null)
			{
				if (Enabled)
				{
					foreach (UOItem i in UOSObjects.Items.Values)
					{
						if (IsStaffItem(i))
							Engine.Instance.SendToClient(new RemoveObject(i));
					}
				}
				else
				{
					foreach (UOItem i in UOSObjects.Items.Values)
					{
						if (IsStaffItem(i))
							Engine.Instance.SendToClient(new WorldItem(i));
					}
				}
			}
		}
	}

	internal class SnoopMessageFilter : Filter
	{
		internal static void Configure()
		{
			Register(new SnoopMessageFilter());
		}

		private SnoopMessageFilter() : base("Snooping Messages")
		{
		}

		private static byte[] Instance { get; } = new byte[] { 0x1C, 0xAE };
		internal override byte[] PacketIDs => Instance;

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			if (args.Block)
				return;

			// 0, 1, 2
			uint serial = p.ReadUInt(); // 3, 4, 5, 6
			ushort body = p.ReadUShort(); // 7, 8
			MessageType type = (MessageType)p.ReadByte(); // 9

			if (type != MessageType.System)
				return;

			ushort hue = p.ReadUShort(); // 10, 11
			ushort font = p.ReadUShort();
			string text;
			if (p.ID == 0xAE)
			{
				p.ReadASCII(4);
				p.ReadASCII(30);
				text = p.ReadUnicode();
			}
			else
			{
				p.ReadASCII(30);
				text = p.ReadASCII();
			}

			if(!string.IsNullOrEmpty(text) && text.StartsWith("You notice") && text.Contains("peek") && text.EndsWith("belongings!") && text.Contains(UOSObjects.Player.Name))
			{
				args.Block = true;
			}
		}
	}

	internal class TradeFilter : Filter
	{
		internal static void Configure()
		{
			Register(new TradeFilter());
		}

		internal TradeFilter() : base("Trade Window")
		{
		}

		private static byte[] Instance { get; } = new byte[] { 0x6F };
		internal override byte[] PacketIDs => Instance;

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			args.Block = true;
			p.Skip(1);
			uint serial = p.ReadUInt();
			Engine.Instance.SendToServer(new TradeCancel(serial));
		}

		private sealed class TradeCancel : PacketWriter
		{
			internal TradeCancel(uint serial) : base(0x6F)
			{
				WriteByte(0x01);//cancel
				WriteUInt(serial);
			}
		}
	}

	/*internal class DragonFilter : Filter
	{
		internal static void Initialize()
		{
			Filter.Register(new DragonFilter());
		}

		internal DragonFilter() : base("Dragon Graphic")
		{
		}

		internal override byte[] PacketIDs { get { return new byte[] { 0x77, 0x78 }; } }

		internal override void OnFilter(Packet p, PacketHandlerEventArgs args)
		{
			uint serial = p.ReadUInt();
			if (SerialHelper.IsMobile(serial))
			{
				UOMobile m = UOSObjects.FindMobile(serial);
				if (m == null)
					UOSObjects.AddMobile(m = new UOMobile(serial));
				ushort body = p.ReadUShort();
			}
		}


		internal override void OnCheckChanged()
		{
			base.OnCheckChanged();
			AnimationsLoader.Instance.GetBodyAnimationGroup()
		}
	}*/
}
