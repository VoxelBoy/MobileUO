using ClassicUO.Game;
using System;
using System.Collections.Generic;
using System.Linq;
//using Assistant.Agents;

namespace Assistant
{
    internal delegate void DropDoneCallback(uint iser, uint dser, Point3D newPos);

    internal class DragDropManager
    {
        public enum ProcStatus
        {
            Nothing,
            Success,
            KeepWaiting,
            ReQueue
        }

        public enum ActionType : byte
        {
            None     = 0x00,
            Dressing = 0x01,
            Organize = 0x02
        }

        private class LiftReq
        {
            private static int NextID = 1;

            public LiftReq(uint s, int a, bool cli, bool last, ActionType lifttype)
            {
                Serial = s;
                Amount = a;
                FromClient = cli;
                DoLast = last;
                Id = NextID++;
                LiftType = lifttype;
            }

            public readonly uint Serial;
            public readonly int Amount;
            public readonly int Id;
            public readonly bool FromClient;
            public readonly bool DoLast;
            public readonly ActionType LiftType;

            public override string ToString()
            {
                return $"{Id}({Serial},{Amount},{FromClient},{DoLast})";
            }
        }

        private class DropReq
        {
            public DropReq(uint s, Point3D pt, ActionType droptype)
            {
                Serial = s;
                Point = pt;
                DropType = droptype;
            }

            public DropReq(uint s, Layer layer, ActionType droptype)
            {
                Serial = s;
                Layer = layer;
                DropType = droptype;
            }

            public uint Serial;
            public readonly Point3D Point;
            public readonly Layer Layer;
            public readonly ActionType DropType;
        }

        internal static void DropCurrent()
        {
            //Log("Drop current requested on {0}", m_Holding);

            if (SerialHelper.IsItem(m_Holding))
            {
                if (UOSObjects.Player.Backpack != null)
                    Engine.Instance.SendToServer(new DropRequest(m_Holding, Point3D.MinusOne, UOSObjects.Player.Backpack.Serial));
                else
                    Engine.Instance.SendToServer(new DropRequest(m_Holding, UOSObjects.Player.Position, 0));
            }
            else
            {
                UOSObjects.Player.SendMessage(MsgLevel.Force, "You are not holding anything");
            }

            Clear();
        }

        private static int m_LastID;

        private static uint m_Pending, m_Holding;
        private static bool m_ClientLiftReq = false;
        private static DateTime m_Lifted = DateTime.MinValue;

        private static readonly Dictionary<uint, Queue<DropReq>>
            m_DropReqs = new Dictionary<uint, Queue<DropReq>>();

        private static readonly LiftReq[] m_LiftReqs = new LiftReq[256];
        private static byte m_Front, m_Back;

        public static UOItem Holding 
        {
            get;
            private set;
        }

        public static uint Pending
        {
            get { return m_Pending; }
        }

        public static int LastIDLifted
        {
            get { return m_LastID; }
        }

        public static void Clear()
        {
            //Log("Clearing....");

            m_DropReqs.Clear();
            for (int i = 0; i < 256; i++)
                m_LiftReqs[i] = null;
            m_Front = m_Back = 0;
            m_Holding = m_Pending = 0;
            Holding = null;
            m_Lifted = DateTime.MinValue;
        }

        public static void DragDrop(UOItem i, uint to)
        {
            DragDrop(i, to, Point3D.MinusOne, i.Amount);
        }

        public static void DragDrop(UOItem i, uint to, Point3D p, int amount)
        {
            Drag(i, amount);
            Drop(i, to, p);
        }

        public static void DragDrop(UOItem i, UOItem to, ActionType actionType = ActionType.None)
        {
            DragDrop(i, to, Point3D.MinusOne, i.Amount, actionType);
        }

        public static void DragDrop(UOItem i, UOItem to, Point3D p, int amount, ActionType actionType = ActionType.None)
        {
            Drag(i, amount, actionType: actionType);
            Drop(i, to, p, actionType);
        }

        public static void DragDrop(UOItem i, Point3D dest)
        {
            DragDrop(i, dest, i.Amount);
        }

        public static void DragDrop(UOItem i, Point3D dest, int amount)
        {
            Drag(i, amount);
            Drop(i, uint.MaxValue, dest);
        }

        public static void DragDrop(UOItem i, UOMobile to, Layer layer, bool doLast = false, ActionType actionType = ActionType.None)
        {
            Drag(i, i.Amount, false, doLast);
            Drop(i, to, layer);
        }

        public static bool Empty
        {
            get { return m_Back == m_Front; }
        }

        public static bool Full
        {
            get { return ((byte)(m_Back + 1)) == m_Front; }
        }

        public static bool IsDressing()
        {
            return m_LiftReqs.Any(lr => lr != null && lr.LiftType == ActionType.Dressing) || m_DropReqs.Any(drd => drd.Value.Any(dr => dr.DropType == ActionType.Dressing));
        }

        public static bool IsOrganizing()
        {
            return m_LiftReqs.Any(lr => lr != null && lr.LiftType == ActionType.Organize) || m_DropReqs.Values.Any(drq => drq.Any(dr => dr.DropType == ActionType.Organize));
        }

        public static int Drag(UOItem i, int amount, bool fromClient = false, bool doLast = false, ActionType actionType = ActionType.None)
        {
            LiftReq lr = new LiftReq(i.Serial, amount, fromClient, doLast, actionType);
            LiftReq prev = null;

            if (Full)
            {
                if (fromClient)
                    Engine.Instance.SendToClient(new LiftRej());
                else
                    UOSObjects.Player.SendMessage(MsgLevel.Error, "Drag drop queue is FULL! Please wait");
                return 0;
            }

            //Log("Queuing Drag request {0}", lr);

            if (m_Back >= m_LiftReqs.Length)
                m_Back = 0;

            if (m_Back <= 0)
                prev = m_LiftReqs[m_LiftReqs.Length - 1];
            else if (m_Back <= m_LiftReqs.Length)
                prev = m_LiftReqs[m_Back - 1];

            // if the current last req must stay last, then insert this one in its place
            if (prev != null && prev.DoLast)
            {
                //Log("Back-Queuing {0}", prev);
                if (m_Back <= 0)
                    m_LiftReqs[m_LiftReqs.Length - 1] = lr;
                else if (m_Back <= m_LiftReqs.Length)
                    m_LiftReqs[m_Back - 1] = lr;

                // and then re-insert it at the end
                lr = prev;
            }

            m_LiftReqs[m_Back++] = lr;

            ActionQueue.SignalLift(!fromClient);
            return lr.Id;
        }

        public static bool Drop(UOItem i, UOMobile to, Layer layer, ActionType actionType = ActionType.None)
        {
            if (m_Pending == i.Serial)
            {
                //Log("Equipping {0} to {1} (@{2})", i, to.Serial, layer);
                Engine.Instance.SendToServer(new EquipRequest(i.Serial, to, layer));
                m_Pending = 0;
                EndHolding(i.Serial);
                m_Lifted = DateTime.MinValue;
                return true;
            }
            else
            {
                bool add = false;

                for (byte j = m_Front; j != m_Back && !add; j++)
                {
                    if (m_LiftReqs[j] != null && m_LiftReqs[j].Serial == i.Serial)
                    {
                        add = true;
                        break;
                    }
                }

                if (add)
                {
                    //Log("Queuing Equip {0} to {1} (@{2})", i, to.Serial, layer);

                    if (!m_DropReqs.TryGetValue(i.Serial, out var q) || q == null)
                        m_DropReqs[i.Serial] = q = new Queue<DropReq>();

                    q.Enqueue(new DropReq(to == null ? 0 : to.Serial, layer, actionType));
                    return true;
                }
                else
                {
                    //Log("Drop/Equip for {0} (to {1} (@{2})) not found, skipped", i,to == null ? 0 : to.Serial, layer);
                    return false;
                }
            }
        }

        public static bool Drop(UOItem i, uint dest, Point3D pt, ActionType actionType = ActionType.None)
        {
            if (m_Pending == i.Serial)
            {
                //Log("Dropping {0} to {1} (@{2})", i, dest, pt);
                Engine.Instance.SendToServer(new DropRequest(i.Serial, pt, dest));
                m_Pending = 0;
                EndHolding(i.Serial);
                m_Lifted = DateTime.MinValue;
                return true;
            }
            else
            {
                bool add = false;

                for (byte j = m_Front; j != m_Back && !add; j++)
                {
                    if (m_LiftReqs[j] != null && m_LiftReqs[j].Serial == i.Serial)
                    {
                        add = true;
                        break;
                    }
                }

                if (add)
                {
                    //Log("Queuing Drop {0} (to {1} (@{2}))", i, dest, pt);

                    if (!m_DropReqs.TryGetValue(i.Serial, out var q) || q == null)
                        m_DropReqs[i.Serial] = q = new Queue<DropReq>();

                    q.Enqueue(new DropReq(dest, pt, actionType));
                    return true;
                }
                else
                {
                    //Log("Drop for {0} (to {1} (@{2})) not found, skipped", i, dest, pt);
                    return false;
                }
            }
        }

        public static bool Drop(UOItem i, UOItem to, Point3D pt, ActionType actionType = ActionType.None)
        {
            return Drop(i, to == null ? uint.MaxValue : to.Serial, pt, actionType);
        }

        public static bool Drop(UOItem i, UOItem to)
        {
            return Drop(i, to.Serial, Point3D.MinusOne);
        }

        public static bool LiftReject()
        {
            //Log("Server rejected lift for item {0}", m_Holding);
            if (m_Holding == 0)
                return true;

            m_Holding = m_Pending = 0;
            Holding = null;
            m_Lifted = DateTime.MinValue;

            return m_ClientLiftReq;
        }

        public static bool HasDragFor(uint s)
        {
            for (byte j = m_Front; j != m_Back; j++)
            {
                if (m_LiftReqs[j] != null && m_LiftReqs[j].Serial == s)
                    return true;
            }

            return false;
        }

        public static bool CancelDragFor(uint s)
        {
            if (Empty)
                return false;

            int skip = 0;
            for (byte j = m_Front; j != m_Back; j++)
            {
                if (skip == 0 && m_LiftReqs[j] != null && m_LiftReqs[j].Serial == s)
                {
                    m_LiftReqs[j] = null;
                    skip++;
                    if (j == m_Front)
                    {
                        m_Front++;
                        break;
                    }
                    else
                    {
                        m_Back--;
                    }
                }

                if (skip > 0)
                    m_LiftReqs[j] = m_LiftReqs[(byte)(j + skip)];
            }

            if (skip > 0)
            {
                m_LiftReqs[m_Back] = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EndHolding(uint s)
        {
            //if ( m_Pending == s )
            //	return false;

            if (m_Holding == s)
            {
                m_Holding = 0;
                Holding = null;
            }

            return true;
        }

        private static DropReq DequeueDropFor(uint s)
        {
            DropReq dr = null;
            if (m_DropReqs.TryGetValue(s, out var q) && q != null)
            {
                if (q.Count > 0)
                    dr = q.Dequeue();
                if (q.Count <= 0)
                    m_DropReqs.Remove(s);
            }

            return dr;
        }

        public static void GracefulStop()
        {
            m_Front = m_Back = 0;

            if (SerialHelper.IsValid(m_Pending))
            {
                m_DropReqs.TryGetValue(m_Pending, out var q);
                m_DropReqs.Clear();
                m_DropReqs[m_Pending] = q;
            }
        }

        public static ProcStatus ProcessNext(int numPending)
        {
            if (m_Pending != 0)
            {
                if (m_Lifted + TimeSpan.FromMinutes(2) < DateTime.UtcNow)
                {
                    //UOItem i = UOSObjects.FindItem(m_Pending);

                    //Log("Lift timeout, forced drop to pack for {0}", m_Pending);

                    if (World.Player != null)
                    {
                        UOSObjects.Player.SendMessage(MsgLevel.Force, "WARNING: Drag/Drop timeout! Dropping item in hand to backpack");

                        if (UOSObjects.Player.Backpack != null)
                            Engine.Instance.SendToServer(new DropRequest(m_Pending, Point3D.MinusOne, UOSObjects.Player.Backpack.Serial));
                        else
                            Engine.Instance.SendToServer(new DropRequest(m_Pending, UOSObjects.Player.Position, 0));
                    }

                    m_Holding = m_Pending = 0;
                    Holding = null;
                    m_Lifted = DateTime.MinValue;
                }
                else
                {
                    return ProcStatus.KeepWaiting;
                }
            }

            if (m_Front == m_Back)
            {
                m_Front = m_Back = 0;
                return ProcStatus.Nothing;
            }

            LiftReq lr = m_LiftReqs[m_Front];

            if (numPending > 0 && lr != null && lr.DoLast)
                return ProcStatus.ReQueue;

            m_LiftReqs[m_Front] = null;
            m_Front++;
            if (lr != null)
            {
                //Log("Lifting {0}", lr);

                UOItem item = UOSObjects.FindItem(lr.Serial);
                if (item != null && item.Container == null)
                {
                    // if the item is on the ground and out of range then dont grab it
                    if (Utility.Distance(item.GetWorldPosition(), UOSObjects.Player.Position) > 3)
                    {
                        Scavenger.Uncache(item.Serial);
                        return ProcStatus.Nothing;
                    }
                }

                Engine.Instance.SendToServer(new LiftRequest(lr.Serial, lr.Amount));

                m_LastID = lr.Id;
                m_Holding = lr.Serial;
                Holding = UOSObjects.FindItem(lr.Serial);
                m_ClientLiftReq = lr.FromClient;

                DropReq dr = DequeueDropFor(lr.Serial);
                if (dr != null)
                {
                    m_Pending = 0;
                    EndHolding(lr.Serial);
                    m_Lifted = DateTime.MinValue;

                    //Log("Dropping {0} to {1}", lr, dr.Serial);

                    if (SerialHelper.IsMobile(dr.Serial) && dr.Layer > Layer.Invalid && dr.Layer <= Layer.LastUserValid)
                        Engine.Instance.SendToServer(new EquipRequest(lr.Serial, dr.Serial, dr.Layer));
                    else
                        Engine.Instance.SendToServer(new DropRequest(lr.Serial, dr.Point, dr.Serial));
                }
                else
                {
                    m_Pending = lr.Serial;
                    m_Lifted = DateTime.UtcNow;
                }

                return ProcStatus.Success;
            }
            else
            {
                //Log("No lift to be done?!");
                return ProcStatus.Nothing;
            }
        }
    }

    public class ActionQueue
    {
        private static uint m_Last = 0;
        private static readonly Queue<uint> m_Queue = new Queue<uint>();
        private static readonly ProcTimer m_Timer = new ProcTimer();
        private static int m_Total = 0;

        public static void DoubleClick(bool silent, uint s)
        {
            if (s != 0)
            {
                if (m_Last != s)
                {
                    m_Queue.Enqueue(s);
                    m_Last = s;
                    m_Total++;
                    if (m_Queue.Count == 1 && !m_Timer.Running)
                        m_Timer.StartMe();
                    else if (!silent && m_Total > 1)
                        UOSObjects.Player.SendMessage($"Queuing action request {m_Queue.Count}... {TimeLeft} left.");
                }
                else if (!silent)
                {
                    UOSObjects.Player.SendMessage("Ignoring action request (already queued)");
                }
            }
        }

        public static void SignalLift(bool silent)
        {
            m_Queue.Enqueue(0);
            m_Total++;
            if ( /*m_Queue.Count == 1 &&*/ !m_Timer.Running)
                m_Timer.StartMe();
            else if (!silent && m_Total > 1)
                UOSObjects.Player.SendMessage($"Queuing dragdrop request {m_Queue.Count}... {TimeLeft} left.");
        }

        public static void Stop()
        {
            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
            m_Queue.Clear();
            DragDropManager.Clear();
        }

        public static void ClearActions()
        {
            m_Queue?.Clear();
        }

        public static bool Empty
        {
            get { return m_Queue.Count <= 0 && !m_Timer.Running; }
        }

        public static string TimeLeft
        {
            get
            {
                if (m_Timer.Running)
                {
                    //Config.GetBool("ObjectDelayEnabled")
                    //double time = Config.GetInt( "ObjectDelay" ) / 1000.0;

                    double time = UOSObjects.Gump.ActionDelay / 1000.0;

                    if (!UOSObjects.Gump.UseObjectsQueue)
                    {
                        time = 0;
                    }

                    double init = 0;
                    if (m_Timer.LastTick != DateTime.MinValue)
                        init = time - (DateTime.UtcNow - m_Timer.LastTick).TotalSeconds;
                    time = init + time * m_Queue.Count;
                    if (time < 0)
                        time = 0;
                    return String.Format("{0:F1} seconds", time);
                }
                else
                {
                    return "0.0 seconds";
                }
            }
        }

        private class ProcTimer : Timer
        {
            private DateTime m_StartTime;
            private DateTime m_LastTick;

            public DateTime LastTick
            {
                get { return m_LastTick; }
            }

            public ProcTimer() : base(TimeSpan.Zero, TimeSpan.Zero)
            {
            }

            public void StartMe()
            {
                m_LastTick = DateTime.UtcNow;
                m_StartTime = DateTime.UtcNow;

                OnTick();

                Delay = Interval;

                Start();
            }

            protected override void OnTick()
            {
                //this code is useless now, since this is a drawback from razor, that requeued instead of maintaining in the queue directly, changing also the priority of the queue, prior to my mods to markdwags repo for generic rework
                //List<uint> requeue = null;

                m_LastTick = DateTime.UtcNow;

                if (m_Queue != null && m_Queue.Count > 0)
                {
                    this.Interval = TimeSpan.FromMilliseconds(UOSObjects.Gump.ActionDelay);

                    //this.Interval = TimeSpan.FromMilliseconds( Config.GetInt( "ObjectDelay" ) );

                    while (m_Queue.Count > 0)
                    {
                        uint s = m_Queue.Peek();
                        if (s == 0) // dragdrop action
                        {
                            DragDropManager.ProcStatus status = DragDropManager.ProcessNext(m_Queue.Count - 1);
                            if (status != DragDropManager.ProcStatus.KeepWaiting)
                            {
                                m_Queue.Dequeue(); // if not waiting then dequeue it

                                if (status == DragDropManager.ProcStatus.ReQueue)
                                    m_Queue.Enqueue(s);
                            }

                            if (status == DragDropManager.ProcStatus.KeepWaiting ||
                                status == DragDropManager.ProcStatus.Success)
                                break; // don't process more if we're waiting or we just processed something
                        }
                        else
                        {
                            m_Queue.Dequeue();
                            Engine.Instance.SendToServer(new DoubleClick(s));
                            break;
                        }
                    }

                    /*if (requeue != null)
                    {
                        for (int i = 0; i < requeue.Count; i++)
                            m_Queue.Enqueue(requeue[i]);
                    }*/
                }
                else
                {
                    Stop();

                    if (m_Total > 1 && UOSObjects.Player != null)
                        UOSObjects.Player.SendMessage($"Finished {m_Total} queued actions in {(((DateTime.UtcNow - m_StartTime) - this.Interval).TotalSeconds)} seconds.");

                    m_Last = 0;
                    m_Total = 0;
                }
            }
        }
    }
}
