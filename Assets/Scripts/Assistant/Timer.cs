using System;
using System.Collections.Generic;

namespace Assistant
{
    public class MinHeap
    {
        private List<Timer> m_List;
        private int m_Size;

        public MinHeap() : this(20)
        {
        }

        public MinHeap(int capacity)
        {
            m_List = new List<Timer>(capacity + 1);
            m_Size = 0;
            m_List.Add(null); // 0th index is never used, always null
        }

        /*public MinHeap(Timer c) : this(c.Count)
        {
            foreach (IComparable o in c)
                m_List.Add(o);
            m_Size = c.Count;
            Heapify();
        }*/

        public void Heapify()
        {
            for (int i = m_Size / 2; i > 0; i--)
                PercolateDown(i);
        }

        private void PercolateDown(int hole)
        {
            Timer tmp = m_List[hole];
            int child;

            for (; hole * 2 <= m_Size; hole = child)
            {
                child = hole * 2;
                if (child != m_Size && (m_List[child + 1]).CompareTo(m_List[child]) < 0)
                    child++;

                if (tmp.CompareTo(m_List[child]) >= 0)
                    m_List[hole] = m_List[child];
                else
                    break;
            }

            m_List[hole] = tmp;
        }

        public Timer Peek()
        {
            return m_List[1];
        }

        public Timer Pop()
        {
            Timer top = Peek();

            m_List[1] = m_List[m_Size--];
            PercolateDown(1);

            return top;
        }

        public void Remove(Timer o)
        {
            for (int i = 1; i <= m_Size; i++)
            {
                if (m_List[i] == o)
                {
                    m_List[i] = m_List[m_Size--];
                    PercolateDown(i);
                    // TODO: Do we ever need to shrink?
                    return;
                }
            }
        }

        public void Clear()
        {
            int capacity = m_List.Count / 2;
            if (capacity < 2)
                capacity = 2;
            m_Size = 0;
            m_List = new List<Timer>(capacity);
            m_List.Add(null);
        }

        public void Add(Timer o)
        {
            // PercolateUp
            int hole = ++m_Size;

            // Grow the list if needed
            while (m_List.Count <= m_Size)
                m_List.Add(null);

            for (; hole > 1 && o.CompareTo(m_List[hole / 2]) < 0; hole /= 2)
                m_List[hole] = m_List[hole / 2];
            m_List[hole] = o;
        }

        public void AddMultiple(ICollection<Timer> col)
        {
            if (col != null && col.Count > 0)
            {
                foreach (Timer o in col)
                {
                    int hole = ++m_Size;

                    // Grow the list as needed
                    while (m_List.Count <= m_Size)
                        m_List.Add(null);

                    m_List[hole] = o;
                }

                Heapify();
            }
        }

        public int Count
        {
            get { return m_Size; }
        }

        public bool IsEmpty
        {
            get { return Count <= 0; }
        }

        public List<Timer> GetRawList()
        {
            var copy = new List<Timer>(m_Size);
            for (int i = 1; i <= m_Size; i++)
                copy.Add(m_List[i]);
            return copy;
        }
    }

    public delegate void TimerCallback();

    public delegate void TimerCallbackState<in T>(T state);

    public abstract class Timer : IComparable<Timer>
    {
        private DateTime m_Next;
        private TimeSpan m_Delay;
        private TimeSpan m_Interval;
        private bool m_Running;
        private int m_Index, m_Count;

        protected abstract void OnTick();

        public Timer(TimeSpan delay) : this(delay, TimeSpan.Zero, 1)
        {
        }

        public Timer(TimeSpan interval, int count) : this(interval, interval, count)
        {
        }

        public Timer(TimeSpan delay, TimeSpan interval) : this(delay, interval, 0)
        {
        }

        public Timer(TimeSpan delay, TimeSpan interval, int count)
        {
            m_Delay = delay;
            m_Interval = interval;
            m_Count = count;
        }

        public void Start()
        {
            if (!m_Running)
            {
                m_Index = 0;
                m_Next = DateTime.UtcNow + m_Delay;
                m_Running = true;
                m_Heap.Add(this);
                ChangedNextTick(true);
            }
        }

        public void Stop()
        {
            if (m_Running)
            {
                m_Running = false;
                m_Heap.Remove(this);
                //ChangedNextTick();
            }
        }

        public int CompareTo(Timer t)
        {
            if (t != null)
                return TimeUntilTick.CompareTo(t.TimeUntilTick);
            else
                return -1;
        }

        public TimeSpan TimeUntilTick
        {
            get { return m_Running ? m_Next - DateTime.UtcNow : TimeSpan.MaxValue; }
        }

        public bool Running
        {
            get { return m_Running; }
        }

        public TimeSpan Delay
        {
            get { return m_Delay; }
            set { m_Delay = value; }
        }

        public TimeSpan Interval
        {
            get { return m_Interval; }
            set { m_Interval = value; }
        }

        private static MinHeap m_Heap = new MinHeap();

        private static void ChangedNextTick()
        {
            ChangedNextTick(false);
        }

        private static void ChangedNextTick(bool allowImmediate)
        {
            if (!m_Heap.IsEmpty)
            {
                Timer t = m_Heap.Peek();
                int interval = (int)Math.Round(t.TimeUntilTick.TotalMilliseconds);
                if (allowImmediate && interval <= 0)
                {
                    Slice();
                }
                /*else
                {
                    if (interval <= 0)
                        
                }*/
            }
        }

        public static void Slice()
        {
            int breakCount = 100;
            List<Timer> readd = new List<Timer>();

            while (!m_Heap.IsEmpty && m_Heap.Peek().TimeUntilTick < TimeSpan.Zero)
            {
                if (breakCount-- <= 0)
                    break;

                Timer t = m_Heap.Pop();

                if (t != null && t.Running)
                {
                    t.OnTick();

                    if (t.Running && (t.m_Count == 0 || (++t.m_Index) < t.m_Count))
                    {
                        t.m_Next = DateTime.UtcNow + t.m_Interval;
                        readd.Add(t);
                    }
                    else
                    {
                        t.Stop();
                    }
                }
            }

            m_Heap.AddMultiple(readd);

            ChangedNextTick();
        }

        private class OneTimeTimer : Timer
        {
            private TimerCallback m_Call;

            public OneTimeTimer(TimeSpan d, TimerCallback call) : base(d)
            {
                m_Call = call;
            }

            protected override void OnTick()
            {
                m_Call();
            }
        }

        public static Timer DelayedCallback(TimeSpan delay, TimerCallback call)
        {
            return new OneTimeTimer(delay, call);
        }

        private class OneTimeTimerState<T> : Timer
        {
            private TimerCallbackState<T> m_Call;
            private T m_State;

            public OneTimeTimerState(TimeSpan d, TimerCallbackState<T> call, T state) : base(d)
            {
                m_Call = call;
                m_State = state;
            }

            protected override void OnTick()
            {
                m_Call(m_State);
            }
        }

        public static Timer DelayedCallbackState<T>(TimeSpan delay, TimerCallbackState<T> call, T state)
        {
            return new OneTimeTimerState<T>(delay, call, state);
        }
    }
}
