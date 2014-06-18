using System;
using System.Windows.Forms;

namespace StagWare.Windows
{
    public class MessageReceivedEventArgs : EventArgs
    {
        #region Properties

        public Message Message { get; set; }

        #endregion

        #region Constructor

        public MessageReceivedEventArgs()
        {
            this.Message = new Message();
        }

        public MessageReceivedEventArgs(Message message)
        {
            this.Message = message;
        }

        #endregion
    }
}
