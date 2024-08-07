using System;
using System.Linq;
using System.Collections.Generic;

using ClassicUO.Game;
using ClassicUO.Network;

using Assistant.Core;
using UOScript;
using AssistGump = ClassicUO.Game.UI.Gumps.AssistantGump;

namespace Assistant
{
    internal class TargetInfo
    {
        public byte Type;
        public uint TargID;
        public byte Flags;
        public uint Serial;
        public int X, Y;
        public int Z;
        public ushort Gfx;
    }

    public enum MobType
    {
        Any,
        Humanoid,
        Monster
    }

    internal partial class Targeting
    {
        public const uint LocalTargID = 0x7FFFFFFF; // uid for target sent from razor

        public delegate void TargetResponseCallback(bool location, uint serial, Point3D p, ushort gfxid);

        public delegate void CancelTargetCallback();

        private static CancelTargetCallback m_OnCancel;
        private static TargetResponseCallback m_OnTarget;

        private static bool m_Intercept;
        private static bool m_HasTarget;
        private static bool m_ClientTarget;
        private static TargetInfo m_LastTarget;
        private static TargetInfo m_LastGroundTarg;
        private static TargetInfo m_LastBeneTarg;
        private static TargetInfo m_LastHarmTarg;


        private static bool m_FromGrabHotKey;

        private static bool m_AllowGround;
        private static uint m_CurrentID;
        private static byte m_CurFlags;

        private static uint m_PreviousID;
        private static bool m_PreviousGround;
        private static byte m_PrevFlags;

        private static uint m_LastCombatant;

        private delegate bool QueueTarget();

        private static QueueTarget TargetSelfAction = new QueueTarget(DoTargetSelf);
        private static QueueTarget LastTargetAction = new QueueTarget(DoLastTarget);
        private static QueueTarget m_QueueTarget;


        private static uint m_SpellTargID = 0;

        public static uint SpellTargetID
        {
            get { return m_SpellTargID; }
            set { m_SpellTargID = value; }
        }

        private static List<uint> m_FilterCancel = new List<uint>();

        public static bool HasTarget
        {
            get 
            { 
                return m_HasTarget; 
            }
        }

        public static bool ServerTarget => !m_Intercept && m_HasTarget;

        public static TargetInfo LastTargetInfo
        {
            get { return m_LastTarget; }
        }

        public static bool FromGrabHotKey
        {
            get { return m_FromGrabHotKey; }
        }

        private static List<ushort> m_MonsterIds = new List<ushort>()
        {
            0x1, 0x2, 0x3, 0x4, 0x7, 0x8, 0x9, 0xC, 0xD, 0xE, 0xF,
            0x10, 0x11, 0x12, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C,
            0x1E, 0x1F, 0x21, 0x23, 0x24, 0x25, 0x27, 0x29, 0x2A, 0x2C,
            0x2D, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3B, 0x3C, 0x3D, 0x42, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
            0x4B, 0x4F, 0x50, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x59, 0x5A,
            0x5B, 0x5C, 0x5D, 0x5E, 0x60, 0x61, 0x62, 0x69, 0x6A, 0x6B, 0x6C,
            0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x87, 0x88, 0x89,
            0x8A, 0x8B, 0x8C, 0x8E, 0x8F, 0x91, 0x93, 0x96, 0x99, 0x9B, 0x9E,
            0x9F, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xB4, 0x4C, 0x4D, 0x3D
        };

        internal enum TargetType : byte
        {
            None        = 0x00, 
            Invalid     = 0x01, // invalid/across server line
            Innocent    = 0x02, //Blue
            Friend      = 0x04, //Green,
            Gray        = 0x08, //Attackable but not criminal (gray)
            Criminal    = 0x10, //gray
            Enemy       = 0x20, //orange
            Murderer    = 0x40, //red
            Invulnerable= 0x80, //invulnerable
            Any         = 0xFE  //any without invalid
        }

        internal static bool ValidTarget(TargetType target, byte noto)
        {
            return (target & (TargetType)(1 << noto)) != 0;
        }

        public static void Initialize()
        {
            PacketHandler.RegisterClientToServerViewer(0x6C, TargetResponse);
            PacketHandler.RegisterServerToClientViewer(0x6C, NewTarget);
            PacketHandler.RegisterServerToClientViewer(0xAA, CombatantChange);

            /*HotKey.Add(HKCategory.Targets, LocString.LastTarget, LastTarget);
            HotKey.Add(HKCategory.Targets, LocString.TargetSelf, TargetSelf);
            HotKey.Add(HKCategory.Targets, LocString.ClearTargQueue, OnClearQueue);

            HotKey.Add(HKCategory.Targets, LocString.SetLT, TargetSetLastTarget);
            HotKey.Add(HKCategory.Targets, LocString.SetLastBeneficial, SetLastTargetBeneficial);
            HotKey.Add(HKCategory.Targets, LocString.SetLastHarmful, SetLastTargetHarmful);

            HotKey.Add(HKCategory.Targets, LocString.AttackLastComb, AttackLastComb);
            HotKey.Add(HKCategory.Targets, LocString.AttackLastTarg, AttackLastTarg);
            HotKey.Add(HKCategory.Targets, LocString.CancelTarget, CancelTarget);

            InitRandomTarget();
            InitNextPrevTargets();
            InitClosestTargets();*/
        }


        private static void CombatantChange(Packet p, PacketHandlerEventArgs e)
        {
            uint ser = p.ReadUInt();
            if (ser != 0 && ser != uint.MaxValue && SerialHelper.IsValid(ser) && ser != UOSObjects.Player.Serial)
                m_LastCombatant = ser;
        }

        internal static void AttackTarget(uint serial)
        {
            if(SerialHelper.IsValid(serial))
                Engine.Instance.SendToServer(new AttackReq(serial));
        }

        internal static void AttackLastComb()
        {
            if (SerialHelper.IsValid(m_LastCombatant))
                Engine.Instance.SendToServer(new AttackReq(m_LastCombatant));
        }

        internal static void OnFriendTargetSelected(bool loc, uint serial, Point3D p, ushort itemid)
        {
            if (SerialHelper.IsMobile(serial) && serial != UOSObjects.Player.Serial && !UOSObjects.Gump.FriendDictionary.ContainsKey(serial))
            {
                UOMobile m = UOSObjects.FindMobile(serial);
                if (m != null)
                {
                    if (string.IsNullOrEmpty(m.Name))
                        m.Name = "(Not Seen)";
                    UOSObjects.Gump.FriendDictionary[serial] = m.Name;
                    UOSObjects.Player.SendMessage(MsgLevel.Info, $"Friend List: Adding {m}");
                    UOSObjects.Gump.UpdateFriendListGump();
                }
            }
        }

        internal static void OnRemoveFriendSelected(bool loc, uint serial, Point3D p, ushort itemid)
        {
            if (SerialHelper.IsValid(serial))
            {
                if(UOSObjects.Gump.FriendDictionary.Remove(serial))
                    UOSObjects.Gump.UpdateFriendListGump();
            }
        }

        internal static void AttackLastTarg()
        {
            TargetInfo targ;
            if (IsSmartTargetingEnabled())
            {
                // If Smart Targetting is being used we'll assume that the user would like to attack the harmful target.
                targ = m_LastHarmTarg;

                // If there is no last harmful target, then we'll attack the last target.
                if (targ == null)
                    targ = m_LastTarget;
            }
            else
            {
                targ = m_LastTarget;
            }

            if (targ != null && SerialHelper.IsValid(targ.Serial))
                Engine.Instance.SendToServer(new AttackReq(targ.Serial));
        }

        public static uint RandomTarget(byte range, bool friends, bool isdead, MobType type, TargetType noto = TargetType.Any, bool noset = false)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.RandomTargets))
                return 0;

            List<UOMobile> list = new List<UOMobile>();
            foreach (UOMobile m in UOSObjects.MobilesInRange(range))
            {
                if (type == MobType.Humanoid)
                {
                    if (!m.IsHuman)
                        continue;
                }
                else if (type == MobType.Monster)
                {
                    if (!m.IsMonster)
                        continue;
                }

                if (!m.Blessed && m.IsGhost == isdead && m.Serial != World.Player.Serial &&
                    Utility.InRange(UOSObjects.Player.Position, m.Position, UOSObjects.Gump.SmartTargetRangeValue))
                {
                    if (noto == TargetType.Any && !friends)
                    {
                        list.Add(m);
                    }
                    else if (friends && FriendsManager.IsFriend(m.Serial))
                    {
                        list.Add(m);
                    }
                    else if(ValidTarget(noto, m.Notoriety))
                    {
                        list.Add(m);
                    }
                }
            }

            if (list.Count > 0)
            {
                UOMobile m = list[Utility.Random(list.Count)];
                if (!noset)
                    SetLastTargetTo(m);
                return m.Serial;

            }
            else if(!noset)
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "No one matching that was found on your screen.");
            return 0;
        }

        internal static HashSet<ushort> Humanoid = new HashSet<ushort>();
        internal static HashSet<ushort> Transformation = new HashSet<ushort>();
        internal enum FilterType : byte
        {
            Invalid         = 0x00,//funzionamento simile a nextprev di razor
            Closest         = 0x01,//solo il più vicino
            Nearest         = 0x02,//ultimi due target
            AnyRange        = 0x0F,
            Humanoid        = 0x10,
            Transformation  = 0x20,
            AnyForm         = 0xF0
        }
        internal static FilterType GetFilterType(UOMobile m)
        {
            FilterType f = FilterType.Invalid;
            if (Humanoid.Contains(m.Body))
                f |= FilterType.Humanoid;
            if (Transformation.Contains(m.Body))
                f |= FilterType.Transformation;
            return f;
        }

        internal static InternalSorter Instance;
        internal class InternalSorter : IComparer<UOEntity>
        {
            private UOEntity m_From;

            public InternalSorter(UOMobile from)
            {
                m_From = from;
            }

            public int Compare(UOEntity x, UOEntity y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                else if (x == null)
                {
                    return -1;
                }
                else if (y == null)
                {
                    return 1;
                }

                return m_From.GetDistanceToSqrt(x).CompareTo(m_From.GetDistanceToSqrt(y));
            }
        }

        internal static void GetTarget(TargetType targets, FilterType filter, bool isenemy, bool quiet, bool next = true, bool self = false)
        {
            List<UOEntity> list = new List<UOEntity>();
            if (UOSObjects.Player == null)
                return;
            byte map = UOSObjects.Player.Map;
            foreach (UOMobile m in UOSObjects.Mobiles.Values)
            {
                if (m.Map == map && Utility.InRange(UOSObjects.Player.Position, m.Position, World.ClientViewRange))
                {
                    if (ValidTarget(targets, m.Notoriety) && ((filter & FilterType.AnyForm) == 0 || (GetFilterType(m) & filter) != 0))
                    {
                        list.Add(m);
                    }
                }
            }
            if (!self)
                list.Remove(UOSObjects.Player);

            if ((filter &= FilterType.AnyRange) != FilterType.Invalid && list.Count > 1)
            {
                list.Sort(Instance);
                if ((filter & FilterType.Closest) != FilterType.Invalid)
                    list.RemoveRange(1, list.Count - 1);
                else if (list.Count > 2)
                    list.RemoveRange(2, list.Count - 2);
            }
            UOEntity e = GetFromTargets(list, next, out _);
            if (e != null)
            {
                MsgLevel level = e is UOMobile m ? (MsgLevel)m.Notoriety : (isenemy ? MsgLevel.Debug : MsgLevel.Friend);
                if(!quiet)
                    UOSObjects.Player.OverheadMessage(PlayerData.GetColorCode(level), $"[{(isenemy ? "Enemy" : "Friend")}]: {e.GetName()}");
                Interpreter.SetAlias(isenemy ? "enemy" : "friend", e.Serial);
            }
        }

        /// <summary>
        /// Index used to keep track of the current Next/Prev target
        /// </summary>
        private static int _nextPrevTargetIndex;
        private static UOEntity GetFromTargets(List<UOEntity> targets, bool nextTarget, out TargetInfo target)
        {
            UOEntity entity = null, old = UOSObjects.FindMobile(m_LastTarget?.Serial ?? 0);
            target = new TargetInfo();
            if (targets.Count <= 0)
            {
                //UOSObjects.Player.SendMessage(MsgLevel.Warning, "No one matching that was found on your screen.");
                return null;
            }

            // Loop through 3 times and break out if you can't get a target for some reason
            for (int i = 0; i < 3; i++)
            {
                if (nextTarget)
                {
                    _nextPrevTargetIndex++;

                    if (_nextPrevTargetIndex >= targets.Count)
                        _nextPrevTargetIndex = 0;
                }
                else
                {
                    _nextPrevTargetIndex--;

                    if (_nextPrevTargetIndex < 0)
                        _nextPrevTargetIndex = targets.Count - 1;
                }

                entity = targets[_nextPrevTargetIndex];


                if (entity != null && entity != UOSObjects.Player && entity != old)
                    break;

                entity = null;
            }

            if (entity == null)
                entity = old;

            if (entity == null)
            {
                //UOSObjects.Player.SendMessage(MsgLevel.Warning, "No one matching that was found on your screen.");
                return null;
            }

            if (m_HasTarget)
                target.Flags = m_CurFlags;
            else
                target.Type = 0;

            target.Gfx = entity.Graphic;
            target.Serial = entity.Serial;
            target.X = entity.Position.X;
            target.Y = entity.Position.Y;
            target.Z = entity.Position.Z;

            return entity;
        }

        /// <summary>
        /// Handles the common Next/Prev logic based on a list of targets passed in already filtered by the calling
        /// functions conditions.
        /// </summary>
        /// <param name="targets">The list of targets (already filtered)</param>
        /// <param name="nextTarget">next target true, previous target false</param>
        /// <param name="removeFriends"></param>
        public static void NextPrevTarget(List<UOEntity> targets, bool nextTarget, bool isenemy)
        {
            GetFromTargets(targets, nextTarget, out TargetInfo target);

            m_LastGroundTarg = m_LastTarget = target;

            if (isenemy)
                m_LastHarmTarg = target;
            else
                m_LastBeneTarg = target;

            if (isenemy && target.Serial > 0)
            {
                Engine.Instance.SendToClient(new ChangeCombatant(target.Serial));
                m_LastCombatant = target.Serial;
            }

            UOSObjects.Player.SendMessage(MsgLevel.Force, "New target set.");

            OverheadTargetMessage(target);
        }

        private static void OnClearQueue()
        {
            ClearQueue();

            UOSObjects.Player.SendMessage(MsgLevel.Force, "Target Queue Cleared");
        }

        internal static void OneTimeTarget(TargetResponseCallback onTarget)
        {
            OneTimeTarget(false, onTarget, null);
        }

        internal static void OneTimeTarget(TargetResponseCallback onTarget, bool fromGrab)
        {
            m_FromGrabHotKey = fromGrab;

            OneTimeTarget(false, onTarget, null);
        }

        internal static void OneTimeTarget(bool ground, TargetResponseCallback onTarget)
        {
            OneTimeTarget(ground, onTarget, null);
        }

        internal static void OneTimeTarget(TargetResponseCallback onTarget, CancelTargetCallback onCancel)
        {
            OneTimeTarget(false, onTarget, onCancel);
        }

        internal static void OneTimeTarget(bool ground, TargetResponseCallback onTarget, CancelTargetCallback onCancel)
        {
            if (m_Intercept && m_OnCancel != null)
            {
                m_OnCancel();
                CancelOneTimeTarget();
            }

            if (m_HasTarget && m_CurrentID != 0 && m_CurrentID != LocalTargID)
            {
                m_PreviousID = m_CurrentID;
                m_PreviousGround = m_AllowGround;
                m_PrevFlags = m_CurFlags;

                m_FilterCancel.Add(m_PreviousID);
            }

            m_Intercept = true;
            m_CurrentID = LocalTargID;
            m_OnTarget = onTarget;
            m_OnCancel = onCancel;

            m_ClientTarget = m_HasTarget = true;
            Engine.Instance.SendToClient(new Target(LocalTargID, ground));
            ClearQueue();
        }

        internal static void CancelOneTimeTarget()
        {
            m_ClientTarget = m_HasTarget = m_FromGrabHotKey = false;
            Engine.Instance.SendToClient(new CancelTarget(LocalTargID));
            EndIntercept();
        }

        internal static bool HasTargetType(string type)
        {
            //['any'/'beneficial'/'harmful'/'neutral'/'server'/'system']
            if (m_HasTarget)
            {
                switch(type)
                {
                    case "any":
                        return true;
                    case "server":
                        return !m_ClientTarget;
                    case "system":
                        return m_ClientTarget;
                    case "neutral":
                        return m_CurFlags == 0x00;
                    case "harmful":
                        return m_CurFlags == 0x01;
                    case "beneficial":
                        return m_CurFlags == 0x02;
                }
            }
            return false;
        }

        internal static void SetAutoTargetAction(params int[] ints)
        {
            if (_AutoTargetTimer != null && _AutoTargetTimer.Running)
                _AutoTargetTimer.Stop();
            _AutoTargetTimer = new AutoTargetTimer(ints);
            _AutoTargetTimer.Start();
        }

        internal static void CancelAutoTargetAction()
        {
            if (_AutoTargetTimer != null && _AutoTargetTimer.Running)
                _AutoTargetTimer.Stop();
        }

        private static AutoTargetTimer _AutoTargetTimer;
        private class AutoTargetTimer : Timer
        {
            private int _Count = 0;
            private int[] _Ints;

            internal AutoTargetTimer(params int[] ints) : base(TimeSpan.Zero, TimeSpan.FromMilliseconds(100))
            {
                _Ints = ints;
            }

            protected override void OnTick()
            {
                _Count++;
                if (_Count <= 100)
                {
                    if (m_HasTarget)
                    {
                        if (_Ints.Length == 0)//last target
                        {
                            Target(m_LastTarget, false);
                        }
                        else if (_Ints.Length == 1)//serial
                        {
                            uint ser = (uint)_Ints[0];
                            if (ser == UOSObjects.Player.Serial)
                                DoTargetSelf(true);
                            else
                                Target(ser);
                        }
                        else if(_Ints.Length == 3)//point3d
                        {
                            Target(new Point3D(_Ints[0], _Ints[1], _Ints[2]));
                        }
                    }
                }
                else
                    Stop();
            }
        }

        private static bool m_LTWasSet;
        public static void TargetSetLastTarget()
        {
            if (UOSObjects.Player != null)
            {
                m_LTWasSet = false;
                OneTimeTarget(false, new TargetResponseCallback(OnSetLastTarget),
                    new CancelTargetCallback(OnSLTCancel));
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Select Last Target");
            }
        }

        private static void OnSLTCancel()
        {
            m_LTWasSet = m_LastTarget != null;
        }

        private static void OnSetLastTarget(bool location, uint serial, Point3D p, ushort gfxid)
        {
            if (serial == UOSObjects.Player.Serial)
            {
                OnSLTCancel();
                return;
            }

            m_LastBeneTarg = m_LastHarmTarg = m_LastGroundTarg = m_LastTarget = new TargetInfo();
            m_LastTarget.Flags = 0;
            m_LastTarget.Gfx = gfxid;
            m_LastTarget.Serial = serial;
            m_LastTarget.Type = (byte)(location ? 1 : 0);
            m_LastTarget.X = p.X;
            m_LastTarget.Y = p.Y;
            m_LastTarget.Z = p.Z;

            m_LTWasSet = true;

            UOSObjects.Player.SendMessage(MsgLevel.Force, "Last Target Set");

            if (SerialHelper.IsValid(serial))
            {
                LastTargetChanged();
                if(SerialHelper.IsMobile(serial))
                    Engine.Instance.SendToClient(new ChangeCombatant(serial));
                m_LastCombatant = serial;
            }
        }

        private static bool m_LTBeneWasSet;

        /// <summary>
        /// Sets the beneficial target
        /// </summary>
        private static void SetLastTargetBeneficial()
        {
            if (!IsSmartTargetingEnabled())
            {
                UOSObjects.Player.SendMessage(MsgLevel.Error, "Smart Targeting is disabled");
                return;
            }

            if (UOSObjects.Player != null)
            {
                m_LTBeneWasSet = false;
                OneTimeTarget(false, OnSetLastTargetBeneficial, OnSLTBeneficialCancel);
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Target new 'Beneficial Target'");
            }
        }

        private static void OnSLTBeneficialCancel()
        {
            if (m_LastBeneTarg != null)
                m_LTBeneWasSet = true;
        }

        private static void OnSetLastTargetBeneficial(bool location, uint serial, Point3D p, ushort gfxid)
        {
            if (serial == UOSObjects.Player.Serial)
            {
                OnSLTBeneficialCancel();
                return;
            }

            m_LastBeneTarg = new TargetInfo
            {
                Flags = 0,
                Gfx = gfxid,
                Serial = serial,
                Type = (byte)(location ? 1 : 0),
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };

            m_LTBeneWasSet = true;

            UOSObjects.Player.SendMessage(MsgLevel.Force, "Last Beneficial Target Set");

            if (SerialHelper.IsMobile(serial))
            {
                LastBeneficialTargetChanged();
            }
        }

        private static bool m_LTHarmWasSet;

        /// <summary>
        /// Sets the harmful target
        /// </summary>
        private static void SetLastTargetHarmful()
        {
            if (!IsSmartTargetingEnabled())
            {
                UOSObjects.Player.SendMessage(MsgLevel.Error, "Smart Targeting is disabled");
                return;
            }

            if (UOSObjects.Player != null)
            {
                OneTimeTarget(false, OnSetLastTargetHarmful, OnSLTHarmfulCancel);
                UOSObjects.Player.SendMessage(MsgLevel.Force, "Target new 'Harmful Target'");
            }
        }

        private static void OnSLTHarmfulCancel()
        {
            if (m_LastTarget != null)
                m_LTHarmWasSet = true;
        }

        private static void OnSetLastTargetHarmful(bool location, uint serial, Point3D p, ushort gfxid)
        {
            if (serial == UOSObjects.Player.Serial)
            {
                OnSLTHarmfulCancel();
                return;
            }

            m_LastHarmTarg = new TargetInfo
            {
                Flags = 0,
                Gfx = gfxid,
                Serial = serial,
                Type = (byte)(location ? 1 : 0),
                X = p.X,
                Y = p.Y,
                Z = p.Z
            };

            m_LTHarmWasSet = true;

            UOSObjects.Player.SendMessage(MsgLevel.Force, "Last Harmful Target Set");

            if (SerialHelper.IsMobile(serial))
            {
                LastHarmfulTargetChanged();
            }
        }

        private static uint m_OldLT = 0;
        private static uint m_OldBeneficialLT = 0;
        private static uint m_OldHarmfulLT = 0;

        private static void RemoveTextFlags(UOEntity ue)
        {
            if (ue != null)
            {
                bool oplchanged = false;

                oplchanged |= ue.ObjPropList.Remove("Last Target");
                oplchanged |= ue.ObjPropList.Remove("Harmful Target");
                oplchanged |= ue.ObjPropList.Remove("Beneficial Target");

                if (oplchanged)
                    ue.OPLChanged();
            }
        }

        private static void AddTextFlags(UOEntity m)
        {
            if (m != null)
            {
                bool oplchanged = false;

                if (IsSmartTargetingEnabled())
                {
                    if (m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial)
                    {
                        oplchanged = true;
                        m.ObjPropList.Add("Harmful Target");
                    }

                    if (m_LastBeneTarg != null && m_LastBeneTarg.Serial == m.Serial)
                    {
                        oplchanged = true;
                        m.ObjPropList.Add("Beneficial Target");
                    }
                }

                if (!oplchanged && m_LastTarget != null && m_LastTarget.Serial == m.Serial)
                {
                    oplchanged = true;
                    m.ObjPropList.Add("Last Target");
                }

                if (oplchanged)
                    m.OPLChanged();
            }
        }

        private static void LastTargetChanged()
        {
            if (m_LastTarget != null)
            {
                m_LTWasSet = true;
                bool lth = UOSObjects.Gump.HLTargetHue > 0;

                if (SerialHelper.IsItem(m_OldLT))
                {
                    RemoveTextFlags(UOSObjects.FindItem(m_OldLT));
                }
                else
                {
                    UOMobile m = UOSObjects.FindMobile(m_OldLT);
                    if (m != null)
                    {
                        if (lth)
                            Engine.Instance.SendToClient(new MobileIncoming(m));

                        RemoveTextFlags(m);
                    }
                }

                if (SerialHelper.IsItem(m_LastTarget.Serial))
                {
                    AddTextFlags(UOSObjects.FindItem(m_LastTarget.Serial));
                }
                else
                {
                    UOMobile m = UOSObjects.FindMobile(m_LastTarget.Serial);
                    if (m != null)
                    {
                        if (IsLastTarget(m) && lth)
                            Engine.Instance.SendToClient(new MobileIncoming(m));

                        CheckLastTargetRange(m);

                        AddTextFlags(m);
                    }
                }

                m_OldLT = m_LastTarget.Serial;
            }
        }

        private static void LastBeneficialTargetChanged()
        {
            if (m_LastBeneTarg != null)
            {
                if (SerialHelper.IsItem(m_OldBeneficialLT))
                {
                    RemoveTextFlags(UOSObjects.FindItem(m_OldBeneficialLT));
                }
                else
                {
                    UOMobile m = UOSObjects.FindMobile(m_OldBeneficialLT);
                    if (m != null)
                    {
                        RemoveTextFlags(m);
                    }
                }

                if (SerialHelper.IsItem(m_LastBeneTarg.Serial))
                {
                    AddTextFlags(UOSObjects.FindItem(m_LastBeneTarg.Serial));
                }
                else
                {
                    UOMobile m = UOSObjects.FindMobile(m_LastBeneTarg.Serial);
                    if (m != null)
                    {
                        CheckLastTargetRange(m);

                        AddTextFlags(m);
                    }
                }

                m_OldBeneficialLT = m_LastBeneTarg.Serial;
            }
        }

        private static void LastHarmfulTargetChanged()
        {
            if (m_LastHarmTarg != null)
            {
                if (SerialHelper.IsItem(m_OldHarmfulLT))
                {
                    RemoveTextFlags(UOSObjects.FindItem(m_OldHarmfulLT));
                }
                else
                {
                    UOMobile m = UOSObjects.FindMobile(m_OldHarmfulLT);
                    if (m != null)
                    {
                        RemoveTextFlags(m);
                    }
                }

                if (SerialHelper.IsItem(m_LastHarmTarg.Serial))
                {
                    AddTextFlags(UOSObjects.FindItem(m_LastHarmTarg.Serial));
                }
                else
                {
                    UOMobile m = UOSObjects.FindMobile(m_LastHarmTarg.Serial);
                    if (m != null)
                    {
                        CheckLastTargetRange(m);

                        AddTextFlags(m);
                    }
                }

                m_OldHarmfulLT = m_LastHarmTarg.Serial;
            }
        }


        public static bool LTWasSet
        {
            get { return m_LTWasSet; }
        }


        public static void SetLastTargetTo(UOMobile m)
        {
            SetLastTargetTo(m, 0);
        }

        public static void SetLastTargetTo(UOMobile m, byte flagType)
        {
            TargetInfo targ = new TargetInfo();
            m_LastGroundTarg = m_LastTarget = targ;

            if ((m_HasTarget && m_CurFlags == 1) || flagType == 1)
                m_LastHarmTarg = targ;
            else if ((m_HasTarget && m_CurFlags == 2) || flagType == 2)
                m_LastBeneTarg = targ;
            else if (flagType == 0)
                m_LastHarmTarg = m_LastBeneTarg = targ;

            targ.Type = 0;
            if (m_HasTarget)
                targ.Flags = m_CurFlags;
            else
                targ.Flags = flagType;

            targ.Gfx = m.Body;
            targ.Serial = m.Serial;
            targ.X = m.Position.X;
            targ.Y = m.Position.Y;
            targ.Z = m.Position.Z;

            Engine.Instance.SendToClient(new ChangeCombatant(m));
            m_LastCombatant = m.Serial;
            UOSObjects.Player.SendMessage(MsgLevel.Force, "New target set");

            OverheadTargetMessage(targ);

            byte wasSmart = UOSObjects.Gump.SmartTarget;
            UOSObjects.Gump.SmartTarget = 0;
            LastTarget();
            UOSObjects.Gump.SmartTarget = wasSmart;
            LastTargetChanged();
        }

        private static void EndIntercept()
        {
            m_Intercept = false;
            m_OnTarget = null;
            m_OnCancel = null;
            m_FromGrabHotKey = false;
        }

        public static void TargetSelf(bool forceQ = false)
        {
            if (UOSObjects.Player == null)
                return;

            //if ( Macros.MacroManager.AcceptActions )
            //	MacroManager.Action( new TargetSelfAction() );

            if (m_HasTarget)
            {
                if (!DoTargetSelf())
                    ResendTarget();
            }
            else if (forceQ || UOSObjects.Gump.UseTargetQueue)
            {
                if (!forceQ)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "Queued Target Self");
                }

                m_QueueTarget = TargetSelfAction;
            }
        }

        private static bool DoTargetSelf()
        {
            return DoTargetSelf(false);
        }

        public static bool DoTargetSelf(bool nointercept)
        {
            if (UOSObjects.Player == null)
                return false;

            if (CheckHealPoisonTarg(m_CurrentID, UOSObjects.Player.Serial))
                return false;

            CancelClientTarget();
            m_HasTarget = false;
            m_FromGrabHotKey = false;

            if (!nointercept && m_Intercept)
            {
                TargetInfo targ = new TargetInfo();
                targ.Serial = UOSObjects.Player.Serial;
                targ.Gfx = UOSObjects.Player.Body;
                targ.Type = 0;
                targ.X = UOSObjects.Player.Position.X;
                targ.Y = UOSObjects.Player.Position.Y;
                targ.Z = UOSObjects.Player.Position.Z;
                targ.TargID = LocalTargID;
                targ.Flags = 0;

                OneTimeResponse(targ);
            }
            else
            {
                Engine.Instance.SendToServer(new TargetResponse(m_CurrentID, UOSObjects.Player));
            }

            return true;
        }

        public static void LastTarget()
        {
            LastTarget(false);
        }

        public static void LastTarget(bool forceQ)
        {
            //if ( Macros.MacroManager.AcceptActions )
            //	MacroManager.Action( new LastTargetAction() );

            if (FromGrabHotKey)
                return;

            if (m_HasTarget)
            {
                if (!DoLastTarget())
                    ResendTarget();
            }
            else if (forceQ || UOSObjects.Gump.UseTargetQueue)
            {
                if (!forceQ)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "Queued Last Target");
                }

                m_QueueTarget = LastTargetAction;
            }
        }

        public static bool DoLastTarget()
        {
            if (FromGrabHotKey)
                return true;

            TargetInfo targ;
            if (IsSmartTargetingEnabled())
            {
                if (m_AllowGround && m_LastGroundTarg != null)
                    targ = m_LastGroundTarg;
                else if (m_CurFlags == 1)
                    targ = m_LastHarmTarg;
                else if (m_CurFlags == 2)
                    targ = m_LastBeneTarg;
                else
                    targ = m_LastTarget;

                if (targ == null)
                    targ = m_LastTarget;
            }
            else
            {
                if (m_AllowGround && m_LastGroundTarg != null)
                    targ = m_LastGroundTarg;
                else
                    targ = m_LastTarget;
            }

            if (targ == null)
                return false;

            Point3D pos = Point3D.Zero;
            if (SerialHelper.IsMobile(targ.Serial))
            {
                UOMobile m = UOSObjects.FindMobile(targ.Serial);
                if (m != null)
                {
                    pos = m.Position;

                    targ.X = pos.X;
                    targ.Y = pos.Y;
                    targ.Z = pos.Z;
                }
                else
                {
                    pos = Point3D.Zero;
                }
            }
            else if (SerialHelper.IsItem(targ.Serial))
            {
                UOItem i = UOSObjects.FindItem(targ.Serial);
                if (i != null)
                {
                    pos = i.GetWorldPosition();

                    targ.X = i.Position.X;
                    targ.Y = i.Position.Y;
                    targ.Z = i.Position.Z;
                }
                else
                {
                    pos = Point3D.Zero;
                    targ.X = targ.Y = targ.Z = 0;
                }
            }
            else
            {
                if (!m_AllowGround && !SerialHelper.IsValid(targ.Serial))
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "Warning: Current target does not allow to target Ground. Last Target NOT performed");
                    return false;
                }
                else
                {
                    pos = new Point3D(targ.X, targ.Y, targ.Z);
                }
            }

            if (UOSObjects.Gump.SmartTargetRange && //Engine.Instance.AllowBit(FeatureBit.RangeCheckLT) &&
                (pos == Point3D.Zero || !Utility.InRange(UOSObjects.Player.Position, pos, UOSObjects.Gump.SmartTargetRangeValue)))
            {
                if (UOSObjects.Gump.UseTargetQueue)
                    m_QueueTarget = LastTargetAction;
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "Requested Target is out of range, Last Target NOT executed!");
                return false;
            }

            if (CheckHealPoisonTarg(m_CurrentID, targ.Serial))
                return false;

            CancelClientTarget();
            m_HasTarget = false;

            targ.TargID = m_CurrentID;

            if (m_Intercept)
                OneTimeResponse(targ);
            else
                Engine.Instance.SendToServer(new TargetResponse(targ));
            return true;
        }

        public static bool DoQueueTarget(TargetInfo targ)
        {
            if (FromGrabHotKey)
                return true;

            TargetInfo info;
            if (IsSmartTargetingEnabled())
            {
                if (m_AllowGround && m_LastGroundTarg != null)
                    targ = m_LastGroundTarg;
                else if (m_CurFlags == 1)
                    targ = m_LastHarmTarg;
                else if (m_CurFlags == 2)
                    targ = m_LastBeneTarg;
                else
                    targ = m_LastTarget;

                if (targ == null)
                    targ = m_LastTarget;
            }
            else
            {
                if (m_AllowGround && m_LastGroundTarg != null)
                    targ = m_LastGroundTarg;
                else
                    targ = m_LastTarget;
            }

            if (targ == null)
                return false;

            Point3D pos = Point3D.Zero;
            if (SerialHelper.IsMobile(targ.Serial))
            {
                UOMobile m = UOSObjects.FindMobile(targ.Serial);
                if (m != null)
                {
                    pos = m.Position;

                    targ.X = pos.X;
                    targ.Y = pos.Y;
                    targ.Z = pos.Z;
                }
                else
                {
                    pos = Point3D.Zero;
                }
            }
            else if (SerialHelper.IsItem(targ.Serial))
            {
                UOItem i = UOSObjects.FindItem(targ.Serial);
                if (i != null)
                {
                    pos = i.GetWorldPosition();

                    targ.X = i.Position.X;
                    targ.Y = i.Position.Y;
                    targ.Z = i.Position.Z;
                }
                else
                {
                    pos = Point3D.Zero;
                    targ.X = targ.Y = targ.Z = 0;
                }
            }
            else
            {
                if (!m_AllowGround && !SerialHelper.IsValid(targ.Serial))
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "Warning: Current target does not allow to target Ground. Last Target NOT performed");
                    return false;
                }
                else
                {
                    pos = new Point3D(targ.X, targ.Y, targ.Z);
                }
            }

            if (UOSObjects.Gump.SmartTargetRange && //Engine.Instance.AllowBit(FeatureBit.RangeCheckLT) &&
                (pos == Point3D.Zero || !Utility.InRange(UOSObjects.Player.Position, pos, UOSObjects.Gump.SmartTargetRangeValue)))
            {
                if (UOSObjects.Gump.UseTargetQueue)
                    m_QueueTarget = LastTargetAction;
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "Requested Target is out of range, Last Target NOT executed!");
                return false;
            }

            if (CheckHealPoisonTarg(m_CurrentID, targ.Serial))
                return false;

            CancelClientTarget();
            m_HasTarget = false;

            targ.TargID = m_CurrentID;

            if (m_Intercept)
                OneTimeResponse(targ);
            else
                Engine.Instance.SendToServer(new TargetResponse(targ));
            return true;
        }

        public static void ClearQueue()
        {
            m_QueueTarget = null;
        }

        private static TimerCallbackState<TargetInfo> m_OneTimeRespCallback = new TimerCallbackState<TargetInfo>(OneTimeResponse);

        private static void OneTimeResponse(TargetInfo info)
        {
            if ((info.X == 0xFFFF && info.Y == 0xFFFF) && (info.Serial == 0 || info.Serial >= 0x80000000))
            {
                m_OnCancel?.Invoke();
            }
            else
            {
                if (ScriptManager.Recording)
                    ScriptManager.AddToScript($"target {info.Serial}");
                m_OnTarget?.Invoke(info.Type == 1 ? true : false, info.Serial, new Point3D(info.X, info.Y, info.Z), info.Gfx);
            }
            EndIntercept();
        }

        internal static void CancelTarget()
        {
            OnClearQueue();
            if (m_HasTarget)
            {
                if(!m_ClientTarget)
                    Engine.Instance.SendToServer(new TargetCancelResponse(m_CurrentID));
                m_HasTarget = false;
            }
            CancelClientTarget();

            m_FromGrabHotKey = false;
        }

        private static void CancelClientTarget()
        {
            if (m_ClientTarget)
            {
                m_FilterCancel.Add((uint)m_CurrentID);
                Engine.Instance.SendToClient(new CancelTarget(m_CurrentID));
                m_ClientTarget = false;
            }
        }

        private static TargetInfo _QueuedTarget = null;
        private static bool OnSimpleTarget()
        {
            if(_QueuedTarget != null)
            {
                _QueuedTarget.TargID = m_CurrentID;
                m_LastGroundTarg = m_LastTarget = _QueuedTarget;
                Engine.Instance.SendToServer(new TargetResponse(_QueuedTarget));
            }
            return true;
        }

        public static void Target(TargetInfo info, bool forceQ)
        {
            if (m_Intercept)
            {
                OneTimeResponse(info);
            }
            else if (m_HasTarget)
            {
                info.TargID = m_CurrentID;
                m_LastGroundTarg = m_LastTarget = info;
                Engine.Instance.SendToServer(new TargetResponse(info));
            }
            else if (forceQ)
            {
                _QueuedTarget = info;
                m_QueueTarget = OnSimpleTarget;
            }
            CancelClientTarget();
            m_HasTarget = false;
            m_FromGrabHotKey = false;
        }

        public static void Target(Point3D pt, bool forceQ = false)
        {
            TargetInfo info = new TargetInfo
            {
                Type = 1,
                Flags = 0,
                Serial = 0,
                X = pt.X,
                Y = pt.Y,
                Z = pt.Z,
                Gfx = 0
            };

            Target(info, forceQ);
        }

        public static void Target(Point3D pt, int gfx, bool forceQ = false)
        {
            TargetInfo info = new TargetInfo
            {
                Type = 1,
                Flags = 0,
                Serial = 0,
                X = pt.X,
                Y = pt.Y,
                Z = pt.Z,
                Gfx = (ushort)(gfx & 0x3FFF)
            };

            Target(info, forceQ);
        }

        public static void Target(uint s, bool forceQ = false)
        {
            TargetInfo info = new TargetInfo
            {
                Type = 0,
                Flags = 0,
                Serial = s
            };

            if (SerialHelper.IsItem(s))
            {
                UOItem item = UOSObjects.FindItem(s);
                if (item != null)
                {
                    info.X = item.Position.X;
                    info.Y = item.Position.Y;
                    info.Z = item.Position.Z;
                    info.Gfx = item.ItemID;
                }
            }
            else if (SerialHelper.IsMobile(s))
            {
                UOMobile m = UOSObjects.FindMobile(s);
                if (m != null)
                {
                    info.X = m.Position.X;
                    info.Y = m.Position.Y;
                    info.Z = m.Position.Z;
                    info.Gfx = m.Body;
                }
            }

            Target(info, forceQ);
        }

        public static void Target(object o, bool forceQ = false)
        {
            if (o is UOItem item)
            {
                TargetInfo info = new TargetInfo
                {
                    Type = 0,
                    Flags = 0,
                    Serial = item.Serial,
                    X = item.Position.X,
                    Y = item.Position.Y,
                    Z = item.Position.Z,
                    Gfx = item.ItemID
                };
                Target(info, forceQ);
            }
            else if (o is UOMobile m)
            {
                TargetInfo info = new TargetInfo
                {
                    Type = 0,
                    Flags = 0,
                    Serial = m.Serial,
                    X = m.Position.X,
                    Y = m.Position.Y,
                    Z = m.Position.Z,
                    Gfx = m.Body
                };
                Target(info, forceQ);
            }
            else if (o is uint u)
            {
                Target(u, forceQ);
            }
            else if (o is TargetInfo ti)
            {
                Target(ti, forceQ);
            }
        }

        private static DateTime _lastFlagCheck = DateTime.UtcNow;
        private static uint _lastFlagCheckSerial;

        public static void CheckTextFlags(UOMobile m)
        {
            if (DateTime.UtcNow - _lastFlagCheck < TimeSpan.FromMilliseconds(250) && m.Serial == _lastFlagCheckSerial)
                return;

            /*if (IgnoreAgent.IsIgnored(m.Serial))
            {
                m.OverheadMessage(Config.GetInt("SysColor"), "[Ignored]");
            }*/

            if (IsSmartTargetingEnabled())
            {
                bool harm = m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial;
                bool bene = m_LastBeneTarg != null && m_LastBeneTarg.Serial == m.Serial;

                if (harm)
                    m.OverheadMessage(0x90, "[Harmful Target]");
                if (bene)
                    m.OverheadMessage(0x3F, "[Beneficial Target]");
            }

            if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
                m.OverheadMessage(0x3B2, "[Last Target]");

            _lastFlagCheck = DateTime.UtcNow;
            _lastFlagCheckSerial = m.Serial;
        }

        public static bool IsLastTarget(UOMobile m)
        {
            if (m != null)
            {
                if (IsSmartTargetingEnabled())
                {
                    if (m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial)
                        return true;
                }
                else
                {
                    if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
                        return true;
                }
            }

            return false;
        }

        public static bool IsBeneficialTarget(UOMobile m)
        {
            if (m != null)
            {
                if (IsSmartTargetingEnabled())
                {
                    if (m_LastBeneTarg != null && m_LastBeneTarg.Serial == m.Serial)
                        return true;
                }
                else
                {
                    if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
                        return true;
                }
            }

            return false;
        }

        public static bool IsHarmfulTarget(UOMobile m)
        {
            if (m != null)
            {
                if (IsSmartTargetingEnabled())
                {
                    if (m_LastHarmTarg != null && m_LastHarmTarg.Serial == m.Serial)
                        return true;
                }
                else
                {
                    if (m_LastTarget != null && m_LastTarget.Serial == m.Serial)
                        return true;
                }
            }

            return false;
        }

        public static void CheckLastTargetRange(UOMobile m)
        {
            if (UOSObjects.Player == null)
                return;

            if (m_HasTarget && m != null && m_LastTarget != null && m.Serial == m_LastTarget.Serial &&
                m_QueueTarget == LastTargetAction)
            {
                if (UOSObjects.Gump.SmartTargetRange && Engine.Instance.AllowBit(FeatureBit.RangeCheckLT))
                {
                    if (Utility.InRange(UOSObjects.Player.Position, m.Position, UOSObjects.Gump.SmartTargetRangeValue))
                    {
                        if (m_QueueTarget())
                            ClearQueue();
                    }
                }
            }
        }

        private static bool CheckHealPoisonTarg(uint targID, uint ser)
        {
            if (UOSObjects.Player == null)
                return false;

            if (targID == m_SpellTargID && SerialHelper.IsMobile(ser) &&
                (UOSObjects.Player.LastSpell == Spell.ToID(1, 4) || UOSObjects.Player.LastSpell == Spell.ToID(4, 5)) &&
                UOSObjects.Gump.BlockInvalidHeal && Engine.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
            {
                UOMobile m = UOSObjects.FindMobile(ser);

                if (m != null && m.Poisoned)
                {
                    UOSObjects.Player.SendMessage(MsgLevel.Warning, "Heal Blocked (the target is poisoned)");
                    return true;
                }
            }

            return false;
        }

        private static void TargetResponse(Packet p, PacketHandlerEventArgs args)
        {
            TargetInfo info = new TargetInfo
            {
                Type = p.ReadByte(),
                TargID = p.ReadUInt(),
                Flags = p.ReadByte(),
                Serial = p.ReadUInt(),
                X = p.ReadUShort(),
                Y = p.ReadUShort(),
                Z = (short)p.ReadUShort(),
                Gfx = p.ReadUShort()
            };

            m_ClientTarget = false;

            OverheadTargetMessage(info);

            // check for cancel
            if (info.X == 0xFFFF && info.Y == 0xFFFF && (info.Serial <= 0 || info.Serial >= 0x80000000))
            {
                m_HasTarget = false;
                m_FromGrabHotKey = false;

                if (m_Intercept)
                {
                    args.Block = true;
                    Timer.DelayedCallbackState(TimeSpan.Zero, m_OneTimeRespCallback, info).Start();

                    if (m_PreviousID != 0)
                    {
                        m_CurrentID = m_PreviousID;
                        m_AllowGround = m_PreviousGround;
                        m_CurFlags = m_PrevFlags;

                        m_PreviousID = 0;

                        ResendTarget();
                    }
                }
                else if (m_FilterCancel.Contains((uint)info.TargID) || info.TargID == LocalTargID)
                {
                    args.Block = true;
                }

                m_FilterCancel.Clear();
                return;
            }

            ClearQueue();

            if (m_Intercept)
            {
                if (info.TargID == LocalTargID)
                {
                    Timer.DelayedCallbackState(TimeSpan.Zero, m_OneTimeRespCallback, info).Start();

                    m_HasTarget = false;
                    m_FromGrabHotKey = false;
                    args.Block = true;

                    if (m_PreviousID != 0)
                    {
                        m_CurrentID = m_PreviousID;
                        m_AllowGround = m_PreviousGround;
                        m_CurFlags = m_PrevFlags;

                        m_PreviousID = 0;

                        ResendTarget();
                    }

                    m_FilterCancel.Clear();

                    return;
                }
                else
                {
                    EndIntercept();
                }
            }

            m_HasTarget = false;

            if (CheckHealPoisonTarg(m_CurrentID, info.Serial))
            {
                ResendTarget();
                args.Block = true;
            }

            if (info.Serial != UOSObjects.Player.Serial)
            {
                if (SerialHelper.IsValid(info.Serial))
                {
                    // only let lasttarget be a non-ground target

                    m_LastTarget = info;
                    if (info.Flags == 1)
                        m_LastHarmTarg = info;
                    else if (info.Flags == 2)
                        m_LastBeneTarg = info;

                    LastTargetChanged();
                    LastBeneficialTargetChanged();
                    LastHarmfulTargetChanged();
                }

                m_LastGroundTarg = info; // ground target is the true last target
                if (ScriptManager.Recording)
                    ScriptManager.AddToScript(info.Serial == 0 ? $"targettile {info.X} {info.Y} {info.Z}" : (UOSObjects.Gump.RecordTypeUse ? $"targettype 0x{info.Gfx:X4}" : $"target 0x{info.Serial:X}"));
            }
            else
            {
                if (ScriptManager.Recording)
                {
                    if (UOSObjects.Gump.RecordTypeUse)
                        ScriptManager.AddToScript($"targettype 0x{info.Gfx:X4}");
                    else
                        ScriptManager.AddToScript($"target 0x{info.Serial:X}");
                }
            }

            if (UOSObjects.Player.LastSpell == 52 && !GateTimer.Running)
            {
                GateTimer.Start();
            }

            m_FilterCancel.Clear();
        }

        private static void NewTarget(Packet p, PacketHandlerEventArgs args)
        {
            bool prevAllowGround = m_AllowGround;
            uint prevID = m_CurrentID;
            byte prevFlags = m_CurFlags;
            bool prevClientTarget = m_ClientTarget;

            m_AllowGround = p.ReadBool(); // allow ground
            m_CurrentID = p.ReadUInt(); // target uid
            m_CurFlags = p.ReadByte(); // flags
            // the rest of the packet is 0s

            // check for a server cancel command
            if (!m_AllowGround && m_CurrentID == 0 && m_CurFlags == 3)
            {
                m_HasTarget = false;
                m_FromGrabHotKey = false;

                m_ClientTarget = false;
                if (m_Intercept)
                {
                    EndIntercept();
                    UOSObjects.Player.SendMessage(MsgLevel.Error, "Server sent new target, canceling internal target.");
                }

                return;
            }

            if (Spell.LastCastTime + TimeSpan.FromSeconds(3.0) > DateTime.UtcNow &&
                Spell.LastCastTime + TimeSpan.FromSeconds(0.5) <= DateTime.UtcNow && m_SpellTargID == 0)
                m_SpellTargID = m_CurrentID;

            m_HasTarget = true;
            m_ClientTarget = false;
            if (ScriptManager.Recording)
                ScriptManager.AddToScript("waitfortarget 30000");
            /*if (m_QueueTarget == null && ScriptManager.AddToScript("waitfortarget"))
            {
                args.Block = true;
            }
            else*/
            if (m_QueueTarget != null && m_QueueTarget())
            {
                ClearQueue();
                args.Block = true;
            }

            if (args.Block)
            {
                if (prevClientTarget)
                {
                    m_AllowGround = prevAllowGround;
                    m_CurrentID = prevID;
                    m_CurFlags = prevFlags;

                    m_ClientTarget = true;

                    if (!m_Intercept)
                        CancelClientTarget();
                }
            }
            else
            {
                m_ClientTarget = true;

                if (m_Intercept)
                {
                    m_OnCancel?.Invoke();
                    EndIntercept();
                    UOSObjects.Player.SendMessage(MsgLevel.Error, "Server sent new target, canceling internal target.");

                    m_FilterCancel.Add((uint)prevID);
                }
            }
        }

        public static void ResendTarget()
        {
            if (!m_ClientTarget || !m_HasTarget)
            {
                CancelClientTarget();
                m_ClientTarget = m_HasTarget = true;
                Engine.Instance.SendToClient(new Target(m_CurrentID, m_AllowGround, m_CurFlags));
            }
        }

        private static TargetInfo _lastOverheadMessageTarget = new TargetInfo();

        public static void OverheadTargetMessage(TargetInfo info)
        {
            if (info == null)
                return;

            /*if (Config.GetBool("ShowAttackTargetNewOnly") && info.Serial == _lastOverheadMessageTarget.Serial)
                return;

             UOMobile m = null;*/

            if (UOSObjects.Gump.HLTargetHue > 0 && SerialHelper.IsMobile(info.Serial))//Config.GetBool("ShowAttackTargetOverhead") &&
            {
                UOMobile m = UOSObjects.FindMobile(info.Serial);

                if (m == null)
                    return;

                UOSObjects.Player.OverheadMessage(FriendsManager.IsFriend(m.Serial) ? PlayerData.GetColorCode(MsgLevel.Friend) : m.GetNotorietyColorInt(), $"Target: {m.Name}");
            /*}

            if (Config.GetBool("ShowTextTargetIndicator") && info.Serial != 0 && info.Serial.IsMobile)
            {
                // lets not look it up again they had the previous feature enabled
                if (m == null)
                    m = UOSObjects.FindMobile(info.Serial);

                if (m == null)
                    return;*/

                m.OverheadMessage(UOSObjects.Gump.HLTargetHue, $"*{m.Name}*");
            }

            _lastOverheadMessageTarget = info;
        }

        private static bool IsSmartTargetingEnabled()
        {
            return UOSObjects.Gump.SmartTarget > 0 && Engine.Instance.AllowBit(FeatureBit.SmartLT);
        }

        public static void ClosestTarget(MobType type, params int[] noto)
        {
            ClosestTarget(12, false, false, type, noto);
        }

        public static void ClosestTarget(bool friends, MobType type, params int[] noto)
        {
            ClosestTarget(12, friends, false, type, noto);
        }

        public static void ClosestTarget(bool friends, bool isdead, MobType type, params int[] noto)
        {
            ClosestTarget(12, friends, isdead, type, noto);
        }

        /*public static void ClosestTarget(byte range, TargetType target, bool friends, bool isdead, MobType type)
        {

        }*/

        public static void ClosestTarget(byte range, bool friends, bool isdead, MobType type, params int[] noto)
        {
            if (!Engine.Instance.AllowBit(FeatureBit.ClosestTargets))
                return;

            List<UOMobile> list = new List<UOMobile>();
            foreach (UOMobile m in UOSObjects.MobilesInRange(range))
            {
                if (type == MobType.Humanoid)
                {
                    if (!m.IsHuman)
                        continue;
                }
                else if (type == MobType.Monster)
                {
                    if (!m.IsMonster)
                        continue;
                }
                if (!m.Blessed && m.IsGhost == isdead && m.Serial != World.Player.Serial &&
                    Utility.InRange(UOSObjects.Player.Position, m.Position, UOSObjects.Gump.SmartTargetRangeValue))
                {
                    if (noto.Length == 0 && !friends)
                    {
                        list.Add(m);
                    }
                    else if (friends && FriendsManager.IsFriend(m.Serial))
                    {
                        list.Add(m);
                    }
                    else
                    {
                        for (int i = 0; i < noto.Length; i++)
                        {
                            if (noto[i] == m.Notoriety)
                            {
                                list.Add(m);
                                break;
                            }
                        }
                    }
                }
            }

            UOMobile closest = null;
            double closestDist = double.MaxValue;

            foreach (UOMobile m in list)
            {
                double dist = Utility.DistanceSqrt(m.Position, UOSObjects.Player.Position);

                if (dist < closestDist || closest == null)
                {
                    closestDist = dist;
                    closest = m;
                }
            }

            if (closest != null)
                SetLastTargetTo(closest);
            else
                UOSObjects.Player.SendMessage(MsgLevel.Warning, "No one matching that was found on your screen.");
        }
    }
}
