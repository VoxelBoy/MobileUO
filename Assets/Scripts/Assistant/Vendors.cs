using System.Collections.Generic;
using ClassicUO.Utility.Collections;
using ClassicUO.Network;
using ClassicUO.Game;
using System.Linq;
using System;

namespace Assistant
{
    internal class Vendors
    {
        private static IEnumerable<TSource> ExceptImpl<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> bannedElements = new HashSet<TSource>(second, comparer);
            foreach (TSource item in first)
            {
                if (bannedElements.Add(item))
                {
                    yield return item;
                }
            }
        }

        public class NumberIBuySellComparer : IComparer<IBuySell>
        {
            public static NumberIBuySellComparer Instance = new NumberIBuySellComparer();
            public int Compare(IBuySell a, IBuySell b)
            {
                if (a.Num < b.Num)
                    return -1;
                if (a.Num > b.Num)
                    return 1;
                return 0;//should never happen
            }
        }

        internal interface IBuySell
        {
            bool Enabled { get; set; }
            IBuySell Selected { get; set; }
            string Name { get; }
            bool Complete { get; set; }
            ushort MaxAmount { get; set; }
            
            ICollection<IBuySell> BuySellList { get; }
            ushort Num { get; }
            List<BuySellEntry> BuySellItems { get; }
        }

        internal class Buy : IBuySell
        {
            private class ItemXYComparer : IComparer<UOItem>
            {
                public static readonly ItemXYComparer Instance = new ItemXYComparer();

                private ItemXYComparer()
                {
                }

                public int Compare(UOItem x, UOItem y)
                {
                    if (x == null)
                    {
                        return 1;
                    }
                    else if (y == null)
                    {
                        return -1;
                    }

                    int xsum = x.Position.X + x.Position.Y * 200;
                    int ysum = y.Position.X + y.Position.Y * 200;

                    return xsum.CompareTo(ysum);
                }
            }

            internal static IBuySell CreateOne()
            {
                IBuySell bs = null;
                if (BuyList.Count == 0)
                {
                    BuyList.Add(bs = new Buy(0), new List<BuySellEntry>());
                    return bs;
                }
                List<int> values = new List<int>();
                foreach (IBuySell bsk in BuyList.Keys)
                {
                    values.Add(bsk.Num);
                }
                ushort? firstAvailable = (ushort)Enumerable.Range(0, int.MaxValue).Except(values).FirstOrDefault();
                if (firstAvailable.HasValue && firstAvailable.Value < ushort.MaxValue)
                {
                    BuyList.Add(bs = new Buy(firstAvailable.Value), new List<BuySellEntry>());
                    BuyList.SortKeys(NumberIBuySellComparer.Instance);
                }
                return bs;
            }

            public override int GetHashCode()
            {
                return Num + 1;
            }

            internal static OrderedDictionary<IBuySell, List<BuySellEntry>> BuyList { get; } = new OrderedDictionary<IBuySell, List<BuySellEntry>>();

            public static void Initialize()
            {
                PacketHandler.RegisterServerToClientViewer(0x74, new PacketViewerCallback(ExtBuyInfo));
                PacketHandler.RegisterServerToClientViewer(0x24, new PacketViewerCallback(DisplayBuy));
                PacketHandler.RegisterServerToClientViewer(0x3B, new PacketViewerCallback(EndVendorBuy));
            }

            public bool Enabled
            {
                get => BuyEnabled;
                set => BuyEnabled = value;
            }

            public IBuySell Selected
            {
                get => BuySelected;
                set => BuySelected = value;
            }

            public ushort MaxAmount { get => 0; set { } }

            public bool Complete { get; set; }

            public ICollection<IBuySell> BuySellList => BuyList.Keys;
            public List<BuySellEntry> BuySellItems
            {
                get
                {
                    BuyList.TryGetValue(this, out var list);
                    return list;
                }
            }

            internal static bool BuyEnabled { get; set; }
            internal static IBuySell BuySelected { get; set; }
            public ushort Num { get; }

            public Buy(ushort num)
            {
                Num = num;
            }

            private static void DisplayBuy(Packet p, PacketHandlerEventArgs args)
            {
                if (!BuyEnabled || !Engine.Instance.AllowBit(FeatureBit.BuyAgent) || BuySelected == null || !BuyList.TryGetValue(BuySelected, out var bentry))
                    return;
                if (UOSObjects.Player.Backpack == null)
                {
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Buy: You don't have a backpack!");
                    return;
                }

                uint serial = p.ReadUInt();
                ushort gump = p.ReadUShort();

                if (gump != 0x30 || !SerialHelper.IsMobile(serial) || UOSObjects.Player == null)
                {
                    return;
                }

                UOMobile vendor = UOSObjects.FindMobile(serial);
                if (vendor == null)
                {
                    return;
                }

                UOItem pack = vendor.GetItemOnLayer(Layer.ShopBuy);
                if (pack == null || pack.Contains == null || pack.Contains.Count <= 0)
                {
                    return;
                }

                pack.Contains.Sort(ItemXYComparer.Instance);

                int total = 0;
                int cost = 0;
                List<VendorBuyItem> buyList = new List<VendorBuyItem>();
                Dictionary<ushort, int> found = new Dictionary<ushort, int>();
                bool lowGoldWarn = false;
                var buy = BuySelected;
                for (int i = 0; i < pack.Contains.Count; ++i)
                {
                    UOItem item = pack.Contains[i];
                    if (item == null)
                    {
                        continue;
                    }

                    for (int a = 0; a < bentry.Count; a++)
                    {
                        var b = bentry[a];
                        if (b == null || b.Amount == 0)
                        {
                            continue;
                        }

                        bool dupe = false;
                        foreach (VendorBuyItem vbi in buyList)
                        {
                            if (vbi.Serial == item.Serial)
                            {
                                dupe = true;
                                break;
                            }
                        }

                        if (dupe)
                        {
                            continue;
                        }

                        // fucking osi and their blank scrolls
                        if (b.ItemID == item.ItemID || (b.ItemID == 0x0E34 && item.ItemID == 0x0EF3) || (b.ItemID == 0x0EF3 && item.ItemID == 0x0E34))
                        {
                            bool ok;

                            int count = buy.Complete ? UOSObjects.Player.Backpack.GetCount(b.ItemID) : 0;
                            
                            if (ok = found.TryGetValue(b.ItemID, out int plus))
                            {
                                count += plus;
                            }

                            if (count < b.Amount)
                            {
                                count = b.Amount - count;
                                if (count > item.Amount)
                                {
                                    count = item.Amount;
                                }
                                else if (count <= 0)
                                {
                                    continue;
                                }

                                if (!ok)
                                {
                                    found[b.ItemID] = count;
                                }
                                else
                                {
                                    found[b.ItemID] = plus + count;
                                }

                                buyList.Add(new VendorBuyItem(item.Serial, count, item.Price));
                                ++total;
                                cost += item.Price * count;
                            }
                        }
                    }
                }

                if (cost > UOSObjects.Player.Gold && cost < 2000 && buyList.Count > 0)
                {
                    lowGoldWarn = true;
                    do
                    {
                        VendorBuyItem vbi = buyList[0];
                        if (cost - vbi.TotalCost <= UOSObjects.Player.Gold)
                        {
                            while (cost > World.Player.Gold && vbi.Amount > 0)
                            {
                                cost -= vbi.Price;
                                --vbi.Amount;
                            }

                            if (vbi.Amount <= 0)
                            {
                                buyList.RemoveAt(0);
                                --total;
                            }
                        }
                        else
                        {
                            cost -= vbi.TotalCost;
                            --total;
                            buyList.RemoveAt(0);
                        }
                    } while (cost > UOSObjects.Player.Gold && buyList.Count > 0);
                }

                if (buyList.Count > 0)
                {
                    args.Block = true;
                    BuyLists[serial] = buyList;
                    Engine.Instance.SendToServer(new VendorBuyResponse(serial, buyList));
                    UOSObjects.Player.SendMessage(MsgLevel.Friend, $"Buy: {total} item{(total > 1 ? "s" : "")} matching your list. Cost was {cost}");
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, $"Buy: No items matching your list were found.");
                }
                if (lowGoldWarn)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Error, "Buy: did not attempt to buy some items in your buy list because you do not have enough gold.");
                }
            }

            internal static void ClearAll()
            {
                BuyLists.Clear();
            }

            private static Dictionary<uint, List<VendorBuyItem>> BuyLists { get; } = new Dictionary<uint, List<VendorBuyItem>>();

            private static void ExtBuyInfo(Packet p, PacketHandlerEventArgs args)
            {
                if (!Engine.Instance.AllowBit(FeatureBit.BuyAgent))
                    return;
                uint ser = p.ReadUInt();
                UOItem pack = UOSObjects.FindItem(ser);
                if (pack == null)
                {
                    return;
                }

                byte count = p.ReadByte();
                if (count < pack.Contains.Count)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Debug, "Buy: warning! Contains count {0} does not match ExtInfo {1}.", pack.Contains.Count, count);
                }

                pack.Contains.Sort(ItemXYComparer.Instance);

                for (int i = 0; i < pack.Contains.Count; ++i)
                {
                    UOItem item = pack.Contains[i];
                    item.Price = (int)p.ReadUInt();
                    byte len = p.ReadByte();
                    item.BuyDesc = p.ReadASCII(len);
                }
            }

            //this particular packet will confirm our buy, if we ended it correctly, we'll found it here, if ended incorrectly, this will be removed
            private static void EndVendorBuy(Packet p, PacketHandlerEventArgs args)
            {
                if (!Engine.Instance.AllowBit(FeatureBit.BuyAgent))
                    return;
                uint serial = p.ReadUInt();
                if (BuyLists.TryGetValue(serial, out var list))
                {
                    BuyLists.Remove(serial);
                    UOMobile vendor = UOSObjects.FindMobile(serial);
                    if (vendor == null)
                        return;

                    UOItem pack = vendor.GetItemOnLayer(Layer.ShopBuy);
                    if (pack == null || pack.Contains == null || pack.Contains.Count <= 0)
                        return;

                    for(int i = list.Count - 1; i >= 0; --i)
                    {
                        VendorBuyItem vbi = list[i];
                        UOItem item = UOSObjects.FindItem(vbi.Serial);
                        if (item == null || !pack.Contains.Contains(item))
                            continue;
                        item.Amount -= (ushort)vbi.Amount;
                        if(item.Amount <= 0)
                            item.Remove();
                    }
                }
            }

            public string Name { get { return $"Buy-{Num + 1}"; } }
        }

        public class Sell : IBuySell
        {
            internal static bool SellEnabled { get; set; }
            internal static IBuySell SellSelected { get; set; }
            internal static uint HotBag { get; set; }

            internal static OrderedDictionary<IBuySell, List<BuySellEntry>> SellList { get; } = new OrderedDictionary<IBuySell, List<BuySellEntry>>();

            public ushort Num { get; }
            public Sell(ushort num)
            {
                Num = num;
            }

            public bool Enabled
            {
                get => SellEnabled;
                set => SellEnabled = value;
            }

            public IBuySell Selected { get => SellSelected; set => SellSelected = value; }

            public bool Complete { get => false; set { } }

            public ushort MaxAmount { get; set; } = 999;

            public ICollection<IBuySell> BuySellList => SellList.Keys;

            public List<BuySellEntry> BuySellItems
            {
                get
                {
                    SellList.TryGetValue(this, out var list);
                    return list;
                }
            }

            internal static IBuySell CreateOne()
            {
                IBuySell bs = null;
                if (SellList.Count == 0)
                {
                    SellList.Add(bs = new Sell(0), new List<BuySellEntry>());
                    return bs;
                }
                List<int> values = new List<int>();
                foreach (IBuySell bsk in SellList.Keys)
                {
                    values.Add(bsk.Num);
                }
                ushort? firstAvailable = (ushort)Enumerable.Range(0, int.MaxValue).Except(values).FirstOrDefault();
                if (firstAvailable.HasValue && firstAvailable.Value < ushort.MaxValue)
                {
                    SellList.Add(bs = new Sell(firstAvailable.Value), new List<BuySellEntry>());
                    SellList.SortKeys(NumberIBuySellComparer.Instance);
                }
                return bs;
            }

            public override int GetHashCode()
            {
                return Num + 1;
            }

            public static void Initialize()
            {
                PacketHandler.RegisterServerToClientViewer(0x9E, new PacketViewerCallback(OnVendorSell));
                PacketHandler.RegisterClientToServerViewer(0x09, new PacketViewerCallback(OnSingleClick));
                UOItem.OnItemCreated += CheckHBOPL;
            }

            private static void CheckHBOPL(UOItem item)
            {
                if (item.Serial == HotBag)
                {
                    item.ObjPropList.Add("(Sell HotBag)");
                }
            }

            private static void OnSingleClick(Packet pvSrc, PacketHandlerEventArgs args)
            {
                uint serial = pvSrc.ReadUInt();
                if (HotBag == serial)
                {
                    ushort gfx = 0;
                    UOItem c = UOSObjects.FindItem(HotBag);
                    if (c != null)
                    {
                        gfx = c.ItemID;
                    }

                    Engine.Instance.SendToClient(new UnicodeMessage(HotBag, gfx, Assistant.MessageType.Label, 0x3B2, 3, "ENU", "", "(Sell HotBag)"));
                }
            }

            private static void OnVendorSell(Packet pvSrc, PacketHandlerEventArgs args)
            {
                if (!SellEnabled || !Engine.Instance.AllowBit(FeatureBit.SellAgent) || SellList.Count == 0)
                {
                    return;
                }
                if(SellSelected == null || !SellList.TryGetValue(SellSelected, out var sentry) || sentry == null)
                    return;
                if (UOSObjects.Player.Backpack == null)
                {
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Sell: You don't have a backpack!");
                    return;
                }
                UOItem hb = HotBag == 0 ? UOSObjects.Player.Backpack : UOSObjects.FindItem(HotBag);
                if (hb == null)
                    hb = UOSObjects.Player.Backpack;

                int total = 0;

                uint serial = pvSrc.ReadUInt();
                UOMobile vendor = UOSObjects.FindMobile(serial);
                if (vendor == null)
                {
                    UOSObjects.AddMobile(vendor = new UOMobile(serial));
                }

                int count = pvSrc.ReadUShort();
                var sell = SellSelected;
                int maxSell = sell.MaxAmount;
                int sold = 0;
                List<SellListItem> list = new List<SellListItem>(count);
                for (int i = 0; i < count && (sold < maxSell || maxSell <= 0); i++)
                {
                    uint ser = pvSrc.ReadUInt();
                    ushort gfx = pvSrc.ReadUShort();
                    ushort hue = pvSrc.ReadUShort();
                    ushort amount = pvSrc.ReadUShort();
                    ushort price = pvSrc.ReadUShort();

                    pvSrc.ReadASCII(pvSrc.ReadUShort());//name

                    UOItem item = UOSObjects.FindItem(ser);
                    if(item != null && item != hb && item.IsChildOf(hb))
                    {
                        foreach(var se in sentry)
                        {
                            if (se.Amount > 0 && se.ItemID == gfx)
                            {
                                if (sold + amount > maxSell && maxSell > 0)
                                {
                                    amount = Math.Min((ushort)(maxSell - sold), se.Amount);
                                }
                                else
                                    amount = Math.Min(amount, se.Amount);

                                list.Add(new SellListItem(ser, amount));
                                total += amount * price;
                                sold += amount;
                            }
                            //if ( sold >= maxSell && maxSell > 0 ) break;
                        }
                    }
                }

                if (list.Count > 0)
                {
                    Engine.Instance.SendToServer(new VendorSellResponse(vendor, list));
                    UOSObjects.Player.SendMessage(MsgLevel.Force, $"Sell: Selling {sold} item{(sold > 1 ? "s" : "")} for {total}");
                    args.Block = true;
                }
            }

            public string Name { get { return $"Sell-{Num + 1}"; } }

            private void SetHotBag()
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Target the Sell HotBag container");
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(OnHBTarget));
            }

            private void OnHBTarget(bool location, uint serial, Point3D loc, ushort gfx)
            {
                UOItem hb = UOSObjects.FindItem(HotBag);
                if (hb != null)
                {
                    if (hb.ObjPropList.Remove("(Sell HotBag)"))
                    {
                        hb.OPLChanged();
                    }
                }

                if (location && SerialHelper.IsItem(serial))
                {
                    HotBag = serial;
                    hb = UOSObjects.FindItem(HotBag);
                    if (hb != null)
                    {
                        hb.ObjPropList.Add("(Sell HotBag)");
                        hb.OPLChanged();
                    }
                }
                else
                    HotBag = 0;
            }
        }
    }

    internal class BuySellEntry
    {
        internal BuySellEntry(ushort itemid, ushort amount)
        {
            ItemID = itemid;
            Amount = amount;
        }

        internal ushort Amount;

        internal ushort ItemID { get; }

        public override string ToString()
        {
            return $"{ItemID} {Amount}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BuySellEntry);
        }

        public bool Equals(BuySellEntry other)
        {
            if (other == null)
                return false;
            return other.ItemID == ItemID;
        }

        public override int GetHashCode()
        {
            return ItemID;
        }
    }
}
