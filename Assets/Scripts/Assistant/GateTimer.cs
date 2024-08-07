using System;
using System.Linq;

using ClassicUO.IO.Resources;

namespace Assistant
{
    public class GateTimer
    {
        private static int m_Count;
        private static Timer m_Timer;

        private static readonly int[] m_ClilocsStop = { 502632 };

        private static readonly int[] m_ClilocsRestart = { 501024 };

        static GateTimer()
        {
            m_Timer = new InternalTimer();
        }

        public static int Count
        {
            get { return m_Count; }
        }

        public static void OnAsciiMessage(string msg)
        {
            if (Running)
            {
                if (m_ClilocsStop.Any(t => ClilocLoader.Instance.GetString(t) == msg))
                {
                    Stop();
                }

                if (m_ClilocsRestart.Any(t => ClilocLoader.Instance.GetString(t) == msg))
                {
                    Start();
                }
            }
        }

        public static bool Running
        {
            get { return m_Timer.Running; }
        }

        public static void Start()
        {
            m_Count = 0;

            if (m_Timer.Running)
            {
                m_Timer.Stop();
            }

            m_Timer.Start();
        }

        public static void Stop()
        {
            m_Timer.Stop();
        }

        private class InternalTimer : Timer
        {
            public InternalTimer() : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            {
            }

            protected override void OnTick()
            {
                m_Count++;
                if (m_Count > 30)
                {
                    Stop();
                }
            }
        }
    }
}
