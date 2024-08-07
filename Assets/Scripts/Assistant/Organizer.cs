using ClassicUO.Game;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.Managers;
using System;
using System.Collections.Generic;

namespace Assistant
{
    internal class Organizer
    {
        //this won't check the dragdrop queue, it only checks if the organizer timer is actually running.
        internal static bool IsTimerActive => _Timer != null && _Timer.Running;
        internal static void Stop()
        {
            _Timer?.Stop();
            UOSObjects.Gump.OrganizerStatus(false);
        }

        private static uint _Source;
        internal void ContainerSelection()
        {
            if (_Source == 0)
            {
                _Source = uint.MaxValue;
                UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "Organizer: Select the *Source* Container");
                Targeting.OneTimeTarget(false, OnSourceSelected, OnSourceCancelled);
            }
        }

        private void OnSourceSelected(bool loc, uint serial, Point3D p, ushort itemid)
        {
            if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);
                if (item != null && item.IsContainer && (_Source == 0 || _Source != item.Serial))
                {
                    _Source = item.Serial;
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "Organizer: Valid *Source* selected, now select the *Target* when prompted");
                    Timer.DelayedCallback(TimeSpan.FromMilliseconds(600), ContinueTarget).Start();//we let all the remaining target iteration end, this should also prevent very responsive double tap
                    return;
                }
                else
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Organizer: Select a valid source container");
            }
            else
                UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Organizer: Invalid target");
            _Source = 0;
            _organizeAfter = false;
        }

        private void ContinueTarget()
        {
            UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "Organizer: Select the *Target* Container");
            Targeting.OneTimeTarget(false, OnTargetSelected, OnTargetCancelled);
        }

        private void OnSourceCancelled()
        {
            UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Organizer: Container selection aborted");
            _Source = 0;
            _organizeAfter = false;
        }

        private void OnTargetSelected(bool loc, uint serial, Point3D p, ushort itemid)
        {
            if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);
                if (item != null && item.IsContainer && _Source > 0 && _Source != item.Serial)
                {
                    SourceCont = _Source;
                    TargetCont = item.Serial;
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "Organizer: Source and Target container are set");
                    if(_organizeAfter)
                        BeginOrganize();
                }
                else
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Organizer: Select a valid container");
            }
            _Source = 0;
            _organizeAfter = false;
        }

        private void OnTargetCancelled()
        {
            if (SerialHelper.IsItem(_Source) && UOSObjects.Player.Backpack != null && _Source != UOSObjects.Player.Backpack.Serial)
            {
                TargetCont = UOSObjects.Player.Backpack.Serial;
                SourceCont = _Source;
                UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Warning), "Organizer: Target cancelled, *source* is valid, using your Backpack as target container");
                if(_organizeAfter)
                    BeginOrganize();
            }
            else
                UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Error), "Organizer: Container selection aborted");
            _Source = 0;
            _organizeAfter = false;
        }

        internal static void ClearAll()
        {
            _Source = 0;
            _organizeAfter = false;
            _Timer?.Stop();
            Organizers.Clear();
        }


        private static OrganizerTimer _Timer;
        internal static List<Organizer> Organizers = new List<Organizer>();

        public static ushort CreateNewFree()
        {
            ushort i = 0;
            for (; i < Organizers.Count; ++i)
            {
                if (Organizers[i] == null)
                {
                    Organizers[i] = new Organizer($"Organizer-{i + 1}");
                    return i;
                }
            }
            if (i < ushort.MaxValue)
            {
                Organizers.Add(new Organizer($"Organizer-{i + 1}"));
                return i;
            }
            return (ushort)(i - 1);
        }

        internal string Name { get; }

        internal uint SourceCont { get; set; }
        internal uint TargetCont { get; set; }
        internal bool Stack { get; set; } = false;
        internal bool Complete { get; set; } = false;
        internal bool Loop { get; set; } = false;
        internal List<ItemDisplay> Items = new List<ItemDisplay>();
        internal Organizer(string name)
        {
            Name = name;
        }

        private static bool _organizeAfter;
        internal void Organize()
        {
            if (SerialHelper.IsItem(SourceCont) && UOSObjects.FindItem(SourceCont) != null)
            {
                if (SerialHelper.IsItem(TargetCont) && UOSObjects.FindItem(TargetCont) != null)
                {
                    BeginOrganize();
                    return;
                }
            }
            _organizeAfter = true;
            ContainerSelection();
        }

        private void BeginOrganize()
        {
            _Timer?.Stop();
            _Timer = new OrganizerTimer(this);
            _Timer.Start();
        }

        private class OrganizerTimer : Timer
        {
            private Organizer _Organizer;
            private UOItem _Source;
            private UOItem _Dest;
            private int _Num = 0;
            private bool _Init = false;
            private int x, endx, y, endy;

            internal OrganizerTimer(Organizer organizer) : base(TimeSpan.Zero, TimeSpan.FromMilliseconds(UOSObjects.Gump.ActionDelay))
            {
                _Organizer = organizer;
                if(SerialHelper.IsItem(organizer.SourceCont) && (_Source = UOSObjects.FindItem(organizer.SourceCont)) != null)
                {
                    if (SerialHelper.IsItem(organizer.TargetCont) && (_Dest = UOSObjects.FindItem(organizer.TargetCont)) != null)
                        _Init = _Organizer.Items.Count > _Num;
                    if (_Init)
                    {
                        var c = ContainerManager.Get(_Dest.Graphic);
                        x = c.Bounds.X;
                        endx = c.Bounds.X + c.Bounds.Width;
                        y = c.Bounds.Y;
                        endy = c.Bounds.Y + c.Bounds.Height;
                        _Done.Add(_Dest);
                    }
                    UOSObjects.Gump.OrganizerStatus(_Init);
                }
            }

            private bool _CycleAction = false;
            private HashSet<UOItem> _Done = new HashSet<UOItem>();
            protected override void OnTick()
            {
                //apparently, uosteam can use organizer, even if it's like a restock agent...
                /*if(!Engine.Instance.AllowBit(FeatureBit.RestockAgent))
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Error, "Organizers and Restock agents are not allowed by your server");
                    Organizer.Stop();
                    return;
                }*/
                if (_Init)
                {
                    Interval = TimeSpan.FromMilliseconds(UOSObjects.Gump.ActionDelay);
                    ItemDisplay oi = _Organizer.Items[_Num];
                    List<UOItem> items = _Source.FindItemsByID(oi.Graphic, false, oi.Hue, true);
                    UOItem item = null;
                    if (items.Count > 0)
                    {
                        for(int i = 0; i < items.Count; i++)
                        {
                            if(!_Done.Contains(items[i]))
                            {
                                _Done.Add(item = items[i]);//we keep track of already attempted items, to avoid infinite cycle
                                break;
                            }
                        }
                    }
                    if (item != null)
                    {
                        int amt;
                        if (_Organizer.Complete && oi.Amount > 0)
                        {
                            amt = Math.Min((int)oi.Amount - _Dest.GetCount(oi.Graphic), item.Amount);
                        }
                        else
                            amt = (int)(oi.Amount == 0 || oi.Amount > item.Amount ? item.Amount : oi.Amount);
                        if (amt <= 0)
                        {
                            ++_Num;
                            Interval = TimeSpan.FromMilliseconds(10);
                        }
                        else
                        {
                            DragDropManager.DragDrop(item, _Dest, _Organizer.Stack ? Point3D.MinusOne : new Point3D(Utility.Random(x, endx), Utility.Random(y, endy), 0), amt, DragDropManager.ActionType.Organize);
                            _CycleAction = true;
                        }
                    }
                    else
                    {
                        ++_Num;
                        Interval = TimeSpan.FromMilliseconds(10);
                    }
                    if (_Num >= _Organizer.Items.Count)
                    {
                        if (_Organizer.Loop && _CycleAction)
                        {
                            _Num = 0;
                            _CycleAction = false;
                        }
                        else
                        {
                            UOSObjects.Player.SendMessage(MsgLevel.Friend, "No more items to organize");
                            Organizer.Stop();
                        }
                    }
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Error, "Source and Destination container not set or invalid");
                    Organizer.Stop();
                }
            }
        }
    }

    internal class ItemDisplay : IEquatable<ItemDisplay>
    {
        internal string Name { get; set; }
        internal ushort Graphic { get; }
        internal short Hue { get; set; }
        internal uint Amount { get; set; }
        internal bool Enabled { get; set; }

        internal ItemDisplay(ushort graphic, string name, short hue = -1, bool enabled = true)
        {
            Graphic = graphic;
            Name = name;
            Hue = hue;
            Enabled = enabled;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ItemDisplay);
        }

        public bool Equals(ItemDisplay other)
        {
            if (other == null)
                return false;
            return other.Graphic == Graphic && other.Hue == Hue;
        }

        public override int GetHashCode()
        {
            return (Graphic << 16) + Hue;
        }
    }
}
