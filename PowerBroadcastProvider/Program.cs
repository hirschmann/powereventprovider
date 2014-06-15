using StagWare.Windows.Monitoring;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PowerBroadcastProvider
{
    static class Program
    {
        const string EventLogSourceName = "PowerBroadcastProvider";
        const int PowerSchemePersonalityChangedEventId = 1000;
        const int PowerLineStatusChangedEventId = 2000;
        const int LidswitchStateChangedEventId = 3000;
        const int BatteryStatusChangedEventId = 4000;
        const int DisplayStateChangedEventId = 5000;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var pb = new PowerBroadcasts(PowerSettingsNotification.All))
            {
                pb.BatteryStatusChanged += pb_BatteryStatusChanged;
                pb.LidswitchStateChanged += pb_LidswitchStateChanged;
                pb.PowerLineStatusChanged += pb_PowerLineStatusChanged;
                pb.PowerSchemePersonalityChanged += pb_PowerSchemePersonalityChanged;
                pb.DisplayStateChanged += pb_DisplayStateChanged;

                Application.Run();
            }
        }

        private static void WriteLogEntry(string message, int id)
        {
            try
            {
                Debug.WriteLine("Id: " + id);
                Debug.WriteLine("Message: " + message);
                Debug.WriteLine("");
                EventLog.WriteEntry(EventLogSourceName, message, EventLogEntryType.Information, id);
            }
            catch
            {
            }
        }

        static void pb_PowerSchemePersonalityChanged(object sender, PowerSchemeEventArgs e)
        {
            int id = PowerSchemePersonalityChangedEventId + (int)e.Personality;
            string msg = "The active power scheme personality has changed: "
                + e.Personality.ToString();

            WriteLogEntry(msg, id);
        }

        static void pb_PowerLineStatusChanged(object sender, PowerLineStatusEventArgs e)
        {
            int id = PowerLineStatusChangedEventId + (int)e.PowerCondition;
            string msg = "The system power source has changed: "
                + e.PowerCondition.ToString();

            WriteLogEntry(msg, id);
        }

        static void pb_LidswitchStateChanged(object sender, LidswitchStateEventArgs e)
        {
            int id = LidswitchStateChangedEventId + Convert.ToInt32(e.IsLidOpen);
            string msg = "The lid switch state has changed: "
                + (e.IsLidOpen ? "Open" : "Closed");

            WriteLogEntry(msg, id);
        }

        static void pb_BatteryStatusChanged(object sender, BatteryStatusEventArgs e)
        {
            int id = BatteryStatusChangedEventId + e.BatteryPercentageRemaining;
            string msg = "The remaining battery capacity has changed: "
                + e.BatteryPercentageRemaining + "%";

            WriteLogEntry(msg, id);
        }

        static void pb_DisplayStateChanged(object sender, DisplayStateEventArgs e)
        {
            int id = DisplayStateChangedEventId + (int)e.DisplayState;
            string msg = "The current monitor's display state has changed: "
                + e.DisplayState.ToString();

            WriteLogEntry(msg, id);
        }
    }
}
