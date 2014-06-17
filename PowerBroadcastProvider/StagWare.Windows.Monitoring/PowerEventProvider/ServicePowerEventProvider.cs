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

        #region Properties

        public override IntPtr ReceiverHandle { get; private set; }

        public override HandleType HandleType
        {
            get { return Monitoring.HandleType.ServiceHandle; }
        }

        #endregion

        #region Constructors

        public ServicePowerEventProvider(string serviceName)
        {
            NativeMethods.RegisterServiceCtrlHandlerEx(
                serviceName,
                new NativeMethods.ServiceControlHandlerEx(HandlerEx),
                IntPtr.Zero);
        }

        #endregion

        #region Protected Methods

        protected int HandlerEx(int control, int eventType, IntPtr eventData, IntPtr context)
        {
            var args = new PowerEventArgs()
            {
                EventType = eventType,
                Data = eventData
            };

            OnEventReceived(args);

            return 0;
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
