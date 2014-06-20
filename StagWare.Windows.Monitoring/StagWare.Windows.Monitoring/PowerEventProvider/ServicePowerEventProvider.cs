using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StagWare.Windows.Monitoring
{
    public class ServicePowerEventProvider : PowerEventProvider
    {
        #region Nested Types

        private static class NativeMethods
        {
            public const int NO_ERROR = 0;

            public const int SERVICE_CONTROL_STOP = 0x00000001;
            public const int SERVICE_CONTROL_PAUSE = 0x00000002;
            public const int SERVICE_CONTROL_CONTINUE = 0x00000003;
            public const int SERVICE_CONTROL_INTERROGATE = 0x00000004;
            public const int SERVICE_CONTROL_SHUTDOWN = 0x00000005;

            public const int SERVICE_CONTROL_POWEREVENT = 0x0000000D;

            public delegate int ServiceControlHandlerEx(int control,
                int eventType, IntPtr eventData, IntPtr context);

            [DllImport("Kernel32")]
            public extern static Boolean CloseHandle(IntPtr handle);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern IntPtr RegisterServiceCtrlHandlerEx(
                string lpServiceName,
                ServiceControlHandlerEx cbex,
                IntPtr context);
        }

        #endregion

        #region Private Fields

        IntPtr receiverHandle;
        NativeMethods.ServiceControlHandlerEx handler;

        #endregion

        #region Events

        public event EventHandler ServiceStop;
        public event EventHandler ServicePause;
        public event EventHandler ServiceContinue;
        public event EventHandler ServiceShutdown;

        #endregion

        #region Properties

        public override IntPtr ReceiverHandle
        {
            get { return receiverHandle; }
        }

        public override HandleType HandleType
        {
            get { return Monitoring.HandleType.ServiceHandle; }
        }

        #endregion

        #region Constructors

        public ServicePowerEventProvider(string serviceName)
        {
            // Make sure to keep a reference of the delegate in scope
            // to prevent it from being garbage collected.
            this.handler = new NativeMethods.ServiceControlHandlerEx(HandlerEx);

            this.receiverHandle = NativeMethods.RegisterServiceCtrlHandlerEx(
                serviceName,
                handler,
                IntPtr.Zero);
        }

        #endregion

        #region Protected Methods

        protected int HandlerEx(int control, int eventType, IntPtr eventData, IntPtr context)
        {
            switch (control)
            {
                case NativeMethods.SERVICE_CONTROL_STOP:
                    OnServiceStop();
                    break;

                case NativeMethods.SERVICE_CONTROL_PAUSE:
                    OnServicePause();
                    break;

                case NativeMethods.SERVICE_CONTROL_CONTINUE:
                    OnServiceContinue();
                    break;

                case NativeMethods.SERVICE_CONTROL_INTERROGATE:
                    return NativeMethods.NO_ERROR;

                case NativeMethods.SERVICE_CONTROL_SHUTDOWN:
                    OnServiceShutdown();
                    break;

                case NativeMethods.SERVICE_CONTROL_POWEREVENT:
                    var args = new PowerEventArgs()
                    {
                        EventType = eventType,
                        Data = eventData
                    };

                    OnEventReceived(args);
                    break;
            }            

            return NativeMethods.NO_ERROR;
        }

        protected void OnServiceStop()
        {
            if (this.ServiceStop != null)
            {
                ServiceStop(this, EventArgs.Empty);
            }
        }

        protected void OnServicePause()
        {
            if (this.ServicePause != null)
            {
                ServicePause(this, EventArgs.Empty);
            }
        }

        protected void OnServiceContinue()
        {
            if (this.ServiceContinue != null)
            {
                ServiceContinue(this, EventArgs.Empty);
            }
        }

        protected void OnServiceShutdown()
        {
            if (this.ServiceShutdown != null)
            {
                this.ServiceShutdown(this, EventArgs.Empty);
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

                NativeMethods.CloseHandle(this.ReceiverHandle);

                disposed = true;
            }
        }

        ~ServicePowerEventProvider()
        {
            Dispose(false);
        }

        #endregion
    }
}
