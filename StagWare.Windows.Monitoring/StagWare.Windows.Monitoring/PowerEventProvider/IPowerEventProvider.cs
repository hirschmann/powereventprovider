using System;

namespace StagWare.Windows.Monitoring
{
    public enum HandleType : int
    {
        WindowHandle = 0,
        ServiceHandle = 1
    }

    public interface IPowerEventProvider : IDisposable
    {
        event EventHandler<PowerEventArgs> EventReceived;
        IntPtr ReceiverHandle { get; }
        HandleType HandleType { get; }
    }
}
