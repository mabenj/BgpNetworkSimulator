using System;

namespace Router
{
    public class BgpNotificationMessage: IBgpMessage
    {
        public BgpNotificationMessage(byte[] message)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }
}