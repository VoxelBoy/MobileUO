using System.Collections.Generic;
using ClassicUO.Utility.Collections;
using ClassicUO.Network;
using ClassicUO.Game;
using System.Linq;
using System;

namespace Assistant
{
    internal class Scavenger
    {
        private static uint m_Bag;
        internal static OrderedDictionary<ushort, List<ItemDisplay>> ItemIDsHues { get; } = new OrderedDictionary<ushort, List<ItemDisplay>>();

        private static UOItem m_BagRef;

        public static void Initialize()
        {
            PacketHandler.RegisterClientToServerViewer(0x09, new PacketViewerCallback(OnSingleClick));

            UOItem.OnItemCreated += CheckBagOPL;
        }

        internal static void AddToHotBag()
        {
            UOSObjects.Player.SendMessage(MsgLevel.Force, "Scavenger: Target Item to Scavenge");
            Targeting.OneTimeTarget(false, OnTarget);
        }

        internal static void SetHotBag()
        {
            UOSObjects.Player.SendMessage("Scavenger: Target the Scavenger HotBag");
            Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(OnTargetBag));
        }

        private static void CheckBagOPL(UOItem item)
        {
            if (item.Serial == m_Bag)
            {
                m_BagRef = item;
                Timer.DelayedCallback(TimeSpan.FromMilliseconds(200), OnPositiveCheck).Start();
            }
        }

        private static void OnPositiveCheck()
        {
            if(m_BagRef != null && m_BagRef.ObjPropList != null)
                m_BagRef.ObjPropList.Add("Scavenger HotBag");
        }

        private static void OnSingleClick(Packet pvSrc, PacketHandlerEventArgs args)
        {
            uint serial = pvSrc.ReadUInt();
            if (m_Bag == serial)
            {
                ushort gfx = 0;
                UOItem c = UOSObjects.FindItem(m_Bag);
                if (c != null)
                {
                    gfx = c.ItemID;
                }

                Engine.Instance.SendToClient(new UnicodeMessage(m_Bag, gfx, Assistant.MessageType.Label, 0x3B2, 3, "ENU", "", "Scavenger HotBag"));
            }
        }

        internal static void ClearAll()
        {
            Enabled = false;
            ItemIDsHues.Clear();
            Cached.Activator(false);
            m_BagRef = null;
            m_Bag = 0;
        }

        internal static void OnEnabledChanged()
        {
            Enabled = UOSObjects.Gump.EnabledScavenger.IsChecked;
        }

        private static bool _Enabled;
        public static bool Enabled
        {
            get => _Enabled;
            private set
            {
                _Enabled = value;
                Cached.Activator(_Enabled);
            }
        }

        private class Cached : Timer
        {
            private static byte _Pos = 0;
            private static Cached _Timer { get; } = new Cached();
            private static HashSet<uint>[] _Cached { get; } = new HashSet<uint>[2] { new HashSet<uint>(), new HashSet<uint>() };
            internal Cached() : base(TimeSpan.Zero, TimeSpan.FromSeconds(15))
            {
            }

            internal static void Add(uint s)
            {
                _Cached[_Pos].Add(s);
            }

            internal static void Remove(uint s)
            {
                foreach (var cache in _Cached)
                    cache.Remove(s);
            }

            internal static bool Contains(uint s)
            {
                return _Cached.Any(cache => cache.Contains(s));
            }

            internal static void Activator(bool start)
            {
                if(start)
                {
                    if (!_Timer.Running)
                        _Timer.Start();
                }
                else
                {
                    if (_Timer.Running)
                    {
                        _Timer.Stop();
                        Clear();
                    }
                }
            }

            internal static void Clear()
            {
                foreach (var cache in _Cached)
                    cache.Clear();
            }

            protected override void OnTick()
            {
                if (_Pos > 0)
                    --_Pos;
                else
                    ++_Pos;
                _Cached[_Pos].Clear();
            }
        }

        internal static void ClearCache(bool msg = false)
        {
            Cached.Clear();

            if (msg && UOSObjects.Player != null)
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Scavenger Item cache cleared.");
            }
        }

        private static void OnTarget(bool location, uint serial, Point3D loc, ushort gfx)
        {
            if (location || !SerialHelper.IsItem(serial))
            {
                return;
            }

            UOItem item = UOSObjects.FindItem(serial);
            if (item == null)
            {
                return;
            }

            if (!ItemIDsHues.TryGetValue(item.ItemID, out var hueset))
                ItemIDsHues[item.ItemID] = hueset = new List<ItemDisplay>();
            ItemDisplay id = new ItemDisplay(item.ItemID, item.DisplayName, (short)item.Hue);
            if (hueset.Contains(id))
            {
                UOSObjects.Player.SendMessage(MsgLevel.Error, "Scavenger: Same Item is already in scavenge list");
            }
            else
            {
                hueset.Add(id);
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Scavenger: Item added to scavenge list");
                UOSObjects.Gump.UpdateScavengerItemsGump(id);
            }
        }

        private static void OnTargetBag(bool location, uint serial, Point3D loc, ushort gfx)
        {
            if (location || !SerialHelper.IsItem(serial))
            {
                return;
            }

            if (m_BagRef == null)
            {
                m_BagRef = UOSObjects.FindItem(m_Bag);
            }

            if (m_BagRef != null)
            {
                m_BagRef.ObjPropList.Remove("Scavenger HotBag");
                m_BagRef.OPLChanged();
            }

            m_Bag = serial;
            m_BagRef = UOSObjects.FindItem(m_Bag);
            if (m_BagRef != null)
            {
                m_BagRef.ObjPropList.Add("Scavenger HotBag");
                m_BagRef.OPLChanged();
            }

            UOSObjects.Player.SendMessage(MsgLevel.Force, $"Scavenger: Setting HotBag 0x{m_Bag:X}");
        }

        public static void Uncache(uint s)
        {
            Cached.Remove(s);
        }

        internal static ItemDisplay Remove(ItemDisplay id)
        {
            if (ItemIDsHues.TryGetValue(id.Graphic, out var list))
            {
                int pos = list.IndexOf(id);
                if (pos >= 0)
                {
                    list.RemoveAt(pos);
                    if (pos > 0)
                        return list[pos - 1];
                    else if (pos + 1 < list.Count)
                        return list[pos];
                }
                if(list.Count == 0)
                {
                    list = null;
                    pos = ItemIDsHues.IndexOf(id.Graphic);
                    if(pos >= 0)
                    {
                        ItemIDsHues.RemoveAt(pos);
                        if (pos > 0)
                            list = ItemIDsHues[pos - 1];
                        else if (pos + 1 < ItemIDsHues.Count)
                            list = ItemIDsHues[pos];
                    }
                    if(list != null && list.Count > 0)
                    {
                        return list[list.Count - 1];
                    }
                }
            }
            return null;
        }

        internal static void Scavenge(UOItem item)
        {
            if (!Enabled || UOSObjects.Player.IsGhost)
            {
                return;
            }
            else if(UOSObjects.Player.Backpack == null)
            {
                Utility.SendTimedWarning("You don't have any Backpack!");
                return;
            }
            else if(UOSObjects.Player.Weight >= UOSObjects.Player.MaxWeight)
            {
                Utility.SendTimedWarning("You are overloaded, Scavenger will NOT pickup items anymore!");
                return;
            }
            else if(!ItemIDsHues.TryGetValue(item.ItemID, out var list) || !list.Any(id => id.Enabled && (id.Hue == -1 || id.Hue == item.Hue)))
            {
                return;
            }

            if (Cached.Contains(item.Serial))
            {
                return;
            }

            UOItem bag = m_BagRef;
            if (bag == null || bag.Deleted)
            {
                bag = m_BagRef = UOSObjects.FindItem(m_Bag);
            }

            if (bag == null || bag.Deleted || !bag.IsChildOf(UOSObjects.Player.Backpack))
            {
                bag = UOSObjects.Player.Backpack;
            }

            Cached.Add(item.Serial);
            DragDropManager.DragDrop(item, bag);
        }
    }
}
