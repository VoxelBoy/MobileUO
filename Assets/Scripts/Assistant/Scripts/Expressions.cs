using System;
using System.Collections.Generic;
using UOScript;

using ClassicUO.Game;

namespace Assistant.Scripts
{
    public static class Expressions
    {
        private static double DummyExpression(string expression, Argument[] args, bool quiet)
        {
            Console.WriteLine("Executing expression {0} {1}", expression, args);

            return 0.0;
        }

        public static void Register()
        {
            Interpreter.RegisterExpressionHandler("findalias", FindAlias, "findalias ('alias')");
            Interpreter.RegisterExpressionHandler("contents", Contents, "contents (serial) (operator) (value)");
            Interpreter.RegisterExpressionHandler("inregion", InRegion, null);//"inregion ('region type') [serial] [range]"
            Interpreter.RegisterExpressionHandler("skill", SkillExpression, "skill ('skill name')");
            Interpreter.RegisterExpressionHandler("x", X, "x [serial]");
            Interpreter.RegisterExpressionHandler("y", Y, "y [serial]");
            Interpreter.RegisterExpressionHandler("z", Z, "z [serial]");
            Interpreter.RegisterExpressionHandler("physical", Physical, "physical (operator) (value)");
            Interpreter.RegisterExpressionHandler("fire", Fire, "fire (operator) (value)");
            Interpreter.RegisterExpressionHandler("cold", Cold, "cold (operator) (value)");
            Interpreter.RegisterExpressionHandler("poison", Poison, "poison (operator) (value)");
            Interpreter.RegisterExpressionHandler("energy", Energy, "energy (operator) (value)");
            Interpreter.RegisterExpressionHandler("str", Str, "str (operator) (value)");
            Interpreter.RegisterExpressionHandler("dex", Dex, "dex (operator) (value)");
            Interpreter.RegisterExpressionHandler("int", Int, "int (operator) (value)");
            Interpreter.RegisterExpressionHandler("hits", Hits, "hits [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("maxhits", MaxHits, "maxhits [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("diffhits", DiffHits, "diffhits [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("stam", Stam, "stam [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("maxstam", MaxStam, "maxstam [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("mana", Mana, "mana [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("maxmana", MaxMana, "maxmana [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("usequeue", UseQueue, null);//"usequeue (operator) (value)"
            Interpreter.RegisterExpressionHandler("dressing", Dressing, "dressing");
            Interpreter.RegisterExpressionHandler("organizing", Organizing, "organizing");
            Interpreter.RegisterExpressionHandler("followers", Followers, "followers (operator) (value)");
            Interpreter.RegisterExpressionHandler("maxfollowers", MaxFollowers, "maxfollowers (operator) (value)");
            Interpreter.RegisterExpressionHandler("gold", Gold, "gold (operator) (value)");
            Interpreter.RegisterExpressionHandler("hidden", Hidden, "hidden [serial]");
            Interpreter.RegisterExpressionHandler("abilitypoints", Luck, "abilitypoints (operator) (value)");
            Interpreter.RegisterExpressionHandler("faithpoints", TithingPoints, "faithpoints (operator) (value)");
            Interpreter.RegisterExpressionHandler("weight", Weight, "weight (operator) (value)");
            Interpreter.RegisterExpressionHandler("maxweight", MaxWeight, "maxweight (operator) (value)");
            Interpreter.RegisterExpressionHandler("diffweight", DiffWeight, "diffweight (operator) (value)");
            Interpreter.RegisterExpressionHandler("serial", Serial, "serial ('alias') (operator) (value)");
            Interpreter.RegisterExpressionHandler("graphic", Graphic, "graphic (serial) (operator) (value)");
            Interpreter.RegisterExpressionHandler("color", Color, "color (serial) (operator) (value)");
            Interpreter.RegisterExpressionHandler("amount", Amount, "amount (serial) (operator) (value)");
            Interpreter.RegisterExpressionHandler("name", Name, "name [serial] (== or !=) (value)");
            Interpreter.RegisterExpressionHandler("dead", Dead, "dead [serial]");
            Interpreter.RegisterExpressionHandler("direction", Direction, "direction [serial] (operator) (value)");
            Interpreter.RegisterExpressionHandler("flying", Flying, "flying [serial]");
            Interpreter.RegisterExpressionHandler("paralyzed", Paralyzed, "paralyzed [serial]");
            Interpreter.RegisterExpressionHandler("poisoned", Poisoned, "poisoned [serial]");
            Interpreter.RegisterExpressionHandler("mounted", Mounted, "mounted [serial]");
            Interpreter.RegisterExpressionHandler("yellowhits", YellowHits, "yellowhits [serial]");
            Interpreter.RegisterExpressionHandler("criminal", Criminal, "criminal [serial]");
            Interpreter.RegisterExpressionHandler("enemy", Enemy, "enemy [serial]");
            Interpreter.RegisterExpressionHandler("friend", Friend, "friend [serial]");
            Interpreter.RegisterExpressionHandler("gray", Gray, "gray [serial]");
            Interpreter.RegisterExpressionHandler("innocent", Innocent, "innocent [serial]");
            Interpreter.RegisterExpressionHandler("invulnerable", Invulnerable, "invulnerable [serial]");
            Interpreter.RegisterExpressionHandler("murderer", Murderer, "murderer [serial]");
            Interpreter.RegisterExpressionHandler("findobject", FindObject, "findobject (serial) [color] [source] [amount] [range]");
            Interpreter.RegisterExpressionHandler("distance", Distance, "distance (serial) (operator) (value)");
            Interpreter.RegisterExpressionHandler("inrange", InRange, "inrange (serial) (range)");
            Interpreter.RegisterExpressionHandler("buffexists", BuffExists, "buffexists ('buff name')");
            Interpreter.RegisterExpressionHandler("property", Property, null);//, "property ('name') (serial) [operator] [value]");
            Interpreter.RegisterExpressionHandler("findtype", FindType, "findtype (graphic) [color] [source] [amount] [range]");
            Interpreter.RegisterExpressionHandler("findlayer", FindLayer, "findlayer (serial) (layer)");
            Interpreter.RegisterExpressionHandler("skillstate", SkillState, "skillstate ('skill name') (== or !=) ('up/down/locked')");
            Interpreter.RegisterExpressionHandler("counttype", CountType, "counttype (graphic) (color) (source) (operator) (value)");
            Interpreter.RegisterExpressionHandler("counttypeground", CountTypeGround, "counttypeground (graphic) (color) (range) (operator) (value)");
            Interpreter.RegisterExpressionHandler("findwand", FindWand, null);//, "findwand ('spell name'/'any'/'undefined') [source] [minimum charges]");
            Interpreter.RegisterExpressionHandler("inparty", InParty, "inparty (serial)");
            Interpreter.RegisterExpressionHandler("infriendslist", InFriendsList, "infriendlist (serial)");
            Interpreter.RegisterExpressionHandler("war", War, "war [serial]");
            Interpreter.RegisterExpressionHandler("ingump", InGump, "ingump (gump id/'any') ('text')");
            Interpreter.RegisterExpressionHandler("gumpexists", GumpExists, "gumpexists (gump id/'any')");
            Interpreter.RegisterExpressionHandler("injournal", InJournal, "injournal ('text') ['author'/'system']");
            Interpreter.RegisterExpressionHandler("listexists", ListExists, "listexists ('list name')");
            Interpreter.RegisterExpressionHandler("list", ListLength, "list ('list name') (operator) (value)");
            Interpreter.RegisterExpressionHandler("inlist", InList, "inlist ('list name') ('element value')");
            Interpreter.RegisterExpressionHandler("targetexists", TargetExists, "targetexists ['any'/'beneficial'/'harmful'/'neutral'/'server'/'system']");
            Interpreter.RegisterExpressionHandler("waitingfortarget", WaitingForTarget, "waitingfortarget");
            Interpreter.RegisterExpressionHandler("timer", TimerValue, "timer ('timer name') (operator) (value)");
            Interpreter.RegisterExpressionHandler("timerexists", TimerExists, "timerexists ('timer name')");
        }

        private static bool FindAlias(string expression, Argument[] args, bool quiet)
        {
            if (args.Length < 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = Interpreter.GetAlias(args[0].AsString());

            return serial != uint.MaxValue;
        }

        private static int Contents(string expression, Argument[] args, bool quiet)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            uint serial = args[0].AsSerial();

            UOItem container = UOSObjects.FindItem(serial);
            if (container == null || !container.IsContainer)
            {
                ScriptManager.Error(quiet, "Serial not found or is not a container.");
                return 0;
            }

            return container.ItemCount;
        }

        private static bool InRegion(string expression, Argument[] args, bool quiet)
        {
            new RunTimeError(null, $"Expression {expression} not yet supported.");
            return false;
        }

        private static double SkillExpression(string expression, Argument[] args, bool quiet)
        {
            if (args.Length < 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            if (UOSObjects.Player == null)
                return 0;

            var skill = ScriptManager.GetSkill(args[0].AsString());
            if(skill == null)
            {
                new RunTimeError(null, $"Unknown skill name: {args[0].AsString()}");
                return 0;
            }

            return skill.Value;
        }

        private static int X(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.Position.X;
            else if (args.Length != 1)
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.Position.X;
        }

        private static int Y(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.Position.Y;
            else if (args.Length != 1)
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.Position.Y;
        }

        private static int Z(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.Position.Z;
            else if (args.Length != 1)
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.Position.Z;
        }

        private static int Physical(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.AR;
        }

        private static int Fire(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.FireResistance;
        }

        private static int Cold(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.ColdResistance;
        }

        private static int Poison(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.PoisonResistance;
        }

        private static int Energy(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.EnergyResistance;
        }

        private static int Str(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            return UOSObjects.Player.Str;
        }

        private static int Dex(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            return UOSObjects.Player.Dex;
        }

        private static int Int(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            return UOSObjects.Player.Int;
        }

        private static int Hits(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.Hits;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.Hits;
        }

        private static int MaxHits(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.HitsMax;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.HitsMax;
        }

        private static int DiffHits(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.HitsMax - UOSObjects.Player.Hits;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.HitsMax - mobile.Hits;
        }


        private static int Stam(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.Stam;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.Stam;
        }

        private static int MaxStam(string expression, Argument[] args, bool quiet)
        {
            if (UOSObjects.Player == null)
                return 0;

            if (args.Length == 0)
                return UOSObjects.Player.StamMax;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.StamMax;
        }

        private static int Mana(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Mana;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.Mana;
        }

        private static int MaxMana(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.ManaMax;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return 0;
            }

            return mobile.ManaMax;
        }

        private static bool UseQueue(string expression, Argument[] args, bool quiet) { new RunTimeError(null, $"Expression {expression} not yet supported."); return false; }

        private static bool Dressing(string expression, Argument[] args, bool quiet)
        {
            return DragDropManager.IsDressing();
        }

        private static bool Organizing(string expression, Argument[] args, bool quiet)
        {
            return DragDropManager.IsOrganizing();
        }

        private static int Followers(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.Followers;
        }

        private static int MaxFollowers(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.FollowersMax;
        }

        private static uint Gold(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.Gold;
        }

        private static bool Hidden(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return !UOSObjects.Player.Visible;
            }
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return true;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return true;
            }
            else if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);

                if (item == null)
                {
                    ScriptManager.Error(quiet, expression, "item not found");
                    return true;
                }

                return !item.Visible;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return true;
                }

                return !mobile.Visible;
            }
        }

        private static int Luck(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.Luck;
        }

        private static int TithingPoints(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.Tithe;
        }

        private static double Weight(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.Weight;
        }

        private static int MaxWeight(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.MaxWeight;
        }

        private static int DiffWeight(string expression, Argument[] args, bool quiet)
        {
            return UOSObjects.Player.MaxWeight - UOSObjects.Player.Weight;
        }

        private static uint Serial(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            uint serial = Interpreter.GetAlias(args[0].AsString());

            return serial;
        }

        private static int Graphic(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Body;
            }
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return 0;
            }
            else if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);

                if (item == null)
                {
                    ScriptManager.Error(quiet, expression, "item not found");
                    return 0;
                }

                return item.ItemID;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return 0;
                }

                return mobile.Body;
            }
        }

        private static int Color(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return 0;
            }
            else if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);

                if (item == null)
                {
                    ScriptManager.Error(quiet, expression, "item not found");
                    return 0;
                }

                return item.Hue;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return 0;
                }

                return mobile.Hue;
            }
        }

        private static int Amount(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial) || SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return 0;
            }
            else
            {
                UOItem item = UOSObjects.FindItem(serial);

                if (item == null)
                {
                    ScriptManager.Error(quiet, expression, "item not found");
                    return 0;
                }

                return item.Amount;
            }
        }

        private static string Name(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Name;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return string.Empty;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsValid(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return string.Empty;
            }
            else if (SerialHelper.IsItem(serial))
            {
                UOItem item = UOSObjects.FindItem(serial);

                if (item == null)
                {
                    ScriptManager.Error(quiet, expression, "item not found");
                    return string.Empty;
                }

                return item.Name;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return string.Empty;
                }

                return mobile.Name;
            }
        }

        private static bool Dead(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.IsGhost;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                return mobile.IsGhost;
            }
        }

        private static int Direction(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return (int)UOSObjects.Player.Direction;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return 0;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return 0;
                }

                return (int)mobile.Direction;
            }
        }

        private static bool Flying(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Flying;
            }

            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                return mobile.Flying;
            }
        }

        private static bool Paralyzed(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Paralyzed;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                return mobile.Paralyzed;
            }
        }

        private static bool Poisoned(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Poisoned;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                return mobile.Poisoned;
            }
        }

        private static bool Mounted(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                if (UOSObjects.Player.GetItemOnLayer(Layer.Mount) != null)
                    return true;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }
            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                if(mobile.GetItemOnLayer(Layer.Mount) != null)
                    return true;
            }
            return false;
        }

        private static bool YellowHits(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Blessed;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                return mobile.Blessed;
            }
        }
        private static bool Criminal(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x4;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x4;
        }

        private static bool Enemy(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x5;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x5;
        }

        private static bool Friend(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x2;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x2;
        }

        private static bool Gray(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x3;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x3;
        }

        private static bool Innocent(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x1;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x1;
        }

        private static bool Invulnerable(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x7;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x7;
        }
        private static bool Murderer(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.Notoriety == 0x6;
            else if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }
            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.Notoriety == 0x6;
        }

        private static int Distance(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return int.MaxValue;
            }

            uint objSerial = args[0].AsSerial();
            if (!SerialHelper.IsValid(objSerial))
            {
                ScriptManager.Error(quiet, "invalid serial!");
                return int.MaxValue;
            }
            if (SerialHelper.IsMobile(objSerial))
            {
                foreach (UOMobile m in UOSObjects.Mobiles.Values)
                {
                    if (m.Serial == objSerial)
                        return Utility.Distance(UOSObjects.Player.Position, m.Position);
                }
            }
            else
            {
                foreach (UOItem i in UOSObjects.Items.Values)
                {
                    if (i.Serial == objSerial)
                        return Utility.Distance(UOSObjects.Player.Position, i.GetWorldPosition());
                }
            }
            return int.MaxValue;
        }

        private static bool InRange(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 2)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint objSerial = args[0].AsSerial();
            if (!SerialHelper.IsValid(objSerial))
            {
                ScriptManager.Error(quiet, "invalid serial!");
                return false;
            }
            int range = args[1].AsInt();
            if (range >= 0)
            {
                if (SerialHelper.IsMobile(objSerial))
                {
                    foreach(UOMobile m in UOSObjects.MobilesInRange(range, false))
                    {
                        if (m.Serial == objSerial)
                            return true;
                    }
                }
                else
                {
                    foreach (UOItem i in UOSObjects.ItemsInRange(range, false))
                    {
                        if (i.Serial == objSerial)
                            return true;
                    }
                }
            }
            else
                ScriptManager.Error(quiet, "range can't be a negative value!");
            return false;
        }

        private static bool BuffExists(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }
            string name = args[0].AsString();
            if (!string.IsNullOrEmpty(name) && PlayerData.BuffNames.TryGetValue(name.ToLower(XmlFileParser.Culture), out int icon))
            {
                return UOSObjects.Player.BuffsDebuffs.Exists(b => b.IconNumber == icon);
            }
            else
                ScriptManager.Error(quiet, "buffexists: not a valid buff name, check for buff names in bufficons.xml inside Data directory");
            return false; 
        }

        private static bool Property(string expression, Argument[] args, bool quiet)
        { 
            new RunTimeError(null, $"Expression {expression} not yet supported."); 
            return false; 
        }

        private static bool FindObject(string expression, Argument[] args, bool quiet)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint objSerial = args[0].AsSerial();
            if (!SerialHelper.IsValid(objSerial))
            {
                return false;
            }
            UOEntity entity = null;
            if (SerialHelper.IsItem(objSerial))
                entity = UOSObjects.FindItem(objSerial);
            else
                entity = UOSObjects.FindMobile(objSerial);
            if(entity == null)
            {
                if(!quiet)
                    ScriptManager.Error(quiet, $"No valid object found in findobject -> {args[0].AsString()}");
                return false;
            }

            uint? color = null;
            if (args.Length >= 2 && args[1].AsString().ToLower() != "any")
            {
                color = args[1].AsUInt();
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

            uint? amount = null;
            if (args.Length >= 4 && args[3].AsString().ToLower() != "any")
            {
                amount = args[3].AsUInt();
            }

            uint? range = null;
            if (args.Length >= 5 && args[4].AsString().ToLower() != "any")
            {
                range = args[4].AsUInt();
            }
            
            if (args.Length < 3 || source == 0)
            {
                // No source provided or invalid. Treat as UOSObjects.
                if (color.HasValue && entity.Hue != color.Value)
                {
                    entity = null;
                }
                else if (entity is UOItem i)
                {
                    if (sourceStr == "ground" && !i.OnGround)
                        entity = null;
                    else if (amount.HasValue && i.Amount < amount)
                        entity = null;
                    else if (range.HasValue && !Utility.InRange(UOSObjects.Player.Position, i.GetWorldPosition(), (int)range.Value))
                        entity = null;
                }
                else if (range.HasValue && !Utility.InRange(UOSObjects.Player.Position, entity.Position, (int)range.Value))
                {
                    entity = null;
                }
            }
            else
            {
                if (entity is UOItem item)
                {
                    UOItem container = UOSObjects.FindItem(source);
                    if (container != null && container.IsContainer && item.Container == container)
                    {
                        if ((color.HasValue && item.Hue != color.Value) ||
                            (sourceStr == "ground" && !item.OnGround) ||
                            (amount.HasValue && item.Amount < amount) ||
                            (range.HasValue && !Utility.InRange(UOSObjects.Player.Position, item.GetWorldPosition(), (int)range.Value)))
                        {
                            entity = null;
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
                else
                {

                }
            }

            if (entity != null)
            {
                Interpreter.RegisterAliasHandler("found", delegate { return entity.Serial; });
                return true;
            }

            if (!quiet)
            {
                ScriptManager.Error(quiet, $"Script Error: Couldn't find '{args[0].AsString()}'");
            }

            return false;
        }

        private static bool FindType(string expression, Argument[] args, bool quiet)
        {
            if (args.Length < 1)
            {
                ScriptManager.Error(quiet, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            string graphicString = args[0].AsString();
            ushort graphicId = args[0].AsUShort();
            if(graphicId == 0)
                return false;
            
            uint? color = null;
            if (args.Length >= 2 && args[1].AsString().ToLower() != "any")
            {
                color = args[1].AsUInt();
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

            uint? amount = null;
            if (args.Length >= 4 && args[3].AsString().ToLower() != "any")
            {
                amount = args[3].AsUInt();
            }

            uint? range = null;
            if (args.Length >= 5 && args[4].AsString().ToLower() != "any")
            {
                range = args[4].AsUInt();
            }

            List<uint> list = new List<uint>();

            if (args.Length < 3 || source == 0)
            {
                // No source provided or invalid. Treat as UOSObjects.
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

                            if (amount.HasValue && i.Amount < amount)
                            {
                                continue;
                            }

                            if (range.HasValue && !Utility.InRange(UOSObjects.Player.Position, i.GetWorldPosition(), (int)range.Value))
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
                    UOItem item = container.FindItemByID(graphicId);
                    if (item != null &&
                        (!color.HasValue || item.Hue == color.Value) &&
                        (sourceStr != "ground" || item.OnGround) &&
                        (!amount.HasValue || item.Amount >= amount) &&
                        (!range.HasValue || Utility.InRange(UOSObjects.Player.Position, item.GetWorldPosition(), (int)range.Value)))
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
                uint found = list[Utility.Random(list.Count)];
                Interpreter.RegisterAliasHandler("found", delegate { return found; });
                return true;
            }

            if (!quiet)
            {
                ScriptManager.Error(quiet, $"Script Error: Couldn't find '{graphicString}'");
            }

            return false; ;
        }

        private static bool FindLayer(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 2)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            UOItem layeredItem = mobile.GetItemOnLayer((Layer)args[1].AsInt());

            return layeredItem != null;
        }

        private static string SkillState(string expression, Argument[] args, bool quiet)
        {
            if(args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return "unknown";
            }
            var skill = ScriptManager.GetSkill(args[0].AsString());
            if(skill == null)
            {
                new RunTimeError(null, $"Unknown skill name: {args[0].AsString()}");
                return "unknown";
            }

            switch (skill.Lock)
            {
                case LockType.Down:
                    return "down";
                case LockType.Up:
                    return "up";
                case LockType.Locked:
                    return "locked";
            }
            return "unknown";
        }

        private static int CountType(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 3)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var graphic = args[0].AsInt();

            int hue = int.MaxValue;
            if (args[1].AsString().ToLower() != "any")
                hue = args[1].AsInt();

            var container = UOSObjects.FindItem(args[2].AsSerial());

            if (container == null)
            {
                ScriptManager.Error(quiet, expression, "Unable to find source container");
                return 0;
            }

            int count = 0;
            foreach (var item in container.Contents(true))
            {
                if (item.ItemID != graphic)
                    continue;

                if (hue != int.MaxValue && item.Hue != hue)
                    continue;

                count++;
            }

            return count;
        }

        private static int CountTypeGround(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 3)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var graphic = args[0].AsInt();

            int hue = int.MaxValue;
            if (args[1].AsString().ToLower() != "any")
                hue = args[1].AsInt();

            int range = Math.Max(0, args[2].AsInt());
            int count = 0;

            foreach (var item in UOSObjects.ItemsInRange(range, false))
            {
                if (item.ItemID != graphic)
                    continue;

                if (hue != int.MaxValue && item.Hue != hue)
                    continue;

                count++;
            }

            return count;
        }

        private static bool FindWand(string expression, Argument[] args, bool quiet)
        { 
            new RunTimeError(null, $"Expression {expression} not yet supported."); 
            return false; 
        }

        private static bool InParty(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
                return UOSObjects.Player.InParty;

            var mobile = UOSObjects.FindMobile(args[0].AsSerial());

            if (mobile == null)
            {
                ScriptManager.Error(quiet, expression, "mobile not found.");
                return false;
            }

            return mobile.InParty;
        }

        private static bool InFriendsList(string expression, Argument[] args, bool quiet)
        { 
            new RunTimeError(null, $"Expression {expression} not yet supported."); 
            return false; 
        }

        private static bool War(string expression, Argument[] args, bool quiet)
        {
            if (args.Length == 0)
            {
                return UOSObjects.Player.Warmode;
            }
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            uint serial = args[0].AsSerial();

            if (!SerialHelper.IsMobile(serial))
            {
                ScriptManager.Error(quiet, expression, "serial invalid");
                return false;
            }
            else
            {
                UOMobile mobile = UOSObjects.FindMobile(serial);

                if (mobile == null)
                {
                    ScriptManager.Error(quiet, expression, "mobile not found");
                    return false;
                }

                return mobile.Warmode;
            }
        }

        private static bool InGump(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 2)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }
            uint gumpid = args[0].AsUInt();
            if (UOSObjects.Player.OpenedGumps.TryGetValue(gumpid, out var list))
            {
                string gumpstring = args[1].AsString();
                foreach (var g in list)
                {
                    if (g.GumpStrings.Contains(gumpstring))
                        return true;
                }
            }
            return false;
        }

        private static bool GumpExists(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }
            if (args[0].AsString() == "any")
                return UOSObjects.Player.OpenedGumps.Count > 0;

            return UOSObjects.Player.OpenedGumps.TryGetValue(args[0].AsUInt(), out var glist) && glist.Count > 0;
        }

        private static bool InJournal(string expression, Argument[] args, bool quiet)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.SpeechJournalChecks))
            {
                ScriptManager.Error(quiet, "injournal: this functionality is not allowed by your server");
                return false;
            }
            if (args.Length == 0)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            string text = args[0].AsString();
            string name = args.Length > 1 ? args[1].AsString() : "system";
            if (name == "system")
            {
                if(Journal.ContainsSafe(text))
                    return true;
            }
            else if (Journal.ContainsFrom(name, text))
                return true;
            ScriptManager.Error(quiet, "injournal: text not found");
            return false;
        }

        private static bool ListExists(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            if (Interpreter.ListExists(args[0].AsString()))
                return true;

            return false;
        }

        private static int ListLength(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            return Interpreter.ListLength(args[0].AsString());
        }

        private static bool InList(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            if (Interpreter.ListContains(args[0].AsString(), args[1]))
                return true;

            return false;
        }

        private static bool TargetExists(string expression, Argument[] args, bool quiet)
        {
            if(args.Length > 0)
            {
                bool hastargtype = false;
                for (int i = 0; !hastargtype && i < args.Length; i++)
                {
                    hastargtype = Targeting.HasTargetType(args[i].AsString());
                }
                return hastargtype;
            }
            return Targeting.HasTarget;
        }

        private static bool WaitingForTarget(string expression, Argument[] args, bool quiet)
        {
            new RunTimeError(null, $"Expression {expression} not yet supported."); 
            return false; 
        }

        private static int TimerValue(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return 0;
            }

            var ts = Interpreter.GetTimer(args[0].AsString());

            return (int)ts.TotalMilliseconds;
        }

        private static bool TimerExists(string expression, Argument[] args, bool quiet)
        {
            if (args.Length != 1)
            {
                new RunTimeError(null, $"Usage: {Interpreter.GetCmdHelper(expression)}");
                return false;
            }

            return Interpreter.TimerExists(args[0].AsString());
        }

        
    }
}