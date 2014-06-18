using System;

namespace StagWare.Windows.Monitoring
{
    public abstract class PowerEventProvider : IPowerEventProvider
    {
        public abstract IntPtr ReceiverHandle { get; }
        public abstract HandleType HandleType { get; }
        public event EventHandler<PowerEventArgs> EventReceived;
        public abstract void Dispose();

        protected void OnEventReceived(PowerEventArgs e)
        {
            if (this.EventReceived != null)
            {
                EventReceived(this, e);
            }
        }
    }
}
