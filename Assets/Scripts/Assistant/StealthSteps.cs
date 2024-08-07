namespace Assistant
{
    public class StealthSteps
    {
        private static int m_Count;
        private static bool m_Hidden = false;

        public static int Count
        {
            get { return m_Count; }
        }

        public static bool Counting
        {
            get { return m_Hidden; }
        }

        public static bool Hidden
        {
            get { return m_Hidden; }
        }

        public static void OnMove()
        {
            if (m_Hidden && m_Count < 30 && UOSObjects.Player != null && UOSObjects.Gump.CountStealthSteps)
            {
                m_Count++;
                UOSObjects.Player.SendMessage(MsgLevel.Error, $"Stealth steps: {m_Count}");
            }
        }

        public static void Hide()
        {
            m_Hidden = true;
            m_Count = 0;
        }

        public static void Unhide()
        {
            m_Hidden = false;
            m_Count = 0;
        }
    }
}
