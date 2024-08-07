using ClassicUO.Game;
using ClassicUO.Network;
using ClassicUO.Game.Managers;
using ClassicUO.IO.Resources;
using ClassicUO.Game.UI.Controls;
using SDL2;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Assistant.Core;
using Assistant.Scripts;

using AssistGump = ClassicUO.Game.UI.Gumps.AssistantGump;

namespace Assistant
{
    internal enum MacroAction : byte
    {
        None        = 0x00,
        PassToUO    = 0x01,
        Loop        = 0x02,
        PassUOLoop  = 0x03,
        NoInterrupt = 0x04,
        PassUONoInt = 0x05,
        LoopNoInt   = 0x06,
        UOLoopNoInt = 0x07
    }

    internal class HotKeyOpts
    {
        internal MacroAction Options;
        internal string Action { get; }
        internal string Param { get; }
        internal string Macro { get; set; }

        internal HotKeyOpts(MacroAction options, string action, string param = null)
        {
            Options = options;
            Action = action;
            Param = param;
            if (param != null)
                Macro = string.Empty;
        }

        internal bool PassToUO
        {
            get
            {
                return (Options & MacroAction.PassToUO) != 0;
            }
            set
            {
                if (value)
                {
                    Options |= MacroAction.PassToUO;
                }
                else
                {
                    Options &= ~MacroAction.PassToUO;
                }
            }
        }
        internal bool Loop
        {
            get
            {
                return (Options & MacroAction.Loop) != 0;
            }
            set
            {
                if (value)
                {
                    Options |= MacroAction.Loop;
                }
                else
                {
                    Options &= ~MacroAction.Loop;
                }
            }
        }
        internal bool NoAutoInterrupt
        {
            get
            {
                return (Options & MacroAction.NoInterrupt) != 0;
            }
            set
            {
                if (value)
                {
                    Options |= MacroAction.NoInterrupt;
                }
                else
                {
                    Options &= ~MacroAction.NoInterrupt;
                }
            }
        }
    }

    public class HotKeys
    {
        internal static void ClearHotkeys()
        {
            _RevHotKeyContainer.Clear();
            _HotKeyContainer.Clear();
        }

        internal static void AddHotkey(uint key, HotKeyOpts keyopt, AssistHotkeyBox box, ref string hkname, AssistGump gump, bool overwrite = false)
        {
            if (keyopt == null || string.IsNullOrEmpty(keyopt.Action) || !_HotKeyActions.ContainsKey(keyopt.Action))
                return;
            string val = keyopt.Param ?? keyopt.Action;
            _HotKeyContainer.TryGetValue(key, out HotKeyOpts ops);
            string oval = null;
            if (ops != null)
            {
                oval = ops.Param ?? ops.Action;
            }
            if (!string.IsNullOrEmpty(oval) && oval != val && !overwrite)
            {
                UIManager.Add(new AssistGump.OverWriteHKGump(key, keyopt, box, ref hkname, ops.Param ?? ops.Action));
            }
            else
            {
                if(ops != null && _RevHotKeyContainer.TryGetValue(oval, out uint vkey))
                {
                    _HotKeyContainer.Remove(vkey);
                    _RevHotKeyContainer.Remove(oval);
                }
                if(_RevHotKeyContainer.TryGetValue(val, out vkey))
                {
                    _HotKeyContainer.Remove(vkey);
                    _RevHotKeyContainer.Remove(val);
                }
                _HotKeyContainer[key] = keyopt;
                _RevHotKeyContainer[val] = key;
                if (gump != null)
                {
                    GetSDLfromVK(key, out int skey, out int smod);
                    if (skey > 0)
                    {
                        AssistHotkeyBox hkbox = null;
                        switch(gump.ActivePage)
                        {
                            case (int)AssistGump.PageType.Hotkeys when gump.SelectedHK != null:
                            {
                                hkbox = gump._macrokeyName;
                                goto case (int)AssistGump.PageType.LastMain;
                            }
                            case (int)AssistGump.PageType.Macros when gump.MacroSelected != null:
                            {
                                hkbox = gump._keyName;
                                goto case (int)AssistGump.PageType.LastMain;
                            }
                            case (int)AssistGump.PageType.LastMain:
                            {
                                if (gump.SelectedHK == gump.MacroSelected)
                                    hkbox.SetKey((SDL.SDL_Keycode)skey, (SDL.SDL_Keymod)smod);
                                else if (hkbox.Key == (SDL.SDL_Keycode)skey && hkbox.Mod == (SDL.SDL_Keymod)smod)
                                    hkbox.SetKey(SDL.SDL_Keycode.SDLK_UNKNOWN, SDL.SDL_Keymod.KMOD_NONE);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static Dictionary<string, uint> _RevHotKeyContainer = new Dictionary<string, uint>();
        private static Dictionary<uint, HotKeyOpts> _HotKeyContainer = new Dictionary<uint, HotKeyOpts>();
        internal static ReadOnlyDictionary<uint, HotKeyOpts> AllHotKeys => new ReadOnlyDictionary<uint, HotKeyOpts>(_HotKeyContainer);
        private static Dictionary<string, Func<string, bool>> _HotKeyActions = new Dictionary<string, Func<string, bool>>();

        internal static void GetSDLKeyCodes(string hkname, out int key, out int mod)
        {
            if(!string.IsNullOrEmpty(hkname) && _RevHotKeyContainer.TryGetValue(hkname, out uint vkey))
            {
                GetSDLfromVK(vkey, out key, out mod);
            }
            else
            {
                key = 0;
                mod = 0;
            }
        }

        private static void GetSDLfromVK(uint vkey, out int key, out int mod)
        {
            XmlFileParser.vkToSDLkey.TryGetValue(vkey & 0x1FF, out SDL.SDL_Keycode code);
            XmlFileParser.vkmToSDLmod.TryGetValue(vkey & 0xE00, out SDL.SDL_Keymod kmod);
            key = (int)code;
            if (key > 0)
                mod = (int)kmod;
            else
                mod = 0;
        }

        internal static bool NonBlockHotKeyAction(uint vkey)
        {
            bool noblock = true;
            if (!UOSObjects.Gump._keyName.IsActive && !UOSObjects.Gump._macrokeyName.IsActive && _HotKeyContainer.TryGetValue(vkey, out HotKeyOpts hk) && !string.IsNullOrEmpty(hk.Action) && (!UOSObjects.Gump.ToggleHotKeys || hk.Action == "main.togglehotkeys" || hk.Action == "macro.play") && _HotKeyActions.TryGetValue(hk.Action, out Func<string, bool> func))
            {
                string param = hk.Param;
                if (param == null)
                {
                    if (hk.Action.StartsWith("spells."))
                        param = hk.Action.Substring(hk.Action.LastIndexOf('.') + 1);
                }
                noblock = !func(param) || hk.PassToUO;
            }
            return noblock;
        }

        internal class SkillHotKeys
        {
            public static void CleanUP()
            {
                foreach (KeyValuePair<string, int> kvp in ScriptManager.SkillMap)
                {
                    _HotKeyActions.Remove(kvp.Key);
                }
            }

            public static void Initialize()
            {
                _HotKeyActions["skills.last"] = (input) =>
                {
                    if (UOSObjects.Player.LastSkill >= 0 && UOSObjects.Player.LastSkill < SkillsLoader.Instance.Skills.Count)
                    {
                        SkillEntry se = SkillsLoader.Instance.Skills[UOSObjects.Player.LastSkill];
                        if (se.HasAction)
                        {
                            Engine.Instance.SendToServer(new UseSkill(UOSObjects.Player.LastSkill));
                            if (ScriptManager.Recording)
                                ScriptManager.AddToScript($"useskill '{se.Name.ToLower(XmlFileParser.Culture)}'");
                        }
                    }
                    return true;
                };
                foreach (KeyValuePair<string, int> kvp in ScriptManager.SkillMap)
                {
                    _HotKeyActions[$"skills.{kvp.Key}"] = (input) =>
                    {
                        Engine.Instance.SendToServer(new UseSkill(kvp.Value));
                        if (ScriptManager.Recording)
                            ScriptManager.AddToScript($"useskill '{kvp.Key}'");
                        return true;
                    };
                }
            }
        }

        internal static void AddHotKeyFunc(string keyname, Func<string, bool> action)
        {
            _HotKeyActions[keyname] = action;//.ToLower(XmlFileParser.Culture).Replace(" ", "").Split('(')[0]] = action;
        }

        internal static void RemoveHotKey(string realkeyname)
        {
            if(_RevHotKeyContainer.TryGetValue(realkeyname, out uint keyval))
            {
                _HotKeyContainer.Remove(keyval);
                _RevHotKeyContainer.Remove(realkeyname);
                if (UOSObjects.Gump.SelectedHK == realkeyname)
                    UOSObjects.Gump.SelectedHK = null;
            }
        }

        internal static bool GetVKfromSDL(int key, int mod, out uint vkey)
        {
            if(XmlFileParser.SDLkeyToVK.TryGetValue((SDL.SDL_Keycode)key, out (uint, string) qkey))
            {
                vkey = qkey.Item1;
                if (XmlFileParser.SDLmodToVK.TryGetValue(mod & 0x3C3, out uint mkey))
                    vkey |= mkey;
                return true;
            }
            vkey = 0;
            return false;
        }

        internal static void PlayFunc(string funcname, string param = null)
        {
            if (_HotKeyActions.TryGetValue(funcname, out var func))
            {
                func?.Invoke(param);
            }
        }

        public static void Initialize()
        {
            AddHotKeyFunc("main.ping", (input) => 
            {
                Ping.StartPing(5);
                return true;
            });
            AddHotKeyFunc("main.resyncronize", (input) =>
            {
                Resync();
                return true;
            });
            AddHotKeyFunc("main.togglehotkeys", (input) =>
            {
                Commands.HotKeys(null, null, false, false);
                return true;
            });
            AddHotKeyFunc("main.snapshot", (input) =>
            {
                UOSObjects.SnapShot(false);
                return true;
            });
            AddHotKeyFunc("actions.grabitem", (input) =>
            {
                GrabItem();
                return true;
            });
            AddHotKeyFunc("actions.dropcurrent", (input) =>
            {
                DragDropManager.DropCurrent();
                return true;
            });
            AddHotKeyFunc("actions.togglemounted", (input) =>
            {
                return Commands.ToggleMounted(null, null, false, false);
            });
            AddHotKeyFunc("actions.use.lastobject", (input) =>
            {
                if (SerialHelper.IsValid(UOSObjects.Player.LastObject))
                {
                    Engine.Instance.SendToServer(new DoubleClick(UOSObjects.Player.LastObject));
                    return true;
                }
                UOSObjects.Player.SendMessage(MsgLevel.Error, "No valid last object present!");
                return false;
            });
            AddHotKeyFunc("actions.use.lefthand", (input) =>
            {
                UOItem i = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
                if (i != null)
                {
                    Engine.Instance.SendToServer(new DoubleClick(i.Serial));
                    return true;
                }
                UOSObjects.Player.SendMessage(MsgLevel.Error, "Use: No object found on Left Hand!");
                return false;
            });
            AddHotKeyFunc("actions.use.righthand", (input) =>
            {
                UOItem i = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
                if (i != null)
                {
                    Engine.Instance.SendToServer(new DoubleClick(i.Serial));
                    return true;
                }
                UOSObjects.Player.SendMessage(MsgLevel.Error, "Use: No object found on Right Hand!");
                return false;
            });
            AddHotKeyFunc("actions.shownames.all", (input) =>
            {
                AllNames();
                return true;
            });
            AddHotKeyFunc("actions.shownames.corpses", (input) =>
            {
                AllCorpses();
                return true;
            });
            AddHotKeyFunc("actions.shownames.mobiles", (input) =>
            {
                AllMobiles();
                return true;
            });
            AddHotKeyFunc("actions.creatures.come", (input) =>
            {
                PetAllCome();
                return true;
            });
            AddHotKeyFunc("actions.creatures.follow", (input) =>
            {
                PetAllFollow();
                return true;
            });
            AddHotKeyFunc("actions.creatures.guard", (input) =>
            {
                PetAllGuard();
                return true;
            });
            AddHotKeyFunc("actions.creatures.kill", (input) =>
            {
                PetAllKill();
                return true;
            });
            AddHotKeyFunc("actions.creatures.stay", (input) =>
            {
                PetAllStay();
                return true;
            });
            AddHotKeyFunc("actions.creatures.stop", (input) =>
            {
                PetAllStop();
                return true;
            });
            AddHotKeyFunc("agents.autoloottarget", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.AutolootAgent))
                {
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "Target Container to Loot");
                    Targeting.OneTimeTarget(false, AutoLootOnTarget);
                }
                return true;
            });
            AddHotKeyFunc("agents.toggleautoloot", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.AutolootAgent))
                {
                    UOSObjects.Gump.AutoLoot = !UOSObjects.Gump.AutoLoot;
                    if (UOSObjects.Gump.AutoLoot)
                        UOSObjects.Player.SendMessage(MsgLevel.Friend, "AutoLoot Enabled!");
                    else
                        UOSObjects.Player.SendMessage(MsgLevel.Warning, "AutoLoot Disabled!");
                }
                return true;
            });
            AddHotKeyFunc("agents.togglescavenger", (input) =>
            {
                UOSObjects.Gump.EnabledScavenger.IsChecked = !UOSObjects.Gump.EnabledScavenger.IsChecked;
                return true;
            });
            AddHotKeyFunc("combat.abilities.primary", (input) =>
            {
                SpecialMoves.SetPrimaryAbility();
                return true;
            });
            AddHotKeyFunc("combat.abilities.secondary", (input) =>
            {
                SpecialMoves.SetSecondaryAbility();
                return true;
            });
            AddHotKeyFunc("combat.abilities.stun", (input) =>
            {
                SpecialMoves.OnStun();
                return true;
            });
            AddHotKeyFunc("combat.abilities.disarm", (input) =>
            {
                SpecialMoves.OnDisarm();
                return true;
            });
            AddHotKeyFunc("combat.attack.enemy", (input) =>
            {
                uint ser = Targeting.RandomTarget(UOSObjects.Gump.SmartTargetRangeValue, false, false, MobType.Any, Targeting.TargetType.Enemy, true);
                if (SerialHelper.IsMobile(ser))
                    Engine.Instance.SendToServer(new AttackReq(ser));
                return true;
            });
            AddHotKeyFunc("combat.attack.lasttarget", (input) =>
            {
                Targeting.AttackLastTarg();
                return true;
            });
            AddHotKeyFunc("combat.attack.lastcombatant", (input) =>
            {
                Targeting.AttackLastComb();
                return true;
            });
            AddHotKeyFunc("combat.bandage.self", (input) =>
            {
                BandageSelf();
                return true;
            });
            AddHotKeyFunc("combat.bandage.last", (input) =>
            {
                BandageLastTarg();
                return true;
            });
            AddHotKeyFunc("combat.bandage.mount", (input) =>
            {
                BandageMount();
                return true;
            });
            AddHotKeyFunc("combat.bandage.target", (input) =>
            {
                Bandage();
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.agility", (input) =>
            {
                if(Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3848);
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.cure", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3847, 0);
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.explosion", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3853);
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.heal", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3852);
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.refresh", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3851);
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.strength", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3849);
                return true;
            });
            AddHotKeyFunc("combat.consume.potions.nightsight", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(3846);
                return true;
            });
            AddHotKeyFunc("combat.consume.miscellaneous.enchantedapple", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(12248, 1160);
                return true;
            });
            AddHotKeyFunc("combat.consume.miscellaneous.roseoftrinsic", (input) =>
            {
                if (Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                    OnUseItem(4129, 14);
                return true;
            });
            AddHotKeyFunc("combat.togglehands.left", (input) =>
            {
                Dress.ToggleLeft();
                return true;
            });
            AddHotKeyFunc("combat.togglehands.right", (input) =>
            {
                Dress.ToggleRight();
                return true;
            });
            AddHotKeyFunc("combat.equipwands.clumsy", (input) =>
            {
                UseWand(WandEffect.Clumsiness, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.identification", (input) =>
            {
                UseWand(WandEffect.Identification, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.heal", (input) =>
            {
                UseWand(WandEffect.Healing, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.feeblemind", (input) =>
            {
                UseWand(WandEffect.Feeblemindedness, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.weakness", (input) =>
            {
                UseWand(WandEffect.Weakness, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.magicarrow", (input) =>
            {
                UseWand(WandEffect.MagicArrow, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.harm", (input) =>
            {
                UseWand(WandEffect.Harming, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.fireball", (input) =>
            {
                UseWand(WandEffect.Fireball, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.greaterheal", (input) =>
            {
                UseWand(WandEffect.GreaterHealing, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.lightning", (input) =>
            {
                UseWand(WandEffect.Lightning, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("combat.equipwands.manadrain", (input) =>
            {
                UseWand(WandEffect.ManaDraining, UOSObjects.Player.Backpack);
                return true;
            });
            AddHotKeyFunc("targeting.attack.enemy", (input) =>
            {
                uint s = UOScript.Interpreter.GetAlias("enemy");
                if(SerialHelper.IsValid(s))
                {
                    Targeting.AttackTarget(s);
                }
                return true;
            });
            AddHotKeyFunc("targeting.attack.last", (input) =>
            {
                Targeting.AttackLastTarg();
                return true;
            });
            AddHotKeyFunc("targeting.friends.add", (input) =>
            {
                UOSObjects.Gump.OnButtonClick(AssistGump.InsertFriendButton);
                return true;
            });
            AddHotKeyFunc("targeting.friends.remove", (input) =>
            {
                UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "Friend List: Target mobile to remove");
                Targeting.OneTimeTarget(false, Targeting.OnRemoveFriendSelected);
                return true;
            });
            AddHotKeyFunc("targeting.set.enemy", (input) =>
            {
                
                return true;
            });
            AddHotKeyFunc("targeting.set.friend", (input) =>
            {
                
                return true;
            });
            AddHotKeyFunc("targeting.set.last", (input) =>
            {
                
                return true;
            });
            AddHotKeyFunc("targeting.set.mount", (input) =>
            {
                
                return true;
            });

            //TODO: HotKeys - combat.consume.Miscellaneous - Orange Petals - Wrath Grapes - Smoke Bomb - Spell Stone - Healing Stone
            // Set the packet handler for single click and establish hotkeys for grab item
            AddHotKeyFunc("macro.play", (input) =>
            {
                if (string.IsNullOrEmpty(input))
                    return false;
                return ScriptManager.PlayScript(input);
            });
            AddHotKeyFunc("macro.stop", (input) =>
            {
                ScriptManager.StopScript();
                return true;
            });
            PacketHandler.RegisterClientToServerViewer(0x09, new PacketViewerCallback(OnGrabItemSingleClick));
        }

        /*private static void ToggleGoldPer()
        {
            if (GoldPerHourTimer.Running)
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Stopping 'GoldPer Timer'");
                GoldPerHourTimer.Stop();
            }
            else
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Starting 'GoldPer Timer' when you loot your first gold");
                GoldPerHourTimer.Start();
            }
        }

        private static void ToggleDamage()
        {
            Engine.MainWindow.ToggleDamageTracker(!DamageTracker.Running);
        }*/

        private enum PetCommands
        {
            AllCome,
            AllFollowMe,
            AllFollow,
            AllGuardMe,
            AllGuard,
            AllKill,
            AllStay,
            AllStop
        }

        private static LootTimer _LootTimer;
        internal static void AutoLootOnTarget(bool loc, uint serial, Point3D p, ushort itemid)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.AutolootAgent))
                return;

            if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);
                if (item != null && (item.IsContainer || item.IsCorpse))
                {
                    if (_LootTimer != null && _LootTimer.Running)
                        _LootTimer.Stop();
                    _LootTimer = new LootTimer(item);
                }
            }
            else
                UOSObjects.Player.OverheadMessage(22, "Autoloot: Invalid target");
        }

        private class LootTimer : Timer
        {
            HashSet<ushort> _toLoot;
            UOItem _Container, _LootCont;

            internal LootTimer(UOItem lootcont) : base(TimeSpan.Zero, TimeSpan.FromMilliseconds(Math.Max(UOSObjects.Gump.ActionDelay, 600)))
            {
                if (UOSObjects.Player.Backpack == null || lootcont == null)
                    return;

                if(UOSObjects.Gump.AutoLootContainer > 0 && SerialHelper.IsItem(UOSObjects.Gump.AutoLootContainer))
                {
                    _Container = UOSObjects.FindItem(UOSObjects.Gump.AutoLootContainer);
                    if (_Container == null || !UOSObjects.Player.Backpack.ContainsItemBySerial(_Container.Serial, true))
                        _Container = UOSObjects.Player.Backpack;
                }
                else
                    _Container = UOSObjects.Player.Backpack;
                _toLoot = new HashSet<ushort>(UOSObjects.Gump.ItemsToLoot.Keys);
                if (_toLoot.Count > 0)
                {
                    _LootCont = lootcont;
                    Start();
                }
            }

            protected override void OnTick()
            {
                if (_Container == null || _LootCont == null)
                {
                    Stop();
                    return;
                }
                UOItem item = _LootCont.FindItemByID(_toLoot);
                if (item != null)
                {
                    DragDropManager.DragDrop(item, _Container);
                }
                else
                    Stop();
            }
        }

        private static void PetAllCome()
        {
            UOSObjects.Player.Say("All Come");
        }

        private static void PetAllFollow()
        {
            UOSObjects.Player.Say("All Follow");
        }

        private static void PetAllGuard()
        {
            UOSObjects.Player.Say("All Guard");
        }

        private static void PetAllKill()
        {
            UOSObjects.Player.Say("All Kill");
        }

        private static void PetAllStay()
        {
            UOSObjects.Player.Say("All Stay");
        }

        private static void PetAllStop()
        {
            UOSObjects.Player.Say("All Stop");
        }

        /*private static void CaptureBod()
        {
            try
            {
                if (BodCapture.IsBodGump(UOSObjects.Player.CurrentGumpI))
                {
                    BodCapture.CaptureBod(UOSObjects.Player.CurrentGumpStrings);

                    UOSObjects.Player.SendMessage(MsgLevel.Force, "BOD has been captured and saved to BODs.csv");
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "The last gump you had open doesn't appear to be a BOD");
                }
            }
            catch
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Unable to capture BOD, probably unknown format");
            }
        }*/


        private static void PartyAccept()
        {
            if (PacketHandlers.PartyLeader != 0)
            {
                Engine.Instance.SendToServer(new AcceptParty(PacketHandlers.PartyLeader));
                PacketHandlers.PartyLeader = 0;
            }
        }

        private static void PartyDecline()
        {
            if (PacketHandlers.PartyLeader != 0)
            {
                Engine.Instance.SendToServer(new DeclineParty(PacketHandlers.PartyLeader));
                PacketHandlers.PartyLeader = 0;
            }
        }

        private static void Dismount()
        {
            if (UOSObjects.Player.GetItemOnLayer(Layer.Mount) != null)
                ActionQueue.DoubleClick(true, UOSObjects.Player.Serial);
            else
                UOSObjects.Player.SendMessage("You are not mounted.");
        }

        private static void AllNames()
        {
            foreach (UOMobile m in UOSObjects.MobilesInRange())
            {
                if (m != UOSObjects.Player)
                    Engine.Instance.SendToServer(new SingleClick(m));

                Targeting.CheckTextFlags(m);

                if (FriendsManager.IsFriend(m.Serial))
                {
                    m.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "[Friend]");
                }
            }

            foreach (UOItem i in UOSObjects.Items.Values)
            {
                if (i.IsCorpse)
                    Engine.Instance.SendToServer(new SingleClick(i));
            }
        }

        private static void AllCorpses()
        {
            foreach (UOItem i in UOSObjects.Items.Values)
            {
                if (i.IsCorpse)
                    Engine.Instance.SendToServer(new SingleClick(i));
            }
        }

        internal static void AllMobiles()
        {
            foreach (UOMobile m in UOSObjects.MobilesInRange())
            {
                if (m != UOSObjects.Player)
                    Engine.Instance.SendToServer(new SingleClick(m));

                Targeting.CheckTextFlags(m);

                if (FriendsManager.IsFriend(m.Serial))
                {
                    m.OverheadMessage(PlayerData.GetColorCode(MsgLevel.Friend), "[Friend]");
                }
            }
        }

        private static void LastSkill()
        {
            if (UOSObjects.Player != null && UOSObjects.Player.LastSkill != -1)
                Engine.Instance.SendToServer(new UseSkill(UOSObjects.Player.LastSkill));
        }

        private static void LastObj()
        {
            if (UOSObjects.Player != null && UOSObjects.Player.LastObject != 0)
                PlayerData.DoubleClick(UOSObjects.Player.LastObject);
        }

        private static void LastSpell()
        {
            if (UOSObjects.Player != null && UOSObjects.Player.LastSpell != -1)
            {
                ushort id = (ushort)UOSObjects.Player.LastSpell;
                Spell.OnHotKey(id);
            }
        }

        private static DateTime m_LastSync;

        private static void Resync()
        {
            if (DateTime.UtcNow - m_LastSync > TimeSpan.FromSeconds(1.0))
            {
                m_LastSync = DateTime.UtcNow;

                Engine.Instance.SendToServer(new ResyncReq());
            }
        }

        public static void BandageMount()
        {
            if (Bandage())
            {
                if (!SerialHelper.IsValid(UOSObjects.Gump.MountSerial))
                {
                    Targeting.OneTimeTarget(false, (ground, serial, location, graphic) =>
                    {
                        Commands.TargetMountResponse(ground, serial, location, graphic);
                        if (!SerialHelper.IsValid(UOSObjects.Gump.MountSerial))
                            UOSObjects.Gump.MountSerial = 0;
                        else
                            Finalize();
                    });
                }
                else
                {
                    Finalize();
                }
            }
            void Finalize()
            {
                Targeting.Target(UOSObjects.Gump.MountSerial, true);
                BandageTimer.Start();
            }
        }

        public static void BandageLastTarg()
        {
            if (Bandage())
            {
                Targeting.LastTarget(true); //force a targetself to be queued
                BandageTimer.Start();
            }
        }

        public static void BandageSelf()
        {
            if (Bandage())
            {
                Targeting.ClearQueue();
                Targeting.TargetSelf(true); //force a targetself to be queued
                BandageTimer.Start();
            }
        }

        public static void BandageRessFriend()
        {
            if(Bandage())
            {
                Targeting.ClearQueue();
                Targeting.ClosestTarget(2, true, true, MobType.Any, 2);//2 is for GuildAlly type
                BandageTimer.Start();
            }
        }

        public static bool Bandage()
        {
            UOItem pack = UOSObjects.Player.Backpack;
            if (pack != null)
            {
                if (!UseItem(pack, 3617))
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "No bandages found");
                    return false;
                }
                return true;
            }
            UOSObjects.Player.SendMessage(MsgLevel.Warning, "No backpack found");
            return false;
        }

        private static void OnUseItem(ushort id, ushort color = ushort.MaxValue)
        {
            UOItem pack = UOSObjects.Player.Backpack;
            if (pack == null)
                return;

            if (id == 3852 && UOSObjects.Player.Poisoned && UOSObjects.Gump.BlockInvalidHeal &&
                Engine.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Heal blocked (target is poisoned)!");
                return;
            }

            if (!UseItem(pack, id, color))
                UOSObjects.Player.SendMessage("No item of type 0x{0:X4} found!", id);
        }

        private static void UseItemInHand()
        {
            UOItem item = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
            if (item == null)
                item = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);

            if (item != null)
                PlayerData.DoubleClick(item.Serial);
        }

        private static void UseItemInRightHand()
        {
            UOItem item = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);

            if (item != null)
                PlayerData.DoubleClick(item.Serial);
        }

        private static void UseItemInLeftHand()
        {
            UOItem item = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);

            if (item != null)
                PlayerData.DoubleClick(item.Serial);
        }

        private static bool UseItem(UOItem cont, ushort find)
        {
            return UseItem(cont, find, ushort.MaxValue);
        }

        private static bool UseItem(UOItem cont, ushort find, ushort hue)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                return false;

            for (int i = 0; i < cont.Contains.Count; i++)
            {
                UOItem item = cont.Contains[i];

                if (item.ItemID == find && (hue == ushort.MaxValue || hue == item.Hue))
                {
                    PlayerData.DoubleClick(item.Serial);
                    return true;
                }
                else if (item.Contains != null && item.Contains.Count > 0)
                {
                    if (UseItem(item, find, hue))
                        return true;
                }
            }

            return false;
        }

        private enum WandEffect
        {
            Clumsiness = 1017326,
            Identification = 1017350,
            Healing = 1017329,
            Feeblemindedness = 1017327,
            Weakness = 1017328,
            MagicArrow = 1060492,
            Harming = 1060489,
            Fireball = 1060487,
            GreaterHealing = 1060488,
            Lightning = 1060491,
            ManaDraining = 1017339,
            Paralyze = 1017340,
            Invisibility = 1017347,
            None = 0
        }

        private static bool UseWand(WandEffect effect, UOItem cont, ushort hue = ushort.MaxValue, params ushort[] itemids)
        {
            if (cont == null || !Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                return false;
            UOItem item;
            if (cont == UOSObjects.Player.Backpack)
            {
                item = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);
                if (item != null && (itemids == null || itemids.Contains(item.ItemID)) && (hue == ushort.MaxValue || hue == item.Hue) && item.ObjPropList.Content.Any(opl => opl.Number == (int)effect && int.TryParse(opl.Args, out int num) && num > 0))
                {
                    PlayerData.DoubleClick(item.Serial);
                    return true;
                }
            }

            for (int i = 0; i < cont.Contains.Count; i++)
            {
                item = cont.Contains[i];

                if ((itemids == null || itemids.Contains(item.ItemID)) && (hue == ushort.MaxValue || hue == item.Hue) && item.ObjPropList.Content.Any(opl => opl.Number == (int)effect && int.TryParse(opl.Args, out int num) && num > 0))
                {
                    PlayerData.DoubleClick(item.Serial);
                    return true;
                }
                else if (item.Contains != null && item.Contains.Count > 0)
                {
                    if (UseWand(effect, item, hue, itemids))
                        return true;
                }
                else
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, $"'{effect}' wand with a minimum of 1 charges not found.");
                }
            }
            return false;
        }

        private static void GrabItem()
        {
            UOSObjects.Player.SendMessage(MsgLevel.Force, "Target item to grab");
            Targeting.OneTimeTarget(OnGrabItem, true);
        }

        private static void OnGrabItem(bool loc, uint serial, Point3D pt, ushort itemId)
        {
            UOItem item;
            if (SerialHelper.IsItem(serial) && (item = UOSObjects.FindItem(serial)) != null && item.Movable && item.Visible)
            {
                UOItem hotbag = UOSObjects.FindItem(UOSObjects.Gump.GrabHotBag) ?? UOSObjects.Player.Backpack;
                if(hotbag != null)
                    DragDropManager.DragDrop(item, hotbag);
            }
            else
            {
                UOSObjects.Player.SendMessage(MsgLevel.Error, "Invalid or inaccessible item.", false);
            }
        }

        private static void SetGrabItemHotBag()
        {
            UOSObjects.Player.SendMessage(MsgLevel.Force, "Set Grab Item HotBag");
            Targeting.OneTimeTarget(OnSetGrabItemHotBag);
        }

        private static void OnSetGrabItemHotBag(bool loc, uint serial, Point3D pt, ushort itemId)
        {
            if (!loc && SerialHelper.IsItem(serial))
            {
                UOItem hb = UOSObjects.FindItem(serial);

                if (hb != null)
                {
                    UOSObjects.Gump.GrabHotBag = serial;
                    //TODO: Grab hotbag
                    //UOSObjects.Gump.
                    //Config.SetProperty("GrabHotBag", serial.Value.ToString());

                    hb.ObjPropList.Add("(Grab Item HotBag)");
                    hb.OPLChanged();

                    UOSObjects.Player.SendMessage(MsgLevel.Force, "Grab Item HotBag Set");
                }
                else
                {
                    UOSObjects.Gump.GrabHotBag = 0;
                }
            }
        }

        private static void OnGrabItemSingleClick(Packet pvSrc, PacketHandlerEventArgs args)
        {
            uint serial = pvSrc.ReadUInt();
            if (UOSObjects.Gump.GrabHotBag == serial)
            {
                ushort gfx = 0;
                UOItem c = UOSObjects.FindItem(UOSObjects.Gump.GrabHotBag);
                if (c != null)
                {
                    gfx = c.ItemID;
                }

                Engine.Instance.SendToClient(new UnicodeMessage(UOSObjects.Gump.GrabHotBag, gfx, MessageType.Label, 0x3B2, 3, "ENU", "", "(Grab Item HotBag)"));
            }
        }
    }
}
