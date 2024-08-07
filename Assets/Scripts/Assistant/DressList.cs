using System;
using System.Collections.Generic;
using System.Xml;
using Assistant.Core;
using Assistant.Scripts;
using ClassicUO.Game;
using ClassicUO.Game.UI.Gumps;

namespace Assistant
{
    internal class DressList
    {
        public static List<DressList> DressLists { get; } = new List<DressList>();

        public static ushort CreateNewFree()
        {
            ushort i = 0;
            for(; i < DressLists.Count; ++i)
            {
                if (DressLists[i] == null)
                {
                    DressLists[i] = new DressList($"Dress-{i + 1}");
                    return i;
                }
            }
            if (i < ushort.MaxValue)
            {
                DressLists.Add(new DressList($"Dress-{i + 1}"));
                return i;
            }
            return (ushort)(i - 1);
        }

        public static void ClearAll(int selected = -1)
        {
            if (selected < 0)
            {
                DressList dl;
                while (DressLists.Count > 0)
                {
                    dl = DressLists[0];
                    if (dl != null)
                    {
                        HotKeys.RemoveHotKey($"agents.dress.{dl.Name.ToLower(XmlFileParser.Culture)}");
                        HotKeys.RemoveHotKey($"agents.undress.{dl.Name.ToLower(XmlFileParser.Culture)}");
                    }
                    DressLists.Remove(dl);
                }
            }
            else if(selected < DressLists.Count)
            {
                if (DressLists[selected] == null)
                    DressLists[selected] = new DressList($"Dress-{selected + 1}");
                else
                    DressLists[selected].LayerItems.Clear();
            }
        }

        public static DressList Find(string name)
        {
            for (int i = 0; i < DressLists.Count; i++)
            {
                DressList list = DressLists[i];
                if (list?.Name == name)
                    return list;
            }

            return null;
        }

        public static UOItem FindUndressBag(UOItem item)
        {
            UOItem undressBag = UOSObjects.Player.Backpack;
            for (int i = 0; i < DressLists.Count; i++)
            {
                DressList list = DressLists[i];
                if (list != null && list.LayerItems.TryGetValue(item.Layer, out DressItem di) && ((di.UsesType && di.ObjType == item.Graphic) || di.Serial == item.Serial))
                {
                    if (SerialHelper.IsItem(list.UndressBag))
                    {
                        UOItem bag = UOSObjects.FindItem(list.UndressBag);
                        if (bag != null && (bag.RootContainer == UOSObjects.Player ||
                                            (bag.RootContainer == null && Utility.InRange(bag.GetWorldPosition(),
                                                 UOSObjects.Player.Position, 2))))
                            undressBag = bag;
                    }

                    break;
                }
            }

            return undressBag;
        }

        public void ImportCurrentItems()
        {
            LayerItems.Clear();
            foreach(UOItem item in UOSObjects.Player.Contains)
            {
                if (item.Layer > Layer.Invalid && item.Layer <= Layer.LastUserValid && item.Layer != Layer.Backpack && item.Layer != Layer.FacialHair && item.Layer != Layer.Hair)
                    LayerItems[item.Layer] = new DressItem(item.Serial, item.Graphic);
            }
        }

        public uint UndressBag { get; set; }

        public DressList(string name) : this(name, new Dictionary<Layer, DressItem>())
        {
        }

        public DressList(string name, Dictionary<Layer, DressItem> items, uint undressbag = 0)
        {
            Name = name;
            LayerItems = items;
            UndressBag = undressbag;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; }

        public Dictionary<Layer, DressItem> LayerItems { get; }

        public void SetUndressBag(uint serial)
        {
            if (UOSObjects.Player.Backpack?.Serial == serial || !SerialHelper.IsItem(serial))
                UndressBag = 0;
            else
                UndressBag = serial;
        }

        /*public void Toggle()
        {
            if (UOSObjects.Player == null || UOSObjects.Player.Backpack == null)
                return;

            int worn = 0;
            int total = LayerItems.Count;

            foreach(KeyValuePair<Layer, DressItem> kvp in LayerItems)
            {
                UOItem item = kvp.Value.Find(kvp.Key);

                if (item == null)
                    total--;
                else if (item.Container == UOSObjects.Player)
                    worn++;
            }

            if (LayerItems.Count == 1)
            {
                if (worn != 0)
                    Undress();
                else
                    Dress();
            }
            else
            {
                if (worn > total / 2)
                    Undress();
                else
                    Dress();
            }
        }*/

        public void Undress()
        {
            if (UOSObjects.Player == null)
                return;

            int count = 0;
            UOItem undressBag = UOSObjects.Player.Backpack;
            if (undressBag == null)
            {
                UOSObjects.Player.SendMessage("Could NOT find your Backpack");
                return;
            }

            if(ScriptManager.Recording)
                ScriptManager.AddToScript($"undress '{Name}'");

            if (SerialHelper.IsItem(UndressBag))
            {
                UOItem bag = UOSObjects.FindItem(UndressBag);
                if (bag != null && (bag.RootContainer == UOSObjects.Player ||
                                    (bag.RootContainer == null && Utility.InRange(bag.GetWorldPosition(),
                                         UOSObjects.Player.Position, 2))))
                    undressBag = bag;
                else
                    UOSObjects.Player.SendMessage("Undress bag was not found or out of range, using your backpack instead");
            }

            foreach(KeyValuePair<Layer, DressItem> kvp in LayerItems)
            {
                UOItem item = kvp.Value.Find(kvp.Key, false);

                if (item == null || DragDropManager.CancelDragFor(item.Serial) || item.Container != UOSObjects.Player)
                {
                    continue;
                }
                else
                {
                    DragDropManager.DragDrop(item, undressBag, DragDropManager.ActionType.Dressing);
                    count++;
                }
            }

            UOSObjects.Player.SendMessage("{0} item(s) queued to be de-equipped", count);
        }

        public static Layer GetLayerFor(UOItem item)
        {
            Layer layer = item.Layer;
            if (layer == Layer.Invalid || layer > Layer.LastUserValid)
                layer = (Layer)item.TileDataInfo.Layer;

            return layer;
        }

        public void Dress()
        {
            if (UOSObjects.Player == null)
                return;

            int skipped = 0, gone = 0, done = 0;
            List<UOItem> list = new List<UOItem>();

            if (UOSObjects.Player.Backpack == null)
            {
                UOSObjects.Player.SendMessage("Could NOT find your Backpack");
                return;
            }

            if (ScriptManager.Recording)
                ScriptManager.AddToScript($"dress '{Name}'");

            foreach(KeyValuePair<Layer, DressItem> kvp in LayerItems)
            {
                UOItem item = kvp.Value.Find(kvp.Key, true);
                if (item == null)
                    gone++;
                else
                    list.Add(item);
            }

            foreach (UOItem item in list)
            {
                if (item.Container == UOSObjects.Player)
                {
                    skipped++;
                }
                else if (item.IsChildOf(UOSObjects.Player.Backpack) || item.RootContainer == null)
                {
                    Layer layer = GetLayerFor(item);
                    if (layer == Layer.Invalid || layer > Layer.LastUserValid || layer == Layer.Backpack)
                        continue;

                    if (UOSObjects.Gump.MoveConflictingItems)
                    {
                        UOItem conflict = UOSObjects.Player.GetItemOnLayer(layer);
                        if (conflict != null)
                            DragDropManager.DragDrop(conflict, FindUndressBag(conflict), DragDropManager.ActionType.Dressing);

                        // try to also undress conflicting hand(s)
                        if (layer == Layer.RightHand)
                            conflict = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
                        else if (layer == Layer.LeftHand)
                            conflict = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
                        else
                            conflict = null;

                        if (conflict != null && (conflict.IsTwoHanded || item.IsTwoHanded))
                            DragDropManager.DragDrop(conflict, FindUndressBag(conflict), DragDropManager.ActionType.Dressing);
                    }

                    DragDropManager.DragDrop(item, UOSObjects.Player, layer, actionType: DragDropManager.ActionType.Dressing);
                    done++;
                }
            }

            if (done > 0)
                UOSObjects.Player.SendMessage("{0} item(s) queued to be equipped", done);
            if (skipped > 0)
                UOSObjects.Player.SendMessage("{0} item(s) already equipped", skipped);
            if (gone > 0)
                UOSObjects.Player.SendMessage("{0} item(s) were not found. (Out of Range)", gone);
        }
    }

    internal class DressItem
    {
        internal uint Serial { get; private set; }
        internal ushort ObjType { get; private set; }
        internal bool UsesType { get; private set; } = UOSObjects.Gump.TypeDress;

        internal DressItem(uint serial, ushort type)
        {
            Serial = serial;
            ObjType = type;
        }

        internal void ChangeItemSerial(UOItem item)
        {
            if (item != null && !item.Deleted)
            {
                ObjType = item.ItemID;
                Serial = item.Serial;
            }
        }

        internal UOItem Find(Layer layer, bool dress)
        {
            UOItem item;
            if (UsesType)
            {
                if (!dress)
                {
                    item = UOSObjects.Player.GetItemOnLayer(layer);
                    if (item != null && item.Graphic != ObjType)
                        item = null;
                }
                else
                    item = UOSObjects.Player.Backpack.FindItemByID(ObjType, true, layer: layer);
                
                if (item != null)
                    ChangeItemSerial(item);
            }
            else
                item = UOSObjects.FindItem(Serial);
            if (item != null)
            {
                ObjType = item.Graphic;
            }
            return item;
        }

        internal string TypeID
        {
            get => UsesType ? ObjType.ToString("X4") : Serial.ToString("X8");
        }
    }
}
