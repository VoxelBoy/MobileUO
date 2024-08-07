using System;

using ClassicUO.IO.Resources;

namespace Assistant
{
    public class BandageTimer
    {
        private static Timer HealTimer { get; }

        private static int[] ClilocNums { get; } = new int[]
        {
            500955,
            500962,
            500963,
            500964,
            500965,
            500966,
            500967,
            500968,
            500969,
            503252,
            503253,
            503254,
            503255,
            503256,
            503257,
            503258,
            503259,
            503260,
            503261,
            1010058,
            1010648,
            1010650,
            1060088,
            1060167
        };

        static BandageTimer()
        {
            HealTimer = new InternalTimer();
        }

        public static void OnLocalizedMessage(int num)
        {
            if (Running)
            {
                if (num == 500955 || (num >= 500962 && num <= 500970) || (num >= 503252 && num <= 503261) ||
                    num == 1010058 || num == 1010648 || num == 1010650 || num == 1060088 || num == 1060167)
                {
                    Stop();

                    if (UOSObjects.Gump.ShowBandageTimerEnd)
                        ShowBandagingStatusMessage("Bandage: Ending");

                    return;
                }
            }
            // Start timer as soon as there is the "You begin applying the bandages." message or if they are re-healing before the timer ends
            if (num == 500956)
            {
                Start();

                if (UOSObjects.Gump.ShowBandageTimerStart)
                    ShowBandagingStatusMessage("Bandage: Starting");
            }
        }

        public static void OnAsciiMessage(string msg)
        {
            if (Running)
            {
                if (msg == "You heal what little damage you had." ||
                    msg == "You heal what little damage the patient had." ||
                    msg == "You did not stay close enough to heal your target.")
                {
                    Stop();

                    if (UOSObjects.Gump.ShowBandageTimerEnd)
                        ShowBandagingStatusMessage("Bandage: Ending");

                    return;
                }

                if (msg == "You begin applying the bandages.") // Timer is running and they start a new bandage
                {
                    Start();

                    if (UOSObjects.Gump.ShowBandageTimerStart)
                        ShowBandagingStatusMessage("Bandage: Starting");

                    return;
                }

                foreach (var t in ClilocNums)
                {
                    if (ClilocLoader.Instance.GetString(t) == msg)
                    {
                        Stop();

                        if (UOSObjects.Gump.ShowBandageTimerEnd)
                            ShowBandagingStatusMessage("Bandage: Ending");

                        break;
                    }
                }
            }
            else
            {
                // Start timer as soon as there is the "You begin applying the bandages." message
                if (msg == "You begin applying the bandages.")
                {
                    Start();

                    if (UOSObjects.Gump.ShowBandageTimerStart)
                        ShowBandagingStatusMessage("Bandage: Starting");
                }
            }
        }

        public static int Count { get; private set; }

        public static bool Running
        {
            get { return HealTimer.Running; }
        }

        public static void Start()
        {
            Count = 0;

            if (HealTimer.Running)
                HealTimer.Stop();
            HealTimer.Start();
        }

        public static void Stop()
        {
            HealTimer.Stop();
        }

        public static void ShowBandagingStatusMessage(string msg)
        {
            if (UOSObjects.Gump.ShowBandageTimerOverhead)
            {
                UOSObjects.Player.OverheadMessage(88, msg);
            }
            else
            {
                UOSObjects.Player.SendMessage(88, msg);
            }
        }

        private class InternalTimer : Timer
        {
            public InternalTimer() : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            {
            }

            protected override void OnTick()
            {
                if (UOSObjects.Player.IsGhost)
                {
                    BandageTimer.Stop();
                    return;
                }

                Count++;

                if (UOSObjects.Gump.ShowBandageTimerStart || UOSObjects.Gump.ShowBandageTimerEnd)
                {
                    /*bool showMessage = !(Config.GetBool("OnlyShowBandageTimerEvery") &&
                                         m_Count % Config.GetInt("OnlyShowBandageTimerSeconds") != 0);

                    if (showMessage)*/
                        ShowBandagingStatusMessage($"Bandage: {Count}s");
                }

                if (Count > 30)
                    Stop();
            }
        }
    }
}
