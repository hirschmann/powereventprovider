using System;

namespace StagWare.Windows.Monitoring
{
    public class WindowPowerEventProvider : PowerEventProvider
    {
        #region Constants

        private const int WM_POWERBROADCAST = 0x0218;

        #endregion

        #region Private Fields

        MessageReceiverWindow window;

        #endregion

        #region Properties

        public override IntPtr ReceiverHandle
        {
            get { return window.Handle; }
        }

        public override HandleType HandleType
        {
            get { return Monitoring.HandleType.WindowHandle; }
        }

        #endregion

        #region Constructor

        public WindowPowerEventProvider()
        {
            window = new MessageReceiverWindow();
            window.MessageReceived += window_MessageReceived;
        }

        #endregion

        #region EventHandlers

        void window_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Msg == WM_POWERBROADCAST)
            {
                var args = new PowerEventArgs()
                {
                    EventType = e.Message.WParam.ToInt32(),
                    Data = e.Message.LParam
                };

                OnEventReceived(args);
            }
        }

        #endregion

        #region IDisposable implementation

        private bool disposed;

        public override void Dispose()
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

                disposed = true;
            }
        }

        ~WindowPowerEventProvider()
        {
            Dispose(false);
        }

        #endregion
    }
}
