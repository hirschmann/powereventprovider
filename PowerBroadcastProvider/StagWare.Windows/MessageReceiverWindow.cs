using System;
using System.Windows.Forms;

namespace StagWare.Windows
{
    public class MessageReceiverWindow : NativeWindow
    {
        #region Events

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Constructors

        public MessageReceiverWindow()
        {
            base.CreateHandle(new CreateParams());
        }

        #endregion

        #region Overrides

        protected override void WndProc(ref Message m)
        {
            OnMessageReceived(m);

            base.WndProc(ref m);
        }

        #endregion

        #region Protected Methods

        protected void OnMessageReceived(Message message)
        {
            if (this.MessageReceived != null)
            {
                MessageReceived(this, new MessageReceivedEventArgs(message));
            }
        }

        #endregion
    }
}
