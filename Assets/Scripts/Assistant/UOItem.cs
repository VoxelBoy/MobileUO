using System;
using System.IO;
using System.Collections.Generic;
//TODO: Agents
//using Assistant.Agents;

using ClassicUO.IO.Resources;
using ClassicUO.Game;

namespace Assistant
{
    internal enum Layer : byte
    {
        Invalid = 0x00,

        FirstValid = 0x01,

        RightHand = 0x01,
        LeftHand = 0x02,
        Shoes = 0x03,
        Pants = 0x04,
        Shirt = 0x05,
        Head = 0x06,
        Gloves = 0x07,
        Ring = 0x08,
        Talisman = 0x09,
        Neck = 0x0A,
        Hair = 0x0B,
        Waist = 0x0C,
        InnerTorso = 0x0D,
        Bracelet = 0x0E,
        Unused_xF = 0x0F,
        FacialHair = 0x10,
        MiddleTorso = 0x11,
        Earrings = 0x12,
        Arms = 0x13,
        Cloak = 0x14,
        Backpack = 0x15,
        OuterTorso = 0x16,
        OuterLegs = 0x17,
        InnerLegs = 0x18,

        LastUserValid = 0x18,

        Mount = 0x19,
        ShopBuy = 0x1A,
        ShopResale = 0x1B,
        ShopSell = 0x1C,
        Bank = 0x1D,

        LastValid = 0x1D
    }

    internal class UOItem : UOEntity
    {
        private ushort m_ItemID;
        private ushort m_Amount;
        private byte m_Direction;

        private bool m_Visible;
        private bool m_Movable;

        private Layer m_Layer;
        private string m_Name;
        private object m_Parent;
        private int m_Price;
        private string m_BuyDesc;
        private List<UOItem> m_Items;
        internal int ItemCount => m_Items.Count;

        private bool m_IsNew;
        private bool m_AutoStack;

        private byte[] m_HousePacket;
        private int m_HouseRev;

        private byte m_GridNum;

        internal UOItem(uint serial) : base(serial)
        {
            m_Items = new List<UOItem>();

            m_Visible = true;
            m_Movable = true;

            OnItemCreated?.Invoke(this);
        }

        public delegate void ItemCreatedEventHandler(UOItem item);
        public static event ItemCreatedEventHandler OnItemCreated;

        internal ushort ItemID
        {
            get { return m_ItemID; }
            set { m_ItemID = value; }
        }

        internal ushort Amount
        {
            get { return m_Amount; }
            set { m_Amount = value; }
        }

        internal byte Direction
        {
            get { return m_Direction; }
            set { m_Direction = value; }
        }

        internal bool Visible
        {
            get { return m_Visible; }
            set { m_Visible = value; }
        }

        internal bool Movable
        {
            get { return m_Movable; }
            set { m_Movable = value; }
        }

        internal string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(m_Name))
                {
                    return m_Name;
                }
                else
                {
                    return DisplayName;
                }
            }
            set
            {
                if (value != null)
                    m_Name = value.Trim();
                else
                    m_Name = null;
            }
        }

        internal string DisplayName
        {
            get
            {
                return TileDataInfo.Name.Replace("%", ""); 
            }
        }

        internal StaticTiles TileDataInfo => m_ItemID < TileDataLoader.Instance.StaticData.Length ? TileDataLoader.Instance.StaticData[m_ItemID] : TileDataLoader.Instance.StaticData[0];

        internal Layer Layer
        {
            get
            {
                
                if ((m_Layer < Layer.FirstValid || m_Layer > Layer.LastValid) &&
                    ((TileDataInfo.Flags & TileFlag.Wearable) != 0 ||
                     (TileDataInfo.Flags & TileFlag.Armor) != 0 ||
                     (TileDataInfo.Flags & TileFlag.Weapon) != 0
                    ))
                {
                    m_Layer = (Layer)TileDataInfo.Layer;
                }

                return m_Layer;
            }
            set { m_Layer = value; }
        }

        internal UOItem FindItemByID(ushort id, bool recurse = true, int hue = -1, Layer layer = Layer.Invalid, bool movable = false)
        {
            return RecurseFindItemByID(this, id, recurse, hue, layer, movable);
        }

        private static UOItem RecurseFindItemByID(UOItem current, ushort id, bool recurse, int hue, Layer layer, bool movable = false)
        {
            if (current != null && current.m_Items.Count > 0)
            {
                List<UOItem> list = current.m_Items;

                for (int i = 0; i < list.Count; ++i)
                {
                    UOItem item = list[i];

                    if (item.ItemID == id && (hue == -1 || hue == item.Hue) && (layer == Layer.Invalid || layer == item.Layer) && (!movable || item.Movable))
                    {
                        return item;
                    }
                    else if (recurse && item.IsContainer)
                    {
                        UOItem check = RecurseFindItemByID(item, id, recurse, hue, layer, movable);

                        if (check != null)
                        {
                            return check;
                        }
                    }
                }
            }

            return null;
        }

        internal List<UOItem> FindItemsByID(ushort id, bool recurse = true, int hue = -1, bool movable = false)
        {
            List<UOItem> items = new List<UOItem>();
            return RecurseFindItemsByID(this, items, id, recurse, hue, movable);
        }

        private static List<UOItem> RecurseFindItemsByID(UOItem current, List<UOItem> items, ushort id, bool recurse, int hue, bool movable = false)
        {
            if (current != null && current.m_Items.Count > 0)
            {
                List<UOItem> list = current.m_Items;

                for (int i = 0; i < list.Count; ++i)
                {
                    UOItem item = list[i];

                    if (item.ItemID == id && (hue == -1 || hue == item.Hue) && (!movable || item.Movable))
                    {
                        items.Add(item);
                    }
                    else if (recurse && item.IsContainer)
                    {
                        RecurseFindItemsByID(item, items, id, recurse, hue, movable);
                    }
                }
            }

            return items;
        }

        internal UOItem FindItemByID(HashSet<ushort> itemset, bool recurse = true, int hue = -1)
        {
            List<UOItem> items = new List<UOItem>();
            RecurseFindItemsByID(this, items, itemset, recurse, hue);
            if (items.Count > 0)
                return items[0];
            return null;
        }

        internal List<UOItem> FindItemsByID(HashSet<ushort> itemset, bool recurse = true, int hue = -1)
        {
            return RecurseFindItemsByID(this, new List<UOItem>(), itemset, recurse, hue);
        }

        private static List<UOItem> RecurseFindItemsByID(UOItem current, List<UOItem> items, HashSet<ushort> ids, bool recurse, int hue)
        {
            if (current != null && current.m_Items.Count > 0 && ids.Count > 0)
            {
                List<UOItem> list = current.m_Items;

                for (int i = 0; i < list.Count; ++i)
                {
                    UOItem item = list[i];

                    if (ids.Contains(item.ItemID) && (hue == -1 || item.Hue == hue))
                    {
                        items.Add(item);
                    }
                    else if (recurse && item.IsContainer)
                    {
                        RecurseFindItemsByID(item, items, ids, recurse, hue);
                    }
                }
            }

            return items;
        }

        internal UOItem FindItemByName(string name, bool recurse = true)
        {
            return RecurseFindItemByName(this, name, recurse);
        }

        private static UOItem RecurseFindItemByName(UOItem current, string name, bool recurse)
        {
            if (current != null && current.m_Items.Count > 0)
            {
                List<UOItem> list = current.m_Items;

                for (int i = 0; i < list.Count; ++i)
                {
                    UOItem item = list[i];

                    if (item.Name == name)
                    {
                        return item;
                    }
                    else if (recurse && item.IsContainer)
                    {
                        UOItem check = RecurseFindItemByName(item, name, recurse);

                        if (check != null)
                        {
                            return check;
                        }
                    }
                }
            }

            return null;
        }

        internal bool ContainsItemBySerial(uint serial, bool recurse = true)
        {
            return RecurseContainsItemBySerial(this, serial, recurse);
        }

        private static bool RecurseContainsItemBySerial(UOItem current, uint serial, bool recurse)
        {
            if (current != null && current.m_Items.Count > 0)
            {
                List<UOItem> list = current.m_Items;

                for (int i = 0; i < list.Count; ++i)
                {
                    UOItem item = list[i];

                    if (item.Serial == serial)
                    {
                        return true;
                    }
                    else if (recurse && item.IsContainer)
                    {
                        bool check = RecurseContainsItemBySerial(item, serial, recurse);

                        if (check)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        internal int GetCount(ushort iid)
        {
            int count = 0;
            for (int i = 0; i < m_Items.Count; i++)
            {
                UOItem item = (UOItem)m_Items[i];
                if (item.ItemID == iid)
                    count += item.Amount;
                // fucking osi blank scrolls
                else if ((item.ItemID == 0x0E34 && iid == 0x0EF3) || (item.ItemID == 0x0EF3 && iid == 0x0E34))
                    count += item.Amount;
                count += item.GetCount(iid);
            }

            return count;
        }

        internal object Container
        {
            get
            {
                if (m_Parent is uint && UpdateContainer())
                    m_NeedContUpdate.Remove(this);
                return m_Parent;
            }
            set
            {
                if ((m_Parent != null && m_Parent.Equals(value))
                    || (value is uint vval && m_Parent is UOEntity entity && entity.Serial == vval)
                    || (m_Parent is uint parent && value is UOEntity ventity && ventity.Serial == parent))
                {
                    return;
                }

                if (m_Parent is UOMobile mobile)
                    mobile.RemoveItem(this);
                else if (m_Parent is UOItem item)
                    item.RemoveItem(this);

                if (value is UOMobile vmobile)
                    m_Parent = vmobile.Serial;
                else if (value is UOItem vitem)
                    m_Parent = vitem.Serial;
                else
                    m_Parent = value;

                if (!UpdateContainer() && m_NeedContUpdate != null)
                    m_NeedContUpdate.Add(this);
            }
        }

        internal uint GetContainerSerial()
        {
            if (Container is uint ser)
            {
                return ser;
            }
            else if (Container is UOEntity cnt)
            {
                return cnt.Serial;
            }
            return 0;
        }

        internal uint GetRootContainerSerial()
        {
            object root = RootContainer;
            if (root is uint ser)
            {
                return ser;
            }
            else if (root is UOEntity cnt)
            {
                return cnt.Serial;
            }
            return 0;
        }

        internal bool UpdateContainer()
        {
            if (!(m_Parent is uint) || Deleted)
                return true;

            object o = null;
            uint contSer = (uint)m_Parent;
            if (SerialHelper.IsItem(contSer))
                o = UOSObjects.FindItem(contSer);
            else if (SerialHelper.IsMobile(contSer))
                o = UOSObjects.FindMobile(contSer);

            if (o == null)
                return false;

            m_Parent = o;

            if (m_Parent is UOItem)
                ((UOItem)m_Parent).AddItem(this);
            else if (m_Parent is UOMobile)
                ((UOMobile)m_Parent).AddItem(this);

            if (World.Player != null && (IsChildOf(UOSObjects.Player.Backpack) || IsChildOf(UOSObjects.Player.Quiver)))
            {
                //TODO: SearchExemptions
                bool exempt = false;// = SearchExemptionAgent.IsExempt(this);

                if (m_IsNew)
                {
                    if (m_AutoStack)
                        AutoStackResource();
                    //do we really need pouch check for CUO?
                    if (IsContainer && !exempt)// && (!IsPouch || !Config.GetBool("NoSearchPouches")) && UOSObjects.Gump.AutoSearchContainers)
                    {
                        PacketHandlers.IgnoreGumps.Add(this);
                        PlayerData.DoubleClick(Serial);

                        for (int c = 0; c < Contains.Count; ++c)
                        {
                            UOItem icheck = Contains[c];
                            //TODO: SearchExemptionAgent
                            if (icheck.IsContainer)// && !SearchExemptionAgent.IsExempt(icheck) && (!icheck.IsPouch || !Config.GetBool("NoSearchPouches")))
                            {
                                PacketHandlers.IgnoreGumps.Add(icheck);
                                PlayerData.DoubleClick(icheck.Serial);
                            }
                        }
                    }
                }
            }

            m_AutoStack = m_IsNew = false;

            return true;
        }

        private static List<UOItem> m_NeedContUpdate = new List<UOItem>();

        internal static void UpdateContainers()
        {
            int i = 0;
            while (i < m_NeedContUpdate.Count)
            {
                if (((UOItem)m_NeedContUpdate[i]).UpdateContainer())
                    m_NeedContUpdate.RemoveAt(i);
                else
                    i++;
            }
        }

        private static List<uint> m_AutoStackCache = new List<uint>();

        internal void AutoStackResource()
        {
            //do we need to check for autostack? does it have really any utility?
            if (!IsResource || m_AutoStackCache.Contains(Serial))// || !Config.GetBool("AutoStack")
                return;

            foreach (UOItem check in UOSObjects.Items.Values)
            {
                if (check.Container == null && check.ItemID == ItemID && check.Hue == Hue &&
                    Utility.InRange(UOSObjects.Player.Position, check.Position, 2))
                {
                    DragDropManager.DragDrop(this, check);
                    m_AutoStackCache.Add(Serial);
                    return;
                }
            }
            DragDropManager.DragDrop(this, UOSObjects.Player.Position);
            m_AutoStackCache.Add(Serial);
        }

        internal object RootContainer
        {
            get
            {
                int die = 100;
                object cont = this.Container;
                while (cont != null && cont is UOItem item && --die > 0)
                    cont = item.Container;

                return cont;
            }
        }

        internal bool IsChildOf(object parent)
        {
            uint parentSerial;
            if (parent is UOMobile)
                return parent == RootContainer;
            else if (parent is UOItem)
                parentSerial = ((UOItem)parent).Serial;
            else
                return false;

            object check = this;
            int die = 100;
            while (check != null && check is UOItem item && --die > 0)
            {
                if (item.Serial == parentSerial)
                    return true;
                else
                    check = item.Container;
            }

            return false;
        }

        internal override ushort Graphic => ItemID;

        internal override Point3D WorldPosition => GetWorldPosition();

        internal Point3D GetWorldPosition()
        {
            int die = 100;
            object root = this.Container;
            while (root != null && root is UOItem item && item.Container != null && --die > 0)
                root = item.Container;

            if (root is UOEntity entity)
                return entity.Position;
            else
                return Position;
        }

        private void AddItem(UOItem item)
        {
            for (int i = 0; i < m_Items.Count; ++i)
            {
                if (m_Items[i] == item)
                    return;
            }

            m_Items.Add(item);
        }

        private void RemoveItem(UOItem item)
        {
            m_Items.Remove(item);
        }

        internal byte GetPacketFlags()
        {
            byte flags = 0;

            if (!m_Visible)
            {
                flags |= 0x80;
            }

            if (m_Movable)
            {
                flags |= 0x20;
            }

            return flags;
        }

        internal int DistanceTo(UOMobile m)
        {
            int x = Math.Abs(this.Position.X - m.Position.X);
            int y = Math.Abs(this.Position.Y - m.Position.Y);

            return x > y ? x : y;
        }

        internal void ProcessPacketFlags(byte flags)
        {
            m_Visible = ((flags & 0x80) == 0);
            m_Movable = ((flags & 0x20) != 0);
        }

        private Timer m_RemoveTimer = null;

        internal void RemoveRequest()
        {
            if (m_RemoveTimer == null)
                m_RemoveTimer = Timer.DelayedCallback(TimeSpan.FromSeconds(0.5), new TimerCallback(Remove));
            else if (m_RemoveTimer.Running)
                m_RemoveTimer.Stop();

            m_RemoveTimer.Start();
        }

        internal bool CancelRemove()
        {
            if (m_RemoveTimer != null && m_RemoveTimer.Running)
            {
                m_RemoveTimer.Stop();
                return true;
            }
            else
            {
                return false;
            }
        }

        internal override void Remove()
        {
            List<UOItem> rem = new List<UOItem>(m_Items);
            m_Items.Clear();
            for (int i = 0; i < rem.Count; ++i)
                (rem[i]).Remove();

            if (m_Parent is UOMobile mobile)
                mobile.RemoveItem(this);
            else if (m_Parent is UOItem item)
                item.RemoveItem(this);

            UOSObjects.RemoveItem(this);
            base.Remove();
        }

        internal List<UOItem> Contains
        {
            get { return m_Items; }
        }

        public IEnumerable<UOItem> Contents(bool recurse = true)
        {
            if (m_Items == null)
                yield break;

            foreach (var item in m_Items)
            {
                yield return item;

                foreach (var child in item.Contents(recurse))
                    yield return child;
            }
        }

        // possibly 4 bit x/y - 16x16?
        internal byte GridNum
        {
            get { return m_GridNum; }
            set { m_GridNum = value; }
        }

        internal bool OnGround
        {
            get { return Container == null; }
        }

        internal bool IsContainer
        {
            get
            {
                ushort iid = m_ItemID;
                return (m_Items.Count > 0 && !IsCorpse) || (iid >= 0x9A8 && iid <= 0x9AC) ||
                       (iid >= 0x9B0 && iid <= 0x9B2) ||
                       (iid >= 0xA2C && iid <= 0xA53) || (iid >= 0xA97 && iid <= 0xA9E) ||
                       (iid >= 0xE3C && iid <= 0xE43) ||
                       (iid >= 0xE75 && iid <= 0xE80 && iid != 0xE7B) || iid == 0x1E80 || iid == 0x1E81 ||
                       iid == 0x232A || iid == 0x232B ||
                       iid == 0x2B02 || iid == 0x2B03 || iid == 0x2FB7 || iid == 0x3171;
            }
        }

        internal bool IsBagOfSending
        {
            get { return Hue >= 0x0400 && m_ItemID == 0xE76; }
        }

        internal bool IsInBank
        {
            get
            {
                if (m_Parent is UOItem)
                    return ((UOItem)m_Parent).IsInBank;
                else if (m_Parent is UOMobile)
                    return this.Layer == Layer.Bank;
                else
                    return false;
            }
        }

        internal bool IsNew
        {
            get { return m_IsNew; }
            set { m_IsNew = value; }
        }

        internal bool AutoStack
        {
            get { return m_AutoStack; }
            set { m_AutoStack = value; }
        }

        internal bool IsMulti
        {
            get { return m_ItemID >= 0x4000; }
        }

        internal bool IsPouch
        {
            get { return m_ItemID == 0x0E79; }
        }

        internal bool IsCorpse
        {
            get { return m_ItemID == 0x2006 || (m_ItemID >= 0x0ECA && m_ItemID <= 0x0ED2); }
        }

        internal bool IsDoor
        {
            get
            {
                ushort iid = m_ItemID;
                return (iid >= 0x0675 && iid <= 0x06F6) || (iid >= 0x0821 && iid <= 0x0875) ||
                       (iid >= 0x1FED && iid <= 0x1FFC) ||
                       (iid >= 0x241F && iid <= 0x2424) || (iid >= 0x2A05 && iid <= 0x2A1C);
            }
        }

        internal bool IsResource
        {
            get
            {
                ushort iid = m_ItemID;
                return (iid >= 0x19B7 && iid <= 0x19BA) || // ore
                       (iid >= 0x09CC && iid <= 0x09CF) || // fishes
                       (iid >= 0x1BDD && iid <= 0x1BE2) || // logs
                       iid == 0x1779 || // granite / stone
                       iid == 0x11EA || iid == 0x11EB // sand
                    ;
            }
        }

        internal bool IsPotion
        {
            get
            {
                return (m_ItemID >= 0x0F06 && m_ItemID <= 0x0F0D) ||
                       m_ItemID == 0x2790 || m_ItemID == 0x27DB; // Ninja belt (works like a potion)
            }
        }

        internal bool IsVirtueShield
        {
            get
            {
                ushort iid = m_ItemID;
                return (iid >= 0x1bc3 && iid <= 0x1bc5); // virtue shields
            }
        }

        internal bool IsTwoHanded
        {
            get
            {
                ushort iid = m_ItemID;
                return (
                           // everything in layer 2 except shields is 2handed
                           Layer == Layer.LeftHand &&
                           !((iid >= 0x1b72 && iid <= 0x1b7b) || IsVirtueShield) // shields
                       ) ||

                       // and all of these layer 1 weapons:
                       (iid == 0x13fc || iid == 0x13fd) || // hxbow
                       (iid == 0x13AF || iid == 0x13b2) || // war axe & bow
                       (iid >= 0x0F43 && iid <= 0x0F50) || // axes & xbow
                       (iid == 0x1438 || iid == 0x1439) || // war hammer
                       (iid == 0x1442 || iid == 0x1443) || // 2handed axe
                       (iid == 0x1402 || iid == 0x1403) || // short spear
                       (iid == 0x26c1 || iid == 0x26cb) || // aos gay blade
                       (iid == 0x26c2 || iid == 0x26cc) || // aos gay bow
                       (iid == 0x26c3 || iid == 0x26cd) // aos gay xbow
                    ;
            }
        }

        public override string ToString()
        {
            return $"{Name} 0x{Serial:X8}";
        }

        internal int Price
        {
            get { return m_Price; }
            set { m_Price = value; }
        }

        internal string BuyDesc
        {
            get { return m_BuyDesc; }
            set { m_BuyDesc = value; }
        }

        internal override string GetName()
        {
            return $"{Name} 0x{Serial:X8}";
        }
    }
}
