using System;

namespace StagWare.Windows.Monitoring
{
    public class PowerLineStatusEventArgs : EventArgs
    {
        public SystemPowerCondition PowerCondition { get; set; }
    }
}
