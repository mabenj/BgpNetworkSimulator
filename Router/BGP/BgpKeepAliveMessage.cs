using System;

namespace Router
{
    public class BgpKeepAliveMessage: IBgpMessage
    {
        public BgpKeepAliveMessage(byte[] message)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }
}