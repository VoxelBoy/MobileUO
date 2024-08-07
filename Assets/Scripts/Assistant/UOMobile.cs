using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

//using Assistant.Agents;
//using Assistant.UI;

using ClassicUO.Network;
using ClassicUO.IO.Resources;

namespace Assistant
{
    [Flags]
    internal enum Direction : byte
    {
        North = 0x0,
        Right = 0x1,
        East = 0x2,
        Down = 0x3,
        South = 0x4,
        Left = 0x5,
        West = 0x6,
        Up = 0x7,
        Running = 0x80,
        ValueMask = 0x87
    }

    //internal enum BodyType : byte
    //{
    //    Empty,
    //    Monster,
    //    Sea_Monster,
    //    Animal,
    //    Human,
    //    Equipment
    //}

    internal class UOMobile : UOEntity
    {
        private ushort m_Body;
        private Direction m_Direction;
        private string m_Name;

        private byte m_Notoriety;

        private bool m_Visible;
        private bool m_Female;
        private bool m_Poisoned;
        private bool m_Blessed;
        private bool m_Warmode;
        private bool m_Flying;
        private bool m_Paralyzed;
        private bool m_IgnoreMobiles;
        private bool m_Unknown3;

        private bool m_CanRename;
        //end new

        private ushort m_HitsMax, m_Hits;
        protected ushort m_StamMax, m_Stam, m_ManaMax, m_Mana;

        private List<UOItem> m_Items = new List<UOItem>();

        private byte m_Map;

        public override string ToString()
        {
            return $"{Name} 0x{Serial:X8}";
        }

        internal override string GetName()
        {
            return $"{Name} 0x{Serial:X}";
        }

        internal UOMobile(uint serial) : base(serial)
        {
            m_Map = UOSObjects.Player == null ? (byte)0 : UOSObjects.Player.Map;
            m_Visible = true;

            //Agent.InvokeMobileCreated(this);
        }

        internal string Name
        {
            get
            {
                if (m_Name == null)
                    return "";
                else
                    return m_Name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != m_Name)
                {
                    string trim = ClilocConversion(value);
                    if (trim.Length > 0)
                    {
                        m_Name = trim;
                    }
                }
            }
        }

        private static StringBuilder _InternalSB = new StringBuilder(32);

        private static string ClilocConversion(string old)
        {
            _InternalSB.Clear();
            string[] arr = old.Split(' ');
            for (int i = 0; i < arr.Length; i++)
            {
                string ss = arr[i];
                if (ss.Length > 1 && ss.StartsWith("#"))
                {
                    if (int.TryParse(ss.Substring(1), out int x))
                    {
                        ss = ClilocLoader.Instance.GetString(x);
                        if (string.IsNullOrEmpty(ss))
                        {
                            ss = arr[i];
                        }
                    }
                }

                _InternalSB.Append(ss);
                _InternalSB.Append(' ');
            }

            return _InternalSB.ToString().Trim();
        }

        internal ushort Body
        {
            get { return m_Body; }
            set { m_Body = value; }
        }

        internal Direction Direction
        {
            get { return m_Direction; }
            set
            {
                if (value != m_Direction)
                {
                    var oldDir = m_Direction;
                    m_Direction = value;
                    OnDirectionChanging(oldDir);
                }
            }
        }

        internal bool Visible
        {
            get { return m_Visible; }
            set { m_Visible = value; }
        }

        internal bool Poisoned
        {
            get { return m_Poisoned; }
            set { m_Poisoned = value; }
        }

        internal bool Blessed
        {
            get { return m_Blessed; }
            set { m_Blessed = value; }
        }

        public bool Paralyzed
        {
            get { return m_Paralyzed; }
            set { m_Paralyzed = value; }
        }

        public bool Flying
        {
            get { return m_Flying; }
            set { m_Flying = value; }
        }

        internal bool IsGhost
        {
            get
            {
                return m_Body == 402
                       || m_Body == 403
                       || m_Body == 607
                       || m_Body == 608
                       || m_Body == 970;
            }
        }

        internal bool IsHuman
        {
            get
            {
                return m_Body == 400
                        || m_Body == 401
                        || m_Body == 402
                        || m_Body == 403
                        || m_Body == 605
                        || m_Body == 606
                        || m_Body == 607
                        || m_Body == 608
                        || m_Body == 970; //player ghost
            }
        }

        internal bool IsMonster
        {
            get { return !IsHuman; }
        }

        internal bool Unknown2
        {
            get { return m_IgnoreMobiles; }
            set { m_IgnoreMobiles = value; }
        }

        internal bool Unknown3
        {
            get { return m_Unknown3; }
            set { m_Unknown3 = value; }
        }

        internal bool CanRename //A pet! (where the health bar is open, we can add this to an arraylist of mobiles...
        {
            get { return m_CanRename; }
            set { m_CanRename = value; }
        }
        //end new

        internal override ushort Graphic => Body;

        internal bool Warmode
        {
            get { return m_Warmode; }
            set { m_Warmode = value; }
        }

        internal bool Female
        {
            get { return m_Female; }
            set { m_Female = value; }
        }

        internal byte Notoriety
        {
            get { return m_Notoriety; }
            set
            {
                if (value != Notoriety)
                {
                    OnNotoChange(m_Notoriety, value);
                    m_Notoriety = value;
                }
            }
        }

        protected virtual void OnNotoChange(byte old, byte cur)
        {
        }

        // grey, blue, green, 'canbeattacked'
        private static uint[] m_NotoHues = new uint[8]
        {
            // hue color #30
            0x000000, // black		unused 0
            0x30d0e0, // blue		0x0059 1 
            0x60e000, // green		0x003F 2
            0x9090b2, // greyish	0x03b2 3
            0x909090, // grey		   "   4
            0xd88038, // orange		0x0090 5
            0xb01000, // red		0x0022 6
            0xe0e000 // yellow		0x0035 7
        };

        private static int[] m_NotoHuesInt = new int[8]
        {
            1, // black		unused 0
            0x059, // blue		0x0059 1
            0x03F, // green		0x003F 2
            0x3B2, // greyish	0x03b2 3
            0x3B2, // grey		   "   4
            0x090, // orange		0x0090 5
            0x022, // red		0x0022 6
            0x035, // yellow		0x0035 7
        };

        internal uint GetNotorietyColor()
        {
            if (m_Notoriety < 0 || m_Notoriety >= m_NotoHues.Length)
                return m_NotoHues[0];
            else
                return m_NotoHues[m_Notoriety];
        }

        internal int GetNotorietyColorInt()
        {
            if (m_Notoriety < 0 || m_Notoriety >= m_NotoHues.Length)
                return m_NotoHuesInt[0];
            else
                return m_NotoHuesInt[m_Notoriety];
        }

        internal byte GetStatusCode()
        {
            if (m_Poisoned)
                return 1;
            else
                return 0;
        }

        internal ushort HitsMax
        {
            get { return m_HitsMax; }
            set { m_HitsMax = value; }
        }

        internal ushort Hits
        {
            get { return m_Hits; }
            set { m_Hits = value; }
        }

        internal ushort Stam
        {
            get { return m_Stam; }
            set { m_Stam = value; }
        }

        internal ushort StamMax
        {
            get { return m_StamMax; }
            set { m_StamMax = value; }
        }

        internal ushort Mana
        {
            get { return m_Mana; }
            set { m_Mana = value; }
        }

        internal ushort ManaMax
        {
            get { return m_ManaMax; }
            set { m_ManaMax = value; }
        }


        internal byte Map
        {
            get { return m_Map; }
            set
            {
                if (m_Map != value)
                {
                    OnMapChange(m_Map, value);
                    m_Map = value;
                }
            }
        }

        internal virtual void OnMapChange(byte old, byte cur)
        {
        }

        internal void AddItem(UOItem item)
        {
            m_Items.Add(item);
        }

        internal void RemoveItem(UOItem item)
        {
            m_Items.Remove(item);
        }

        internal override void Remove()
        {
            List<UOItem> rem = new List<UOItem>(m_Items);
            m_Items.Clear();

            for (int i = 0; i < rem.Count; i++)
                rem[i].Remove();

            if (!InParty)
            {
                base.Remove();
                UOSObjects.RemoveMobile(this);
            }
            else
            {
                Visible = false;
            }
        }

        internal bool InParty
        {
            get { return PacketHandlers.Party.Contains(this.Serial); }
        }

        internal UOItem GetItemOnLayer(Layer layer)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                UOItem item = m_Items[i];
                if (item.Layer == layer)
                    return item;
            }

            return null;
        }

        internal UOItem Backpack
        {
            get { return GetItemOnLayer(Layer.Backpack); }
        }

        internal UOItem Quiver
        {
            get
            {
                UOItem item = GetItemOnLayer(Layer.Cloak);

                if (item != null && item.IsContainer)
                    return item;
                else
                    return null;
            }
        }

        internal UOItem FindItemByID(ushort id)
        {
            for (int i = 0; i < Contains.Count; i++)
            {
                UOItem item = Contains[i];
                if (item.ItemID == id)
                    return item;
            }

            return null;
        }

        internal override void OnPositionChanging(Point3D oldPos)
        {
            /*if (this != UOSObjects.Player && Engine.MainWindow.MapWindow != null)
                Engine.MainWindow.SafeAction(s => s.MapWindow.CheckLocalUpdate(this));*/

            base.OnPositionChanging(oldPos);
        }

        internal virtual void OnDirectionChanging(Direction oldDir)
        {
        }

        internal int GetPacketFlags()
        {
            int flags = 0x0;

            if (m_Paralyzed)
                flags |= 0x01;

            if (m_Female)
                flags |= 0x02;

            if (m_Poisoned && !PacketHandlers.UseNewStatus)
                flags |= 0x04;

            if (m_Flying)
                flags |= 0x04;

            if (m_Blessed)
                flags |= 0x08;

            if (m_Warmode)
                flags |= 0x40;

            if (!m_Visible)
                flags |= 0x80;

            if (m_IgnoreMobiles)
                flags |= 0x10;

            if (m_Unknown3)
                flags |= 0x20;

            return flags;
        }

        internal void ProcessPacketFlags(byte flags)
        {
            if (!PacketHandlers.UseNewStatus)
                m_Poisoned = (flags & 0x04) != 0;
            else
                m_Flying = (flags & 0x04) != 0;

            m_Paralyzed = (flags & 0x01) != 0; //new
            m_Female = (flags & 0x02) != 0;
            m_Blessed = (flags & 0x08) != 0;
            m_IgnoreMobiles = (flags & 0x10) != 0; //new
            m_Unknown3 = (flags & 0x10) != 0; //new
            m_Warmode = (flags & 0x40) != 0;
            m_Visible = (flags & 0x80) == 0;
        }

        internal List<UOItem> Contains
        {
            get { return m_Items; }
        }

        internal void OverheadMessageFrom(int hue, string from, string format, params object[] args)
        {
            OverheadMessageFrom(hue, from, String.Format(format, args));
        }

        internal void OverheadMessageFrom(int hue, string from, string text, bool ascii = false)
        {
            if (ascii)
            {
                Engine.Instance.SendToClient(new AsciiMessage(Serial, m_Body, MessageType.Regular, hue, 3, from, text));
            }
            else
            {
                Engine.Instance.SendToClient(new UnicodeMessage(Serial, m_Body, MessageType.Regular, hue, 3, "ENU", from, text));
            }
        }

        internal void OverheadMessage(int hue, string format, params object[] args)
        {
            OverheadMessage(hue, String.Format(format, args));
        }

        internal void OverheadMessage(int hue, string text)
        {
            OverheadMessageFrom(hue, "UOSteam", text);
        }

        private Point2D m_ButtonPoint = Point2D.Zero;

        internal Point2D ButtonPoint
        {
            get { return m_ButtonPoint; }
            set { m_ButtonPoint = value; }
        }

        private static List<Layer> _layers = new List<Layer>
        {
            Layer.Backpack,
            Layer.Invalid,
            Layer.FirstValid,
            Layer.RightHand,
            Layer.LeftHand,
            Layer.Shoes,
            Layer.Pants,
            Layer.Shirt,
            Layer.Head,
            Layer.Neck,
            Layer.Gloves,
            Layer.InnerTorso,
            Layer.MiddleTorso,
            Layer.Arms,
            Layer.Cloak,
            Layer.OuterTorso,
            Layer.OuterLegs,
            Layer.InnerLegs,
            Layer.LastUserValid,
            Layer.Mount,
            Layer.LastValid,
            Layer.Hair
        };

        internal void ResetLayerHue()
        {
            if (IsGhost)
                return;

            foreach (Layer l in _layers)
            {
                UOItem i = GetItemOnLayer(l);

                if (i == null)
                    continue;

                if (i.ItemID == 0x204E && i.Hue == 0x08FD) // death shroud
                    i.ItemID = 0x1F03;

                Engine.Instance.SendToClient(new EquipmentItem(i, i.Hue, Serial));
            }
        }

        internal void SetLayerHue(int hue)
        {
            if (IsGhost)
                return;

            foreach (Layer l in _layers)
            {
                UOItem i = GetItemOnLayer(l);
                if (i == null)
                    continue;

                Engine.Instance.SendToClient(new EquipmentItem(i, (ushort)hue, Serial));
            }
        }

        internal Packet SetMobileHue(Packet p, int hue)
        {
            if (IsGhost)
                return p;
            WriteHueToPacket(p, (ushort)hue);
            return p;
        }

        private static void WriteHueToPacket(Packet p, ushort color)
        {
            p.Seek(p.Length - 3);
            p.WriteUShort(color);
            p.Seek(p.Length);
        }
    }
}

