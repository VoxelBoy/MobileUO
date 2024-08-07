using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
//using Assistant.Agents;
using Assistant.Core;
//using Assistant.Macros;
//using Assistant.UI;
using ClassicUO.IO.Resources;
using ClassicUO.Game;
using ClassicUO.Configuration;
using ClassicUO.Network;

namespace Assistant
{
    internal enum LockType : byte
    {
        Up = 0,
        Down = 1,
        Locked = 2
    }

    internal enum MsgLevel
    {
        None = 0,
        Info = 1,
        Friend = 2,
        Advise = 3,
        Force = 4,
        Debug = 5,
        Error = 6,
        Warning = 7
    }

    internal class Skill
    {
        internal static int Count = 55;

        private LockType m_Lock;
        private ushort m_Value;
        private ushort m_Base;
        private ushort m_Cap;
        private short m_Delta;
        private int m_Idx;

        internal Skill(int idx)
        {
            m_Idx = idx;
        }

        internal int Index
        {
            get { return m_Idx; }
        }

        internal LockType Lock
        {
            get { return m_Lock; }
            set { m_Lock = value; }
        }

        internal ushort FixedValue
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        internal ushort FixedBase
        {
            get { return m_Base; }
            set
            {
                m_Delta += (short)(value - m_Base);
                m_Base = value;
            }
        }

        internal ushort FixedCap
        {
            get { return m_Cap; }
            set { m_Cap = value; }
        }

        internal double Value
        {
            get { return m_Value / 10.0; }
            set { m_Value = (ushort)(value * 10.0); }
        }

        internal double Base
        {
            get { return m_Base / 10.0; }
            set { m_Base = (ushort)(value * 10.0); }
        }

        internal double Cap
        {
            get { return m_Cap / 10.0; }
            set { m_Cap = (ushort)(value * 10.0); }
        }

        internal double Delta
        {
            get { return m_Delta / 10.0; }
            set { m_Delta = (short)(value * 10); }
        }
    }

    internal enum SkillName
    {
        Alchemy = 0,
        Anatomy = 1,
        AnimalLore = 2,
        ItemID = 3,
        ArmsLore = 4,
        Parry = 5,
        Begging = 6,
        Blacksmith = 7,
        Fletching = 8,
        Peacemaking = 9,
        Camping = 10,
        Carpentry = 11,
        Cartography = 12,
        Cooking = 13,
        DetectHidden = 14,
        Discordance = 15,
        EvalInt = 16,
        Healing = 17,
        Fishing = 18,
        Forensics = 19,
        Herding = 20,
        Hiding = 21,
        Provocation = 22,
        Inscribe = 23,
        Lockpicking = 24,
        Magery = 25,
        MagicResist = 26,
        Tactics = 27,
        Snooping = 28,
        Musicianship = 29,
        Poisoning = 30,
        Archery = 31,
        SpiritSpeak = 32,
        Stealing = 33,
        Tailoring = 34,
        AnimalTaming = 35,
        TasteID = 36,
        Tinkering = 37,
        Tracking = 38,
        Veterinary = 39,
        Swords = 40,
        Macing = 41,
        Fencing = 42,
        Wrestling = 43,
        Lumberjacking = 44,
        Mining = 45,
        Meditation = 46,
        Stealth = 47,
        RemoveTrap = 48,
        Necromancy = 49,
        Focus = 50,
        Chivalry = 51,
        Bushido = 52,
        Ninjitsu = 53,
        SpellWeaving = 54
    }

    internal enum MaleSounds
    {
        Ah = 0x41A,
        Ahha = 0x41B,
        Applaud = 0x41C,
        BlowNose = 0x41D,
        Burp = 0x41E,
        Cheer = 0x41F,
        ClearThroat = 0x420,
        Cough = 0x421,
        CoughBS = 0x422,
        Cry = 0x423,
        Fart = 0x429,
        Gasp = 0x42A,
        Giggle = 0x42B,
        Groan = 0x42C,
        Growl = 0x42D,
        Hey = 0x42E,
        Hiccup = 0x42F,
        Huh = 0x430,
        Kiss = 0x431,
        Laugh = 0x432,
        No = 0x433,
        Oh = 0x434,
        Oomph1 = 0x435,
        Oomph2 = 0x436,
        Oomph3 = 0x437,
        Oomph4 = 0x438,
        Oomph5 = 0x439,
        Oomph6 = 0x43A,
        Oomph7 = 0x43B,
        Oomph8 = 0x43C,
        Oomph9 = 0x43D,
        Oooh = 0x43E,
        Oops = 0x43F,
        Puke = 0x440,
        Scream = 0x441,
        Shush = 0x442,
        Sigh = 0x443,
        Sneeze = 0x444,
        Sniff = 0x445,
        Snore = 0x446,
        Spit = 0x447,
        Whistle = 0x448,
        Yawn = 0x449,
        Yea = 0x44A,
        Yell = 0x44B,
    }

    internal enum FemaleSounds
    {
        Ah = 0x30B,
        Ahha = 0x30C,
        Applaud = 0x30D,
        BlowNose = 0x30E,
        Burp = 0x30F,
        Cheer = 0x310,
        ClearThroat = 0x311,
        Cough = 0x312,
        CoughBS = 0x313,
        Cry = 0x314,
        Fart = 0x319,
        Gasp = 0x31A,
        Giggle = 0x31B,
        Groan = 0x31C,
        Growl = 0x31D,
        Hey = 0x31E,
        Hiccup = 0x31F,
        Huh = 0x320,
        Kiss = 0x321,
        Laugh = 0x322,
        No = 0x323,
        Oh = 0x324,
        Oomph1 = 0x325,
        Oomph2 = 0x326,
        Oomph3 = 0x327,
        Oomph4 = 0x328,
        Oomph5 = 0x329,
        Oomph6 = 0x32A,
        Oomph7 = 0x32B,
        Oooh = 0x32C,
        Oops = 0x32D,
        Puke = 0x32E,
        Scream = 0x32F,
        Shush = 0x330,
        Sigh = 0x331,
        Sneeze = 0x332,
        Sniff = 0x333,
        Snore = 0x334,
        Spit = 0x335,
        Whistle = 0x336,
        Yawn = 0x337,
        Yea = 0x338,
        Yell = 0x339,
    }

    internal class PlayerData : UOMobile
    {
        internal int VisRange = 18;

        internal int MultiVisRange
        {
            get { return VisRange + 5; }
        }

        private int m_MaxWeight = -1;

        private short m_FireResist, m_ColdResist, m_PoisonResist, m_EnergyResist, m_Luck;
        private ushort m_DamageMin, m_DamageMax;

        private ushort m_Str, m_Dex, m_Int;
        private LockType m_StrLock, m_DexLock, m_IntLock;
        private uint m_Gold;
        private ushort m_Weight;
        private Skill[] m_Skills;
        private ushort m_AR;
        private ushort m_StatCap;
        private byte m_Followers;
        private byte m_FollowersMax;
        private int m_Tithe;
        private sbyte m_LocalLight;
        private byte m_GlobalLight;
        private ushort m_Features;
        private byte m_Season;
        private byte m_DefaultSeason;
        //private int[] m_MapPatches = new int[10];


        private bool m_SkillsSent;
        //private Timer m_CriminalTime;
        private DateTime m_CriminalStart = DateTime.MinValue;
        internal static Dictionary<string, int> BuffNames { get; } = new Dictionary<string, int>();

        internal List<BuffsDebuffs> BuffsDebuffs { get; } = new List<BuffsDebuffs>();

        internal HashSet<uint> OpenedCorpses { get; } = new HashSet<uint>();

        internal PlayerData(uint serial) : base(serial)
        {
            Targeting.Instance = new Targeting.InternalSorter(this);
            m_Skills = new Skill[Skill.Count];
            for (int i = 0; i < m_Skills.Length; i++)
                m_Skills[i] = new Skill(i);
        }

        internal ushort Str
        {
            get { return m_Str; }
            set { m_Str = value; }
        }

        internal ushort Dex
        {
            get { return m_Dex; }
            set { m_Dex = value; }
        }

        internal ushort Int
        {
            get { return m_Int; }
            set { m_Int = value; }
        }

        internal uint Gold
        {
            get { return m_Gold; }
            set { m_Gold = value; }
        }

        internal ushort Weight
        {
            get { return m_Weight; }
            set { m_Weight = value; }
        }

        internal ushort MaxWeight
        {
            get
            {
                if (m_MaxWeight == -1)
                    return (ushort)((m_Str * 3.5) + 40);
                else
                    return (ushort)m_MaxWeight;
            }
            set { m_MaxWeight = value; }
        }

        internal short FireResistance
        {
            get { return m_FireResist; }
            set { m_FireResist = value; }
        }

        internal short ColdResistance
        {
            get { return m_ColdResist; }
            set { m_ColdResist = value; }
        }

        internal short PoisonResistance
        {
            get { return m_PoisonResist; }
            set { m_PoisonResist = value; }
        }

        internal short EnergyResistance
        {
            get { return m_EnergyResist; }
            set { m_EnergyResist = value; }
        }

        internal short Luck
        {
            get { return m_Luck; }
            set { m_Luck = value; }
        }

        internal ushort DamageMin
        {
            get { return m_DamageMin; }
            set { m_DamageMin = value; }
        }

        internal ushort DamageMax
        {
            get { return m_DamageMax; }
            set { m_DamageMax = value; }
        }

        internal LockType StrLock
        {
            get { return m_StrLock; }
            set { m_StrLock = value; }
        }

        internal LockType DexLock
        {
            get { return m_DexLock; }
            set { m_DexLock = value; }
        }

        internal LockType IntLock
        {
            get { return m_IntLock; }
            set { m_IntLock = value; }
        }

        internal ushort StatCap
        {
            get { return m_StatCap; }
            set { m_StatCap = value; }
        }

        internal ushort AR
        {
            get { return m_AR; }
            set { m_AR = value; }
        }

        internal byte Followers
        {
            get { return m_Followers; }
            set { m_Followers = value; }
        }

        internal byte FollowersMax
        {
            get { return m_FollowersMax; }
            set { m_FollowersMax = value; }
        }

        internal int Tithe
        {
            get { return m_Tithe; }
            set { m_Tithe = value; }
        }

        internal Skill[] Skills
        {
            get { return m_Skills; }
        }

        internal bool SkillsSent
        {
            get { return m_SkillsSent; }
            set { m_SkillsSent = value; }
        }

        internal int CriminalTime
        {
            get
            {
                if (m_CriminalStart != DateTime.MinValue)
                {
                    int sec = (int)(DateTime.UtcNow - m_CriminalStart).TotalSeconds;
                    if (sec > 300)
                    {
                        /*if (m_CriminalTime != null)
                            m_CriminalTime.Stop();*/
                        m_CriminalStart = DateTime.MinValue;
                        return 0;
                    }
                    else
                    {
                        return sec;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        /*public void TryOpenCorpses()
        {
            if (UOSObjects.Gump.OpenCorpses)
            {
                if ((ProfileManager.Current.CorpseOpenOptions == 1 || ProfileManager.Current.CorpseOpenOptions == 3) && TargetManager.IsTargeting)
                    return;

                if ((ProfileManager.Current.CorpseOpenOptions == 2 || ProfileManager.Current.CorpseOpenOptions == 3) && IsHidden)
                    return;

                foreach (Item item in World.Items)
                {
                    if (!item.IsDestroyed && item.IsCorpse && item.Distance <= ProfileManager.Current.AutoOpenCorpseRange && !AutoOpenedCorpses.Contains(item.Serial))
                    {
                        AutoOpenedCorpses.Add(item.Serial);
                        GameActions.DoubleClickQueued(item.Serial);
                    }
                }
            }
        }*/

        private void AutoOpenDoors(bool onDirChange)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.AutoOpenDoors) || !UOSObjects.Gump.OpenDoors || (!Visible && (UOSObjects.Gump.OpenDoorsMode & 2) != 0) || (Targeting.ServerTarget && (UOSObjects.Gump.OpenDoorsMode & 1) != 0))
                return;

            if (Body != 0x03DB && !IsGhost && !Blessed && ((int)(Direction & Direction.Up)) % 2 == 0)
            {
                int x = Position.X, y = Position.Y, z = Position.Z;

                /* Check if one more tile in the direction we just moved is a door */
                Utility.Offset(Direction, ref x, ref y);

                List<UOItem> doors = UOSObjects.ItemsInRange(1);
                foreach (UOItem s in doors)
                {
                    if (s.IsDoor && s.Position.X == x && s.Position.Y == y && s.Position.Z - 15 <= z && s.Position.Z + 15 >= z)
                    {
                        // ClassicUO requires a slight pause before attempting to
                        // open a door after a direction change
                        if (onDirChange)
                        {
                            Timer.DelayedCallbackState(TimeSpan.FromMilliseconds(5), RequestOpen, s).Start();
                        }
                        else
                        {
                            RequestOpen(s);
                        }
                        break;
                    }
                }
            }
        }

        private void RequestOpen(UOItem item)
        {
            if(UOSObjects.Gump.UseDoors)
            {
                if (item != null)
                    Engine.Instance.SendToServer(new PDoubleClickRequest(item.Serial));
            }
            else
                Engine.Instance.SendToServer(new OpenDoorMacro());
        }

        internal override void OnPositionChanging(Point3D oldPos)
        {
            if (!IsGhost)
                StealthSteps.OnMove();

            AutoOpenDoors(false);

            List<UOMobile> mlist = new List<UOMobile>(UOSObjects.Mobiles.Values);
            for (int i = 0; i < mlist.Count; i++)
            {
                UOMobile m = mlist[i];
                if (m != this)
                {
                    if (!Utility.InRange(m.Position, Position, VisRange))
                        m.Remove();
                    //else
                      //  Targeting.CheckLastTargetRange(m);
                }
            }

            List<UOItem> ilist = new List<UOItem>(UOSObjects.ItemsInRange(Math.Max(VisRange, MultiVisRange), false, true));
            for (int i = 0; i < ilist.Count; i++)
            {
                UOItem item = ilist[i];
                if (item.Deleted || item.Container != null)
                    continue;

                int dist = Utility.Distance(item.GetWorldPosition(), Position);
                if (item != DragDropManager.Holding && (dist > MultiVisRange || (!item.IsMulti && dist > VisRange)))
                    item.Remove();
                else if (!IsGhost && Visible && dist <= 2 && Scavenger.Enabled && item.Movable)
                    Scavenger.Scavenge(item);
            }

            base.OnPositionChanging(oldPos);
        }

        internal override void OnDirectionChanging(Direction oldDir)
        {
            AutoOpenDoors(true);
        }

        internal override void OnMapChange(byte old, byte cur)
        {
            List<UOMobile> list = new List<UOMobile>(UOSObjects.Mobiles.Values);
            for (int i = 0; i < list.Count; i++)
            {
                UOMobile m = list[i];
                if (m != this && m.Map != cur)
                    m.Remove();
            }

            list = null;

            UOSObjects.Items.Clear();
            //Counter.Reset();
            for (int i = 0; i < Contains.Count; i++)
            {
                UOItem item = (UOItem)Contains[i];
                UOSObjects.AddItem(item);
                item.Contains.Clear();
            }

            if (UOSObjects.Gump.AutoSearchContainers && Backpack != null)
                PlayerData.DoubleClick(Backpack.Serial);

            //UOAssist.PostMapChange(cur);
        }

        protected override void OnNotoChange(byte old, byte cur)
        {
            if ((old == 3 || old == 4) && (cur != 3 && cur != 4))
            {
                // grey is turning off
                // SendMessage( "You are no longer a criminal." );
                /*if (m_CriminalTime != null)
                    m_CriminalTime.Stop();*/
                m_CriminalStart = DateTime.MinValue;
                //Engine.Instance.RequestTitlebarUpdate();
            }
            else if ((cur == 3 || cur == 4) && (old != 3 && old != 4 && old != 0))
            {
                // grey is turning on
                ResetCriminalTimer();
            }
        }

        internal void ResetCriminalTimer()
        {
            if (m_CriminalStart == DateTime.MinValue || DateTime.UtcNow - m_CriminalStart >= TimeSpan.FromSeconds(1))
            {
                m_CriminalStart = DateTime.UtcNow;
                /*if (m_CriminalTime == null)
                    m_CriminalTime = new CriminalTimer(this);
                m_CriminalTime.Start();*/
                //Engine.Instance.RequestTitlebarUpdate();
            }
        }

        /*private class CriminalTimer : Timer
        {
            private PlayerData m_Player;

            internal CriminalTimer(PlayerData player) : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            {
                m_Player = player;
            }

            protected override void OnTick()
            {
                //Engine.Instance.RequestTitlebarUpdate();
            }
        }

        internal void SendMessage(MsgLevel lvl, LocString loc, params object[] args)
        {
            SendMessage(lvl, Language.Format(loc, args));
        }

        internal void SendMessage(MsgLevel lvl, LocString loc)
        {
            SendMessage(lvl, Language.GetString(loc));
        }

        internal void SendMessage(LocString loc, params object[] args)
        {
            SendMessage(MsgLevel.Info, Language.Format(loc, args));
        }

        internal void SendMessage(LocString loc)
        {
            SendMessage(MsgLevel.Info, Language.GetString(loc));
        }*/

        internal void SendMessage(int hue, string text)
        {
            Engine.Instance.SendToClient(new UnicodeMessage(0xFFFFFFFF, -1, MessageType.Regular, hue, 3, "ENU", "System", text));
        }

        internal void SendMessage(MsgLevel lvl, string format, params object[] args)
        {
            SendMessage(lvl, String.Format(format, args));
        }

        internal void SendMessage(string format, params object[] args)
        {
            SendMessage(MsgLevel.Info, String.Format(format, args));
        }

        internal void SendMessage(string text)
        {
            SendMessage(MsgLevel.Info, text);
        }

        internal static int GetColorCode(MsgLevel lvl)
        {
            switch (lvl)
            {
                case MsgLevel.Info:
                    return 0x59;
                case MsgLevel.Friend:
                    return 0x3F;
                case MsgLevel.Advise:
                    return 0x384;
                case MsgLevel.Force:
                    return 0x7E8;
                case MsgLevel.Debug:
                    return 0x90;
                case MsgLevel.Error:
                    return 0x20;
                case MsgLevel.Warning:
                    return 0x35;
                default:
                    return 945;
            }
        }

        internal void SendMessage(MsgLevel lvl, string text)
        {
            if (text.Length > 0)
            {
                int hue = GetColorCode(lvl);

                Engine.Instance.SendToClient(new UnicodeMessage(0xFFFFFFFF, -1, MessageType.Regular, hue, 3, "ENU", "System", text));
            }
        }

        internal void Say(int hue, string msg, MessageType msgtype = MessageType.Regular)
        {
            Engine.Instance.SendToServer(new ClientUniEncodedCommandMessage(msgtype, (ushort)hue, 3, msgtype == MessageType.Emote ? $"*{msg}*" : msg));
        }

        internal void Say(string msg)
        {
            Say(ProfileManager.Current.SpeechHue, msg);
        }

        internal class GumpData
        {
            //gump ID is univocal, on runuo and servuo it depends on gethashcode, even if gethashcode is not guaranteed to be always the same, it will remain the same as long as the machine doesn't changes or the underlying system won't change
            internal uint GumpID { get; }
            internal uint ServerID { get; }
            internal List<string> GumpStrings { get; }
            internal GumpData(uint gumpid, uint serverid, List<string> strings = null)
            {
                GumpID = gumpid;
                ServerID = serverid;
                GumpStrings = strings;
            }
        }

        internal Dictionary<uint, List<GumpData>> OpenedGumps = new Dictionary<uint, List<GumpData>>();//not saved, on logout all gumps are gone
        //TODO: GumpResponseAction
        //internal GumpResponseAction LastGumpResponseAction;
        internal delegate void ContextQueuedResponse(uint serial, int option);
        internal List<ContextMenuResponse> ContextResponses = new List<ContextMenuResponse>();
        internal uint CurrentMenuS;
        internal ushort CurrentMenuI;
        internal bool HasMenu;

        internal bool HasPrompt;
        internal uint PromptSenderSerial;
        internal uint PromptID;
        internal uint PromptType;
        internal string PromptInputText;

        internal void CancelPrompt()
        {
            Engine.Instance.SendToServer(new PromptResponse(UOSObjects.Player.PromptSenderSerial, UOSObjects.Player.PromptID, 0, string.Empty));
            UOSObjects.Player.HasPrompt = false;
        }

        internal void ResponsePrompt(string text)
        {
            Engine.Instance.SendToServer(new PromptResponse(UOSObjects.Player.PromptSenderSerial, UOSObjects.Player.PromptID, 1, text));

            PromptInputText = text;
            UOSObjects.Player.HasPrompt = false;
        }

        private ushort m_SpeechHue;

        internal ushort SpeechHue
        {
            get { return m_SpeechHue; }
            set { m_SpeechHue = value; }
        }

        internal sbyte LocalLightLevel
        {
            get { return m_LocalLight; }
            set { m_LocalLight = value; }
        }

        internal byte GlobalLightLevel
        {
            get { return m_GlobalLight; }
            set { m_GlobalLight = value; }
        }

        internal enum SeasonFlag
        {
            Spring,
            Summer,
            Fall,
            Winter,
            Desolation
        }

        internal byte Season
        {
            get { return m_Season; }
            set { m_Season = value; }
        }

        internal byte DefaultSeason
        {
            get { return m_DefaultSeason; }
            set { m_DefaultSeason = value; }
        }

        /// <summary>
        /// Sets the player's season, set a default to revert back if required
        /// </summary>
        /// <param name="defaultSeason"></param>
        internal void SetSeason(byte defaultSeason = 0)
        {
            if (UOSObjects.Gump.FixedSeason < 5)
            {
                byte season = UOSObjects.Gump.FixedSeason;

                UOSObjects.Player.Season = season;
                UOSObjects.Player.DefaultSeason = defaultSeason;
                if (!m_SeasonTimer.Running)
                    m_SeasonTimer.Start();
            }
            else
            {
                UOSObjects.Player.Season = defaultSeason;
                UOSObjects.Player.DefaultSeason = defaultSeason;
            }
        }

        internal static Timer m_SeasonTimer = new SeasonTimer();

        private class SeasonTimer : Timer
        {
            internal SeasonTimer() : base(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3))
            {
            }

            protected override void OnTick()
            {
                Engine.Instance.SendToClient(new SeasonChange(UOSObjects.Player.Season, true));
                m_SeasonTimer.Stop();
            }
        }

        internal ushort Features
        {
            get { return m_Features; }
            set { m_Features = value; }
        }

        /*internal int[] MapPatches
        {
            get { return m_MapPatches; }
            set { m_MapPatches = value; }
        }*/

        private int m_LastSkill = -1;

        internal int LastSkill
        {
            get { return m_LastSkill; }
            set { m_LastSkill = value; }
        }

        internal uint LastObject { get; set; } = 0;

        private int m_LastSpell = -1;

        internal int LastSpell
        {
            get { return m_LastSpell; }
            set { m_LastSpell = value; }
        }

        //private UOEntity m_LastCtxM = null;
        //internal UOEntity LastContextMenu { get { return m_LastCtxM; } set { m_LastCtxM = value; } }

        internal bool UseItem(UOItem cont, ushort find)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.PotionHotkeys))
                return false;

            for (int i = 0; i < cont.Contains.Count; i++)
            {
                UOItem item = (UOItem)cont.Contains[i];

                if (item.ItemID == find)
                {
                    PlayerData.DoubleClick(item.Serial);
                    return true;
                }
                else if (item.Contains != null && item.Contains.Count > 0)
                {
                    if (UseItem(item, find))
                        return true;
                }
            }

            return false;
        }

        internal static bool DoubleClick(uint s, bool silent = true)
        {
            if (s != 0)
            {
                UOItem free = null, pack = UOSObjects.Player.Backpack;
                if (SerialHelper.IsItem(s) && pack != null && UOSObjects.Gump.HandsBeforePotions && Engine.Instance.AllowBit(FeatureBit.AutoPotionEquip))
                {
                    UOItem i = UOSObjects.FindItem(s);
                    if (i != null && i.IsPotion && i.ItemID != 3853) // dont unequip for exploison potions
                    {
                        // dont worry about uneqipping RuneBooks or SpellBooks
                        UOItem left = UOSObjects.Player.GetItemOnLayer(Layer.LeftHand);
                        UOItem right = UOSObjects.Player.GetItemOnLayer(Layer.RightHand);

                        if (left != null && (right != null || left.IsTwoHanded))
                            free = left;
                        else if (right != null && right.IsTwoHanded)
                            free = right;

                        if (free != null)
                        {
                            if (DragDropManager.HasDragFor(free.Serial))
                                free = null;
                            else
                                DragDropManager.DragDrop(free, pack);
                        }
                    }
                }

                ActionQueue.DoubleClick(silent, s);
                if (free != null)
                    DragDropManager.DragDrop(free, UOSObjects.Player, free.Layer, true);

                if (SerialHelper.IsItem(s))
                    UOSObjects.Player.LastObject = s;
            }

            return false;
        }
    }
}
