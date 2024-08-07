using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Assistant.Core;
using ClassicUO;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Game;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Data;
using ClassicUO.IO.Resources;
using UOScript;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ClassicUO.Game.GameObjects;
using ClassicUO.Input;
using System.Runtime.InteropServices;
using ClassicUO.Network;
using System.Threading.Tasks;

namespace Assistant.Scripts
{
    internal static class Commands
    {
        public static void Register()
        {
            // Commands. From UOSteam Documentation
            Interpreter.RegisterCommandHandler("attack", Attack, "attack (serial)");
            Interpreter.RegisterCommandHandler("warmode", WarMode, "warmode ['on'/'off']");

            // Menu
            Interpreter.RegisterCommandHandler("clearjournal", ClearJournal, "clearjournal");
            Interpreter.RegisterCommandHandler("waitforjournal", WaitForJournal, "waitforjournal ('text') (timeout) ['author'/'system']");


            Interpreter.RegisterCommandHandler("msg", Msg, "msg ('text') [color]");
            Interpreter.RegisterCommandHandler("whispermsg", WhisperMsg, "whispermsg ('text')");
            Interpreter.RegisterCommandHandler("yellmsg", YellMsg, "yellmsg ('text')");
            Interpreter.RegisterCommandHandler("emotemsg", EmoteMsg, "emotemsg ('text') [color]");

            Interpreter.RegisterCommandHandler("partymsg", PartyMsg, "partymsg ('text')");
            Interpreter.RegisterCommandHandler("guildmsg", GuildMsg, "guildmsg ('text')");
            Interpreter.RegisterCommandHandler("allymsg", AllyMsg, "allymsg ('text')");
            Interpreter.RegisterCommandHandler("headmsg", HeadMsg, "headmsg ('text') [color] [serial]");
            Interpreter.RegisterCommandHandler("sysmsg", SysMsg, "sysmsg ('text') [color]");
            Interpreter.RegisterCommandHandler("chatmsg", DummyCommand, null);//chatmsg ('text')
            Interpreter.RegisterCommandHandler("timermsg", DummyCommand, null);//timermsg ('timer name') [color]

            Interpreter.RegisterCommandHandler("poplist", PopList, "poplist ('list name') ('element value'/'front'/'back')");
            Interpreter.RegisterCommandHandler("pushlist", PushList, "pushlist ('list name') ('element value') ['front'/'back']");
            Interpreter.RegisterCommandHandler("removelist", RemoveList, "removelist ('list name')");
            Interpreter.RegisterCommandHandler("createlist", CreateList, "createlist ('list name')");
            Interpreter.RegisterCommandHandler("clearlist", ClearList, "clearlist ('list name')");

            Interpreter.RegisterCommandHandler("setalias", SetAlias, "setalias ('name') [serial]");
            Interpreter.RegisterCommandHandler("unsetalias", UnsetAlias, "unsetalias ('name')");

            Interpreter.RegisterCommandHandler("shownames", ShowNames, "shownames ['all'/mobiles'/'corpses'] [range]");

            Interpreter.RegisterCommandHandler("contextmenu", ContextMenu, "contextmenu (serial) (option)"); //ContextMenuAction
            Interpreter.RegisterCommandHandler("waitforcontext", WaitForContext, "waitforcontext (serial) (option) (timeout)"); //WaitForMenuAction

            // Targets
            Interpreter.RegisterCommandHandler("target", Target, "target (serial) [timeout]");
            Interpreter.RegisterCommandHandler("targettype", TargetType, "targettype (graphic) [color] [range]");
            Interpreter.RegisterCommandHandler("targetground", TargetGround, "targetground (graphic) [color] [range]");
            Interpreter.RegisterCommandHandler("targettile", TargetTile, "targettile ('last'/'current'/(x y z))");
            Interpreter.RegisterCommandHandler("targettileoffset", TargetTileOffset, "targettileoffset (x y z)");
            Interpreter.RegisterCommandHandler("targettilerelative", TargetTileRelative, "targettilerelative (serial) (range)");
            Interpreter.RegisterCommandHandler("waitfortarget", WaitForTarget, "waitfortarget (timeout)");
            Interpreter.RegisterCommandHandler("canceltarget", CancelTarget, "canceltarget");
            Interpreter.RegisterCommandHandler("cleartargetqueue", ClearTargetQueue, null);
            Interpreter.RegisterCommandHandler("autotargetlast", AutoTargetLast, "autotargetlast");
            Interpreter.RegisterCommandHandler("autotargetself", AutoTargetSelf, "autotargetself");
            Interpreter.RegisterCommandHandler("autotargetobject", AutoTargetObject, "autotargetobject (serial)");
            Interpreter.RegisterCommandHandler("autotargettype", TargetType, "autotargettype (graphic) [color] [range]");
            Interpreter.RegisterCommandHandler("autotargettile", TargetTile, "autotargettile ('last'/'current'/(x y z))");
            Interpreter.RegisterCommandHandler("autotargettileoffset", TargetTileOffset, "autotargettileoffset (x y z)");
            Interpreter.RegisterCommandHandler("autotargettilerelative", TargetTileRelative, "autotargettilerelative (serial) (range)");
            Interpreter.RegisterCommandHandler("autotargetghost", AutoTargetGhost, "autotargetghost (range) [z-range]");
            Interpreter.RegisterCommandHandler("autotargetground", TargetGround, "autotargetground (graphic) [color] [range]");
            Interpreter.RegisterCommandHandler("cancelautotarget", CancelAutoTarget, "cancelautotarget");
            Interpreter.RegisterCommandHandler("getenemy", GetEnemy, "getenemy ('innocent'/'criminal'/'enemy'/'murderer'/'friend'/'gray'/'invulnerable'/'any') ['humanoid'/'transformation'/'nearest'/'closest']");
            Interpreter.RegisterCommandHandler("getfriend", GetFriend, "getfriend ('innocent'/'criminal'/'enemy'/'murderer'/'friend'/'gray'/'invulnerable'/'any') ['humanoid'/'transformation'/'nearest'/'closest']");

            Interpreter.RegisterCommandHandler("settimer", SetTimer, "settimer ('timer name') (value)");
            Interpreter.RegisterCommandHandler("removetimer", RemoveTimer, "removetimer ('timer name')");
            Interpreter.RegisterCommandHandler("createtimer", CreateTimer, "createtimer ('timer name')");

            Interpreter.RegisterCommandHandler("clickobject", ClickObject, "clickobject (serial)");

            // Using stuff
            Interpreter.RegisterCommandHandler("usetype", UseType, "usetype (graphic) [color] [source] [range]");
            Interpreter.RegisterCommandHandler("useobject", UseObject, "useobject (serial)");
            Interpreter.RegisterCommandHandler("useonce", UseOnce, "useonce (graphic) [color]");

            Interpreter.RegisterCommandHandler("fly", Fly, null);
            Interpreter.RegisterCommandHandler("land", Land, null);

            Interpreter.RegisterCommandHandler("bandage", Bandage, "bandage [serial]");
            Interpreter.RegisterCommandHandler("bandageself", BandageSelf, "bandageself");

            Interpreter.RegisterCommandHandler("clearuseonce", ClearUseOnce, null);//clearuseonce
            Interpreter.RegisterCommandHandler("clearusequeue", ClearUseQueue, null);//clearuseonce
            Interpreter.RegisterCommandHandler("moveitem", MoveItem, "moveitem (serial) (destination) [(x y z)] [amount]");
            Interpreter.RegisterCommandHandler("moveitemoffset", MoveItemOffset, "moveitemoffset (serial) (destination) [(x y z)] [amount]");
            Interpreter.RegisterCommandHandler("movetype", MoveType, "movetype (graphic) (source) (destination) [(x y z)] [color] [amount] [range]");
            Interpreter.RegisterCommandHandler("movetypeoffset", MoveTypeOffset, "movetypeoffset (graphic) (source) (destination) [(x y z)] [color] [amount] [range]");

            Interpreter.RegisterCommandHandler("feed", Feed, "feed (serial) ('food name'/'food group'/'any'/graphic) [color] [amount]");
            Interpreter.RegisterCommandHandler("rename", Rename, "rename (serial) ('new name')");
            Interpreter.RegisterCommandHandler("togglehands", ToggleHands, "togglehands ('left'/'right')");
            Interpreter.RegisterCommandHandler("equipitem", EquipItem, "equipitem (serial) (layer)");
            Interpreter.RegisterCommandHandler("equipwand", DummyCommand, null);
            Interpreter.RegisterCommandHandler("buy", DummyCommand, null);
            Interpreter.RegisterCommandHandler("sell", DummyCommand, null);
            Interpreter.RegisterCommandHandler("clearbuy", DummyCommand, null);
            Interpreter.RegisterCommandHandler("clearsell", DummyCommand, null);
            Interpreter.RegisterCommandHandler("organizer", DummyCommand, null);
            Interpreter.RegisterCommandHandler("autoloot", AutoLoot, "autoloot");
            Interpreter.RegisterCommandHandler("toggleautoloot", ToggleAutoLoot, "toggleautoloot");
            Interpreter.RegisterCommandHandler("togglescavenger", ToggleScavenger, null);
            Interpreter.RegisterCommandHandler("clearhands", ClearHands, "clearhands ('left'/'right'/'both')");

            Interpreter.RegisterCommandHandler("togglemounted", ToggleMounted, "togglemounted");

            // Gump
            Interpreter.RegisterCommandHandler("waitforgump", WaitForGump, "waitforgump (gump id/'any') (timeout)");
            Interpreter.RegisterCommandHandler("replygump", ReplyGump, "replygump (gump id/'any') (button) [checkboxid/'textid \"text response\"']"); // GumpResponseAction
            Interpreter.RegisterCommandHandler("closegump", CloseGump, "closegump ('paperdoll'/'status'/'profile'/'container') ('serial')"); // GumpResponseAction

            // Dress
            Interpreter.RegisterCommandHandler("dress", DressCommand, "dress ['profile name']"); //DressAction
            Interpreter.RegisterCommandHandler("undress", UnDressCommand, "undress ['profile name']"); //UndressAction
            Interpreter.RegisterCommandHandler("dressconfig", DressConfig, "dressconfig"); //DressConfig

            // Prompt
            Interpreter.RegisterCommandHandler("promptalias", PromptAlias, "promptalias ('alias name')");
            Interpreter.RegisterCommandHandler("promptmsg", PromptMsg, "promptmsg ('text')");
            Interpreter.RegisterCommandHandler("cancelprompt", CancelPrompt, "cancelprompt");
            Interpreter.RegisterCommandHandler("waitforprompt", WaitForPrompt, "waitforprompt (timeout)"); //WaitForPromptAction

            // General Waits/Pauses
            Interpreter.RegisterCommandHandler("pause", Pause, "pause (timeout)"); //PauseAction

            // Misc
            //Interpreter.RegisterCommandHandler("setability", SetAbility, "setability ('primary'/'secondary'/'stun'/'disarm') ['on'/'off']");
            Interpreter.RegisterCommandHandler("useskill", UseSkill, "useskill ('skill name'/'last')");
            Interpreter.RegisterCommandHandler("walk", Walk, "walk ('direction')");//blu
            Interpreter.RegisterCommandHandler("turn", Turn, "turn ('direction')");//blu
            Interpreter.RegisterCommandHandler("run", Run, "run ('direction')");//blu
            Interpreter.RegisterCommandHandler("setskill", DummyCommand, null);

            Interpreter.RegisterCommandHandler("info", DummyCommand, null);
            Interpreter.RegisterCommandHandler("ping", Ping, "ping");
            Interpreter.RegisterCommandHandler("playmacro", PlayMacro, "playmacro ('macro name')");
            Interpreter.RegisterCommandHandler("playsound", PlaySound, "playsound (sound id)");
            Interpreter.RegisterCommandHandler("resync", Resync, "resync");
            Interpreter.RegisterCommandHandler("snapshot", SnapShot, "snapshot");
            Interpreter.RegisterCommandHandler("hotkeys", HotKeys, "hotkeys");
            Interpreter.RegisterCommandHandler("where", DummyCommand, "where");
            Interpreter.RegisterCommandHandler("messagebox", MessageBox, "messagebox ('title') ('body')");
            Interpreter.RegisterCommandHandler("clickscreen", ClickScreen, "clickscreen (x) (y) ['single'/'double'] ['left'/'right']");
            Interpreter.RegisterCommandHandler("paperdoll", Paperdoll, "paperdoll [serial]");
            Interpreter.RegisterCommandHandler("cast", Cast, "cast ('spell name') [serial]");
            Interpreter.RegisterCommandHandler("helpbutton", HelpButton, "helpbutton");
            Interpreter.RegisterCommandHandler("guildbutton", GuildButton, "guildbutton");
            Interpreter.RegisterCommandHandler("questsbutton", QuestsButton, "questsbutton");
            Interpreter.RegisterCommandHandler("logoutbutton", LogoutButton, "logoutbutton");
            Interpreter.RegisterCommandHandler("virtue", Virtue, "virtue ('honor'/'sacrifice'/'valor')");

            Interpreter.RegisterCommandHandler("addfriend", AddFriend, "addfriend");
            Interpreter.RegisterCommandHandler("removefriend", RemoveFriend, "removefriend");
            //Interpreter.RegisterCommandHandler("ignoreobject", DummyCommand);
            //Interpreter.RegisterCommandHandler("clearignorelist", DummyCommand);
            Interpreter.RegisterCommandHandler("waitforproperties", DummyCommand, null);
            Interpreter.RegisterCommandHandler("autocolorpick", AutoColorPick, "autocolorpick (color)");
            Interpreter.RegisterCommandHandler("waitforcontents", WaitForContents, "waitforcontents (serial) (timeout)");
            Interpreter.RegisterCommandHandler("miniheal", DummyCommand, null);
            Interpreter.RegisterCommandHandler("bigheal", DummyCommand, null);
            Interpreter.RegisterCommandHandler("chivalryheal", DummyCommand, null);
        }

        private static bool _hasAction = false;
        private static uint _hasObject = 0;

        private static void GetFilterTargetTypes(Argument[] args, out Targeting.TargetType targetType, out Targeting.FilterType filterType)
        {
            targetType = Targeting.TargetType.None;
            filterType = Targeting.FilterType.Invalid;
            for(int i = 0; i < args.Length; i++)
            {
                string val = args[i].AsString().ToLower(Interpreter.Culture);
                switch(val)
                {
                    case "innocent":
                        targetType |= Targeting.TargetType.Innocent;
                        break;
                    case "criminal":
                        targetType |= Targeting.TargetType.Criminal;
                        break;
                    case "enemy":
                        targetType |= Targeting.TargetType.Enemy;
                        break;
                    case "murderer":
                        targetType |= Targeting.TargetType.Murderer;
                        break;
                    case "friend":
                        targetType |= Targeting.TargetType.Friend;
                        break;
                    case "gray":
                        targetType |= Targeting.TargetType.Gray;
                        break;
                    case "invulnerable":
                        targetType |= Targeting.TargetType.Invulnerable;
                        break;
                    case "any":
                        targetType |= Targeting.TargetType.Any;
                        break;
                    case "humanoid":
                        filterType |= Targeting.FilterType.Humanoid;
                        break;
                    case "transformation":
                        filterType |= Targeting.FilterType.Transformation;
                        break;
                    case "nearest":
                        filterType |= Targeting.FilterType.Nearest;
                        break;
                    case "closest":
                        filterType |= Targeting.FilterType.Closest;
                        break;
                }
            }
        }

        private static bool GetEnemy(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            GetFilterTargetTypes(args, out Targeting.TargetType target, out Targeting.FilterType filter);
            Targeting.GetTarget(target, filter, true, quiet);
            return true;
        }

        private static bool GetFriend(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            GetFilterTargetTypes(args, out Targeting.TargetType target, out Targeting.FilterType filter);
            Targeting.GetTarget(target, filter, false, quiet);
            return true;
        }

        private static bool PlayMacro(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            ScriptManager.PlayScript(args[0].AsString(), true);
            return true;
        }

        private static bool PlaySound(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            int soundid = Utility.ToInt32(args[0].AsString(), -1);
            if(soundid >= 0)
                Client.Game.Scene.Audio.PlaySound(soundid);
            else
                ScriptManager.Error(quiet, "Invalid sound id, only numbers are supported at the moment, and the number must be greater or equal than zero");
            return true;
        }

        public static bool SnapShot(string command, Argument[] args, bool quiet, bool force)
        {
            UOSObjects.SnapShot(quiet);
            return true;
        }

        public static bool HotKeys(string command, Argument[] args, bool quiet, bool force)
        {
            UOSObjects.Gump.ToggleHotKeys = !UOSObjects.Gump.ToggleHotKeys;
            if (!quiet)
            {
                if (UOSObjects.Gump.ToggleHotKeys)
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "Hotkeys Disabled!");
                else
                    UOSObjects.Player.SendMessage(MsgLevel.Friend, "Hotkeys Enabled!");
            }
            return true;
        }

        private static bool DummyCommand(string command, Argument[] args, bool quiet, bool force)
        {
            Console.WriteLine("Executing command {0} {1}", command, args);

            UOSObjects.Player?.SendMessage(MsgLevel.Info, $"Unimplemented command {command}");

            return true;
        }

        public static bool AutoLoot(string command, Argument[] args, bool quiet, bool force)
        {
            Targeting.OneTimeTarget(false, Assistant.HotKeys.AutoLootOnTarget);
            return true;
        }

        public static bool ToggleAutoLoot(string command, Argument[] args, bool quiet, bool force)
        {
            UOSObjects.Gump.AutoLoot = !UOSObjects.Gump.AutoLoot;
            UOSObjects.Player?.SendMessage(UOSObjects.Gump.AutoLoot ? MsgLevel.Friend : MsgLevel.Info, $"AutoLoot {(UOSObjects.Gump.AutoLoot ? "Enabled" : "Disabled" )}");
            return true;
        }

        private static bool Fly(string command, Argument[] args, bool quiet, bool force)
        {
            return true;
        }

        private static bool Land(string command, Argument[] args, bool quiet, bool force)
        {
            return true;
        }

        private static bool CancelPrompt(string command, Argument[] args, bool quiet, bool force)
        {
            if(UOSObjects.Player.HasPrompt)
                UOSObjects.Player.CancelPrompt();

            return true;
        }

        private static bool WaitForPrompt(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.Timeout(args[0].AsUInt(), () =>
            {
                return true;
            });

            return UOSObjects.Player.HasPrompt;
        }

        private static DressList _Temporary;
        public static bool DressCommand(string command, Argument[] args, bool quiet, bool force)
        {
            //we're using a named dresslist or a temporary dresslist?
            if (args.Length == 0)
            {
                if (_Temporary != null)
                    _Temporary.Dress();
                else
                    ScriptManager.Error(quiet, $"No dresslist specified and no temporary 'dressconfig' present - Usage: {Interpreter.GetCmdHelper(command)}");
            }
            else
            {
                var d = DressList.Find(args[0].AsString());
                if (d != null)
                    d.Dress();
                else
                    ScriptManager.Error(quiet, command, $"{args[0].AsString()} not found");
            }

            return true;
        }

        public static bool UnDressCommand(string command, Argument[] args, bool quiet, bool force)
        {
            //we're using a named dresslist or a temporary dresslist?
            if (args.Length == 0)
            {
                if (_Temporary != null)
                    _Temporary.Undress();
                else
                    ScriptManager.Error(quiet, $"No dresslist specified and no temporary 'dressconfig' present - Usage: {Interpreter.GetCmdHelper(command)}");
            }
            else
            {
                var d = DressList.Find(args[0].AsString());
                if (d != null)
                    d.Undress();
                else
                    ScriptManager.Error(quiet, command, $"{args[0].AsString()} not found");
            }

            return true;
        }

        public static bool DressConfig(string command, Argument[] args, bool quiet, bool force)
        {
            if (_Temporary == null)
                _Temporary = new DressList("dressconfig");

            _Temporary.LayerItems.Clear();
            for (int i = 0; i < UOSObjects.Player.Contains.Count; i++)
            {
                UOItem item = UOSObjects.Player.Contains[i];
                if (item.Layer <= Layer.LastUserValid && item.Layer != Layer.Backpack && item.Layer != Layer.Hair &&
                    item.Layer != Layer.FacialHair)
                    _Temporary.LayerItems[item.Layer] = new DressItem(item.Serial, item.Graphic);
            }

            return true;
        }

        private static string[] abilities = new string[4] { "primary", "secondary", "stun", "disarm" };
        private static bool SetAbility(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1 || !abilities.Contains(args[0].AsString()))
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if ((args.Length == 2 && args[1].AsString() == "on") || args.Length == 1)
            {
                switch (args[0].AsString())
                {
                    case "primary":
                        SpecialMoves.SetPrimaryAbility();
                        break;
                    case "secondary":
                        SpecialMoves.SetSecondaryAbility();
                        break;
                    case "stun":
                        Engine.Instance.SendToServer(new StunRequest());
                        break;
                    case "disarm":
                        Engine.Instance.SendToServer(new DisarmRequest());
                        break;
                    default:
                        break;
                }
            }
            else if (args.Length == 2 && args[1].AsString() == "off")
            {
                Engine.Instance.SendToServer(new UseAbility(Ability.None));
                Engine.Instance.SendToClient(ClearAbility.Instance);
            }

            return true;
        }

        private static string[] hands = new string[3] { "left", "right", "both" };
        private static bool ClearHands(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0 || !hands.Contains(args[0].AsString()))
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            switch (args[0].AsString())
            {
                case "left":
                    Dress.Unequip(Layer.LeftHand);
                    break;
                case "right":
                    Dress.Unequip(Layer.RightHand);
                    break;
                default:
                    Dress.Unequip(Layer.LeftHand);
                    Dress.Unequip(Layer.RightHand);
                    break;
            }

            return true;
        }
        private static bool ClickObject(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint serial = args[0].AsSerial();
            Engine.Instance.SendToServer(new SingleClick(serial));

            return true;
        }

        private static bool Bandage(string command, Argument[] args, bool quiet, bool force)
        {
            if (UOSObjects.Player == null)
                return true;

            UOItem pack = UOSObjects.Player.Backpack;
            if (pack != null)
            {
                UOItem obj = pack.FindItemByID(3617);
                if (obj == null)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "No bandages found");
                }
                else
                {
                    Engine.Instance.SendToServer(new DoubleClick(obj.Serial));
                    if(args.Length > 0)
                    {
                        uint serial = args[0].AsSerial();
                        if(SerialHelper.IsMobile(serial))
                        {
                            Targeting.SetAutoTargetAction((int)serial);
                        }
                    }
                }
            }
            else
                ScriptManager.Error(quiet, command, "No backpack could be found");
            return true;
        }

        private static bool BandageSelf(string command, Argument[] args, bool quiet, bool force)
        {
            if (UOSObjects.Player == null)
                return true;

            UOItem pack = UOSObjects.Player.Backpack;
            if (pack != null)
            {
                UOItem obj = pack.FindItemByID(3617);
                if (obj == null)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "No bandages found");
                }
                else
                {
                    Engine.Instance.SendToServer(new DoubleClick(obj.Serial));
                    if (force)
                    {
                        Targeting.ClearQueue();
                        Targeting.DoTargetSelf(true);
                    }
                    else
                        Targeting.TargetSelf(true);
                }
            }

            return true;
        }

        private static bool UseType(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            string graphicString = args[0].AsString();
            uint graphicId = args[0].AsUInt();

            uint? color = null;
            if (args.Length >= 2 && args[1].AsString().ToLower() != "any")
            {
                color = args[1].AsUInt();
                if (color == uint.MaxValue)
                    color = null;
            }

            string sourceStr = null;
            uint source = 0;

            if (args.Length >= 3)
            {
                sourceStr = args[2].AsString().ToLower();
                if (sourceStr != "world" && sourceStr != "any" && sourceStr != "ground")
                {
                    source = args[2].AsSerial();
                }
            }

            uint? range = null;
            if (args.Length >= 4 && args[3].AsString().ToLower() != "any")
            {
                range = args[3].AsUInt();
            }

            List<uint> list = new List<uint>();

            if (args.Length < 3 || source == 0)
            {
                // No source provided or invalid. Treat as world.
                foreach (UOMobile find in UOSObjects.MobilesInRange())
                {
                    if (find.Body == graphicId)
                    {
                        if (color.HasValue && find.Hue != color.Value)
                        {
                            continue;
                        }

                        // This expression does not support checking if mobiles on ground or an amount of mobiles.

                        if (range.HasValue && !Utility.InRange(UOSObjects.Player.Position, find.Position, (int)range.Value))
                        {
                            continue;
                        }

                        list.Add(find.Serial);
                    }
                }

                if (list.Count == 0)
                {
                    foreach (UOItem i in UOSObjects.Items.Values)
                    {
                        if (i.ItemID == graphicId && !i.IsInBank)
                        {
                            if (color.HasValue && i.Hue != color.Value)
                            {
                                continue;
                            }

                            if (sourceStr == "ground" && !i.OnGround)
                            {
                                continue;
                            }

                            if (range.HasValue && !Utility.InRange(UOSObjects.Player.Position, i.Position, (int)range.Value))
                            {
                                continue;
                            }

                            list.Add(i.Serial);
                        }
                    }
                }
            }
            else if (source != 0)
            {
                UOItem container = UOSObjects.FindItem(source);
                if (container != null && container.IsContainer)
                {
                    // TODO need an Argument.ToUShort() in interpreter as ItemId stores ushort.
                    UOItem item = container.FindItemByID((ushort)graphicId);
                    if (item != null &&
                        (!color.HasValue || item.Hue == color.Value) &&
                        (sourceStr != "ground" || item.OnGround) &&
                        (!range.HasValue || Utility.InRange(UOSObjects.Player.Position, item.Position, (int)range.Value)))
                    {
                        list.Add(item.Serial);
                    }
                }
                else if (container == null)
                {
                    ScriptManager.Error(quiet, $"Script Error: Couldn't find source '{sourceStr}'");
                }
                else if (!container.IsContainer)
                {
                    ScriptManager.Error(quiet, $"Script Error: Source '{sourceStr}' is not a container!");
                }
            }

            if (list.Count > 0)
            {
                uint click = list[Utility.Random(list.Count)];
                if (click != 0)
                {
                    Engine.Instance.SendToServer(new DoubleClick(click));
                    Interpreter.Pause(UOSObjects.Gump.ActionDelay);
                    return true;
                }
            }

            if (!quiet)
            {
                ScriptManager.Error(quiet, $"Script Error: Couldn't find '{graphicString}'");
            }
            return true;
        }

        private static bool UseObject(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, command, "invalid serial");
                return true;
            }

            Engine.Instance.SendToServer(new DoubleClick(serial));
            Interpreter.Pause(UOSObjects.Gump.ActionDelay);
            return true;
        }

        private static bool UseOnce(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
            }

            ushort graphicId = args[0].AsUShort();
            int color = -1;
            if (args.Length >= 2)
            {
                color = args[1].AsInt();
            }
            UOItem pack = UOSObjects.Player.Backpack;
            if (pack != null)
            {
                UOItem item = pack.FindItemByID(graphicId, true, color);
                if (item != null)
                {
                    PlayerData.DoubleClick(item.Serial);
                    Interpreter.Pause(UOSObjects.Gump.ActionDelay);
                }
                else
                    ScriptManager.Error(quiet, command, $"Couldn't find item with graphic '0x{graphicId:X}'");
            }
            else
                ScriptManager.Error(quiet, command, "No backpack could be found");
            return true;
        }

        private static bool ClearUseOnce(string command, Argument[] args, bool quiet, bool force)
        {
            return true;
        }

        private static bool ClearUseQueue(string command, Argument[] args, bool quiet, bool force)
        {
            ActionQueue.ClearActions();
            return true;
        }

        private static bool MoveItem(string command, Argument[] args, bool quiet, bool force)
        {
            string nameStr = null;
            if (args.Length < 2 || ((nameStr = args[1].AsString()) == "ground" && args.Length < 5))
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint serial = args[0].AsSerial();
            Point3D p = Point3D.MinusOne;
            uint destination = 0;
            if (nameStr == "ground" || args.Length >= 5)
            {
                int x = args[2].AsInt(), y = args[3].AsInt(), z = args[4].AsInt();
                if (x >= 0 && y >= 0)
                {
                    p = new Point3D(x, y, z);
                    if (p == Point3D.Zero)
                        p = Point3D.MinusOne;
                }
            }
            destination = args[1].AsSerial(false);
            int amount = -1;
            if (args.Length > 5)
                amount = Math.Max(1, args[5].AsInt());
            UOItem item;
            if (!SerialHelper.IsValid(serial) || (item = UOSObjects.FindItem(serial)) == null)
            {
                ScriptManager.Error(quiet, command, $"invalid item '0x{serial:X8}'");
            }
            else if(p == Point3D.MinusOne && !SerialHelper.IsValid(destination) && nameStr != "any")
            {
                ScriptManager.Error(quiet, command, $"invalid destination '0x{destination:X8}'");
            }
            else
            {
                if(nameStr == "any")
                {
                    if (item.Container is uint)
                        destination = (uint)item.Container;
                    else if (item.Container is UOItem cnt)
                        destination = cnt.Serial;
                }
                if (p == Point3D.MinusOne || destination > 0)
                    DragDropManager.DragDrop(item, destination, p, amount < 1 ? item.Amount : amount);
                else
                    DragDropManager.DragDrop(item, p, amount < 1 ? item.Amount : amount);
                Interpreter.Pause(UOSObjects.Gump.ActionDelay);
            }
            return true;
        }

        private static bool MoveItemOffset(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 5)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint serial = args[0].AsSerial();
            uint destination = args[1].AsSerial(false);
            int amount = -1;
            if (args.Length > 5)
                amount = Math.Max(1, args[5].AsInt());
            UOItem item;
            string nameStr = args[1].AsString();
            if (!SerialHelper.IsValid(serial) || (item = UOSObjects.FindItem(serial)) == null)
            {
                ScriptManager.Error(quiet, command, $"invalid item '0x{serial:X8}'");
            }
            else if (nameStr != "ground" && nameStr != "any" && !SerialHelper.IsValid(destination))
            {
                ScriptManager.Error(quiet, command, $"invalid destination '0x{destination:X8}'");
            }
            else
            {
                if(nameStr == "any")
                {
                    if (item.Container is uint)
                    {
                        destination = (uint)item.Container;
                    }
                    else if (item.Container is UOItem cnt)
                    {
                        destination = cnt.Serial;
                    }
                }
                Point3D p = nameStr == "ground" ? item.WorldPosition : (destination == 0 && nameStr == "any" ? item.WorldPosition : item.Position);
                p.X += args[2].AsInt();
                p.Y += args[3].AsInt();
                p.Z += args[4].AsInt();
                if (destination > 0)
                    DragDropManager.DragDrop(item, destination, p, amount < 1 ? item.Amount : amount);
                else
                    DragDropManager.DragDrop(item, p, amount < 1 ? item.Amount : amount);
                Interpreter.Pause(UOSObjects.Gump.ActionDelay);
            }
            return true;
        }

        private static bool MoveType(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 3)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            int graphicId = args[0].AsInt();
            if(graphicId <= 0 || graphicId >= ushort.MaxValue)
            {
                ScriptManager.Error(quiet, command, $"invalid graphic '0x{graphicId:X}'");
                return true;
            }
            ContainerType sourceType;
            switch(args[1].AsString())
            {
                case "world":
                case "ground":
                    sourceType = ContainerType.Ground;
                    break;
                case "any":
                    sourceType = ContainerType.Any;
                    break;
                default:
                    sourceType = ContainerType.None;
                    break;
            }
            uint source = 0;
            if(sourceType == ContainerType.None)
            {
                source = args[1].AsSerial();
                sourceType = ContainerType.Serial;
            }
            ContainerType destType;
            switch (args[2].AsString())
            {
                case "world":
                case "ground":
                    destType = ContainerType.Ground;
                    break;
                default:
                    destType = ContainerType.None;
                    break;
            }
            uint destination = 0;
            if (destType == ContainerType.None)
            {
                destination = args[2].AsSerial();
                destType = ContainerType.Serial;
            }
            int color = -1;
            int amount = -1;
            int range = -1;
            Point3D p = Point3D.MinusOne;
            if (args.Length > 8)
                range = Math.Max(0, args[8].AsInt());
            if (args.Length > 7)
                amount = Math.Max(1, args[7].AsInt());
            if (args.Length > 6)
                color = args[6].AsInt();
            if (args.Length > 5)
            {
                int x = args[3].AsInt(), y = args[4].AsInt(), z = args[5].AsInt();
                if (x >= 0 && y >= 0)
                {
                    p = new Point3D(x, y, z);
                    if (p == Point3D.Zero)
                        p = Point3D.MinusOne;
                }
            }
            UOEntity sent, dend;
            UOItem item = UOSObjects.FindItemByType(graphicId, color, range, sourceType);
            if(item == null)
            {
                ScriptManager.Error(quiet, command, $"item not found");
            }
            else if ((sourceType == ContainerType.Ground && !item.OnGround) || (sourceType == ContainerType.Serial && (!SerialHelper.IsValid(source) || (sent = UOSObjects.FindEntity(source)) == null)) || ((destType & ContainerType.Ground) != ContainerType.Ground && (!SerialHelper.IsValid(destination) || (dend = UOSObjects.FindEntity(destination)) == null)))
            {
                ScriptManager.Error(quiet, command, $"invalid source '0x{source:X}' or destination '0x{destination:X}' for item '{item}'");
            }
            else
            {
                if(!item.OnGround && ((destType & ContainerType.Ground) == ContainerType.Ground || destination == 0))
                    DragDropManager.DragDrop(item, p, amount < 1 ? item.Amount : amount);
                else
                    DragDropManager.DragDrop(item, destination, p, amount < 1 ? item.Amount : amount);
                Interpreter.Pause(UOSObjects.Gump.ActionDelay);
            }
            return true;
        }

        private static bool MoveTypeOffset(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 6)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            int graphicId = args[0].AsInt();
            if (graphicId <= 0 || graphicId >= ushort.MaxValue)
            {
                ScriptManager.Error(quiet, command, $"invalid graphic '0x{graphicId:X}'");
                return true;
            }
            ContainerType sourceType;
            switch (args[1].AsString())
            {
                case "world":
                case "ground":
                    sourceType = ContainerType.Ground;
                    break;
                case "any":
                    sourceType = ContainerType.Any;
                    break;
                default:
                    sourceType = ContainerType.None;
                    break;
            }
            uint source = 0;
            if (sourceType == ContainerType.None)
            {
                source = args[1].AsSerial();
                sourceType = ContainerType.Serial;
            }
            ContainerType destType;
            switch (args[2].AsString())
            {
                case "world":
                case "ground":
                    destType = ContainerType.Ground;
                    break;
                default:
                    destType = ContainerType.None;
                    break;
            }
            uint destination = 0;
            if (destType == ContainerType.None)
            {
                destination = args[2].AsSerial();
                destType = ContainerType.Serial;
            }
            int color = -1;
            int amount = -1;
            int range = -1;
            if (args.Length > 8)
                range = Math.Max(0, args[8].AsInt());
            if (args.Length > 7)
                amount = Math.Max(1, args[7].AsInt());
            if (args.Length > 6)
                color = args[6].AsInt();
            UOEntity sent, dend;
            UOItem item = UOSObjects.FindItemByType(graphicId, color, range, sourceType);
            if (item == null)
            {
                ScriptManager.Error(quiet, command, $"item not found");
            }
            else if ((sourceType == ContainerType.Ground && !item.OnGround) || (sourceType == ContainerType.Serial && (!SerialHelper.IsValid(source) || (sent = UOSObjects.FindEntity(source)) == null)) || ((destType & ContainerType.Ground) != ContainerType.Ground && (!SerialHelper.IsValid(destination) || (dend = UOSObjects.FindEntity(destination)) == null)))
            {
                ScriptManager.Error(quiet, command, $"invalid source '0x{source:X}' or destination '0x{destination:X}' for item '{item}'");
            }
            else
            {
                Point3D p;
                if (!item.OnGround && ((destType & ContainerType.Ground) == ContainerType.Ground || destination == 0))
                {
                    p = new Point3D(item.WorldPosition.X + args[3].AsInt(), item.WorldPosition.Y + args[4].AsInt(), item.WorldPosition.Z + args[5].AsInt());
                    DragDropManager.DragDrop(item, p, amount < 1 ? item.Amount : amount);
                }
                else
                {
                    p = new Point3D(item.Position.X + args[3].AsInt(), item.Position.Y + args[4].AsInt(), item.Position.Z + args[5].AsInt());
                    DragDropManager.DragDrop(item, destination, p, amount < 1 ? item.Amount : amount);
                }
                Interpreter.Pause(UOSObjects.Gump.ActionDelay);
            }
            return true;
        }

        private static Dictionary<string, Direction> _Directions = new Dictionary<string, Direction>()
        {
            { "north", Direction.North },
            { "northeast", Direction.Right },
            { "right", Direction.Right },
            { "east", Direction.East },
            { "southeast", Direction.Down },
            { "down", Direction.Down },
            { "south", Direction.South },
            { "southwest", Direction.Left },
            { "left", Direction.Left },
            { "west", Direction.West },
            { "northwest", Direction.Up },
            { "up", Direction.Up }
        };

        private static Queue<Direction> _MoveDirection = new Queue<Direction>();
        private static bool Walk(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            else if(args[0] != null)
            {
                _MoveDirection.Clear();
                for(int i = 0; i < args.Length; i++)
                {
                    if(_Directions.TryGetValue(args[i].AsString().ToLower(), out Direction d))
                    {
                        _MoveDirection.Enqueue(d);
                    }
                }
                args[0] = null;
            }
            if (ScriptManager.LastWalk < DateTime.UtcNow)
            {
                return false;
            }

            ScriptManager.LastWalk = DateTime.UtcNow + TimeSpan.FromMilliseconds(MovementSpeed.TimeToCompleteMovement(false,
                                                                             World.Player.IsMounted ||
                                                                             World.Player.SpeedMode == CharacterSpeedType.FastUnmount ||
                                                                             World.Player.SpeedMode == CharacterSpeedType.FastUnmountAndCantRun ||
                                                                             World.Player.IsFlying
                                                                             ));

            Direction dir = _MoveDirection.Dequeue();
            if ((UOSObjects.Player.Direction & Direction.Up) != dir)
                Engine.Instance.RequestMove(dir, false);
            Engine.Instance.RequestMove(dir, false);
            return _MoveDirection.Count < 1;
        }

        private static bool Turn(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (_Directions.TryGetValue(args[0].AsString().ToLower(), out Direction dir))
            {
                if((UOSObjects.Player.Direction & Direction.Up) != dir)
                    Engine.Instance.RequestMove(dir, false);
            }

            return true;
        }

        private static bool Run(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            else if (args[0] != null)
            {
                _MoveDirection.Clear();
                for (int i = 0; i < args.Length; i++)
                {
                    if (_Directions.TryGetValue(args[i].AsString().ToLower(), out Direction d))
                    {
                        _MoveDirection.Enqueue(d);
                    }
                }
                args[0] = null;
            }
            if (ScriptManager.LastWalk < DateTime.UtcNow)
            {
                return false;
            }

            ScriptManager.LastWalk = DateTime.UtcNow + TimeSpan.FromMilliseconds(MovementSpeed.TimeToCompleteMovement(true,
                                                                             World.Player.IsMounted ||
                                                                             World.Player.SpeedMode == CharacterSpeedType.FastUnmount ||
                                                                             World.Player.SpeedMode == CharacterSpeedType.FastUnmountAndCantRun ||
                                                                             World.Player.IsFlying
                                                                             ));

            Direction dir = _MoveDirection.Dequeue();
            if ((UOSObjects.Player.Direction & Direction.Up) != dir)
                Engine.Instance.RequestMove(dir, false);
            Engine.Instance.RequestMove(dir, true);
            return _MoveDirection.Count < 1;
        }

        private static bool UseSkill(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args[0].AsString() == "last")
            {
                Engine.Instance.SendToServer(new UseSkill(UOSObjects.Player.LastSkill));
            }
            else
            {
                Skill sk = ScriptManager.GetSkill(args[0].AsString());
                if (sk != null && sk.Index < SkillsLoader.Instance.Skills.Count)
                {
                    if(SkillsLoader.Instance.Skills[sk.Index].HasAction)
                        Engine.Instance.SendToServer(new UseSkill(sk.Index));
                    else
                        new RunTimeError(null, $"Non usable skill: {args[0].AsString()}");
                }
                else
                    new RunTimeError(null, $"Unknown skill name: {args[0].AsString()}");
            }

            return true;
        }

        private static bool Feed(string command, Argument[] args, bool quiet, bool force)
        {
            if(args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            UOItem backpack = UOSObjects.Player.GetItemOnLayer(Layer.Backpack);
            if(backpack == null)
            {
                ScriptManager.Error(quiet, "Backpack not found");
                return true;
            }
            uint targetSerial = args[0].AsSerial();
            if (SerialHelper.IsMobile(targetSerial))
            {
                ushort graph = args[1].AsUShort(false);
                int hue = -1;
                int amount = -1;
                if(args.Length > 2)
                {
                    hue = args[2].AsInt();
                    if (args.Length > 3)
                        amount = args[3].AsInt();
                }
                UOItem item = null;
                if (graph > 0)
                {
                    item = backpack.FindItemByID(graph, true, hue);
                }
                else//not a graphic id, maybe a name?
                {
                    item = backpack.FindItemByID(Foods.GetFoodGraphics(args[1].AsString()), true, hue);
                }
                if(item != null)
                {
                    DragDropManager.DragDrop(item, targetSerial, Point3D.MinusOne, amount < 1 ? item.Amount : amount);
                }
                else
                    ScriptManager.Error(quiet, $"No valid food found");
            }
            return true;
        }

        private static bool Rename(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint targetSerial = args[0].AsSerial();
            if(SerialHelper.IsValid(targetSerial))
                Engine.Instance.SendToServer(new RenameReq(targetSerial, args[1].AsString()));
            return true;
        }

        
        private static string _nextPromptAliasName = "";
        private static void OnPromptAliasTarget(bool location, uint serial, Point3D p, ushort gfxid)
        {
            if (SerialHelper.IsValid(serial))
            {
                if(!string.IsNullOrEmpty(_nextPromptAliasName))
                    Interpreter.SetAlias(_nextPromptAliasName, serial);
            }
            else
                ScriptManager.Error(false, "Invalid object targeted");
        }

        private static bool PromptAlias(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, "Usage: promptalias ('name')");
                return true;
            }

            if (!_hasAction)
            {
                _hasAction = true;
                _nextPromptAliasName = args[0].AsString();
                Targeting.OneTimeTarget(false, OnPromptAliasTarget);
                return false;
            }

            if (!Targeting.HasTarget)
            {
                _hasAction = false;
                _nextPromptAliasName = "";
                return true;
            }

            return false;
        }

        private static bool Pause(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.Pause(args[0].AsUInt());
            return true;
        }

        private static bool WaitForGump(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.Timeout(args[1].AsUInt(), () => { return true; });
            bool any = args[0].AsString() == "any";
            if (any)
            {
                if (UOSObjects.Player.OpenedGumps.Count > 0)
                {
                    return true;
                }
            }
            else
            {
                uint gumpId = args[0].AsSerial();
                if(UOSObjects.Player.OpenedGumps.TryGetValue(gumpId, out var glist) && glist.Count > 0)
                    return true;
            }
            return false;
        }

        public static bool Attack(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, "attack - invalid serial");
                return true;
            }

            if (SerialHelper.IsMobile(serial))
                Engine.Instance.SendToServer(new AttackReq(serial));

            return true;
        }

        public static bool WarMode(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length > 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
            {
                switch(args[0].AsString().ToLower(XmlFileParser.Culture))
                {
                    case "on":
                    case "true":
                        Engine.Instance.SendToServer(new SetWarMode(true));
                        break;
                    case "off":
                    case "false":
                        Engine.Instance.SendToServer(new SetWarMode(false));
                        break;
                    default:
                        ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                        break;
                }
            }
            else
                Engine.Instance.SendToServer(new SetWarMode(!UOSObjects.Player.Warmode));
            return true;
        }

        private static bool ClearJournal(string command, Argument[] args, bool quiet, bool force)
        {
            Journal.Clear();
            return true;
        }

        private static bool WaitForJournal(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            bool system = args.Length > 2 ? args[2].AsString() == "system" : true, found;
            if (system)
            {
                found = Journal.ContainsSafe(args[0].AsString());
            }
            else
                found = Journal.ContainsFrom(args[2].AsString(), args[0].AsString());

            if (!found)
            {
                Interpreter.Timeout(args[1].AsUInt(), () => { return true; });
                return false;
            }
            return true;
        }

        public static bool Msg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.Say(0x03B1, args[0].AsString());
            else
                UOSObjects.Player.Say(args[1].AsInt(), args[0].AsString());

            return true;
        }

        public static bool WhisperMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.Say(0x03B1, args[0].AsString(), MessageType.Whisper);
            else
                UOSObjects.Player.Say(args[1].AsInt(), args[0].AsString(), MessageType.Whisper);

            return true;
        }

        public static bool YellMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.Say(0x03B1, args[0].AsString(), MessageType.Yell);
            else
                UOSObjects.Player.Say(args[1].AsInt(), args[0].AsString(), MessageType.Yell);

            return true;
        }

        public static bool EmoteMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.Say(0x03B1, args[0].AsString(), MessageType.Emote);
            else
                UOSObjects.Player.Say(args[1].AsInt(), args[0].AsString(), MessageType.Emote);

            return true;
        }

        public static bool PartyMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            if (PacketHandlers.PartyLeader == 0)
            {
                ScriptManager.Error(quiet, command, "You must be in a party to use 'partymsg'");
                return true;
            }

            Engine.Instance.SendToServer(new SendPartyMessage(args[0].AsString()));
            return true;
        }

        public static bool GuildMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.Say(0x03B1, args[0].AsString(), MessageType.Guild);
            else
                UOSObjects.Player.Say(args[1].AsInt(), args[0].AsString(), MessageType.Guild);

            return true;
        }

        public static bool AllyMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.Say(0x03B1, args[0].AsString(), MessageType.Alliance);
            else
                UOSObjects.Player.Say(args[1].AsInt(), args[0].AsString(), MessageType.Alliance);

            return true;
        }

        public static bool HeadMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.OverheadMessage(0x03B1, args[0].AsString());
            else
            {
                int hue = Utility.ToInt32(args[1].AsString(), 0);

                if (args.Length >= 3)
                {
                    uint serial = args[2].AsSerial();
                    UOMobile m = UOSObjects.FindMobile((uint)serial);

                    if (m != null)
                        m.OverheadMessage(hue, args[0].AsString());
                }
                else
                    UOSObjects.Player.OverheadMessage(hue, args[0].AsString());
            }

            return true;
        }

        public static bool SysMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
                UOSObjects.Player.SendMessage(0x03B1, args[0].AsString());
            else if (args.Length == 2)
                UOSObjects.Player.SendMessage(args[1].AsInt(), args[0].AsString());

            return true;
        }

        private static bool PopList(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args[1].AsString() == "front")
            {
                if (force)
                    while (Interpreter.PopList(args[0].AsString(), true)) { }
                else
                    Interpreter.PopList(args[0].AsString(), true);
            }
            else if (args[1].AsString() == "back")
            {
                if (force)
                    while (Interpreter.PopList(args[0].AsString(), false)) { }
                else
                    Interpreter.PopList(args[0].AsString(), false);
            }
            else
            {
                if (force)
                    while (Interpreter.PopList(args[0].AsString(), args[1])) { }
                else
                    Interpreter.PopList(args[0].AsString(), args[1]);
            }

            return true;
        }

        private static bool PushList(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            bool front = false;
            if (args.Length >= 3)
            {
                if (args[2].AsString() == "front")
                    front = true;
            }

            Interpreter.PushList(args[0].AsString(), args[1], front, force);

            return true;
        }

        private static bool RemoveList(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.DestroyList(args[0].AsString());

            return true;
        }

        private static bool CreateList(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.CreateList(args[0].AsString());

            return true;
        }

        private static bool ClearList(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.ClearList(args[0].AsString());

            return true;
        }

        private static bool SetAlias(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args.Length == 1)
            {
                return PromptAlias(command, args, true, force);
            }
            else
                Interpreter.SetAlias(args[0].AsString(), args[1].AsSerial());

            return true;
        }

        private static bool UnsetAlias(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.SetAlias(args[0].AsString(), 0);

            return true;
        }

        private static bool ShowNames(string command, Argument[] args, bool quiet, bool force)
        {
            if (World.Player == null)
                return true;
            byte type = 1;//mobile as default
            int range = World.ClientViewRange;
            if (args.Length > 0)
            {
                switch (args[0].AsString())
                {
                    case "any":
                    case "all":
                        type = 3;
                        break;
                    case "mobiles":
                        type = 1;
                        break;
                    case "corpses":
                        type = 2;
                        break;
                }
            }
            if (args.Length > 1)
                range = args[1].AsInt();
            switch (type)
            {
                case 3:
                case 1:
                    foreach (UOMobile m in UOSObjects.MobilesInRange(range))
                    {
                        if (m != UOSObjects.Player)
                            Engine.Instance.SendToServer(new SingleClick(m));
                    }
                    if (type == 3)
                        goto case 2;
                    break;
                case 2:
                    foreach (UOItem i in UOSObjects.ItemsInRange(range))
                    {
                        if (i.IsCorpse)
                            Engine.Instance.SendToServer(new SingleClick(i));
                    }
                    break;
            }
            return true;
        }

        public static bool ContextMenu(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            uint s = args[0].AsSerial();
            ushort index = args[1].AsUShort();

            if (s == 0 && UOSObjects.Player != null)
                s = UOSObjects.Player.Serial;

            Engine.Instance.SendToServer(new ContextMenuRequest(s));
            Engine.Instance.SendToServer(new ContextMenuResponse(s, index));
            return true;
        }

        private static bool WaitForContext(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 3)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.Timeout(args[2].AsUInt(), () =>
            {
                return true;
            });

            if (UOSObjects.Player.HasMenu)
            {
                Engine.Instance.SendToServer(new ContextMenuResponse(args[0].AsSerial(), args[1].AsUShort()));
                return true;
            }
            return false;
        }

        private static bool Target(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (!Targeting.HasTarget)
            {
                if (args.Length > 1)
                {
                    Interpreter.Timeout(args[1].AsUInt(), () =>
                    {
                        return true;
                    });
                    return false;
                }
                else
                    ScriptManager.Error(quiet, command, "No target cursor available. Consider using waitfortarget.");
            }
            else
                Targeting.Target(args[0].AsSerial());

            return true;
        }

        private static bool TargetType(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1 || args.Length > 3)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (command == "targettype" && !Targeting.HasTarget)
            {
                ScriptManager.Error(quiet, command, "No target cursor available. Consider using waitfortarget.");
                return true;
            }

            var graphic = args[0].AsInt();

            uint serial = uint.MaxValue;

            switch (args.Length)
            {
                case 1:
                {
                    // Only graphic
                    
                    UOEntity ent = UOSObjects.FindEntityByType(graphic);
                    if (ent != null)
                        serial = ent.Serial;
                    break;
                }
                case 2:
                {
                    // graphic and color
                    var color = args[1].AsUShort();
                    UOEntity ent = UOSObjects.FindEntityByType(graphic, color);
                    if (ent != null)
                        serial = ent.Serial;
                    break;
                }
                case 3:
                {
                    // graphic, color, range
                    var color = args[1].AsUShort();
                    var range = args[2].AsInt();
                    UOEntity ent = UOSObjects.FindEntityByType(graphic, color, range);
                    if (ent != null)
                        serial = ent.Serial;
                    break;
                }
            }

            if (serial == uint.MaxValue)
            {
                new RunTimeError(null, "Unable to find suitable target");
                return true;
            }
            if(command == "targettype")
                Targeting.Target(serial);
            else
                Targeting.SetAutoTargetAction((int)serial);
            return true;
        }

        public static bool PromptMsg(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            UOSObjects.Player.ResponsePrompt(args[0].AsString());
            return true;
        }

        public static bool ToggleHands(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length == 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (args[0].AsString() == "left")
                Dress.ToggleLeft();
            else
                Dress.ToggleRight();

            return true;
        }

        public static bool EquipItem(string command, Argument[] args, bool quiet, bool force)
        {
            if (UOSObjects.Player == null)
                return true;

            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            UOItem equip = UOSObjects.FindItem(args[0].AsSerial());
            Layer layer = args[1].AsLayer();
            if (equip != null && layer != Layer.Invalid && layer <= Layer.LastUserValid)
                Dress.Equip(equip, layer);

            return true;
        }

        public static bool ToggleScavenger(string command, Argument[] args, bool quiet, bool force)
        {
            UOSObjects.Gump.EnabledScavenger.IsChecked = !UOSObjects.Gump.EnabledScavenger.IsChecked;
            return true;
        }

        private static bool Ping(string command, Argument[] args, bool quiet, bool force)
        {
            Assistant.Ping.StartPing(5);

            return true;
        }

        private static bool Resync(string command, Argument[] args, bool quiet, bool force)
        {
            Engine.Instance.SendToServer(new ResyncReq());

            return true;
        }

        internal static bool HasMessageGump = false, MessageEnded = true;
        private static bool MessageBox(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            if(!HasMessageGump)
            {
                if (MessageEnded)
                {
                    string title = args[0].AsString(), body = args[1].AsString();
                    MessageEnded = false;
                    HasMessageGump = true;
                    UIManager.Add(new AssistantGump.MessageBoxGump(title, body));
                }
                else
                    MessageEnded = true;
            }
            return MessageEnded;
        }

        private static bool ClickScreen(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            int x = Utility.ToInt32(args[0].AsString(), -1);
            int y = Utility.ToInt32(args[1].AsString(), -1);
            if(x >= 0 && y >= 0 && x <= Client.Game.Window.ClientBounds.Width && y <= Client.Game.Window.ClientBounds.Height)
            {
                Mouse.Position.X = x;
                Mouse.Position.Y = y;
                if(args.Length > 2 && args[2].AsString() == "double")
                {
                    if (args.Length > 3 && args[3].AsString() == "right")
                    {
                        UIManager.OnRightMouseDoubleClick();
                        UIManager.OnRightMouseButtonUp();
                    }
                    else
                    {
                        UIManager.OnLeftMouseDoubleClick();
                        UIManager.OnLeftMouseButtonUp();
                    }
                }
                else
                {
                    if (args.Length > 3 && args[3].AsString() == "right")
                    {
                        UIManager.OnRightMouseButtonDown();
                        UIManager.OnRightMouseButtonUp();
                    }
                    else
                    {
                        UIManager.OnLeftMouseButtonDown();
                        UIManager.OnLeftMouseButtonUp();
                    }
                }
            }
            else
                ScriptManager.Error(quiet, command, "x or y coordinates are out of bounds or negative value!");
            return true;
        }

        private static bool Paperdoll(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length > 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
            }
            else
            {
                uint serial = args.Length == 0 ? UOSObjects.Player.Serial : args[0].AsSerial();
                Engine.Instance.SendToServer(new DoubleClick(serial));
            }

            return true;
        }

        public static bool Cast(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Spell spell = int.TryParse(args[0].AsString(), out int spellnum)
                ? Spell.Get(spellnum)
                : Spell.GetByName(args[0].AsString());

            if (spell != null)
            {
                if (args.Length > 1)
                {
                    uint s = args[1].AsSerial();
                    if (force)
                        Targeting.ClearQueue();
                    if (SerialHelper.IsValid(s))
                    {
                        Targeting.Target(s);
                    }
                    else if (!quiet)
                        ScriptManager.Error(quiet, command, "invalid serial or alias");
                }
                spell.OnCast(new CastSpellFromMacro((ushort)spell.GetID()));
            }
            else if (!quiet)
            {
                ScriptManager.Error(quiet, command, "spell name or number not valid");
            }

            return true;
        }

        private static bool HelpButton(string command, Argument[] args, bool quiet, bool force)
        {
            GameActions.RequestHelp();
            return true;
        }

        private static bool GuildButton(string command, Argument[] args, bool quiet, bool force)
        {
            GameActions.OpenGuildGump();
            return true;
        }

        private static bool QuestsButton(string command, Argument[] args, bool quiet, bool force)
        {
            GameActions.RequestQuestMenu();
            return true;
        }

        private static bool LogoutButton(string command, Argument[] args, bool quiet, bool force)
        {
            Client.Game.GetScene<ClassicUO.Game.Scenes.GameScene>()?.RequestQuitGame();
            return true;
        }

        private static bool Virtue(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            byte id = 0;
            switch(args[0].AsString().ToLowerInvariant())
            {
                case "honor":
                    id = 1; break;
                case "sacrifice":
                    id = 2; break;
                case "valor":
                    id = 3; break;
                default:
                    ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}"); break;
            }
            if (id > 0)
                Engine.Instance.SendToServer(new PInvokeVirtueRequest(id));
            return true;
        }

        private static bool AddFriend(string command, Argument[] args, bool quiet, bool force)
        {
            if (!_hasAction)
            {
                _hasAction = true;
                Targeting.OneTimeTarget(false, FriendSelected, FriendTargetCancel);
            }
            return !_hasAction;
        }

        private static bool RemoveFriend(string command, Argument[] args, bool quiet, bool force)
        {
            if (!_hasAction)
            {
                _hasAction = true;
                Targeting.OneTimeTarget(false, RemoveFriendSelected, FriendTargetCancel);
            }
            return !_hasAction;
        }

        private static void FriendSelected(bool loc, uint serial, Point3D p, ushort itemid)
        {
            _hasAction = false;
            Targeting.OnFriendTargetSelected(loc, serial, p, itemid);
        }

        private static void RemoveFriendSelected(bool loc, uint serial, Point3D p, ushort itemid)
        {
            _hasAction = false;
            Targeting.OnRemoveFriendSelected(loc, serial, p, itemid);
        }

        private static void FriendTargetCancel()
        {
            _hasAction = false;
        }

        private static int _colorPick = 0;
        private static bool AutoColorPick(string command, Argument[] args, bool quiet, bool force)
        {
            if(args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            _colorPick = args[0].AsInt();
            return true;
        }

        private static bool WaitForContents(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            if(!_hasAction)
            {
                if (_hasObject == 0)
                {
                    uint ser = args[0].AsSerial();
                    if (SerialHelper.IsItem(ser))
                    {
                        _hasAction = true;
                        PacketHandler.RegisterServerToClientViewer(0x3C, OnContentPacket);
                    }
                    else
                        return true;
                }
                else
                {
                    _hasAction = false;
                    _hasObject = 0;
                    PacketHandler.RemoveServerToClientViewer(0x3C, OnContentPacket);
                    return true;
                }
            }
            Interpreter.Timeout(args[1].AsUInt(), () =>
            {
                _hasAction = false;
                _hasObject = 0;
                PacketHandler.RemoveServerToClientViewer(0x3C, OnContentPacket);
                return true;
            });
            return false;
        }

        private static void OnContentPacket(ClassicUO.Network.Packet p, PacketHandlerEventArgs args)
        {
            if (!_hasAction)
                return;
            int count = p.ReadUShort();
            for (int i = 0; i < count; i++)
            {
                if (!DragDropManager.EndHolding(p.ReadUInt()))
                    continue;
                p.Skip(9 + (Engine.UsePostKRPackets ? 1 : 0));
                if(_hasObject == p.ReadUInt())
                {
                    _hasAction = false;
                }
                p.ReadUShort();
            }
        }

        private static bool SetTimer(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }


            Interpreter.SetTimer(args[0].AsString(), args[1].AsInt());
            return true;
        }

        private static bool RemoveTimer(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.RemoveTimer(args[0].AsString());
            return true;
        }

        private static bool CreateTimer(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.CreateTimer(args[0].AsString());
            return true;
        }

        private static bool TargetTileRelative(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (!Targeting.HasTarget && command == "targettilerelative")
            {
                ScriptManager.Error(quiet, command, "No target cursor available. Consider using waitfortarget.");
                return true;
            }

            var serial = args[0].AsSerial();
            var range = args[1].AsInt();

            var mobile = UOSObjects.FindMobile(serial);

            if (mobile == null)
            {
                /* TODO: Search items if mobile not found. Although this isn't very useful. */
                ScriptManager.Error(quiet, command, "item or mobile not found.");
                return true;
            }

            var position = new Point3D(mobile.Position);

            switch (mobile.Direction)
            {
                case Direction.North:
                    position.Y -= range;
                    break;
                case Direction.Right:
                    position.X += range;
                    position.Y -= range;
                    break;
                case Direction.East:
                    position.X += range;
                    break;
                case Direction.Down:
                    position.X += range;
                    position.Y += range;
                    break;
                case Direction.South:
                    position.Y += range;
                    break;
                case Direction.Left:
                    position.X -= range;
                    position.Y += range;
                    break;
                case Direction.West:
                    position.X -= range;
                    break;
                case Direction.Up:
                    position.X -= range;
                    position.Y -= range;
                    break;
            }
            if (command == "targettilerelative")
                Targeting.Target(position);
            else
                Targeting.SetAutoTargetAction(position.X, position.Y, position.Z);
            return true;
        }

        private static bool WaitForTarget(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            Interpreter.Timeout(args[0].AsUInt(), () =>
            {
                return true;
            });

            return Targeting.HasTarget;
        }

        private static bool TargetGround(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1 || args.Length > 3)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (!Targeting.HasTarget && command == "targetground")
            {
                ScriptManager.Error(quiet, command, "No target cursor available. Consider using waitfortarget.");
                return true;
            }

            ushort graphic = args[0].AsUShort();
            if (graphic == 0)
            {
                ScriptManager.Error(quiet, command, "invalid graphic in targetground");
                return true;
            }
            int color = args.Length >= 2 ? args[1].AsUShort() : -1;
            int range = args.Length >= 3 ? args[2].AsInt() : World.ClientViewRange;
            Point3D p = Point3D.MinusOne;
            foreach (UOEntity ie in UOSObjects.EntitiesInRange(range))
            {
                if (ie.Graphic == graphic && (color == -1 || ie.Hue == color))
                {
                    p = ie.Position;
                    break;
                }
            }
            if (p != Point3D.MinusOne)
            {
                if (command == "targetground")
                    Targeting.Target(p);
                else
                    Targeting.SetAutoTargetAction(p.X, p.Y, p.Z);
            }
            else
                ScriptManager.Error(quiet, command, "No valid target found");
            return true;
        }

        private static bool TargetTile(string command, Argument[] args, bool quiet, bool force)
        {
            if (!(args.Length == 1 || args.Length == 3))
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (!Targeting.HasTarget && command == "targettile")
            {
                ScriptManager.Error(quiet, command, "No target cursor available. Consider using waitfortarget.");
                return true;
            }

            Point3D position = Point3D.MinusOne;

            switch (args.Length)
            {
                case 1:
                {
                    var alias = args[0].AsString();
                    if (alias == "last")
                    {
                        if (Targeting.LastTargetInfo.Type != 1)
                        {
                            new RunTimeError(null, "Last target was not a ground target");
                            return true;
                        }

                        position = new Point3D(Targeting.LastTargetInfo.X, Targeting.LastTargetInfo.Y, Targeting.LastTargetInfo.Z);
                    }
                    else if (alias == "current")
                    {
                        position = UOSObjects.Player.Position;
                    }
                    break;
                }
                case 3:
                    position = new Point3D(args[0].AsInt(), args[1].AsInt(), args[2].AsInt());
                    break;
            }

            if (position == Point3D.MinusOne)
            {
                ScriptManager.Error(quiet, command, "No valid target found");
                return true;
            }
            if(command == "targettile")
                Targeting.Target(position);
            else
                Targeting.SetAutoTargetAction(position.X, position.Y, position.Z);
            return true;
        }

        private static bool TargetTileOffset(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 3)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (!Targeting.HasTarget && command == "targettileoffset")
            {
                ScriptManager.Error(quiet, command, "No target cursor available. Consider using waitfortarget.");
                return true;
            }

            var position = new Point3D(UOSObjects.Player.Position);

            position.X += args[0].AsInt();
            position.Y += args[1].AsInt();
            position.Z += args[2].AsInt();
            if (command == "targettileoffset")
                Targeting.Target(position);
            else
                Targeting.SetAutoTargetAction(position.X, position.Y, position.Z);
            return true;
        }

        private static bool CancelTarget(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }

            if (Targeting.HasTarget)
                Targeting.CancelTarget();

            return true;
        }

        private static bool ClearTargetQueue(string command, Argument[] args, bool quiet, bool force)
        {
            ScriptManager.Error(quiet, command, "There is no target queue for normal targets, to queue a target, consider using autotarget* commands");
            return true;
        }

        private static bool AutoTargetLast(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            Targeting.SetAutoTargetAction();
            return true;
        }

        private static bool AutoTargetSelf(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 0)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            Targeting.SetAutoTargetAction((int)UOSObjects.Player.Serial);
            return true;
        }

        private static bool AutoTargetObject(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            uint ser = args[0].AsSerial();
            if (!SerialHelper.IsValid(ser))
                ScriptManager.Error(quiet, command, "invalid target serial");
            else
                Targeting.SetAutoTargetAction((int)ser);
            return true;
        }

        private static bool AutoTargetGhost(string command, Argument[] args, bool quiet, bool force)
        {
            if(args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            int range = args[0].AsInt();
            int zrange = -1;
            if (args.Length > 1)
                zrange = args[1].AsInt();

            List<UOMobile> l = UOSObjects.MobilesInRange(range, false);
            if (l.Count > 0)
            {
                l.Sort(UOSObjects.PlayerDistanceComparer.Instance);
                Targeting.SetAutoTargetAction((int)l[0].Serial);
            }
            else
                ScriptManager.Error(quiet, command, "No valid target found");
            return true;
        }

        private static bool CancelAutoTarget(string command, Argument[] args, bool quiet, bool force)
        {
            Targeting.CancelAutoTargetAction();
            return true;
        }

        public static bool ToggleMounted(string command, Argument[] args, bool quiet, bool force)
        {
            if (UOSObjects.Gump.MountSerial == uint.MaxValue)
            {
                UOSObjects.Gump.MountSerial = 0;
                Targeting.OneTimeTarget(false, TargetMountResponse, CancelMountResponse);
                return false;
            }
            if (UOSObjects.Gump.MountSerial == 0)
                return false;
            FinalizeMounting(quiet, command);
            return true;
        }

        internal static void TargetMountResponse(bool location, uint serial, Point3D p, ushort gfxid)
        {
            if (serial != 0)
                UOSObjects.Gump.MountSerial = serial;
            else
                UOSObjects.Gump.MountSerial = uint.MaxValue;
        }

        private static void CancelMountResponse()
        {
            UOSObjects.Gump.MountSerial = uint.MaxValue;
        }

        private static void FinalizeMounting(bool quiet, string command)
        {
            if (SerialHelper.IsValid(UOSObjects.Gump.MountSerial))
            {
                uint? ser = UOSObjects.FindMobile(UOSObjects.Gump.MountSerial)?.Serial;
                if (!ser.HasValue)
                    ser = UOSObjects.Player.GetItemOnLayer(Layer.Mount)?.Serial;
                if (ser.HasValue)
                {
                    Engine.Instance.SendToServer(new DoubleClick(ser.Value));
                    return;
                }
                ScriptManager.Error(quiet, command, "Mount not found");
            }
            else
            {
                ScriptManager.Error(quiet, command, "Invalid mount type selected");
            }
            UOSObjects.Gump.MountSerial = uint.MaxValue;
        }

        public static bool ReplyGump(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            bool any = false;
            if (args[0].AsString() == "any")
                any = true;
            uint gumpid = 0;
            if(!any)
                gumpid = args[0].AsUInt();
            List<PlayerData.GumpData> gumps;
            if (any)
            {
                gumps = new List<PlayerData.GumpData>();
                foreach(var list in UOSObjects.Player.OpenedGumps.Values)
                {
                    gumps.AddRange(list);
                }
            }
            else if(!UOSObjects.Player.OpenedGumps.TryGetValue(gumpid, out gumps) || gumps.Count == 0)
            {
                ScriptManager.Error(quiet, command, $"gump id 0x{gumpid:X} not found");
            }
            if (gumps != null && gumps.Count > 0)
            {
                var gump = gumps[gumps.Count - 1];
                int buttonId = args[1].AsInt();
                List<int> checkboxes = new List<int>();
                List<GumpTextEntry> textentries = new List<GumpTextEntry>();
                if (args.Length > 2)
                {
                    for (int i = 2; i < args.Length; i++)
                    {
                        string[] split = args[i].AsString().Split(' ');
                        if (split.Length > 1)
                        {
                            textentries.Add(new GumpTextEntry(Utility.ToUInt16(split[0], 0), args[i].AsString().Remove(0, split[0].Length)));
                        }
                        else
                            checkboxes.Add(args[i].AsInt(false));
                    }
                }
                Engine.Instance.SendToServer(new GumpResponse(gump.ServerID, gump.GumpID, buttonId, checkboxes, textentries));
                Engine.Instance.SendToClient(new CloseGump(gump.GumpID));
            }
            return true;
        }

        public static bool CloseGump(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(command)}");
                return true;
            }
            uint serial = args[1].AsSerial();
            UOEntity entity = null;
            if(SerialHelper.IsMobile(serial))
                entity = UOSObjects.FindMobile(serial);
            else if(SerialHelper.IsItem(serial))
                entity = UOSObjects.FindItem(serial);
            if (entity != null)
            {
                switch(args[0].AsString())
                {
                    case "paperdoll":
                    {
                        UIManager.GetGump<PaperDollGump>(serial)?.Dispose();
                        break;
                    }
                    case "status":
                    {
                        UIManager.GetGump<StatusGumpBase>(serial)?.Dispose();
                        break;
                    }
                    case "profile":
                    {
                        UIManager.GetGump<ProfileGump>(serial == UOSObjects.Player.Serial ? Constants.PROFILE_LOCALSERIAL : serial)?.Dispose();
                        break;
                    }
                    case "container":
                    {
                        UIManager.GetGump<ContainerGump>(serial)?.Dispose();
                        break;
                    }
                }
            }
            else
                ScriptManager.Error(quiet, command, "No objects with that serial was found");
            return true;
        }

        /*private static bool LiftItem(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, "Usage: lift (serial) [amount]");
                return true;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, "lift - invalid serial");
                return true;
            }

            ushort amount = Utility.ToUInt16(args[1].AsString(), 1);

            UOItem item = UOSObjects.FindItem(serial);
            if (item != null)
            {
                DragDropManager.Drag(item, amount <= item.Amount ? amount : item.Amount);
            }
            else
            {
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "Warning: Cannot find item (Out of Range)");
            }

            return true;
        }

        private static bool LiftType(string command, Argument[] args, bool quiet, bool force)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, "Usage: lifttype (gfx/'name of item') [amount]");
                return true;
            }

            string gfxStr = args[0].AsString();
            ushort gfx = Utility.ToUInt16(gfxStr, 0);
            ushort amount = Utility.ToUInt16(args[1].AsString(), 1);

            UOItem item;

            // No graphic id, maybe searching by name?
            if (gfx == 0)
            {
                item = UOSObjects.Player.Backpack != null ? UOSObjects.Player.Backpack.FindItemByName(gfxStr, true) : null;

                if (item == null)
                {
                    ScriptManager.Error(quiet, $"Script Error: Couldn't find '{gfxStr}'");
                    return true;
                }
            }
            else
            {
                item = UOSObjects.Player.Backpack != null ? UOSObjects.Player.Backpack.FindItemByID(gfx) : null;
            }

            if (item != null)
            {
                if (item.Amount < amount)
                    amount = item.Amount;
                DragDropManager.Drag(item, amount);
            }
            else
            {
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "No item of type 0x{0} found!", gfx.ToString("X4"));
            }

            return true;
        }*/
    }
}
