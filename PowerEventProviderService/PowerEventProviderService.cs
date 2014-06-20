using StagWare.Windows.Monitoring;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Linq;
using PowerEventProviderService.Properties;

namespace PowerEventProviderService
{
    public partial class PowerEventProviderService : ServiceBase
    {
        #region Constants

        const int PowerSchemePersonalityChangedEventId = 1000;
        const int PowerLineStatusChangedEventId = 2000;
        const int LidswitchStateChangedEventId = 3000;
        const int BatteryStatusChangedEventId = 4000;
        const int DisplayStateChangedEventId = 5000;

        #endregion

        #region Private Fields

        private PowerBroadcasts pb;

        #endregion

        #region Constructor

        public PowerEventProviderService()
        {
            InitializeComponent();
        }

        #endregion

        #region Protected Methods

        protected override void OnStart(string[] args)
        {
            var provider = new ServicePowerEventProvider(this.ServiceName);
            provider.ServiceStop += provider_ServiceStop;
            this.pb = new PowerBroadcasts(GetEnabledNotifications(), provider);

            pb.BatteryStatusChanged += pb_BatteryStatusChanged;
            pb.LidswitchStateChanged += pb_LidswitchStateChanged;
            pb.PowerLineStatusChanged += pb_PowerLineStatusChanged;
            pb.PowerSchemePersonalityChanged += pb_PowerSchemePersonalityChanged;
            pb.DisplayStateChanged += pb_DisplayStateChanged;
        }

        protected override void OnStop()
        {
            pb.Dispose();
            pb = null;
        }

        #endregion

        #region Private Methods

        private static PowerSettingsNotification GetEnabledNotifications()
        {
            var notifications = PowerSettingsNotification.None;

            if (Settings.Default.MonitorBatteryPercentage)
            {
                notifications |= PowerSettingsNotification.BatteryPercentage;
            }

            if (Settings.Default.MonitorDisplayState)
            {
                notifications |= PowerSettingsNotification.DisplayState;
            }

            if (Settings.Default.MonitorLidswitchState)
            {
                notifications |= PowerSettingsNotification.LidswitchState;
            }

            if (Settings.Default.MonitorPowerSchemePersonality)
            {
                notifications |= PowerSettingsNotification.PowerSchemePersonality;
            }

            if (Settings.Default.MonitorPowerSource)
            {
                notifications |= PowerSettingsNotification.PowerSource;
            }

            return notifications;
        }

        private void WriteLogEntry(string message, int id)
        {
            try
            {
                this.EventLog.WriteEntry(message, EventLogEntryType.Information, id);
            }
            catch
            {
            }
        }

        #endregion

        #region EventHandlers

        void provider_ServiceStop(object sender, EventArgs e)
        {
            OnStop();
        }

        private void pb_PowerSchemePersonalityChanged(object sender, PowerSchemeEventArgs e)
        {
            int id = PowerSchemePersonalityChangedEventId + (int)e.Personality;
            string msg = "The active power scheme personality has changed: "
                + e.Personality.ToString();

            WriteLogEntry(msg, id);
        }

        private void pb_PowerLineStatusChanged(object sender, PowerLineStatusEventArgs e)
        {
            int id = PowerLineStatusChangedEventId + (int)e.PowerCondition;
            string msg = "The system power source has changed: "
                + e.PowerCondition.ToString();

            WriteLogEntry(msg, id);
        }

        private void pb_LidswitchStateChanged(object sender, LidswitchStateEventArgs e)
        {
            int id = LidswitchStateChangedEventId + Convert.ToInt32(e.IsLidOpen);
            string msg = "The lid switch state has changed: "
                + (e.IsLidOpen ? "Open" : "Closed");

            WriteLogEntry(msg, id);
        }

        private void pb_BatteryStatusChanged(object sender, BatteryStatusEventArgs e)
        {
            int id = BatteryStatusChangedEventId + e.BatteryPercentageRemaining;
            string msg = "The remaining battery capacity has changed: "
                + e.BatteryPercentageRemaining + "%";

            WriteLogEntry(msg, id);
        }

        private void pb_DisplayStateChanged(object sender, DisplayStateEventArgs e)
        {
            int id = DisplayStateChangedEventId + (int)e.DisplayState;
            string msg = "The current monitor's display state has changed: "
                + e.DisplayState.ToString();

            WriteLogEntry(msg, id);
        }

        #endregion
    }
}
