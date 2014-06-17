using System;

namespace StagWare.Windows.Monitoring
{
    public class LidswitchStateEventArgs : EventArgs
    {
        public bool IsLidOpen { get; set; }
    }
}
