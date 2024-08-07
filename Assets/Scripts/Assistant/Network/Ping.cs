using System;

namespace Assistant
{
    public class Ping
    {
        private static DateTime m_Start;
        private static byte m_Seq;
        private static double m_Time, m_Min, m_Max;
        private static int m_Total;
        private static int m_Count;

        public static bool Response(byte seq)
        {
            if (seq == m_Seq && m_Start != DateTime.MinValue)
            {
                double ms = (DateTime.UtcNow - m_Start).TotalMilliseconds;

                if (ms < m_Min)
                    m_Min = ms;
                if (ms > m_Max)
                    m_Max = ms;

                if (m_Count-- > 0)
                {
                    m_Time += ms;
                    UOSObjects.Player.SendMessage(MsgLevel.Force, $"Response: {ms:F1}ms");
                    DoPing();
                }
                else
                {
                    m_Start = DateTime.MinValue;
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "Ping Result:");
                    UOSObjects.Player.SendMessage(MsgLevel.Force, "Min: {0:F1}ms  Max: {1:F1}ms  Avg: {2:F1}ms", m_Min, m_Max, m_Time / ((double)m_Total));
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void StartPing(int count)
        {
            if (count <= 0 || count > 20)
                m_Count = 5;
            else
                m_Count = count;

            m_Total = m_Count;
            m_Time = 0;
            m_Min = double.MaxValue;
            m_Max = 0;

            UOSObjects.Player.SendMessage(MsgLevel.Force, "Pinging server with {0} packets ({1} bytes)...", m_Count, m_Count * 2);
            DoPing();
        }

        private static void DoPing()
        {
            m_Seq = (byte)Utility.Random(256);
            m_Start = DateTime.UtcNow;
            Engine.Instance.SendToServer(new PingPacket(m_Seq));
        }
    }
}
