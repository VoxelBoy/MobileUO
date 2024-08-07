using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using ClassicUO.Network;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Utility;
using SpellFlag = ClassicUO.Game.Managers.TargetType;

namespace Assistant
{
    internal class Spell
    {
        readonly public SpellFlag Flag;
        readonly public int Circle;
        readonly public int Number;
        readonly public string WordsOfPower;
        readonly public string[] Reagents;

        public Spell(int flag, int n, int c, string power, List<Reagents> reags)
        {
            Flag = (SpellFlag)flag;
            Number = n;
            Circle = c;
            WordsOfPower = power;
            Reagents = new string[reags.Count];
            for (int i = 0; i < reags.Count; i++)
                Reagents[i] = StringHelper.AddSpaceBeforeCapital(reags[i].ToString());
        }

        public int Name
        {
            get
            {
                if (Circle <= 8) // Mage
                    return 3002011 + ((Circle - 1) * 8) + (Number - 1);
                else if (Circle == 10) // Necr
                    return 1060509 + Number - 1;
                else if (Circle == 20) // Chiv
                    return 1060585 + Number - 1;
                else if (Circle == 40) // Bush
                    return 1060595 + Number - 1;
                else if (Circle == 50) // Ninj
                    return 1060610 + Number - 1;
                else if (Circle == 60) // Elfs
                    return 1071026 + Number - 1;
                else
                    return -1;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} (#{1})", SpellDefinition.FullIndexGetSpell(Number).Name, Number);
        }

        public int GetID()
        {
            return Number;
        }

        public void OnCast(PacketBase p)
        {
            Cast();
            Engine.Instance.SendToServer(p);
        }

        private void Cast()
        {
            if (UOSObjects.Gump.HandsBeforeCasting && Engine.Instance.AllowBit(FeatureBit.UnequipBeforeCast))
            {
                UOItem pack = UOSObjects.Player.Backpack;
                if (pack != null)
                {
                    // dont worry about uneqipping RuneBooks or SpellBooks
                    UOItem item = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
#if DEBUG
                    if (item != null && item.ItemID != 0x22C5 && item.ItemID != 0xE3B && item.ItemID != 0xEFA &&
                        !item.IsVirtueShield)
#else
					if ( item != null && item.ItemID != 0x22C5 && item.ItemID != 0xE3B && item.ItemID != 0xEFA )
#endif
                    {
                        DragDropManager.Drag(item, item.Amount);
                        DragDropManager.Drop(item, pack);
                    }

                    item = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
#if DEBUG
                    if (item != null && item.ItemID != 0x22C5 && item.ItemID != 0xE3B && item.ItemID != 0xEFA &&
                        !item.IsVirtueShield)
#else
					if ( item != null && item.ItemID != 0x22C5 && item.ItemID != 0xE3B && item.ItemID != 0xEFA )
#endif
                    {
                        DragDropManager.Drag(item, item.Amount);
                        DragDropManager.Drop(item, pack);
                    }
                }
            }

            if (UOSObjects.Player != null)
            {
                UOSObjects.Player.LastSpell = GetID();
                LastCastTime = DateTime.UtcNow;
                Targeting.SpellTargetID = 0;
            }
        }

        public static DateTime LastCastTime = DateTime.MinValue;

        internal static Dictionary<int, Spell> SpellsByID = new Dictionary<int, Spell>();
        internal static Dictionary<string, Spell> SpellsByName = new Dictionary<string, Spell>();

        //private static HotKeyCallbackState HotKeyCallback;

        static Spell()
        {
            
        }

        public static void HealOrCureSelf()
        {
            Spell s = null;

            if (!Engine.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
            {
                if (UOSObjects.Player.Hits + 30 < UOSObjects.Player.HitsMax && UOSObjects.Player.Mana >= 12)
                    s = Get(4, 5); // greater heal
                else
                    s = Get(1, 4); // mini heal
            }
            else
            {
                if (UOSObjects.Player.Poisoned && Engine.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
                {
                    s = Get(2, 3); // cure 
                }
                else if (UOSObjects.Player.Hits + 2 < UOSObjects.Player.HitsMax)
                {
                    if (UOSObjects.Player.Hits + 30 < UOSObjects.Player.HitsMax && UOSObjects.Player.Mana >= 12)
                        s = Get(4, 5); // greater heal
                    else
                        s = Get(1, 4); // mini heal
                }
                else
                {
                    if (UOSObjects.Player.Mana >= 12)
                        s = Get(4, 5); // greater heal
                    else
                        s = Get(1, 4); // mini heal
                }
            }

            if (s != null)
            {
                if (UOSObjects.Player.Poisoned || UOSObjects.Player.Hits < UOSObjects.Player.HitsMax)
                    Targeting.TargetSelf(true);
                Engine.Instance.SendToServer(new CastSpellFromMacro((ushort)s.GetID()));
                s.Cast();
            }
        }

        public static void MiniHealOrCureSelf()
        {
            Spell s = null;

            if (!Engine.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
            {
                s = Get(1, 4); // mini heal
            }
            else
            {
                if (UOSObjects.Player.Poisoned)
                    s = Get(2, 3); // cure
                else
                    s = Get(1, 4); // mini heal
            }

            if (s != null)
            {
                if (UOSObjects.Player.Poisoned || UOSObjects.Player.Hits < UOSObjects.Player.HitsMax)
                    Targeting.TargetSelf(true);
                Engine.Instance.SendToServer(new CastSpellFromMacro((ushort)s.GetID()));
                s.Cast();
            }
        }

        public static void GHealOrCureSelf()
        {
            Spell s = null;

            if (!Engine.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
            {
                s = Get(4, 5); // gheal
            }
            else
            {
                if (UOSObjects.Player.Poisoned)
                    s = Get(2, 3); // cure
                else
                    s = Get(4, 5); // gheal
            }

            if (s != null)
            {
                if (UOSObjects.Player.Poisoned || UOSObjects.Player.Hits < UOSObjects.Player.HitsMax)
                    Targeting.TargetSelf(true);
                Engine.Instance.SendToServer(new CastSpellFromMacro((ushort)s.GetID()));
                s.Cast();
            }
        }

        public static void Interrupt()
        {
            UOItem item = FindUsedLayer();

            if (item != null)
            {
                Engine.Instance.SendToServer(new LiftRequest(item, 1)); // unequip
                Engine.Instance.SendToServer(new EquipRequest(item.Serial, UOSObjects.Player, item.Layer)); // Equip
            }
        }

        private static UOItem FindUsedLayer()
        {
            UOItem layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Shirt);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Shoes);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Pants);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Head);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Gloves);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Ring);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Neck);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Waist);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.InnerTorso);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Bracelet);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.MiddleTorso);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Earrings);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Arms);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.Cloak);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.OuterTorso);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.OuterLegs);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.InnerLegs);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
            if (layeredItem != null)
                return layeredItem;

            layeredItem = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
            if (layeredItem != null)
                return layeredItem;

            return null;
        }

        public static void Initialize()
        {
            // no code, this is here to make sure out static ctor is init'd by the core
        }

        public static void OnHotKey(ushort id)
        {
            Spell s = Spell.Get(id);
            if (s != null)
            {
                s.OnCast(new CastSpellFromMacro(id));
                //if ( Macros.MacroManager.AcceptActions )
                //	Macros.MacroManager.Action( new Macros.MacroCastSpellAction( s ) );
            }
        }

        public static int ToID(int circle, int num)
        {
            if (circle < 10)
                return ((circle - 1) * 8) + num;
            else
                return (circle * 10) + num;
        }

        public static Spell Get(int num)
        {
            Spell s;
            SpellsByID.TryGetValue(num, out s);
            return s;
        }

        public static Spell GetByName(string name)
        {
            SpellsByName.TryGetValue(name.ToLower(), out Spell s);
            return s;
        }

        public static string GetName(int num)
        {
            var res = SpellsByName.FirstOrDefault(kvp => kvp.Value.Number == num);
            if (res.Key == null)
                return SpellsByName.First().Key;
            return res.Key;
        }

        public static Spell Get(int circle, int num)
        {
            return Get(Spell.ToID(circle, num));
        }
    }
}
