using System;

namespace Assistant
{
    public class SkillTimer
    {
        private static int m_Count;
        private static Timer m_Timer;

        static SkillTimer()
        {
            m_Timer = new InternalTimer();
        }

        public static int Count
        {
            get { return m_Count; }
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
                if (m_Count > 10)
                {
                    Stop();
                }
            }
        }
    }
}
