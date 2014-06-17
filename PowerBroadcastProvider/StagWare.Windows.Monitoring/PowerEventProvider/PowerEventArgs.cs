using System;

namespace StagWare.Windows.Monitoring
{
    public class PowerEventArgs : EventArgs
    {
        public int EventType { get; set; }
        public IntPtr Data { get; set; }
    }
}
