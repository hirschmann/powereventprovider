using StagWare.System.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace StagWare.Windows.Monitoring
{
    [Flags]
    public enum PowerSettingsNotification : uint
    {
        None = 0x00,
        PowerSource = 0x01,
        BatteryPercentage = 0x02,
        LidswitchState = 0x04,
        PowerSchemePersonality = 0x08,
        DisplayState = 0x10,

        All = PowerSource | BatteryPercentage | LidswitchState 
            | PowerSchemePersonality | DisplayState
    }

    public enum SystemPowerCondition : uint
    {
        PoAc = 0,
        PoDc = 1,
        PoHot = 2,
        PoConditionMaximum = 3
    }

    public enum PowerSchemePersonality : uint
    {        
        PowerSaver = 0,
        Automatic = 1,
        HighPerformance = 2
    }

    public enum DisplayState : uint
    {
        Off = 0,
        On = 1,
        Dimmed = 2
    }

    public class PowerBroadcasts : IDisposable
    {
        #region Nested Types

        private static class NativeMethods
        {
            #region Structs

            [StructLayout(LayoutKind.Explicit, Size = 36)]
            public struct POWERBROADCAST_SETTING
            {
                [FieldOffset(0)]
                public Guid PowerSetting;

                [FieldOffset(16)]
                public uint DataLength;

                // Set offset to 20 for Data and PowerSchemePersonality.
                // If DataLength is sizeof(GUID) (=16 byte), only PowerSchemePersonality is valid.
                // Else only Data is valid.
                [FieldOffset(20)]
                public uint Data;

                [FieldOffset(20)]
                public Guid PowerSchemePersonality;
            }

            #endregion

            #region Constants

            public static readonly Guid GUID_ACDC_POWER_SOURCE = new Guid("5d3e9a59-e9D5-4b00-a6bd-ff34ff516548");
            public static readonly Guid GUID_BATTERY_PERCENTAGE_REMAINING = new Guid("a7ad8041-b45a-4cae-87a3-eecbb468a9e1");
            public static readonly Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid("ba3e0f4d-b817-4094-a2d1-d56379e6a0f3");

            public static readonly Guid GUID_POWERSCHEME_PERSONALITY = new Guid("245d8541-3943-4422-b025-13a784f679b7");
            public static readonly Guid GUID_MAX_POWER_SAVINGS = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            public static readonly Guid GUID_MIN_POWER_SAVINGS = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
            public static readonly Guid GUID_TYPICAL_POWER_SAVINGS = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");

            public static readonly Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99e6-e5a17ebd1aea");
            public static readonly Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");

            public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
            public const int WM_POWERBROADCAST = 0x0218;
            public const int PBT_POWERSETTINGCHANGE = 0x8013;

            #endregion

            #region Methods

            [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification")]
            public static extern IntPtr RegisterPowerSettingNotification(
                IntPtr recipientHandle,
                ref Guid powerSettingGuid,
                Int32 flags);

            [DllImport(@"User32", EntryPoint = "UnregisterPowerSettingNotification")]
            public static extern bool UnregisterPowerSettingNotification(IntPtr handle);

            #endregion
        }

        #endregion

        #region Constants

        private static readonly Version NT62Version = new Version(6, 2);

        #endregion

        #region Private Fields

        private MessageReceiverWindow window;
        private Dictionary<PowerSettingsNotification, IntPtr> notificationHandles;

        #endregion

        #region Events

        public event EventHandler<BatteryStatusEventArgs> BatteryStatusChanged;
        public event EventHandler<PowerLineStatusEventArgs> PowerLineStatusChanged;
        public event EventHandler<PowerSchemeEventArgs> PowerSchemePersonalityChanged;
        public event EventHandler<LidswitchStateEventArgs> LidswitchStateChanged;
        public event EventHandler<DisplayStateEventArgs> DisplayStateChanged;

        #endregion

        #region Constructor

        public PowerBroadcasts()
            : this(PowerSettingsNotification.None)
        {
        }

        public PowerBroadcasts(PowerSettingsNotification notifications)
        {
            this.notificationHandles = new Dictionary<PowerSettingsNotification, IntPtr>();
            this.window = new MessageReceiverWindow();
            this.window.MessageReceived += new EventHandler<MessageReceivedEventArgs>(window_MessageReceived);

            RegisterNotifications(notifications);
        }

        #endregion

        #region Public Methods

        public void RegisterNotifications(PowerSettingsNotification notifications)
        {
            foreach (PowerSettingsNotification n in notifications.GetFlags())
            {
                if (!this.notificationHandles.ContainsKey(n))
                {
                    Guid guid = Guid.Empty;

                    switch (n)
                    {
                        case PowerSettingsNotification.BatteryPercentage:
                            guid = NativeMethods.GUID_BATTERY_PERCENTAGE_REMAINING;
                            break;

                        case PowerSettingsNotification.LidswitchState:
                            guid = NativeMethods.GUID_LIDSWITCH_STATE_CHANGE;
                            break;

                        case PowerSettingsNotification.PowerSchemePersonality:
                            guid = NativeMethods.GUID_POWERSCHEME_PERSONALITY;
                            break;

                        case PowerSettingsNotification.PowerSource:
                            guid = NativeMethods.GUID_ACDC_POWER_SOURCE;
                            break;

                        case PowerSettingsNotification.DisplayState:
                            if (Environment.OSVersion.Version >= NT62Version)
                            {
                                guid = NativeMethods.GUID_CONSOLE_DISPLAY_STATE;
                            }
                            else
                            {
                                guid = NativeMethods.GUID_MONITOR_POWER_ON;
                            }
                            break;
                    }

                    IntPtr handle = NativeMethods.RegisterPowerSettingNotification(
                        this.window.Handle,
                        ref guid,
                        NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);

                    this.notificationHandles.Add(n, handle);
                }
            }
        }

        public void UnregisterNotifications(PowerSettingsNotification notifications)
        {
            foreach (PowerSettingsNotification n in notifications.GetFlags())
            {
                if (this.notificationHandles.ContainsKey(n))
                {
                    NativeMethods.UnregisterPowerSettingNotification(this.notificationHandles[n]);
                    this.notificationHandles.Remove(n);
                }
            }
        }

        #endregion

        #region Protected Methods

        protected void OnBatteryStatusChanged(BatteryStatusEventArgs e)
        {
            if (this.BatteryStatusChanged != null)
            {
                BatteryStatusChanged(this, e);
            }
        }

        protected void OnPowerLineStatusChanged(PowerLineStatusEventArgs e)
        {
            if (this.PowerLineStatusChanged != null)
            {
                PowerLineStatusChanged(this, e);
            }
        }

        protected void OnPowerSchemePersonalityChanged(PowerSchemeEventArgs e)
        {
            if (this.PowerSchemePersonalityChanged != null)
            {
                PowerSchemePersonalityChanged(this, e);
            }
        }

        protected void OnLidswitchStateChanged(LidswitchStateEventArgs e)
        {
            if (this.LidswitchStateChanged != null)
            {
                LidswitchStateChanged(this, e);
            }
        }

        protected void OnDisplayStateChanged(DisplayStateEventArgs args)
        {
            if (this.DisplayStateChanged != null)
            {
                DisplayStateChanged(this, args);
            }
        }

        #endregion

        #region EventHandlers

        private void window_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if ((e.Message.Msg == NativeMethods.WM_POWERBROADCAST)
                && (e.Message.WParam.ToInt32() == NativeMethods.PBT_POWERSETTINGCHANGE))
            {
                var powerSettings = (NativeMethods.POWERBROADCAST_SETTING)Marshal.PtrToStructure(
                    e.Message.LParam, typeof(NativeMethods.POWERBROADCAST_SETTING));
                
                if (powerSettings.PowerSetting == NativeMethods.GUID_ACDC_POWER_SOURCE)
                {
                    var args = new PowerLineStatusEventArgs()
                    {
                        PowerCondition = (SystemPowerCondition)powerSettings.Data
                    };

                    OnPowerLineStatusChanged(args);
                }
                else if (powerSettings.PowerSetting == NativeMethods.GUID_BATTERY_PERCENTAGE_REMAINING)
                {
                    var args = new BatteryStatusEventArgs()
                    {
                        BatteryPercentageRemaining = (int)powerSettings.Data
                    };

                    OnBatteryStatusChanged(args);
                }
                else if (powerSettings.PowerSetting == NativeMethods.GUID_LIDSWITCH_STATE_CHANGE)
                {
                    var args = new LidswitchStateEventArgs()
                    {
                        IsLidOpen = powerSettings.Data != 0
                    };

                    OnLidswitchStateChanged(args);
                }
                else if (powerSettings.PowerSetting == NativeMethods.GUID_POWERSCHEME_PERSONALITY)
                {
                    var args = new PowerSchemeEventArgs();

                    if (powerSettings.PowerSchemePersonality == NativeMethods.GUID_TYPICAL_POWER_SAVINGS)
                    {
                        args.Personality = PowerSchemePersonality.Automatic;
                    }
                    else if (powerSettings.PowerSchemePersonality == NativeMethods.GUID_MIN_POWER_SAVINGS)
                    {
                        args.Personality = PowerSchemePersonality.PowerSaver;
                    }
                    else if (powerSettings.PowerSchemePersonality == NativeMethods.GUID_MAX_POWER_SAVINGS)
                    {
                        args.Personality = PowerSchemePersonality.HighPerformance;
                    }

                    OnPowerSchemePersonalityChanged(args);
                }
                else if (powerSettings.PowerSetting == NativeMethods.GUID_MONITOR_POWER_ON
                    || powerSettings.PowerSetting == NativeMethods.GUID_CONSOLE_DISPLAY_STATE)
                {
                    var args = new DisplayStateEventArgs()
                    {
                        DisplayState = (DisplayState)powerSettings.Data
                    };

                    OnDisplayStateChanged(args);
                }
            }
        }        

        #endregion

        #region IDisposable implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    //TODO: Dispose managed resources.
                }

                this.window.DestroyHandle();

                foreach (IntPtr handle in this.notificationHandles.Values)
                {
                    NativeMethods.UnregisterPowerSettingNotification(handle);
                }

                disposed = true;
            }
        }

        ~PowerBroadcasts()
        {
            Dispose(false);
        }

        #endregion
    }
}
