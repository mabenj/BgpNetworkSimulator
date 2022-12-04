using System;

namespace Router
{
    public class BgpUpdateMessage: IBgpMessage
    {
        public BgpUpdateMessage(byte[] message)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }
}