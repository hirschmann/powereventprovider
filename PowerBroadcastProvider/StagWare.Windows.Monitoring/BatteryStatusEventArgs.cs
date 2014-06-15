using System;

namespace StagWare.Windows.Monitoring
{
    public class BatteryStatusEventArgs : EventArgs
    {
        public int BatteryPercentageRemaining { get; set; }
    }
}
